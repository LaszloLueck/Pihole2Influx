using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dck_pihole2influx.Transport.Telnet;
using Optional;
using Optional.Linq;

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
    
    public class TopDomainsConverter : TelnetResultConverter, IBaseConverter
    {
        public Dictionary<string, PatternValue> GetPattern()
        {
            return new Dictionary<string, PatternValue>();
        }

        public override PiholeCommands GetPiholeCommand()
        {
            return PiholeCommands.Topdomains;
        }

        public override async Task<string> GetJsonObjectFromDictionaryAsync(bool prettyPrint)
        {
            var obj = ConvertDictionaryOpt(DictionaryOpt);
            
            var to = obj.OrderBy(element =>
            {
                var concreteObject = (IntOutputNumberedList) element.Value;
                return concreteObject.Position;
            });
            
            return await ConvertOutputToJson(to, prettyPrint);
        }

        protected override Option<(string, IBaseResult)> CalculateTupleFromString(string line)
        {
            return ConvertResultForNumberedUrlList(line, GetPattern());
        }
    }
}