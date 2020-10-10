using System.Collections.Generic;

namespace dck_pihole2influx.StatObjects
{
    public interface IBaseConverter
    {
        Dictionary<string, PatternValue> GetPattern();
        
    }
}