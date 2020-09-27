using System;
using dck_pihole2influx.StatObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Optional;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class StatsConverterTest
    {
        private readonly TelnetResultConverter _telnetResultConverter;

        public StatsConverterTest()
        {
            _telnetResultConverter = new StatsConverter();
        }
        
        [TestMethod, Description("fill in with a valid return from the telnet method and convert the result.")]
        public void CheckValidTelnetStringAndReturnSomeResults()
        {
            var testee = @"domains_being_blocked 116007
dns_queries_today 30163
ads_blocked_today 5650
ads_percentage_today 18.731558
unique_domains 1056
queries_forwarded 4275
queries_cached 20238
clients_ever_seen 11
unique_clients 9
status enabled
---EOM---



";
            
            _telnetResultConverter.Convert(testee);

            var expectedResult =
                "[{\"key\":\"DomainsBeingBlocked\",\"value\":116007},{\"key\":\"DnsQueriesToday\",\"value\":30163},{\"key\":\"AdsBlockedToday\",\"value\":5650},{\"key\":\"AdsPercentageToday\",\"value\":18.731558},{\"key\":\"UniqueDomains\",\"value\":1056},{\"key\":\"QueriesForwarded\",\"value\":4275},{\"key\":\"QueriesCached\",\"value\":20238},{\"key\":\"ClientsEverSeen\",\"value\":11},{\"key\":\"UniqueClients\",\"value\":9},{\"key\":\"Status\",\"value\":\"enabled\"}]";

            Assert.AreEqual(Option.Some<string>(expectedResult),_telnetResultConverter.GetJsonFromObject());
        }
        
        
    }
}