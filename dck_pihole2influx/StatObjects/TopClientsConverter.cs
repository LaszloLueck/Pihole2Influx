using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using dck_pihole2influx.Transport.Telnet;
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

        public override Task<string> GetJsonObjectFromDictionaryAsync(bool prettyPrint)
        {
            throw new NotImplementedException();
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