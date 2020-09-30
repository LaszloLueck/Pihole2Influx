using System.Collections.Concurrent;
using System.Collections.Generic;
using dck_pihole2influx.StatObjects;
using FluentAssertions;
using FluentAssertions.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class CacheInfoConverterTest
    {
        private readonly TelnetResultConverter _telnetResultConverter;

        public CacheInfoConverterTest()
        {
            _telnetResultConverter = new CacheInfoConverter();
        }

        [TestMethod, Description("fill in with a valid return from the telnet method and convert the result.")]
        public void CheckValidTelnetStringAndReturnSomeResults()
        {
            var testee = @"cache-size: 10000
cache-live-freed: 0
cache-inserted: 98590
---EOM---


";
            _telnetResultConverter.Convert(testee).Wait();
            var dictionaryExpected = new Dictionary<string, dynamic>
            {
                {CacheInfoConverter.CacheSize, 10000},
                {CacheInfoConverter.CacheLiveFreed, 0},
                {CacheInfoConverter.CacheInserted, 98590}
            };

            var resultDic = _telnetResultConverter
                .DictionaryOpt
                .ValueOr(new ConcurrentDictionary<string, dynamic>());

            resultDic.Should().BeEquivalentTo(dictionaryExpected);

            var resultTokens = JToken.Parse(_telnetResultConverter.GetJsonFromObjectAsync().Result);

            var jsonExpected =
                $"{{\"{CacheInfoConverter.CacheSize}\":10000,\"{CacheInfoConverter.CacheLiveFreed}\":0,\"{CacheInfoConverter.CacheInserted}\":98590}}";

            var expectedToken = JToken.Parse(jsonExpected);


            resultTokens.Should().BeEquivalentTo(expectedToken);
        }

        [TestMethod, Description("Return None because one or more parameter are missing in result")]
        public void CheckValidTelnetButMissingKeyValueAndReturnNone()
        {
            var testee = @"cache-size: 10000
cache-inserted: 98590
---EOM---


";
            var dictionaryExpected = new Dictionary<string, dynamic>
            {
                {CacheInfoConverter.CacheSize, 10000},
                {CacheInfoConverter.CacheInserted, 98590}
            };

            _telnetResultConverter.Convert(testee).Wait();

            var resultDic = _telnetResultConverter.DictionaryOpt.ValueOr(new ConcurrentDictionary<string, dynamic>());
            
            resultDic.Should().BeEquivalentTo(dictionaryExpected);
            
            var jsonExpected = $"{{\"{CacheInfoConverter.CacheSize}\":10000,\"{CacheInfoConverter.CacheInserted}\":98590}}";

            var expectedToken = JToken.Parse(jsonExpected);

            var resultToken = JToken.Parse(_telnetResultConverter.GetJsonFromObjectAsync().Result);

            resultToken.Should().BeEquivalentTo(expectedToken);
            
        }

        [TestMethod]
        public void CheckValidStructureTelnetStringInvalidValueAndReturnResultWithAlternate()
        {
            var testee = @"cache-size: 10000
cache-live-freed: 0
cache-inserted: abcde
---EOM---



";
            _telnetResultConverter.Convert(testee).Wait();

            var expectedDictionary = new Dictionary<string, dynamic>
            {
                {CacheInfoConverter.CacheSize, 10000},
                {CacheInfoConverter.CacheLiveFreed, 0},
                {CacheInfoConverter.CacheInserted, 0}
            };

            var resultDictionary = _telnetResultConverter.DictionaryOpt
                .ValueOr(new ConcurrentDictionary<string, dynamic>());

            resultDictionary.Should().BeEquivalentTo(expectedDictionary);

            var expectedJson =
                $"{{\"{CacheInfoConverter.CacheSize}\":10000,\"{CacheInfoConverter.CacheLiveFreed}\":0,\"{CacheInfoConverter.CacheInserted}\":0}}";
            var expectedToken = JToken.Parse(expectedJson);

            var resultToken = JToken.Parse(_telnetResultConverter.GetJsonFromObjectAsync().Result);

            resultToken.Should().BeEquivalentTo(expectedToken);
        }

        [TestMethod]
        public void CheckInvalidTelnetStringAndReturnNone()
        {
            var testee = @"Some text string";

            _telnetResultConverter.Convert(testee);
            "{}".Should().Be(_telnetResultConverter.GetJsonFromObjectAsync().Result);
        }

        [TestMethod]
        public void CheckEmptyTelnetStringAndReturnNone()
        {
            var testee = "";
            _telnetResultConverter.Convert(testee);
            "{}".Should().Be(_telnetResultConverter.GetJsonFromObjectAsync().Result);
        }
    }
}