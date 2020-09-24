using System;
using System.Collections.Generic;
using System.Linq;
using dck_pihole2influx.Logging;
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
        public Option<string> AsJsonOpt { get; private set; }

        public void Convert(string input)
        {
            _input = input;
            DictionaryOpt = GetDtoFromResult();
            AsJsonOpt = GetJsonFromDto();
        }
        
        protected abstract Dictionary<string, (string, ValueTypes)> GetPattern();

        private Option<Dictionary<string, dynamic>> GetDtoFromResult()
        {
            var splitted = _input.Split("\n");
            try
            {
                var ret = new List<(string, dynamic)>();
                foreach (var s in splitted)
                {
                    GetPattern().FirstOrNone(value => s.Contains(value.Key)).Map(result =>
                    {
                        var (key, valueTuple) = result;
                        return valueTuple.Item2 switch
                        {
                            ValueTypes.Int => ValueConverterBase<int>.Convert(s, key, 0)
                                .Map<(string, dynamic)>(
                                    value => (valueTuple.Item1, ((BaseValue<int>) value).GetValue())),
                            ValueTypes.String => ValueConverterBase<string>.Convert(s, key, "")
                                .Map<(string, dynamic)>(value =>
                                    (valueTuple.Item1, ((BaseValue<string>) value).GetValue())),
                            _ => Option.None<(string, dynamic)>()
                        };
                    }).Flatten().MatchSome(tuple => ret.Add(tuple));
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

        private Option<string> GetJsonFromDto()
        {
            return DictionaryOpt.Map<string>(value =>
            {
                var d = value.Select(line => new {key = line.Key, value = line.Value});
                return JsonConvert.SerializeObject(d);
            });
        }
    }
}