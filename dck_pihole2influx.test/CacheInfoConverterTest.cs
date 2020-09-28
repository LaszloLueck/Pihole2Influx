using System.Collections.Generic;
using dck_pihole2influx.StatObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Optional;

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
            var jsonExpected =
                $"[{{\"key\":\"{CacheInfoConverter.CacheSize}\",\"value\":10000}},{{\"key\":\"{CacheInfoConverter.CacheLiveFreed}\",\"value\":0}},{{\"key\":\"{CacheInfoConverter.CacheInserted}\",\"value\":98590}}]";

            //order the json and make it testable
            var jsArrayExpected = TestUtils.OrderJsonStringFromConvert(jsonExpected);
            var currentJson = _telnetResultConverter
                .GetJsonFromObject()
                .Map(TestUtils.OrderJsonStringFromConvert)
                .ValueOr("");


            Assert.AreEqual(jsArrayExpected, currentJson);


            var dictionaryExpected = TestUtils.OrderDictionaryFromResult(new Dictionary<string, dynamic>
            {
                {CacheInfoConverter.CacheSize, 10000},
                {CacheInfoConverter.CacheLiveFreed, 0},
                {CacheInfoConverter.CacheInserted, 98590}
            });


            var resultDic = TestUtils.OrderDictionaryFromResult(_telnetResultConverter
                .DictionaryOpt
                .ValueOr(new Dictionary<string, dynamic>()));

            CollectionAssert.AreEqual(dictionaryExpected, resultDic);
        }

        [TestMethod]
        public void CheckValidTelnetButMissingKeyValueAndReturnNone()
        {
            var testee = @"cache-size: 10000
cache-inserted: 98590
---EOM---


";
            _telnetResultConverter.Convert(testee);
            var jsonExpected = Option.None<string>();
            Assert.AreEqual(jsonExpected, _telnetResultConverter.GetJsonFromObject());
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

            var expectedDictionary = TestUtils.OrderDictionaryFromResult(new Dictionary<string, dynamic>
            {
                {CacheInfoConverter.CacheSize, 10000},
                {CacheInfoConverter.CacheLiveFreed, 0},
                {CacheInfoConverter.CacheInserted, 0}
            });

            var resultDictionary = _telnetResultConverter.DictionaryOpt.Map(TestUtils.OrderDictionaryFromResult)
                .ValueOr(new Dictionary<string, dynamic>());


            CollectionAssert.AreEqual(expectedDictionary, resultDictionary);

            var jsonExpected = TestUtils.OrderJsonStringFromConvert(
                $"[{{\"key\":\"{CacheInfoConverter.CacheSize}\",\"value\":10000}},{{\"key\":\"{CacheInfoConverter.CacheLiveFreed}\",\"value\":0}},{{\"key\":\"{CacheInfoConverter.CacheInserted}\",\"value\":0}}]");


            Assert.AreEqual(Option.Some(jsonExpected),
                _telnetResultConverter.GetJsonFromObject().Map(TestUtils.OrderJsonStringFromConvert));
        }

        [TestMethod]
        public void CheckInvalidTelnetStringAndReturnNone()
        {
            var testee = @"Some text string";

            _telnetResultConverter.Convert(testee);
            var jsonExpected = Option.None<string>();
            Assert.AreEqual(jsonExpected, _telnetResultConverter.GetJsonFromObject());
        }

        [TestMethod]
        public void CheckEmptyTelnetStringAndReturnNone()
        {
            var testee = "";
            _telnetResultConverter.Convert(testee);
            var jsonExpected = Option.None<string>();
            Assert.AreEqual(jsonExpected, _telnetResultConverter.GetJsonFromObject());
        }
    }
}