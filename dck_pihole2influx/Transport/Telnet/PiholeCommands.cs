namespace dck_pihole2influx.Transport.Telnet
{
    public enum PiholeCommands {
        Stats,
        Quit,
        Topdomains,
        Topads,
        Topclients,
        Forwarddestinations,
        Querytypes,
        Version,
        Dbstats,
        Cacheinfo,
        Overtime
    }
}