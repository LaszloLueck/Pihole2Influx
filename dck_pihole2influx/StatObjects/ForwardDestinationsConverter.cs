using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dck_pihole2influx.Transport.Telnet;
using Optional;

namespace dck_pihole2influx.StatObjects
{
    /// <summary>
    /// Shows statistics about the ratio of the asked forwarders. The resultset looks like:
    ///>forward-dest 
    ///-2 22.31 blocklist blocklist
    ///-1 8.24 cache cache
    ///0 35.31 192.168.1.1 opnsense.localdomain
    ///1 19.39 1.0.0.1 one.one.one.one
    ///2 15.60 1.1.1.1 one.one.one.one
    ///---EOM---
    /// </summary>
    public class ForwardDestinationsConverter : TelnetResultConverter
    {
        protected override Dictionary<string, PatternValue> GetPattern()
        {
            return new Dictionary<string, PatternValue>();
        }

        public override PiholeCommands GetPiholeCommand()
        {
            return PiholeCommands.Forwarddestinations;
        }

        public override ConverterType GetConverterType()
        {
            return ConverterType.NumberedPercentageList;
        }

        public override async Task<string> GetJsonObjectFromDictionaryAsync(bool prettyPrint)
        {
            
            var obj = ConvertDictionaryOpt(DictionaryOpt);
            var to = (from element in obj select GetNumberedPercentageFromKeyValue(element)).OrderBy(element =>
                element.position);
            return await ConvertOutputToJson(to, prettyPrint);
        }
    }
}