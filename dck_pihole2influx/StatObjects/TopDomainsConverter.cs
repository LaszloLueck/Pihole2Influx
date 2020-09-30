using System.Collections.Generic;
using dck_pihole2influx.Transport.Telnet;

namespace dck_pihole2influx.StatObjects
{
    /// <summary>
    /// Topdamains contains a list of top permitted domains with the amount
    /// 0 8462 x.y.z.de
    /// 1 236 safebrowsing-cache.google.com
    /// 2 116 pi.hole
    /// 3 109 z.y.x.de
    /// 4 93 safebrowsing.google.com
    /// 5 96 plus.google.com
    /// </summary>
    
    public class TopDomainsConverter : TelnetResultConverter
    {
        protected override Dictionary<string, PatternValue> GetPattern()
        {
            throw new System.NotImplementedException();
        }

        public override PiholeCommands GetPiholeCommand()
        {
            throw new System.NotImplementedException();
        }

        public override ConverterType GetConverterType()
        {
            return ConverterType.NumberedList;
        }
    }
}