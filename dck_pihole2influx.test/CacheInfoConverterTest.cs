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
            var expectedDictionary = new Dictionary<string, IBaseResult>
            {
                {CacheInfoConverter.CacheSize, new PrimitiveResultInt(10000)},
                {CacheInfoConverter.CacheLiveFreed, new PrimitiveResultInt(0)},
                {CacheInfoConverter.CacheInserted, new PrimitiveResultInt(98590)}
            };

            var resultDic = _telnetResultConverter
                .DictionaryOpt
                .ValueOr(new ConcurrentDictionary<string, IBaseResult>());

            resultDic.Should().BeEquivalentTo(expectedDictionary);

            var resultTokens = JToken.Parse(_telnetResultConverter.GetJsonObjectFromDictionaryAsync( false).Result);

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
            var expectedDictionary = new Dictionary<string, IBaseResult>
            {
                {CacheInfoConverter.CacheSize, new PrimitiveResultInt(10000)},
                {CacheInfoConverter.CacheInserted, new PrimitiveResultInt(98590)}
            };

            _telnetResultConverter.Convert(testee).Wait();

            var resultDic = _telnetResultConverter.DictionaryOpt.ValueOr(new ConcurrentDictionary<string, IBaseResult>());
            
            resultDic.Should().BeEquivalentTo(expectedDictionary);
            
            var jsonExpected = $"{{\"{CacheInfoConverter.CacheSize}\":10000,\"{CacheInfoConverter.CacheInserted}\":98590}}";

            var expectedToken = JToken.Parse(jsonExpected);

            var resultToken = JToken.Parse(_telnetResultConverter.GetJsonObjectFromDictionaryAsync( false).Result);

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

            var expectedDictionary = new Dictionary<string, IBaseResult>
            {
                {CacheInfoConverter.CacheSize, new PrimitiveResultInt(10000)},
                {CacheInfoConverter.CacheLiveFreed, new PrimitiveResultInt(0)},
                {CacheInfoConverter.CacheInserted, new PrimitiveResultInt(0)}
            };
            
            var resultDictionary = _telnetResultConverter.DictionaryOpt
                .ValueOr(new ConcurrentDictionary<string, IBaseResult>());

            resultDictionary.Should().BeEquivalentTo(expectedDictionary);

            var expectedJson =
                $"{{\"{CacheInfoConverter.CacheSize}\":10000,\"{CacheInfoConverter.CacheLiveFreed}\":0,\"{CacheInfoConverter.CacheInserted}\":0}}";
            var expectedToken = JToken.Parse(expectedJson);

            var resultToken = JToken.Parse(_telnetResultConverter.GetJsonObjectFromDictionaryAsync( false).Result);

            resultToken.Should().BeEquivalentTo(expectedToken);
        }

        [TestMethod]
        public void CheckInvalidTelnetStringAndReturnNone()
        {
            var testee = @"Some text string";

            _telnetResultConverter.Convert(testee);
            "{}".Should().Be(_telnetResultConverter.GetJsonObjectFromDictionaryAsync(false).Result);
        }

        [TestMethod]
        public void CheckEmptyTelnetStringAndReturnNone()
        {
            var testee = "";
            _telnetResultConverter.Convert(testee);
            "{}".Should().Be(_telnetResultConverter.GetJsonObjectFromDictionaryAsync( false).Result);
        }
    }
}