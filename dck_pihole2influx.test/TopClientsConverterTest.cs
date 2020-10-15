using System.Collections.Concurrent;
using System.Collections.Generic;
using dck_pihole2influx.StatObjects;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Optional;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class TopClientsConverterTest
    {
        private readonly TelnetResultConverter _telnetResultConverter;

        public TopClientsConverterTest()
        {
            _telnetResultConverter = new TopClientsConverter();
        }

        [TestMethod, Description("fill in with a valid return from the telnet method and convert the result")]
        public void CheckValidTelnetStringAndReturnSomeResults()
        {
            var testee = @"0 24593 192.168.1.1 aaa.localdomain
1 8136 192.168.1.227 bbb.localdomain
2 4704 192.168.1.225 ccc.localdomain
3 3741 192.168.1.174 ddd.localdomain
4 2836 192.168.1.231 eee.localdomain
5 2587 192.168.1.120 
6 2035 192.168.1.196 fff.localdomain
7 2009 192.168.1.226 ggg.localdomain
8 1952 192.168.1.167 hhh.localdomain
9 1807 192.168.1.137 


---EOM---

";
            _telnetResultConverter.Convert(testee).Wait();

            var resultDictionary = new Dictionary<string, IBaseResult>
            {
                {"0", new DoubleStringOutputElement(0,24593, "192.168.1.1", Option.Some("aaa.localdomain"))},
                {"1", new DoubleStringOutputElement(1,8136, "192.168.1.227", Option.Some("bbb.localdomain"))},
                {"2", new DoubleStringOutputElement(2,4704, "192.168.1.225", Option.Some("ccc.localdomain"))},
                {"3", new DoubleStringOutputElement(3,3741, "192.168.1.174", Option.Some("ddd.localdomain"))},
                {"4", new DoubleStringOutputElement(4,2836, "192.168.1.231", Option.Some("eee.localdomain"))},
                {"5", new DoubleStringOutputElement(5,2587, "192.168.1.120", Option.None<string>())},
                {"6", new DoubleStringOutputElement(6, 2035, "192.168.1.196", Option.Some("fff.localdomain"))},
                {"7", new DoubleStringOutputElement(7, 2009, "192.168.1.226", Option.Some("ggg.localdomain"))},
                {"8", new DoubleStringOutputElement(8, 1952, "192.168.1.167", Option.Some("hhh.localdomain"))},
                {"9", new DoubleStringOutputElement(9, 1807, "192.168.1.137", Option.None<string>())}
            };

            var expectedDictionary =
                _telnetResultConverter.DictionaryOpt.ValueOr(new ConcurrentDictionary<string, IBaseResult>());
            
            resultDictionary.Should().BeEquivalentTo(expectedDictionary);


            var expectedJson = "[{\"Count\":0,\"Position\":24593,\"IpAddress\":\"192.168.1.1\",\"HostName\":\"aaa.localdomain\"},{\"Count\":1,\"Position\":8136,\"IpAddress\":\"192.168.1.227\",\"HostName\":\"bbb.localdomain\"},{\"Count\":2,\"Position\":4704,\"IpAddress\":\"192.168.1.225\",\"HostName\":\"ccc.localdomain\"},{\"Count\":3,\"Position\":3741,\"IpAddress\":\"192.168.1.174\",\"HostName\":\"ddd.localdomain\"},{\"Count\":4,\"Position\":2836,\"IpAddress\":\"192.168.1.231\",\"HostName\":\"eee.localdomain\"},{\"Count\":5,\"Position\":2587,\"IpAddress\":\"192.168.1.120\"},{\"Count\":6,\"Position\":2035,\"IpAddress\":\"192.168.1.196\",\"HostName\":\"fff.localdomain\"},{\"Count\":7,\"Position\":2009,\"IpAddress\":\"192.168.1.226\",\"HostName\":\"ggg.localdomain\"},{\"Count\":8,\"Position\":1952,\"IpAddress\":\"192.168.1.167\",\"HostName\":\"hhh.localdomain\"},{\"Count\":9,\"Position\":1807,\"IpAddress\":\"192.168.1.137\"}]";

            var expectedToken = JToken.Parse(expectedJson);

            var currentToken = JToken.Parse(_telnetResultConverter.GetJsonObjectFromDictionaryAsync(false).Result);

            currentToken.Should().BeEquivalentTo(expectedToken);
            
        }
        
        
    }
}