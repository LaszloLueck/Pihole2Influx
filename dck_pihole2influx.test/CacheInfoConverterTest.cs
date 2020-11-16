using System.Collections.Concurrent;
using System.Collections.Generic;
using dck_pihole2influx.StatObjects;
using dck_pihole2influx.Transport.Telnet;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class CacheInfoConverterTest : TestHelperUtils
    {
        private readonly TelnetResultConverter _telnetResultConverter;

        public CacheInfoConverterTest()
        {
            _telnetResultConverter = new CacheInfoConverter();
        }

        [TestMethod, Description("fill in with a valid return from the telnet method and convert the result.")]
        public void CheckValidTelnetStringAndReturnSomeResults()
        {
            var testee = "cache-size: 10000\ncache-live-freed: 0\ncache-inserted: 98590\n---EOM---";
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

            _telnetResultConverter.GetPiholeCommand().ToString().Should().Be(PiholeCommands.Cacheinfo.ToString());
            

            var jsonExpected =
                $"{{\"{CacheInfoConverter.CacheSize}\":10000,\"{CacheInfoConverter.CacheLiveFreed}\":0,\"{CacheInfoConverter.CacheInserted}\":98590}}";

            var orderedExpectedJson = OrderJsonObjectString(jsonExpected).ValueOr("");

            var orderedCurrentJson =
                OrderJsonObjectString(_telnetResultConverter.GetJsonObjectFromDictionaryAsync(false).Result)
                    .ValueOr("");

            orderedCurrentJson.Should().NotBeEmpty();
            orderedExpectedJson.Should().NotBeEmpty();

            orderedCurrentJson.Should().Be(orderedExpectedJson);
        }

        [TestMethod, Description("Return None because one or more parameter are missing in result")]
        public void CheckValidTelnetButMissingKeyValueAndReturnNone()
        {
            var testee = "cache-size: 10000\ncache-inserted: 98590\n---EOM---";
            var expectedDictionary = new Dictionary<string, IBaseResult>
            {
                {CacheInfoConverter.CacheSize, new PrimitiveResultInt(10000)},
                {CacheInfoConverter.CacheInserted, new PrimitiveResultInt(98590)}
            };

            _telnetResultConverter.Convert(testee).Wait();

            var resultDic = _telnetResultConverter.DictionaryOpt.ValueOr(new ConcurrentDictionary<string, IBaseResult>());
            
            resultDic.Should().BeEquivalentTo(expectedDictionary);
            
            var jsonExpected = $"{{\"{CacheInfoConverter.CacheSize}\":10000,\"{CacheInfoConverter.CacheInserted}\":98590}}";

            var orderedExpectedJson = OrderJsonObjectString(jsonExpected).ValueOr("");
            var orderedCurrentJson =
                OrderJsonObjectString(_telnetResultConverter.GetJsonObjectFromDictionaryAsync(false).Result)
                    .ValueOr("");

            orderedExpectedJson.Should().NotBeEmpty();
            orderedCurrentJson.Should().NotBeEmpty();

            orderedCurrentJson.Should().Be(orderedExpectedJson);

        }

        [TestMethod]
        public void CheckValidStructureTelnetStringInvalidValueAndReturnResultWithAlternate()
        {
            var testee = "cache-size: 10000\ncache-live-freed: 0\ncache-inserted: abcde\n---EOM---\n\n\n";
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

            var orderedExpectedJson = OrderJsonObjectString(expectedJson).ValueOr("");
            var orderedCurrentJson =
                OrderJsonObjectString(_telnetResultConverter.GetJsonObjectFromDictionaryAsync(false).Result)
                    .ValueOr("");

            orderedExpectedJson.Should().NotBeEmpty();
            orderedCurrentJson.Should().NotBeEmpty();

            orderedCurrentJson.Should().Be(orderedExpectedJson);
        }

        [TestMethod]
        public void CheckInvalidTelnetStringAndReturnNone()
        {
            var testee = @"Some text string";

            _telnetResultConverter.Convert(testee).Wait();
            "{}".Should().Be(_telnetResultConverter.GetJsonObjectFromDictionaryAsync(false).Result);
        }

        [TestMethod]
        public void CheckEmptyTelnetStringAndReturnNone()
        {
            var testee = "";
            _telnetResultConverter.Convert(testee).Wait();
            "{}".Should().Be(_telnetResultConverter.GetJsonObjectFromDictionaryAsync( false).Result);
        }
    }
}