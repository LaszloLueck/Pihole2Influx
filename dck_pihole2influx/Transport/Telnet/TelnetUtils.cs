using System;
using System.Collections.Generic;

namespace dck_pihole2influx.Transport.Telnet
{
    public static class TelnetUtils
    {

        public enum PiholeCommands {
            STATS,
            QUIT,
            TOPDOMAINS,
            TOPADS,
            TOPCLIENTS,
            FORWARDDESTINATIONS,
            QUERYTYPES,
            VERSION,
            DBSTATS,
            CACHEINFO,
            OVERTIME
        }

        private static Dictionary<PiholeCommands, string> Commands = new Dictionary<PiholeCommands, string>()
        {
            {PiholeCommands.STATS, ">stats"},
            {PiholeCommands.QUIT, ">quit"},
            {PiholeCommands.TOPDOMAINS, ">top-domains"},
            {PiholeCommands.TOPADS, ">top-ads"},
            {PiholeCommands.TOPCLIENTS, ">top-clients"},
            {PiholeCommands.FORWARDDESTINATIONS, ">forward-dest"},
            {PiholeCommands.QUERYTYPES, ">querytypes"},
            {PiholeCommands.VERSION, ">version"},
            {PiholeCommands.DBSTATS, ">dbstats"},
            {PiholeCommands.CACHEINFO, ">cacheinfo"},
            {PiholeCommands.OVERTIME, ">overTime"}
        };
        

        public static string GetCommandByName(PiholeCommands value)
        {
            return Commands[value];
        }
    }
}