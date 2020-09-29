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
            //
            // _telnetResultConverter.Convert(testee).Wait();
            //
            // var expectedResult = TestUtils.OrderJsonStringFromConvert(
            //     $"[{{\"key\":\"{StatsConverter.DomainsBeingBlocked}\",\"value\":116007}},{{\"key\":\"{StatsConverter.DnsQueriesToday}\",\"value\":30163}},{{\"key\":\"{StatsConverter.AdsBlockedToday}\",\"value\":5650}},{{\"key\":\"{StatsConverter.AdsPercentageToday}\",\"value\":18.731558}},{{\"key\":\"{StatsConverter.UniqueDomains}\",\"value\":1056}},{{\"key\":\"{StatsConverter.QueriesForwarded}\",\"value\":4275}},{{\"key\":\"{StatsConverter.QueriesCached}\",\"value\":20238}},{{\"key\":\"{StatsConverter.ClientsEverSeen}\",\"value\":11}},{{\"key\":\"{StatsConverter.UniqueClients}\",\"value\":9}},{{\"key\":\"{StatsConverter.Status}\",\"value\":\"enabled\"}}]");
            //
            // Assert.AreEqual(Option.Some(expectedResult),_telnetResultConverter.GetJsonFromObject().Map(TestUtils.OrderJsonStringFromConvert));
        }
        
        
    }
}