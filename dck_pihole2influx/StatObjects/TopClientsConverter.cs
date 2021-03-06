using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using dck_pihole2influx.Optional.Json;
using dck_pihole2influx.Transport.InfluxDb.Measurements;
using dck_pihole2influx.Transport.Telnet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Optional;

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

        public override Task<List<IBaseMeasurement>> CalculateMeasurementData()
        {
            return Task.Run(() =>
            {
                return DictionaryOpt.Map(dic =>
                {
                    return (from tuple in dic select tuple).Select(tpl =>
                    {
                        var convValue = (DoubleStringOutputElement) tpl.Value;
                        return (IBaseMeasurement) new MeasurementTopClient()
                        {
                            ClientIp = convValue.IpAddress, HostName = convValue.HostName.ValueOr(""),
                            Position = convValue.Position + 1, Count = convValue.Count, Time = DateTime.Now
                        };
                    });
                }).ValueOr(new List<IBaseMeasurement>()).ToList();
            });
        }

        public override async Task<string> GetJsonObjectFromDictionaryAsync(bool prettyPrint)
        {
            var obj = ConvertDictionaryOpt(DictionaryOpt)
                .OrderBy(element => element.Key)
                .Select(element => (DoubleStringOutputElement) element.Value);


            //We have some trouble with serializing Options, so we must write our own json-contractconverter
            var t = Task.Run(async () =>
            {
                var textWriter = new StringWriter();
                var outJsonWriter = new JsonTextWriter(textWriter);
                var doubleStringOutputLists = obj as DoubleStringOutputElement[] ?? obj.ToArray();
                var js = new JsonSerializer
                {
                    NullValueHandling = NullValueHandling.Ignore, ContractResolver = new OptionalContractResolver()
                };
                js.Serialize(outJsonWriter, doubleStringOutputLists);
                await outJsonWriter.FlushAsync();
                await textWriter.FlushAsync();
                var tRes = textWriter.ToString();
                await outJsonWriter.CloseAsync();
                textWriter.Close();
                var jToken = JsonHelper.RemoveEmptyChildren(JToken.Parse(tRes));
                return jToken.ToString();
            });

            return await t;
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