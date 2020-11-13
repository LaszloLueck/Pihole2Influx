using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dck_pihole2influx.Logging;
using dck_pihole2influx.Transport.InfluxDb.Measurements;
using dck_pihole2influx.Transport.Telnet;
using Optional;

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
        private static readonly IMySimpleLogger Log = MySimpleLoggerImpl<TopDomainsConverter>.GetLogger();
        public Dictionary<string, PatternValue> GetPattern()
        {
            return new Dictionary<string, PatternValue>();
        }

        public override Task<List<IBaseMeasurement>> CalculateMeasurementData()
        {
            return Task.Run(() =>
            {
                return DictionaryOpt.Map(dic =>
                {
                    return (from tuple in dic select tuple.Value).Select(element =>
                    {
                        var confValue = (IntOutputNumberedElement) element;
                        return (IBaseMeasurement) new MeasurementTopDomains()
                        {
                            Count = confValue.Count,
                            IpOrHost = confValue.IpOrHost,
                            Position = confValue.Position
                        };
                    });
                }).ValueOr(new List<IBaseMeasurement>()).ToList();
            });
        }
        
        public override PiholeCommands GetPiholeCommand()
        {
            return PiholeCommands.Topdomains;
        }

        public override async Task<string> GetJsonObjectFromDictionaryAsync(bool prettyPrint)
        {
            var obj = ConvertDictionaryOpt(DictionaryOpt);

            var to = obj
                .OrderBy(element => element.Key)
                .Select(element => (IntOutputNumberedElement) element.Value);

            return await ConvertOutputToJson(to, prettyPrint);
        }

        protected override Option<(string, IBaseResult)> CalculateTupleFromString(string line)
        {
            return ConvertResultForNumberedUrlList(line, GetPattern());
        }
    }
}