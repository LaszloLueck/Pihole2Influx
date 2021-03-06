using System.Collections.Concurrent;
using System.Collections.Generic;
using dck_pihole2influx.StatObjects;
using dck_pihole2influx.Transport.Telnet;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class ForwardDestinationsConverterTest : TestHelperUtils
    {
        private readonly TelnetResultConverter _telnetResultConverter;

        public ForwardDestinationsConverterTest()
        {
            _telnetResultConverter = new ForwardDestinationsConverter();
        }

        [TestMethod, Description("fill in with a valid return from the telnet method and convert the result")]
        public void CheckValidTelnetStringAndReturnSomeResults()
        {
            var testee = "-2 22.31 blocklist blocklist\n-1 8.24 cache cache\n0 35.31 192.168.1.1 opnsense.localdomain\n1 19.39 1.0.0.1 one.one.one.one\n2 15.60 1.1.1.1 one.one.one.one\n---EOM---";

            _telnetResultConverter.Convert(testee).Wait();

            var dictionaryExpected = new Dictionary<string, IBaseResult>
            {
                {"-2", new DoubleOutputNumberedElement(22.31d, -2, "blocklist blocklist")},
                {"-1", new DoubleOutputNumberedElement(8.24d, -1, "cache cache")},
                {"0", new DoubleOutputNumberedElement(35.31d, 0, "192.168.1.1 opnsense.localdomain")},
                {"1", new DoubleOutputNumberedElement(19.39d, 1, "1.0.0.1 one.one.one.one")},
                {"2", new DoubleOutputNumberedElement(15.6d, 2, "1.1.1.1 one.one.one.one")}
            };

            var dictionaryResult =
                _telnetResultConverter.DictionaryOpt.ValueOr(new ConcurrentDictionary<string, IBaseResult>());
            dictionaryResult.Should().BeEquivalentTo(dictionaryExpected);

            _telnetResultConverter.GetPiholeCommand().ToString().Should()
                .Be(PiholeCommands.Forwarddestinations.ToString());
            

            var jsonExpected =
                "[{\"Count\":8.24,\"Position\":-1,\"IpOrHost\":\"cache cache\"},{\"Count\":22.31,\"Position\":-2,\"IpOrHost\":\"blocklist blocklist\"},{\"Count\":35.31,\"Position\":0,\"IpOrHost\":\"192.168.1.1 opnsense.localdomain\"},{\"Count\":19.39,\"Position\":1,\"IpOrHost\":\"1.0.0.1 one.one.one.one\"},{\"Count\":15.6,\"Position\":2,\"IpOrHost\":\"1.1.1.1 one.one.one.one\"}]";

            var orderedExpectedJson = OrderJsonArrayString(jsonExpected, "Position").ValueOr("");
            var orderedCurrentJson =
                OrderJsonArrayString(_telnetResultConverter.GetJsonObjectFromDictionaryAsync(false).Result, "Position")
                    .ValueOr("");

            orderedExpectedJson.Should().NotBeEmpty();
            orderedCurrentJson.Should().NotBeEmpty();

            orderedCurrentJson.Should().Be(orderedExpectedJson);
        }
    }
}