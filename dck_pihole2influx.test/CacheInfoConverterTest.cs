using System.Collections;
using System.Collections.Generic;
using System.Linq;
using dck_pihole2influx.StatObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                "[{\"key\":\"CacheSize\",\"value\":10000},{\"key\":\"CacheLiveFreed\",\"value\":0},{\"key\":\"CacheInserted\",\"value\":98590}]";

            //order the json and make it testable
            var jsArrayExpected =
                JsonConvert.SerializeObject(JArray.Parse(jsonExpected).OrderBy(a => (string) a["key"]));
            var currentJson = _telnetResultConverter.GetJsonFromObject(false).Map(json =>
            {
                return JsonConvert.SerializeObject(JArray.Parse(json).OrderBy(a => (string) a["key"]));
            }).ValueOr("");


            Assert.AreEqual(jsArrayExpected, currentJson);


            var dictionaryExpected = new Dictionary<string, dynamic>()
            {
                {"CacheSize", 10000},
                {"CacheLiveFreed", 0},
                {"CacheInserted", 98590}
            }
                .OrderBy(a => a.Key)
                .ToDictionary(a => a.Key);
            var resultDic = _telnetResultConverter
                .DictionaryOpt
                .ValueOr(new Dictionary<string, dynamic>())
                .OrderBy(a => a.Key)
                .ToDictionary(a => a.Key);
            
            CollectionAssert.AreEqual((ICollection) dictionaryExpected, (ICollection) resultDic);
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
            _telnetResultConverter.Convert(testee);
            var jsonExpected =
                "[{\"key\":\"CacheSize\",\"value\":10000},{\"key\":\"CacheLiveFreed\",\"value\":0},{\"key\":\"CacheInserted\",\"value\":0}]";
            Assert.AreEqual(Option.Some(jsonExpected), _telnetResultConverter.GetJsonFromObject());
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