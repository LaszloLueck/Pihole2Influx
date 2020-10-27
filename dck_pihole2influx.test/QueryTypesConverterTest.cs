using System.Collections.Concurrent;
using System.Collections.Generic;
using dck_pihole2influx.StatObjects;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class QueryTypesConverterTest : TestHelperUtils
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
                {"A (IPv4)", new StringDecimalOutput("A (IPv4)", 67.73m)},
                {"AAAA (IPv6)", new StringDecimalOutput("AAAA IPv6", 22.01m)},
                {"ANY", new StringDecimalOutput("ANY", 0.00m)},
                {"SRV", new StringDecimalOutput("SRV", 1.72m)},
                {"SOA", new StringDecimalOutput("SOA", 0.04m)},
                {"PTR", new StringDecimalOutput("PTR", 1.75m)},
                {"TXT", new StringDecimalOutput("TXT", 0.55m)},
                {"NAPTR", new StringDecimalOutput("NAPTR", 0.04m)},
                {"MX", new StringDecimalOutput("MX", 0.00m)},
                {"DS", new StringDecimalOutput("DS", 0.80m)},
                {"RRSIG", new StringDecimalOutput("RRSIG", 0.00m)},
                {"DNSKEY", new StringDecimalOutput("DNSKEY", 0.17m)},
                {"OTHER", new StringDecimalOutput("OTHER", 5.19m)}
            };

            var resultDic = _telnetResultConverter
                .DictionaryOpt
                .ValueOr(new ConcurrentDictionary<string, IBaseResult>());
            
            resultDic.Should().BeEquivalentTo(dictionaryExpected);

            var expectedJson = "[{\"Key\":\"ANY\",\"Value\":0.00},{\"Key\":\"MX\",\"Value\":0.00},{\"Key\":\"RRSIG\",\"Value\":0.00},{\"Key\":\"SOA\",\"Value\":0.04},{\"Key\":\"NAPTR\",\"Value\":0.04},{\"Key\":\"DNSKEY\",\"Value\":0.17},{\"Key\":\"TXT\",\"Value\":0.55},{\"Key\":\"DS\",\"Value\":0.80},{\"Key\":\"SRV\",\"Value\":1.72},{\"Key\":\"PTR\",\"Value\":1.75},{\"Key\":\"OTHER\",\"Value\":5.19},{\"Key\":\"AAAA (IPv6)\",\"Value\":22.01},{\"Key\":\"A (IPv4)\",\"Value\":67.73}]";

            var orderedExpectedJson = OrderJsonArrayString(expectedJson, "Key").ValueOr("");

            var orderedCurrentJson =
                OrderJsonArrayString(_telnetResultConverter.GetJsonObjectFromDictionaryAsync(false).Result, "Key")
                    .ValueOr("");

            orderedExpectedJson.Should().NotBeEmpty();
            orderedCurrentJson.Should().NotBeEmpty();

            orderedCurrentJson.Should().Be(orderedExpectedJson);
        }
    }
}