using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using dck_pihole2influx.Logging;
using Optional;
using Optional.Collections;
using Serilog;

namespace dck_pihole2influx.StatObjects
{
    public class ConverterUtils
    {
        private static readonly ILogger Log = LoggingFactory<ConverterUtils>.CreateLogging();

        protected Option<(string, IBaseResult)> ConvertResultForStandard(string line,
            Dictionary<string, PatternValue> pattern)
        {
            var convertedLineOpt = pattern.FirstOrNone(value => line.Contains(value.Key)).FlatMap(
                result =>
                {
                    var (key, patternValue) = result;
                    return patternValue.ValueType switch
                    {
                        ValueTypes.Int => ValueConverterBase<int>
                            .Convert(line, key, (int) patternValue.AlternativeValue)
                            .Map<(string, IBaseResult)>(
                                value => (patternValue.GivenName,
                                    new PrimitiveResultInt(((BaseValue<int>) value).GetValue()))),
                        ValueTypes.String => ValueConverterBase<string>
                            .Convert(line, key, (string) patternValue.AlternativeValue)
                            .Map<(string, IBaseResult)>(value =>
                                (patternValue.GivenName,
                                    new PrimitiveResultString(((BaseValue<string>) value).GetValue()))),
                        ValueTypes.Float => ValueConverterBase<float>
                            .Convert(line, key, (float) patternValue.AlternativeValue)
                            .Map<(string, IBaseResult)>(value =>
                                (patternValue.GivenName,
                                    new PrimitiveResultFloat(((BaseValue<float>) value).GetValue()))),
                        _ => Option.None<(string, IBaseResult)>()
                    };
                });
            return convertedLineOpt;
        }

        protected Option<(string, IBaseResult)> ConvertResultForNumberedUrlAndIpList(string line,
            Dictionary<string, PatternValue> _)
        {
            /// Every line looks like '0 24593 192.168.1.1 aaa.localdomain'
            /// or '5 2587 192.168.1.120' when no hostname is provided.
            /// For that reason we must pay attention!
            /// (\d)\s(\d{1,})\s(?:[0-9]{1,3}\.){3}[0-9]{1,3}\s?(.*)

            const string pattern = @"(\d)\s(\d{1,})\s(?:[0-9]{1,3}\.){3}[0-9]{1,3}\s?(.*)";


            return Option.None<(string, IBaseResult)>();
        }


        protected Option<(string, IBaseResult)> ConvertResultForNumberedUrlList(string line,
            Dictionary<string, PatternValue> _)
        {
            //Every line looks like '1 236 safebrowsing-cache.google.com'
            //Lets split the parameter by regex.
            //([0-9]{1,2}) ([0-9]{1,3}) ([\w\-\.\d]{1,})
            const string pattern = @"(\d)\s(\d{1,})\s(.*)";

            var matches = Regex.Matches(line, pattern);
            return (from match in matches select GenerateOutputFromMatchOptInt(match)).FirstOrNone().Flatten();
        }

        protected Option<(string, IBaseResult)> ConvertResultForNumberedPercentage(string line,
            Dictionary<string, PatternValue> _)
        {
            //Every line looks like -2 22.31 blocklist blocklist
            //Lets split the parameter by regex.
            //([\-0-9]{1,2})+ ([0-9\.]{1,5})+ ([\w\-\.\d\ ]{1,})
            const string pattern = @"(-?\d)\s([\d\.]{1,})\s(.*)";
            var matches = Regex.Matches(line, pattern);
            return (from match in matches select GenerateOutputFromMatchOptDouble(match)).FirstOrNone().Flatten();
        }

        protected Option<(string, IBaseResult)> ConvertColonSplittedLine(string line,
            Dictionary<string, PatternValue> _)
        {
            //A line looks like this: A (IPv4): 67.73
            //Lets split them by colon and return the first part as key and the second part as double...
            var splitLine = line.Split(":");
            if (splitLine.Length != 2)
                return Option.None<(string, IBaseResult)>();

            double dblValue =
                double.TryParse(splitLine[1], NumberStyles.Number, CultureInfo.InvariantCulture, out dblValue)
                    ? dblValue
                    : 0d;

            IBaseResult retValue = new StringDoubleOutput(splitLine[0], dblValue);
            return Option.Some<(string, IBaseResult)>((splitLine[0], retValue));
        }

        private Option<(string, IBaseResult)> GenerateOutputFromMatchOptDouble(Match match)
        {
            return (double.TryParse(match.Groups[2].Value, NumberStyles.Number, CultureInfo.InvariantCulture,
                    out var doubleParsed)
                    ? Option.Some(doubleParsed)
                    : Option.None<double>())
                .Map<(string, IBaseResult)>(result => (match.Groups[1].Value,
                    new DoubleOutputNumberedList(doubleParsed, match.Groups[1].Value, match.Groups[3].Value)));
        }

        private Option<(string, IBaseResult)> GenerateOutputFromMatchOptInt(Match match)
        {
            return (int.TryParse(match.Groups[2].Value, out var intParsed)
                    ? Option.Some(intParsed)
                    : Option.None<int>())
                .Map<(string, IBaseResult)>(result => (match.Groups[1].Value,
                    new IntOutputNumberedList(intParsed, match.Groups[1].Value, match.Groups[3].Value)));
        }

        protected ConcurrentDictionary<string, IBaseResult> ConvertDictionaryOpt(
            Option<ConcurrentDictionary<string, IBaseResult>> inputOpt)
        {
            return inputOpt.ValueOr(() =>
            {
                Log.Warning("Cannot convert dictionary to json, dictionary is none!");
                return new ConcurrentDictionary<string, IBaseResult>();
            });
        }

        protected NumberedUrlItem GetNumberdUrlFromKeyValue(KeyValuePair<string, dynamic> keyValue)
        {
            int key = int.TryParse(keyValue.Key, out key) ? key : 0;
            var tpl = ((int, string)) keyValue.Value;
            return new NumberedUrlItem(key, tpl.Item1, tpl.Item2);
        }

        protected async Task<string> ConvertOutputToJson<T>(T output, bool prettyPrint)
        {
            try
            {
                await using var stream = new MemoryStream();
                await JsonSerializer.SerializeAsync(stream, output, output.GetType(),
                    new JsonSerializerOptions() {WriteIndented = prettyPrint});
                stream.Position = 0;
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occured while processing a data structure to json string");
                return await Task.Run(() => string.Empty);
            }
        }

        protected Option<ParallelQuery<string>> SplitInputString(string input, string terminator)
        {
            try
            {
                var splitted = input
                    .Split("\n")
                    .Where(s => !string.IsNullOrWhiteSpace(s) && s != terminator)
                    .AsParallel()
                    .AsOrdered();

                return Option.Some(splitted);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occured while splitting the input line");
                return Option.None<ParallelQuery<string>>();
            }
        }

        protected NumberedPercentageItem GetNumberedPercentageFromKeyValue(KeyValuePair<string, IBaseResult> keyvalue)
        {
            int key = int.TryParse(keyvalue.Key, out key) ? key : 0;
            var tpl = (StringDoubleOutput) keyvalue.Value;
            return new NumberedPercentageItem(key, tpl.Value, tpl.Key);
        }
    }
}