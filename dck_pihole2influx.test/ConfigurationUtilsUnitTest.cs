using System;
using dck_pihole2influx.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Optional;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class ConfigurationUtilsUnitTest
    {

        private readonly ConfigurationUtils _configurationUtils;

        public ConfigurationUtilsUnitTest()
        {
            _configurationUtils = new ConfigurationUtils();
        }
        
        
        [TestMethod]
        public void ReturnValidWhenSome_TryParseValueFromString()
        {
            var result = _configurationUtils.TryParseValueFromString(Option.Some<string>("1"), 100);
            Assert.AreEqual(1,result);
        }

        [TestMethod]
        public void ReturnDefaultWhenSome_TryParseValueFromString()
        {
            var result = _configurationUtils.TryParseValueFromString(Option.Some("a"), 100);
            Assert.AreEqual(100,100);
        }

        [TestMethod]
        public void ReturnDefaultWhenNone_TryParseValueFromString()
        {
            var result = _configurationUtils.TryParseValueFromString(Option.None<string>(), 100);
            Assert.AreEqual(100,100);
        }

        [TestMethod]
        public void ReturnSome_ReadEnvironmentVariable()
        {
            Environment.SetEnvironmentVariable("test_value1", "a");
            var result = _configurationUtils.ReadEnvironmentVariable("test_value1");
            Assert.AreEqual(Option.Some<string>("a"), result);
            Environment.SetEnvironmentVariable("test_value1",null);
        }

        [TestMethod]
        public void ReturnNone_ReadEnvironmentVariable()
        {
            var result = _configurationUtils.ReadEnvironmentVariable("test_value2");
            Assert.AreEqual(Option.None<string>(), result);
        }
    }
}