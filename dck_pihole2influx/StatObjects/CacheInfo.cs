using System;
using dck_pihole2influx.Logging;
using Newtonsoft.Json;
using Optional;
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
    public class CacheInfo
    {
        private static readonly ILogger Log = LoggingFactory<CacheInfo>.CreateLogging();

        private readonly string _input;

        public Option<CacheInfoDto> CacheInfoDtoOpt { get; }
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

        private Option<CacheInfoDto> GetDtoFromResult()
        {
            var splitted = _input.Split("\n");
            try
            {
                var returnObject = new CacheInfoDto();
                foreach (var s in splitted)
                {
                    if (s.StartsWith("cache-size: "))
                    {
                        returnObject.CacheSize = Int32.Parse(RemoveKeyAndTrim("cache-size:", s));
                    }

                    if (s.StartsWith("cache-live-freed: "))
                    {
                        returnObject.CacheLiveFreed = Int32.Parse(RemoveKeyAndTrim("cache-live-freed:", s));
                    }

                    if (s.StartsWith("cache-inserted: "))
                    {
                        returnObject.CacheInserted = Int32.Parse(RemoveKeyAndTrim("cache-inserted:", s));
                    }
                }

                return Option.Some<CacheInfoDto>(returnObject);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while create an object from return string");
                Log.Warning(_input);
                return Option.None<CacheInfoDto>();
            }
        }

        private Option<string> GetJsonFromDto()
        {
            return CacheInfoDtoOpt.Map<string>(JsonConvert.SerializeObject);
        }
    }

    public class CacheInfoDto
    {
        public int CacheSize;
        public int CacheLiveFreed;
        public int CacheInserted;
    }
}