using System;
using System.Collections.Generic;
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
            var a = pattern.FirstOrNone(value => line.Contains(value.Key)).FlatMap(
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
            return a;
        }

        public static Option<(string, dynamic)> ConvertResultForNumberedUrlList(string line,
            Dictionary<string, PatternValue> patternDic)
        {
            //Every line looks like '1 236 safebrowsing-cache.google.com'
            //Lets split the parameter by regex.
            //([0-9]{1,2}) ([0-9]{1,3}) ([\w\-\.\d]{1,})
            const string pattern = @"([0-9]{1,2}) ([0-9]{1,3}) ([\w\-\.\d]{1,})";

            MatchCollection matches = Regex.Matches(line, pattern);
            int key = 0;
            int count = 0;
            string domain = "";
            foreach (Match match in matches)
            {
                Console.WriteLine($"A: {match.Groups[1].Value}");
                Console.WriteLine($"B: {match.Groups[2].Value}");
                Console.WriteLine($"C: {match.Groups[3].Value}");
            }

            return Option.None<(string, dynamic)>();
        }
    }
}