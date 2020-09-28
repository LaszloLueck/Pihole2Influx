using System.Collections.Generic;
using dck_pihole2influx.Transport.Telnet;

namespace dck_pihole2influx.StatObjects
{
    /// <summary>
    /// Shows statistics about the pihole server. The resultset looks like:
    /// >stats
    ///domains_being_blocked 116007
    ///dns_queries_today 30163
    ///ads_blocked_today 5650
    ///ads_percentage_today 18.731558
    ///unique_domains 1056
    ///queries_forwarded 4275
    ///queries_cached 20238
    ///clients_ever_seen 11
    ///unique_clients 9
    ///status enabled
    /// </summary>
    public class StatsConverter : TelnetResultConverter
    {
        public const string DomainsBeingBlocked = "DomainsBeingBlocked";
        public const string DnsQueriesToday = "DnsQueriesToday";
        public const string AdsBlockedToday = "AdsBlockedToday";
        public const string AdsPercentageToday = "AdsPercentageToday";
        public const string UniqueDomains = "UniqueDomains";
        public const string QueriesForwarded = "QueriesForwarded";
        public const string QueriesCached = "QueriesCached";
        public const string ClientsEverSeen = "ClientsEverSeen";
        public const string UniqueClients = "UniqueClients";
        public const string Status = "Status";
        
        protected override Dictionary<string, PatternValue> GetPattern() => new Dictionary<string, PatternValue>
        {
            {"domains_being_blocked", new PatternValue(DomainsBeingBlocked, ValueTypes.Int, 0)},
            {"dns_queries_today", new PatternValue(DnsQueriesToday, ValueTypes.Int, 0)},
            {"ads_blocked_today", new PatternValue(AdsBlockedToday, ValueTypes.Int, 0)},
            {"ads_percentage_today", new PatternValue(AdsPercentageToday, ValueTypes.Float, 0)},
            {"unique_domains", new PatternValue(UniqueDomains, ValueTypes.Int, 0)},
            {"queries_forwarded", new PatternValue(QueriesForwarded, ValueTypes.Int, 0)},
            {"queries_cached", new PatternValue(QueriesCached, ValueTypes.Int, 0)},
            {"clients_ever_seen", new PatternValue(ClientsEverSeen, ValueTypes.Int, 0)},
            {"unique_clients", new PatternValue(UniqueClients, ValueTypes.Int, 0)},
            {"status", new PatternValue(Status, ValueTypes.String, "")}
        };

        public override PiholeCommands GetPiholeCommand()
        {
            return PiholeCommands.Stats;
        }
    }
}