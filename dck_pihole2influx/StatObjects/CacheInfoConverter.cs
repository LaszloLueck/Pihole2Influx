using System.Collections.Generic;

namespace dck_pihole2influx.StatObjects
{
    public class CacheInfoConverter : TelnetResultConverter
    {
        protected override Dictionary<string, (string, ValueTypes)> GetPattern() => new Dictionary<string, (string, ValueTypes)>()
        {
            {"cache-size:", ("CacheSize", ValueTypes.Int)},
            {"cache-live-freed:", ("CacheLiveFreed", ValueTypes.Int)},
            {"cache-inserted:", ("CacheInserted", ValueTypes.Int)}
        };
        
        
        
        
        
    }
}