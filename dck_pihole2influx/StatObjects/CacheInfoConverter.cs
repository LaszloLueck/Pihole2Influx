using System.Collections.Generic;
using dck_pihole2influx.Transport.Telnet;

namespace dck_pihole2influx.StatObjects
{
    /// <summary>
    /// CacheInfo shows information about piholes cache state. The resultset looks like:
    /// >cacheinfo
    ///cache-size: 500000
    ///cache-live-freed: 0
    ///cache-inserted: 15529
    /// </summary>
    public class CacheInfoConverter : TelnetResultConverter
    {
        protected override Dictionary<string, PatternValue> GetPattern() => new Dictionary<string, PatternValue>()
        {
            {"cache-size:", new PatternValue("CacheSize", ValueTypes.Int, 0)},
            {"cache-live-freed:", new PatternValue("CacheLiveFreed", ValueTypes.Int, 0)},
            {"cache-inserted:", new PatternValue("CacheInserted", ValueTypes.Int, 0)}
        };

        public override PiholeCommands GetPiholeCommand()
        {
            return PiholeCommands.Cacheinfo;
        }
    }
}