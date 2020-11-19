using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dck_pihole2influx.Transport.InfluxDb.Measurements;
using dck_pihole2influx.Transport.Telnet;
using Optional;

namespace dck_pihole2influx.StatObjects
{
    /// <summary>
    /// CacheInfo shows information about piholes cache state. The resultset looks like:
    /// >cacheinfo
    ///cache-size: 500000
    ///cache-live-freed: 0
    ///cache-inserted: 15529
    /// </summary>
    public class CacheInfoConverter : TelnetResultConverter, IBaseConverter
    {
        public const string CacheSize = "CacheSize";
        public const string CacheLiveFreed = "CacheLiveFreed";
        public const string CacheInserted = "CacheInserted";

        public Dictionary<string, PatternValue> GetPattern() => new Dictionary<string, PatternValue>
        {
            {"cache-size:", new PatternValue(CacheSize, ValueTypes.Int, 0)},
            {"cache-live-freed:", new PatternValue(CacheLiveFreed, ValueTypes.Int, 0)},
            {"cache-inserted:", new PatternValue(CacheInserted, ValueTypes.Int, 0)}
        };

        public override Task<List<IBaseMeasurement>> CalculateMeasurementData()
        {
            return Task.Run(() =>
            {
                return DictionaryOpt.Map(dic =>
                {
                    var cacheSize = ((PrimitiveResultInt) dic[CacheSize]).Value;
                    var cacheLiveFreed = ((PrimitiveResultInt) dic[CacheLiveFreed]).Value;
                    var cacheInserted = ((PrimitiveResultInt) dic[CacheInserted]).Value;

                    var returnValue = new MeasurementCacheInfo()
                    {
                        CacheInserted = cacheInserted,
                        CacheSize = cacheSize,
                        CacheLiveFreed = cacheLiveFreed,
                        Time = DateTime.Now
                    };
                    return new List<IBaseMeasurement> {returnValue};
                }).ValueOr(new List<IBaseMeasurement>()).ToList();
            });
        }

        public override PiholeCommands GetPiholeCommand()
        {
            return PiholeCommands.Cacheinfo;
        }

        public override async Task<string> GetJsonObjectFromDictionaryAsync(bool prettyPrint)
        {
            var obj = ConvertDictionaryOpt(DictionaryOpt)
                .Select(ConvertIBaseResultToPrimitive)
                .ToDictionary(element => element.Item1, element => element.Item2);

            return await ConvertOutputToJson(obj, prettyPrint);
        }

        protected override Option<(string, IBaseResult)> CalculateTupleFromString(string line)
        {
            return ConvertResultForStandard(line, GetPattern());
        }
    }
}