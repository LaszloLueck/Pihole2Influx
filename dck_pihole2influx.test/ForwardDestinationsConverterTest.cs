using System.Collections.Concurrent;
using System.Collections.Generic;
using dck_pihole2influx.StatObjects;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class ForwardDestinationsConverterTest
    {
        private readonly TelnetResultConverter _telnetResultConverter;

        public ForwardDestinationsConverterTest()
        {
            _telnetResultConverter = new ForwardDestinationsConverter();
        }

        [TestMethod, Description("fill in with a valid return from the telnet method and convert the result")]
        public void CheckValidTelnetStringAndReturnSomeResults()
        {
            var testee = @"-2 22.31 blocklist blocklist
    -1 8.24 cache cache
    0 35.31 192.168.1.1 opnsense.localdomain
    1 19.39 1.0.0.1 one.one.one.one
    2 15.60 1.1.1.1 one.one.one.one
    ---EOM---";

            _telnetResultConverter.Convert(testee).Wait();
           
            var dictionaryExpected = new Dictionary<string, IBaseResult>
            {
                {"-2", new DoubleOutputNumberedElement(22.31d, "-2", "blocklist blocklist")},
                {"-1", new DoubleOutputNumberedElement(8.24d, "-1", "cache cache")},
                {"0", new DoubleOutputNumberedElement(35.31d, "0", "192.168.1.1 opnsense.localdomain")},
                {"1", new DoubleOutputNumberedElement(19.39d, "1", "1.0.0.1 one.one.one.one")},
                {"2", new DoubleOutputNumberedElement(15.6d, "2", "1.1.1.1 one.one.one.one")}
            };

            var dictionaryResult =
                _telnetResultConverter.DictionaryOpt.ValueOr(new ConcurrentDictionary<string, IBaseResult>());
            dictionaryResult.Should().BeEquivalentTo(dictionaryExpected);

            var tokenResult = JToken.Parse(_telnetResultConverter.GetJsonObjectFromDictionaryAsync(false).Result);

            var jsonExpected = "[{\"position\":-2,\"percentage\":22.31,\"entry\":\"blocklist blocklist\"},{\"position\":-1,\"percentage\":8.24,\"entry\":\"cache cache\"},{\"position\":0,\"percentage\":35.31,\"entry\":\"192.168.1.1 opnsense.localdomain\"},{\"position\":1,\"percentage\":19.39,\"entry\":\"1.0.0.1 one.one.one.one\"},{\"position\":2,\"percentage\":15.6,\"entry\":\"1.1.1.1 one.one.one.one\"}]";
            var tokenExpected = JToken.Parse(jsonExpected);

            tokenResult.Should().BeEquivalentTo(tokenExpected);

        }

    }
}