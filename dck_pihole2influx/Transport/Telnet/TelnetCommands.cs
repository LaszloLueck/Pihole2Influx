using System;
using System.Collections.Generic;

namespace dck_pihole2influx.Transport.Telnet
{
    public static class TelnetCommands
    {



        private static Dictionary<PiholeCommands, string> _commands = new Dictionary<PiholeCommands, string>
        {
            {PiholeCommands.Stats, ">stats"},
            {PiholeCommands.Quit, ">quit"},
            {PiholeCommands.Topdomains, ">top-domains"},
            {PiholeCommands.Topads, ">top-ads"},
            {PiholeCommands.Topclients, ">top-clients"},
            {PiholeCommands.Forwarddestinations, ">forward-dest"},
            {PiholeCommands.Querytypes, ">querytypes"},
            {PiholeCommands.Version, ">version"},
            {PiholeCommands.Dbstats, ">dbstats"},
            {PiholeCommands.Cacheinfo, ">cacheinfo"},
            {PiholeCommands.Overtime, ">overTime"}
        };
        

        public static string GetCommandByName(PiholeCommands value)
        {
            return _commands[value];
        }
    }
}