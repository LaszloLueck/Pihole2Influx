using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using dck_pihole2influx.StatObjects;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Optional;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class TopDomainsConverterTest
    {
        private readonly TelnetResultConverter _telnetResultConverter;

        public TopDomainsConverterTest()
        {
            _telnetResultConverter = new TopDomainsConverter();
        }


        [TestMethod]
        public void CheckValidTelnetStringAndReturnSomeResults()
        {
            var testee = @"0 8462 x.y.z.de
1 236 safebrowsing-cache.google.com
2 116 pi.hole
3 109 z.y.x.de
4 93 safebrowsing.google.com
5 96 plus.google.com


---EOM---
";
            _telnetResultConverter.Convert(testee).Wait();

            var resultList =
                from entry in _telnetResultConverter.DictionaryOpt.ValueOr(new ConcurrentDictionary<string, dynamic>())
                orderby entry.Key
                select entry;

            var expectedList = new Dictionary<string, dynamic>
            {
                {"0", (8462, "x.y.z.de")},
                {"1", (236, "safebrowsing-cache.google.com")},
                {"2", (116, "pi.hole")},
                {"3", (109, "z.y.x.de")},
                {"4", (93, "safebrowsing.google.com")},
                {"5", (96, "plus.google.com")}
            };

            resultList.Should().BeEquivalentTo(expectedList);

            var expectedJson =
                "[{\"position\":0,\"count\":8462,\"url\":\"x.y.z.de\"},{\"position\":1,\"count\":236,\"url\":\"safebrowsing-cache.google.com\"},{\"position\":2,\"count\":116,\"url\":\"pi.hole\"},{\"position\":3,\"count\":109,\"url\":\"z.y.x.de\"},{\"position\":4,\"count\":93,\"url\":\"safebrowsing.google.com\"},{\"position\":5,\"count\":96,\"url\":\"plus.google.com\"}]";

            var expectedToken = JToken.Parse(expectedJson);

            var currentToken = JToken.Parse(_telnetResultConverter.GetJsonObjectFromDictionaryAsync(false).Result);

            currentToken.Should().BeEquivalentTo(expectedToken);
        }

        [TestMethod]
        public void CheckInvalidTelnetStringAndReturnNone()
        {
            var testee = "my name is methos";

            _telnetResultConverter.Convert(testee).Wait();

            _telnetResultConverter.DictionaryOpt.Should().Be(Option.None<ConcurrentDictionary<string, dynamic>>());
        }
    }
}