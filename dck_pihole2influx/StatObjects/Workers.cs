using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace dck_pihole2influx.StatObjects
{
    public static class  Workers
    {
        public static ConcurrentBag<TelnetResultConverter> GetJobsToDo()
        {
            return new ConcurrentBag<TelnetResultConverter>
            {
                new CacheInfoConverter(),
                new StatsConverter()
                
            };
        }
        
    }
}