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
#pragma warning disable
using Optional.Linq;
#pragma warning restore

namespace dck_pihole2influx.StatObjects
{
    public class ConverterUtils
    {
        private static readonly IMySimpleLogger Log = MySimpleLoggerImpl<ConverterUtils>.GetLogger();

        protected ConverterUtils()
        {
        }

        protected static Option<(string, IBaseResult)> ConvertResultForStandard(string line,
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
                        ValueTypes.Long => ValueConverterBase<long>
                            .Convert(line, key, (long) patternValue.AlternativeValue)
                            .Map<(string, IBaseResult)>(value => (patternValue.GivenName,
                                new PrimitiveResultLong(((BaseValue<long>) value).GetValue()))),
                        _ => Option.None<(string, IBaseResult)>()
                    };
                });
            return convertedLineOpt;
        }

        protected static Option<(string, IBaseResult)> ConvertResultForOvertime(string line,
            Dictionary<string, PatternValue> _)
        {
            const string pattern = @"([0-9]{1,})\s([0-9]{1,})\s([0-9]{1,})";
            var matches = Regex.Matches(line, pattern);
            return (from match in matches select GenerateOutputForOvertime(match)).FirstOrNone().Flatten();
        }

        protected static Option<(string, IBaseResult)> ConvertResultForNumberedUrlAndIpList(string line,
            Dictionary<string, PatternValue> _)
        {
            /// Every line looks like '0 24593 192.168.1.1 aaa.localdomain'
            /// or '5 2587 192.168.1.120' when no hostname is provided.
            /// For that reason we must pay attention!
            /// (\d)\s(\d{1,})\s(?:[0-9]{1,3}\.){3}[0-9]{1,3}\s?(.*)

            const string pattern = @"(\d)\s(\d{1,})\s(\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b)\s?(.*)";
            var matches = Regex.Matches(line, pattern);

            return (from match in matches select GenerateOutputFromQuadruple(match)).FirstOrNone().Flatten();
        }

        protected static Option<(string, IBaseResult)> ConvertResultForNumberedUrlList(string line,
            Dictionary<string, PatternValue> _)
        {
            //Every line looks like '1 236 safebrowsing-cache.google.com'
            //Lets split the parameter by regex.
            //([0-9]{1,2}) ([0-9]{1,3}) ([\w\-\.\d]{1,})
            const string pattern = @"(\d)\s(\d{1,})\s(.*)";

            var matches = Regex.Matches(line, pattern);
            return (from match in matches select GenerateOutputFromMatchOptInt(match)).FirstOrNone().Flatten();
        }

        protected static Option<(string, IBaseResult)> ConvertResultForNumberedPercentage(string line,
            Dictionary<string, PatternValue> _)
        {
            //Every line looks like -2 22.31 blocklist blocklist
            //Lets split the parameter by regex.
            //([\-0-9]{1,2})+ ([0-9\.]{1,5})+ ([\w\-\.\d\ ]{1,})
            const string pattern = @"(-?\d)\s([\d\.]{1,})\s(.*)";
            var matches = Regex.Matches(line, pattern);
            return (from match in matches select GenerateOutputFromMatchOptDouble(match)).FirstOrNone().Flatten();
        }

        protected static Option<(string, IBaseResult)> ConvertColonSplittedLine(string line,
            Dictionary<string, PatternValue> _)
        {
            //A line looks like this: A (IPv4): 67.73
            //Lets split them by colon and return the first part as key and the second part as double...
            var splitLine = line.Split(":");
            if (splitLine.Length != 2)
                return Option.None<(string, IBaseResult)>();

            decimal dblValue =
                decimal.TryParse(splitLine[1], NumberStyles.Number, CultureInfo.InvariantCulture, out dblValue)
                    ? dblValue
                    : 0;

            IBaseResult retValue = new StringDecimalOutput(splitLine[0], dblValue);
            return Option.Some<(string, IBaseResult)>((splitLine[0], retValue));
        }

        private static Option<(string, IBaseResult)> GenerateOutputFromMatchOptDouble(Match match)
        {
            var percentageOpt = (double.TryParse(match.Groups[2].Value, NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var doubleParsed)
                ? Option.Some(doubleParsed)
                : Option.None<double>());

            var positionOpt =
                int.TryParse(match.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture,
                    out var positionParsed)
                    ? Option.Some(positionParsed)
                    : Option.None<int>();

            return (from percentage in percentageOpt
                from position in positionOpt
                select (match.Groups[1].Value,
                    (IBaseResult) new DoubleOutputNumberedElement(percentage, position, match.Groups[3].Value)));
        }

        private static Option<(string, IBaseResult)> GenerateOutputFromMatchOptInt(Match match)
        {
            var countOpt = int.TryParse(match.Groups[2].Value, out var iCount)
                ? Option.Some(iCount)
                : Option.None<int>();

            var positionOpt = int.TryParse(match.Groups[1].Value, out var iPosition)
                ? Option.Some(iPosition)
                : Option.None<int>();

            return (from count in countOpt
                from position in positionOpt
                select (match.Groups[1].Value,
                    (IBaseResult) new IntOutputNumberedElement(count, position, match.Groups[3].Value)));
        }

        private static Option<(string, IBaseResult)> GenerateOutputForOvertime(Match match)
        {
            var timeStampOpt = long.TryParse(match.Groups[1].Value, out var longParsed)
                ? Option.Some(longParsed)
                : Option.None<long>();

            var permitOpt = int.TryParse(match.Groups[2].Value, out var permitParsed)
                ? Option.Some(permitParsed)
                : Option.None<int>();

            var blockOpt = int.TryParse(match.Groups[3].Value, out var blockParsed)
                ? Option.Some(blockParsed)
                : Option.None<int>();

            return timeStampOpt.FlatMap(timeStamp =>
            {
                return permitOpt.FlatMap(permit =>
                {
                    return blockOpt.Map<(string, IBaseResult)>(block =>
                        (timeStamp.ToString(), new OvertimeOutputElement(timeStamp, permit, block)));
                });
            });
        }

        private static Option<(string, IBaseResult)> GenerateOutputFromQuadruple(Match match)
        {
            //0 24593 192.168.1.1 aaa.localdomain or 0 24593 192.168.1.1

            var countOpt = int.TryParse(match.Groups[2].Value, out var iCount)
                ? Option.Some(iCount)
                : Option.None<int>();

            var positionOpt = int.TryParse(match.Groups[1].Value, out var iPosition)
                ? Option.Some(iPosition)
                : Option.None<int>();

            var result = countOpt.FlatMap<(string, IBaseResult)>(count =>
            {
                return positionOpt.Map<(string, IBaseResult)>(position =>
                {
                    var hostOpt = match.Groups[4].SomeWhen(value => value.Value.Length > 0).Map(group => group.Value);

                    return (match.Groups[1].Value,
                        new DoubleStringOutputElement(position, count, match.Groups[3].Value, hostOpt));
                });
            });

            return result;
        }

        protected static ConcurrentDictionary<string, IBaseResult> ConvertDictionaryOpt(
            Option<ConcurrentDictionary<string, IBaseResult>> inputOpt)
        {
            return inputOpt.ValueOr(() =>
            {
                Log.Warning("Cannot convert dictionary to json, dictionary is none!");
                return new ConcurrentDictionary<string, IBaseResult>();
            });
        }

        protected static async Task<string> ConvertOutputToJson<T>(T output, bool prettyPrint)
        {
            try
            {
                await using (var stream = new MemoryStream())
                {
                    await JsonSerializer.SerializeAsync(stream, output, output.GetType(),
                        new JsonSerializerOptions() {WriteIndented = prettyPrint});
                    stream.Position = 0;
                    using var reader = new StreamReader(stream);
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occured while processing a data structure to json string");
                return await Task.Run(() => string.Empty);
            }
        }

        protected static (string, dynamic) ConvertIBaseResultToPrimitive(KeyValuePair<string, IBaseResult> input)
        {
            (string, dynamic) ret = input.Value switch
            {
                PrimitiveResultString primitiveResultString => (input.Key, primitiveResultString.Value),
                PrimitiveResultFloat primitiveResultFloat => (input.Key, primitiveResultFloat.Value),
                PrimitiveResultInt primitiveResultInt => (input.Key, primitiveResultInt.Value),
                PrimitiveResultLong primitiveResultLong => (input.Key, primitiveResultLong.Value),
                _ => ("", "")
            };

            return ret;
        }

        protected static Option<ParallelQuery<string>> SplitInputString(string input, string terminator)
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
    }
}