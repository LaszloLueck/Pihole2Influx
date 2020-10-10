using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dck_pihole2influx.Transport.Telnet;
using Optional;

namespace dck_pihole2influx.StatObjects
{
    
    /// <summary>
    /// Topads contains a list of top blocked daomains with the amount
    /// 0 8 googleads.g.doubleclick.net
    /// 1 6 www.googleadservices.com
    /// 2 1 cdn.mxpnl.com
    /// 3 1 collector.githubapp.com
    /// 4 1 www.googletagmanager.com
    /// 5 1 s.zkcdn.net
    /// </summary>
    public class TopAdsConverter : TelnetResultConverter, IBaseConverter
    {
        public Dictionary<string, PatternValue> GetPattern()
        {
            return new Dictionary<string, PatternValue>();
        }

        public override PiholeCommands GetPiholeCommand()
        {
            return PiholeCommands.Topads;
        }

        public override async Task<string> GetJsonObjectFromDictionaryAsync(bool prettyPrint)
        {
            var obj = ConvertDictionaryOpt(DictionaryOpt);
            var to = (from element in obj select GetNumberdUrlFromKeyValue(element)).OrderBy(
                element => element.position);
            return await ConvertOutputToJson(to, prettyPrint);
        }

        protected override Option<(string, dynamic)> CalculateTupleFromString(string line)
        {
            return ConvertResultForNumberedUrlList(line, GetPattern());
        }
    }
}