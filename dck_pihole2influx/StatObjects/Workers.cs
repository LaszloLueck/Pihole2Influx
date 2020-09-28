using System;
using System.Collections.Generic;

namespace dck_pihole2influx.StatObjects
{
    public static class  Workers
    {
        public static List<TelnetResultConverter> GetJobsToDo()
        {
            return new List<TelnetResultConverter>
            {
                new CacheInfoConverter(),
                new StatsConverter()
                
            };
        }
        
    }
}