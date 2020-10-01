using System.Collections.Generic;
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
            return (from match in matches select GenerateOutputFromMatchOpt(match)).FirstOrNone().Flatten();
        }

        private static Option<(string, dynamic)> GenerateOutputFromMatchOpt(Match match)
        {
            return (int.TryParse(match.Groups[2].Value, out var intParsed)
                    ? Option.Some(intParsed)
                    : Option.None<int>())
                .Map<(string, dynamic)>(result => (match.Groups[1].Value, (intParsed, match.Groups[3].Value)));
        }
    }
}