using System;
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
            _telnetResultConverter.Convert(testee);
            var jsonExpected = "[{\"key\":\"CacheSize\",\"value\":10000},{\"key\":\"CacheLiveFreed\",\"value\":0},{\"key\":\"CacheInserted\",\"value\":98590}]";
 
            Assert.AreEqual(Option.Some<string>(jsonExpected),_telnetResultConverter.GetJsonFromObject(false));
        }
        
    }
}