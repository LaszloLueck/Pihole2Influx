using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using dck_pihole2influx.StatObjects;
using dck_pihole2influx.Transport.Telnet;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Optional;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class TopDomainsConverterTest : TestHelperUtils
    {
        private readonly TelnetResultConverter _telnetResultConverter;

        public TopDomainsConverterTest()
        {
            _telnetResultConverter = new TopDomainsConverter();
        }


        [TestMethod]
        public void CheckValidTelnetStringAndReturnSomeResults()
        {
            var testee = "0 8462 x.y.z.de\n1 236 safebrowsing-cache.google.com\n2 116 pi.hole\n3 109 z.y.x.de\n4 93 safebrowsing.google.com\n5 96 plus.google.com\n\n---EOM---";
            _telnetResultConverter.Convert(testee).Wait();

            var resultList =
                from entry in _telnetResultConverter.DictionaryOpt.ValueOr(new ConcurrentDictionary<string, IBaseResult>())
                orderby entry.Key
                select entry;

            var expectedList = new Dictionary<string, IBaseResult>
            {
                {"0", new IntOutputNumberedElement(8462, "0", "x.y.z.de")},
                {"1", new IntOutputNumberedElement(236, "1", "safebrowsing-cache.google.com")},
                {"3", new IntOutputNumberedElement(109, "3", "z.y.x.de")},
                {"2", new IntOutputNumberedElement(116, "2", "pi.hole")},
                {"4", new IntOutputNumberedElement(93, "4", "safebrowsing.google.com")},
                {"5", new IntOutputNumberedElement(96, "5", "plus.google.com")}
            };

            resultList.Should().BeEquivalentTo(expectedList);
            
            _telnetResultConverter.GetPiholeCommand().ToString().Should().Be(PiholeCommands.Topdomains.ToString());

            var expectedJson =
                "[{\"Count\":8462,\"Position\":\"0\",\"IpOrHost\":\"x.y.z.de\"},{\"Count\":236,\"Position\":\"1\",\"IpOrHost\":\"safebrowsing-cache.google.com\"},{\"Count\":116,\"Position\":\"2\",\"IpOrHost\":\"pi.hole\"},{\"Count\":109,\"Position\":\"3\",\"IpOrHost\":\"z.y.x.de\"},{\"Count\":93,\"Position\":\"4\",\"IpOrHost\":\"safebrowsing.google.com\"},{\"Count\":96,\"Position\":\"5\",\"IpOrHost\":\"plus.google.com\"}]";

            var orderedJsonExpected = OrderJsonArrayString(expectedJson, "Position").ValueOr("");
            var orderedJsonCurrent = OrderJsonArrayString(_telnetResultConverter.GetJsonObjectFromDictionaryAsync(false).Result, "Position").ValueOr("");

            orderedJsonCurrent.Should().NotBeEmpty();
            orderedJsonExpected.Should().NotBeEmpty();
            orderedJsonCurrent.Should().Be(orderedJsonExpected);

        }

        [TestMethod]
        public void CheckInvalidTelnetStringAndReturnNone()
        {
            var testee = "my name is methos";

            _telnetResultConverter.Convert(testee).Wait();

            _telnetResultConverter.DictionaryOpt.Should().Be(Option.None<ConcurrentDictionary<string, IBaseResult>>());
        }
    }
}