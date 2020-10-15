using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using dck_pihole2influx.Transport.Telnet;
using Newtonsoft.Json;
using Optional;
using Optional.Json;

namespace dck_pihole2influx.StatObjects
{
    /// <summary>
    /// Topclients contains a list of the top 10 of the most active clients on pihole.
    /// 0 24593 192.168.1.1 aaa.localdomain
    /// 1 8136 192.168.1.227 bbb.localdomain
    /// 2 4704 192.168.1.225 ccc.localdomain
    /// 3 3741 192.168.1.174 ddd.localdomain
    /// 4 2836 192.168.1.231 eee.localdomain
    /// 5 2587 192.168.1.120 
    /// 6 2035 192.168.1.196 fff.localdomain
    /// 7 2009 192.168.1.226 ggg.localdomain
    /// 8 1952 192.168.1.167 hhh.localdomain
    /// 9 1807 192.168.1.137 
    /// </summary>
    public class TopClientsConverter : TelnetResultConverter, IBaseConverter
    {
        public override PiholeCommands GetPiholeCommand()
        {
            return PiholeCommands.Topclients;
        }

        public override async Task<string> GetJsonObjectFromDictionaryAsync(bool prettyPrint)
        {
            var obj = ConvertDictionaryOpt(DictionaryOpt)
                .OrderBy(element => element.Key)
                .Select(element => (DoubleStringOutputElement) element.Value);



            var textWriter = new StringWriter();
            var outJsonWriter = new JsonTextWriter(textWriter);
            var doubleStringOutputLists = obj as DoubleStringOutputElement[] ?? obj.ToArray();
            var oc = new OptionConverter();
            foreach (var output in doubleStringOutputLists)
            {
                oc.WriteJson(outJsonWriter, output, Newtonsoft.Json.JsonSerializer.Create());
            }
            var res = textWriter.ToString();
            

            return await ConvertOutputToJson(doubleStringOutputLists, prettyPrint);
        }

        protected override Option<(string, IBaseResult)> CalculateTupleFromString(string line)
        {
            return ConvertResultForNumberedUrlAndIpList(line, GetPattern());
        }

        public Dictionary<string, PatternValue> GetPattern()
        {
            return new Dictionary<string, PatternValue>();
        }
    }
}