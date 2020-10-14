using System.Collections.Concurrent;
using System.Collections.Generic;
using dck_pihole2influx.StatObjects;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class QueryTypesConverterTest
    {
        private readonly TelnetResultConverter _telnetResultConverter;

        public QueryTypesConverterTest()
        {
            _telnetResultConverter = new QueryTypesConverter();
        }

        [TestMethod, Description("fill in with a valid return from the telnet method and convert the result")]
        public void CheckValidTelnetStringAndReturnSomeResult()
        {
            var testee = @"A (IPv4): 67.73
AAAA (IPv6): 22.01
ANY: 0.00
SRV: 1.72
SOA: 0.04
PTR: 1.75
TXT: 0.55
NAPTR: 0.04
MX: 0.00
DS: 0.80
RRSIG: 0.00
DNSKEY: 0.17
OTHER: 5.19
---EOM---";

            _telnetResultConverter.Convert(testee).Wait();

            var dictionaryExpected = new Dictionary<string, IBaseResult>
            {
                {"A (IPv4)", new StringDoubleOutput("A (IPv4)", 67.73d)},
                {"AAAA (IPv6)", new StringDoubleOutput("AAAA IPv6", 22.01d)},
                {"ANY", new StringDoubleOutput("ANY", 0.00d)},
                {"SRV", new StringDoubleOutput("SRV", 1.72d)},
                {"SOA", new StringDoubleOutput("SOA", 0.04d)},
                {"PTR", new StringDoubleOutput("PTR", 1.75d)},
                {"TXT", new StringDoubleOutput("TXT", 0.55d)},
                {"NAPTR", new StringDoubleOutput("NAPTR", 0.04d)},
                {"MX", new StringDoubleOutput("MX", 0.00d)},
                {"DS", new StringDoubleOutput("DS", 0.80d)},
                {"RRSIG", new StringDoubleOutput("RRSIG", 0.00d)},
                {"DNSKEY", new StringDoubleOutput("DNSKEY", 0.17d)},
                {"OTHER", new StringDoubleOutput("OTHER", 5.19)}
            };

            var resultDic = _telnetResultConverter
                .DictionaryOpt
                .ValueOr(new ConcurrentDictionary<string, IBaseResult>());
            
            resultDic.Should().BeEquivalentTo(dictionaryExpected);


        }
    }
}