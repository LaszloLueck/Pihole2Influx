using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dck_pihole2influx.Transport.InfluxDb.Measurements;
using dck_pihole2influx.Transport.Telnet;
using Optional;

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
    public class StatsConverter : TelnetResultConverter, IBaseConverter
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

        public Dictionary<string, PatternValue> GetPattern() => new Dictionary<string, PatternValue>
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

        public override Task<List<IBaseMeasurement>> CalculateMeasurementData()
        {
            return Task.Run(() =>
            {
                return DictionaryOpt.Map(dic =>
                {
                    var domainsBeingBlocked = ((PrimitiveResultInt) dic[DomainsBeingBlocked]).Value;
                    var dnsQueriesToday = ((PrimitiveResultInt) dic[DnsQueriesToday]).Value;
                    var adsBlockedToday = ((PrimitiveResultInt) dic[AdsBlockedToday]).Value;
                    var adsPercentageToday = ((PrimitiveResultFloat) dic[AdsPercentageToday]).Value;
                    var uniqueDomain = ((PrimitiveResultInt) dic[UniqueDomains]).Value;
                    var queriesForwarded = ((PrimitiveResultInt) dic[QueriesForwarded]).Value;
                    var queriesCached = ((PrimitiveResultInt) dic[QueriesCached]).Value;
                    var clientsEverSeen = ((PrimitiveResultInt) dic[ClientsEverSeen]).Value;
                    var uniqueClients = ((PrimitiveResultInt) dic[UniqueClients]).Value;
                    var status = ((PrimitiveResultString) dic[Status]).Value;

                    var returnValue = new MeasurementStats()
                    {
                        DomainsBeingBlocked = domainsBeingBlocked,
                        DnsQueriesToday = dnsQueriesToday,
                        AdsBlockedToday = adsBlockedToday,
                        AdsPercentageToday = adsPercentageToday,
                        UniqueDomains = uniqueDomain,
                        QueriesForwarded = queriesForwarded,
                        QueriesCached = queriesCached,
                        ClientsEverSeen = clientsEverSeen,
                        UniqueClients = uniqueClients,
                        Status = status
                    };

                    return new List<IBaseMeasurement>() {returnValue};
                }).ValueOr(new List<IBaseMeasurement>()).ToList();
            });
        }

        public override PiholeCommands GetPiholeCommand()
        {
            return PiholeCommands.Stats;
        }

        public override async Task<string> GetJsonObjectFromDictionaryAsync(bool prettyPrint)
        {
            var obj = ConvertDictionaryOpt(DictionaryOpt)
                .Select(ConvertIBaseResultToPrimitive)
                .ToDictionary(element => element.Item1, element => element.Item2);
            return await ConvertOutputToJson(obj, prettyPrint);
        }

        protected override Option<(string, IBaseResult)> CalculateTupleFromString(string line)
        {
            return ConvertResultForStandard(line, GetPattern());
        }
    }
}