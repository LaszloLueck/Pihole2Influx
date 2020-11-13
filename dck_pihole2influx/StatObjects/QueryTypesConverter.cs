using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dck_pihole2influx.Transport.InfluxDb.Measurements;
using dck_pihole2influx.Transport.Telnet;
using Optional;

namespace dck_pihole2influx.StatObjects
{
    /// <summary>
    /// Querytypes gives an overview about the different dns-querytypes and the percantage distribution
    ///
    /// >querytypes
    /// A (IPv4): 67.73
    /// AAAA (IPv6): 22.01
    /// ANY: 0.00
    /// SRV: 1.72
    /// SOA: 0.04
    /// PTR: 1.75
    /// TXT: 0.55
    /// NAPTR: 0.04
    /// MX: 0.00
    /// DS: 0.80
    /// RRSIG: 0.00
    /// DNSKEY: 0.17
    /// OTHER: 5.19
    /// ---EOM---
    /// 
    /// </summary>
    public class QueryTypesConverter : TelnetResultConverter, IBaseConverter
    {
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
                    return (from tuple in dic select tuple).Select(kv =>
                    {
                        var confValue = (StringDecimalOutput) kv.Value;
                        return (IBaseMeasurement) new MeasurementQueryType()
                            {DnsType = confValue.Key, Value = confValue.Value};
                    });
                }).ValueOr(new List<IBaseMeasurement>()).ToList();
            });
        }

        public override PiholeCommands GetPiholeCommand()
        {
            return PiholeCommands.Querytypes;
        }

        public override async Task<string> GetJsonObjectFromDictionaryAsync(bool prettyPrint)
        {
            var obj = ConvertDictionaryOpt(DictionaryOpt)
                .OrderBy(element => ((StringDecimalOutput) element.Value).Value)
                .Select(element => (StringDecimalOutput) element.Value);


            return await ConvertOutputToJson(obj, prettyPrint);
        }

        protected override Option<(string, IBaseResult)> CalculateTupleFromString(string line)
        {
            return ConvertColonSplittedLine(line, GetPattern());
        }
    }
}