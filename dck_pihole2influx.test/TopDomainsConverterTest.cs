using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using dck_pihole2influx.StatObjects;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        }
    }
}