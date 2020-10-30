using System.Collections.Concurrent;
using System.Collections.Generic;
using dck_pihole2influx.StatObjects;
using dck_pihole2influx.Transport.Telnet;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class StatsConverterTest : TestHelperUtils
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
            var dictionaryExpected = new Dictionary<string, IBaseResult>
            {
                {StatsConverter.DomainsBeingBlocked, new PrimitiveResultInt(116007)},
                {StatsConverter.DnsQueriesToday, new PrimitiveResultInt(30163)},
                {StatsConverter.AdsBlockedToday, new PrimitiveResultInt(5650)},
                {StatsConverter.AdsPercentageToday, new PrimitiveResultFloat(18.731558F)},
                {StatsConverter.UniqueDomains, new PrimitiveResultInt(1056)},
                {StatsConverter.QueriesForwarded, new PrimitiveResultInt(4275)},
                {StatsConverter.QueriesCached, new PrimitiveResultInt(20238)},
                {StatsConverter.ClientsEverSeen, new PrimitiveResultInt(11)},
                {StatsConverter.UniqueClients, new PrimitiveResultInt(9)},
                {StatsConverter.Status, new PrimitiveResultString("enabled")}
            };

            var resultDic = _telnetResultConverter
                .DictionaryOpt
                .ValueOr(new ConcurrentDictionary<string, IBaseResult>());
            
            resultDic.Should().BeEquivalentTo(dictionaryExpected);

            _telnetResultConverter.GetPiholeCommand().ToString().Should().Be(PiholeCommands.Stats.ToString());

            var expectedJson =
                "{\"QueriesCached\":20238,\"DnsQueriesToday\":30163,\"AdsPercentageToday\":18.731558,\"Status\":\"enabled\",\"AdsBlockedToday\":5650,\"UniqueClients\":9,\"DomainsBeingBlocked\":116007,\"QueriesForwarded\":4275,\"ClientsEverSeen\":11,\"UniqueDomains\":1056}";

            var orderedExpectedJson = OrderJsonObjectString(expectedJson).ValueOr("");

            var orderedCurrentJson =
                OrderJsonObjectString(_telnetResultConverter.GetJsonObjectFromDictionaryAsync(false).Result)
                    .ValueOr("");

            orderedExpectedJson.Should().NotBeEmpty();
            orderedCurrentJson.Should().NotBeEmpty();

            orderedCurrentJson.Should().Be(orderedExpectedJson);
        }
        
        //All other possible checks (missing values, wrong types) are tested in CacheInfoConverterTest.cs
        
    }
}