using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Optional;
using Optional.Collections;

namespace dck_pihole2influx.StatObjects
{
    public static class ConverterUtils
    {
        public static Option<(string, dynamic)> ConvertResultForStandard(string line,
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
                            .Map<(string, dynamic)>(
                                value => (patternValue.GivenName,
                                    ((BaseValue<int>) value).GetValue())),
                        ValueTypes.String => ValueConverterBase<string>
                            .Convert(line, key, (string) patternValue.AlternativeValue)
                            .Map<(string, dynamic)>(value =>
                                (patternValue.GivenName, ((BaseValue<string>) value).GetValue())),
                        ValueTypes.Float => ValueConverterBase<float>
                            .Convert(line, key, (float) patternValue.AlternativeValue)
                            .Map<(string, dynamic)>(value =>
                                (patternValue.GivenName, ((BaseValue<float>) value).GetValue())),
                        _ => Option.None<(string, dynamic)>()
                    };
                });
            return convertedLineOpt;
        }

        public static Option<(string, dynamic)> ConvertResultForNumberedUrlList(string line,
            Dictionary<string, PatternValue> patternDic)
        {
            //Every line looks like '1 236 safebrowsing-cache.google.com'
            //Lets split the parameter by regex.
            //([0-9]{1,2}) ([0-9]{1,3}) ([\w\-\.\d]{1,})
            const string pattern = @"([0-9]{1,2}) ([0-9]{1,}) ([\w\-\.\d]{1,})";

            MatchCollection matches = Regex.Matches(line, pattern);
            return (from match in matches select GenerateOutputFromMatchOptInt(match)).FirstOrNone().Flatten();
        }

        public static Option<(string, dynamic)> ConvertResultForNumberedPercentage(string line,
            Dictionary<string, PatternValue> patternDic)
        {
            //Every line looks like -2 22.31 blocklist blocklist
            //Lets split the parameter by regex.
            //([\-0-9]{1,2})+ ([0-9\.]{1,5})+ ([\w\-\.\d\ ]{1,})
            const string pattern = @"([\-0-9]{1,2})+ ([0-9\.]{1,5})+ ([\w\-\.\d\ ]{1,})";
            MatchCollection matches = Regex.Matches(line, pattern);
            return (from match in matches select GenerateOutputFromMatchOptDouble(match)).FirstOrNone().Flatten();
        }

        public static Option<(string, dynamic)> ConvertColonSplittedLine(string line,
            Dictionary<string, PatternValue> patternDic)
        {
            //A line looks like this: A (IPv4): 67.73
            //Lets split them by colon and return the first part as key and the second part as double...
            var splitLine = line.Split(":");
            if (splitLine.Length != 2)
                return Option.None<(string, dynamic)>();

            double dblValue = double.TryParse(splitLine[1], NumberStyles.Number, CultureInfo.InvariantCulture, out dblValue)
                ? dblValue
                : 0d;
            
            return Option.Some<(string, dynamic)>((splitLine[0], dblValue));
            
        }

        private static Option<(string, dynamic)> GenerateOutputFromMatchOptDouble(Match match)
        {
            return (double.TryParse(match.Groups[2].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var intParsed)
                    ? Option.Some(intParsed)
                    : Option.None<double>())
                .Map<(string, dynamic)>(result => (match.Groups[1].Value, (intParsed, match.Groups[3].Value)));
        }

        private static Option<(string, dynamic)> GenerateOutputFromMatchOptInt(Match match)
        {
            return (int.TryParse(match.Groups[2].Value, out var intParsed)
                    ? Option.Some(intParsed)
                    : Option.None<int>())
                .Map<(string, dynamic)>(result => (match.Groups[1].Value, (intParsed, match.Groups[3].Value)));
        }
    }
}