using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using dck_pihole2influx.StatObjects;
using dck_pihole2influx.Transport.Telnet;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class TopAdsConverterTest : TestHelperUtils
    {
        private readonly TelnetResultConverter _telnetResultConverter;

        public TopAdsConverterTest()
        {
            _telnetResultConverter = new TopAdsConverter();
        }

        [TestMethod]
        public void CheckValidTelnetStringAndReturnSomeResults()
        {
            var testee = "0 8 googleads.g.doubleclick.net\n1 6 www.googleadservices.com\n2 1 cdn.mxpnl.com\n3 1 collector.githubapp.com\n4 1 www.googletagmanager.com\n5 1 s.zkcdn.net\n\n---EOM---\n";
            _telnetResultConverter.Convert(testee).Wait();

            var resultList =
                from entry in _telnetResultConverter.DictionaryOpt.ValueOr(
                    new ConcurrentDictionary<string, IBaseResult>())
                orderby entry.Key
                select entry;

            var expectedList = new Dictionary<string, IBaseResult>
            {
                {"0", new IntOutputNumberedElement(8, 0, "googleads.g.doubleclick.net")},
                {"1", new IntOutputNumberedElement(6, 1, "www.googleadservices.com")},
                {"3", new IntOutputNumberedElement(1, 3, "cdn.mxpnl.com")},
                {"2", new IntOutputNumberedElement(1, 2, "collector.githubapp.com")},
                {"4", new IntOutputNumberedElement(1, 4, "www.googletagmanager.com")},
                {"5", new IntOutputNumberedElement(1, 5, "s.zkcdn.net")}
            };

            resultList.Should().BeEquivalentTo(expectedList);

            _telnetResultConverter.GetPiholeCommand().ToString().Should().Be(PiholeCommands.Topads.ToString());

            var expectedJson = "[{\"Count\":8,\"Position\":0,\"IpOrHost\":\"googleads.g.doubleclick.net\"},{\"Count\":6,\"Position\":1,\"IpOrHost\":\"www.googleadservices.com\"},{\"Count\":1,\"Position\":2,\"IpOrHost\":\"cdn.mxpnl.com\"},{\"Count\":1,\"Position\":3,\"IpOrHost\":\"collector.githubapp.com\"},{\"Count\":1,\"Position\":4,\"IpOrHost\":\"www.googletagmanager.com\"},{\"Count\":1,\"Position\":5,\"IpOrHost\":\"s.zkcdn.net\"}]";
            var orderedExpectedJson = OrderJsonArrayString(expectedJson, "Position").ValueOr("");
            var orderedCurrentJson =
                OrderJsonArrayString(_telnetResultConverter.GetJsonObjectFromDictionaryAsync(false).Result, "Position")
                    .ValueOr("");

            orderedExpectedJson.Should().NotBeEmpty();
            orderedCurrentJson.Should().NotBeEmpty();

            orderedCurrentJson.Should().Be(orderedExpectedJson);
        }


    }
}