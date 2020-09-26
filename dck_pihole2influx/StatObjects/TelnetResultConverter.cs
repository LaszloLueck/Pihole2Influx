using System;
using System.Collections.Generic;
using System.Linq;
using dck_pihole2influx.Logging;
using dck_pihole2influx.Transport.Telnet;
using Newtonsoft.Json;
using Optional;
using Optional.Collections;
using Serilog;

namespace dck_pihole2influx.StatObjects
{
    public abstract class TelnetResultConverter
    {
        private static readonly ILogger Log = LoggingFactory<TelnetResultConverter>.CreateLogging();
        private string _input;

        public Option<Dictionary<string, dynamic>> DictionaryOpt { get; private set; }

        public void Convert(string input)
        {
            _input = input;
            DictionaryOpt = GetDtoFromResult();
        }

        protected abstract Dictionary<string, PatternValue> GetPattern();

        public abstract PiholeCommands GetPiholeCommand();

        public virtual string GetTerminator()
        {
            return "---EOM---";
        }

        private Option<Dictionary<string, dynamic>> GetDtoFromResult()
        {
            if (_input.Length == 0)
            {
                Log.Warning("the input string (telnet result) contains no data, please check your configuration.");
                return Option.None<Dictionary<string, dynamic>>();
            }
            var splitted = _input.Split("\n");
            try
            {
                var ret = new List<(string, dynamic)>();
                foreach (var s in splitted)
                {
                    GetPattern().FirstOrNone(value => s.Contains(value.Key)).Map(result =>
                    {
                        var (key, patternValue) = result;
                        return patternValue.ValueType switch
                        {
                            ValueTypes.Int => ValueConverterBase<int>
                                .Convert(s, key, (int) patternValue.AlternativeValue)
                                .Map<(string, dynamic)>(
                                    value => (patternValue.GivenName, ((BaseValue<int>) value).GetValue())),
                            ValueTypes.String => ValueConverterBase<string>
                                .Convert(s, key, (string) patternValue.AlternativeValue)
                                .Map<(string, dynamic)>(value =>
                                    (patternValue.GivenName, ((BaseValue<string>) value).GetValue())),
                            _ => Option.None<(string, dynamic)>()
                        };
                    }).Flatten().MatchSome(tuple => ret.Add(tuple));
                }

                if (ret.Count != GetPattern().Count)
                {
                    Log.Warning($"The results contains less ({ret.Count} entries) data than the configuration ({GetPattern().Count} entries)");
                    return Option.None<Dictionary<string, object>>();
                }
                return Option.Some(ret.ToDictionary(l => l.Item1, l => l.Item2));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while create an object from return string");
                Log.Warning(_input);
                return Option.None<Dictionary<string, dynamic>>();
            }
        }

        public Option<string> GetJsonFromObject(bool prettyPrint)
        {
            return DictionaryOpt.Map(value =>
            {
                var d = value.Select(line => new {key = line.Key, value = line.Value});
                if(prettyPrint)
                    return JsonConvert.SerializeObject(d, Formatting.Indented);
                return JsonConvert.SerializeObject(d);
            });
        }
    }
}