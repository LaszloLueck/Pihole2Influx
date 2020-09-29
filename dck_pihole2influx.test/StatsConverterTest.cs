using System.Collections.Concurrent;
using System.Collections.Generic;
using dck_pihole2influx.StatObjects;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

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
            _telnetResultConverter.Convert(testee).Wait();
            var dictionaryExpected = new Dictionary<string, dynamic>
            {
                {StatsConverter.DomainsBeingBlocked, 116007},
                {StatsConverter.DnsQueriesToday, 30163},
                {StatsConverter.AdsBlockedToday, 5650},
                {StatsConverter.AdsPercentageToday, 18.731558F},
                {StatsConverter.UniqueDomains, 1056},
                {StatsConverter.QueriesForwarded, 4275},
                {StatsConverter.QueriesCached, 20238},
                {StatsConverter.ClientsEverSeen, 11},
                {StatsConverter.UniqueClients, 9},
                {StatsConverter.Status, "enabled"}
            };

            var resultDic = _telnetResultConverter
                .DictionaryOpt
                .ValueOr(new ConcurrentDictionary<string, dynamic>());
            
            resultDic.Should().BeEquivalentTo(dictionaryExpected);


            var expectedJson =
                "{\"QueriesCached\":20238,\"DnsQueriesToday\":30163,\"AdsPercentageToday\":18.731558,\"Status\":\"enabled\",\"AdsBlockedToday\":5650,\"UniqueClients\":9,\"DomainsBeingBlocked\":116007,\"QueriesForwarded\":4275,\"ClientsEverSeen\":11,\"UniqueDomains\":1056}";

            var expectedToken = JToken.Parse(expectedJson);

            var resultToken = JToken.Parse(_telnetResultConverter.GetJsonFromObjectAsync().Result);

            resultToken.Should().BeEquivalentTo(expectedToken);
        }
        
        //All other possible checks (missing values, wrong types) are tested in CacheInfoConverterTest.cs
        
    }
}