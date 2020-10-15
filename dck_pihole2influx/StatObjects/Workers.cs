using System.Collections.Concurrent;

namespace dck_pihole2influx.StatObjects
{
    public static class  Workers
    {
        public static ConcurrentBag<TelnetResultConverter> GetJobsToDo()
        {
            return new ConcurrentBag<TelnetResultConverter>
            {
                new CacheInfoConverter(),
                new StatsConverter(),
                new TopDomainsConverter(),
                new TopAdsConverter(),
                new QueryTypesConverter(),
                new ForwardDestinationsConverter(),
                new TopClientsConverter()
            };
        }
        
    }
}