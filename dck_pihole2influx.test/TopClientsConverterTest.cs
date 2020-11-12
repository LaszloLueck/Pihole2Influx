using System.Collections.Concurrent;
using System.Collections.Generic;
using dck_pihole2influx.StatObjects;
using dck_pihole2influx.Transport.Telnet;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Optional;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class TopClientsConverterTest : TestHelperUtils

    {
        private readonly TelnetResultConverter _telnetResultConverter;

        public TopClientsConverterTest()
        {
            _telnetResultConverter = new TopClientsConverter();
        }

        [TestMethod, Description("fill in with a valid return from the telnet method and convert the result")]
        public void CheckValidTelnetStringAndReturnSomeResults()
        {
            var testee = "0 24593 192.168.1.1 aaa.localdomain\n1 8136 192.168.1.227 bbb.localdomain\n2 4704 192.168.1.225 ccc.localdomain\n3 3741 192.168.1.174 ddd.localdomain\n4 2836 192.168.1.231 eee.localdomain\n5 2587 192.168.1.120\n6 2035 192.168.1.196 fff.localdomain\n7 2009 192.168.1.226 ggg.localdomain\n8 1952 192.168.1.167 hhh.localdomain\n9 1807 192.168.1.137\n\n---EOM---\n\n";
            _telnetResultConverter.Convert(testee).Wait();

            var resultDictionary = new Dictionary<string, IBaseResult>
            {
                {"0", new DoubleStringOutputElement(0, 24593, "192.168.1.1", Option.Some("aaa.localdomain"))},
                {"1", new DoubleStringOutputElement(1, 8136, "192.168.1.227", Option.Some("bbb.localdomain"))},
                {"2", new DoubleStringOutputElement(2, 4704, "192.168.1.225", Option.Some("ccc.localdomain"))},
                {"3", new DoubleStringOutputElement(3, 3741, "192.168.1.174", Option.Some("ddd.localdomain"))},
                {"4", new DoubleStringOutputElement(4, 2836, "192.168.1.231", Option.Some("eee.localdomain"))},
                {"5", new DoubleStringOutputElement(5, 2587, "192.168.1.120", Option.None<string>())},
                {"6", new DoubleStringOutputElement(6, 2035, "192.168.1.196", Option.Some("fff.localdomain"))},
                {"7", new DoubleStringOutputElement(7, 2009, "192.168.1.226", Option.Some("ggg.localdomain"))},
                {"8", new DoubleStringOutputElement(8, 1952, "192.168.1.167", Option.Some("hhh.localdomain"))},
                {"9", new DoubleStringOutputElement(9, 1807, "192.168.1.137", Option.None<string>())}
            };

            var expectedDictionary =
                _telnetResultConverter.DictionaryOpt.ValueOr(new ConcurrentDictionary<string, IBaseResult>());

            resultDictionary.Should().BeEquivalentTo(expectedDictionary);
            
            _telnetResultConverter.GetPiholeCommand().ToString().Should().Be(PiholeCommands.Topclients.ToString());


            var expectedJson =
                "[{\"Count\":24593,\"Position\":0,\"IpAddress\":\"192.168.1.1\",\"HostName\":\"aaa.localdomain\"},{\"Count\":8136,\"Position\":1,\"IpAddress\":\"192.168.1.227\",\"HostName\":\"bbb.localdomain\"},{\"Count\":4704,\"Position\":2,\"IpAddress\":\"192.168.1.225\",\"HostName\":\"ccc.localdomain\"},{\"Count\":3741,\"Position\":3,\"IpAddress\":\"192.168.1.174\",\"HostName\":\"ddd.localdomain\"},{\"Count\":2836,\"Position\":4,\"IpAddress\":\"192.168.1.231\",\"HostName\":\"eee.localdomain\"},{\"Count\":2587,\"Position\":5,\"IpAddress\":\"192.168.1.120\"},{\"Count\":2035,\"Position\":6,\"IpAddress\":\"192.168.1.196\",\"HostName\":\"fff.localdomain\"},{\"Count\":2009,\"Position\":7,\"IpAddress\":\"192.168.1.226\",\"HostName\":\"ggg.localdomain\"},{\"Count\":1952,\"Position\":8,\"IpAddress\":\"192.168.1.167\",\"HostName\":\"hhh.localdomain\"},{\"Count\":1807,\"Position\":9,\"IpAddress\":\"192.168.1.137\"}]";

            var orderedExpectedJson = OrderJsonArrayString(expectedJson, "Count").ValueOr("");
            var orderedCurrentJson =
                OrderJsonArrayString(_telnetResultConverter.GetJsonObjectFromDictionaryAsync(false).Result, "Count")
                    .ValueOr("");

            orderedExpectedJson.Should().NotBeEmpty();
            orderedCurrentJson.Should().NotBeEmpty();

            orderedCurrentJson.Should().Be(orderedExpectedJson);
        }
    }
}