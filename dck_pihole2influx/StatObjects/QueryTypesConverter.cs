using System.Collections.Generic;
using System.Threading.Tasks;
using dck_pihole2influx.Logging;
using dck_pihole2influx.Transport.Telnet;
using Optional;
using Serilog;

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
        private static readonly ILogger Log = LoggingFactory<QueryTypesConverter>.CreateLogging();

        public Dictionary<string, PatternValue> GetPattern()
        {
            return new Dictionary<string, PatternValue>();
        }

        public override PiholeCommands GetPiholeCommand()
        {
            return PiholeCommands.Querytypes;
        }

        public override async Task<string> GetJsonObjectFromDictionaryAsync(bool prettyPrint)
        {
            var obj = ConvertDictionaryOpt(DictionaryOpt);
            return await ConvertOutputToJson(obj, prettyPrint);
        }

        protected override Option<(string, IBaseResult)> CalculateTupleFromString(string line)
        {
            return ConvertColonSplittedLine(line, GetPattern());
        }
    }
}