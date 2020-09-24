using System;
using System.Collections.Generic;
using System.Text.Json;
using dck_pihole2influx.Logging;
using Optional;
using Optional.Collections;
using Serilog;

namespace dck_pihole2influx.StatObjects
{
    /// <summary>
    /// Convert a representation of the pihole telnet command <cacheinfo> to an appropriate object.
    /// >cacheinfo
    ///cache-size: 10000
    ///cache-live-freed: 0
    ///cache-inserted: 98590
    ///---EOM---
    /// </summary>
    public class CacheInfo : IPiholeDataBase
    {
        private static readonly ILogger Log = LoggingFactory<CacheInfo>.CreateLogging();

        private readonly string _input;

        public Option<List<(string, dynamic)>> CacheInfoDtoOpt { get; }
        public Option<string> AsJsonOpt { get; }

        public CacheInfo(string input)
        {
            _input = input;
            CacheInfoDtoOpt = GetDtoFromResult();
            AsJsonOpt = GetJsonFromDto();
        }

        private string RemoveKeyAndTrim(string key, string input)
        {
            return input.Replace(key, "").TrimEnd().TrimStart();
        }

        public enum ValueTypes
        {
            String,
            Int
        }

        private Option<List<(string, dynamic)>> GetDtoFromResult()
        {
            var splitted = _input.Split("\n");
            var pattern = new Dictionary<string, (string, ValueTypes)>()
            {
                {"cache-size:", ("CacheSize", ValueTypes.Int)},
                {"cache-live-freed:", ("CacheLiveFreed", ValueTypes.Int)},
                {"cache-inserted:", ("CacheInserted", ValueTypes.Int)}
            };

            try
            {
                var ret = new List<(string, dynamic)>();
                foreach (var s in splitted)
                {
                    pattern.FirstOrNone(value => s.Contains(value.Key)).Map(result =>
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

                return Option.Some(ret);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while create an object from return string");
                Log.Warning(_input);
                return Option.None<List<(string, dynamic)>>();
            }
        }

        private Option<string> GetJsonFromDto()
        {
            return CacheInfoDtoOpt.Map<string>(value => JsonSerializer.Serialize(value));
        }
    }

    public class CacheInfoDto
    {
        public int CacheSize;
        public int CacheLiveFreed;
        public int CacheInserted;
    }
}