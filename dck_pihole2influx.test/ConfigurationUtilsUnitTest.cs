using System;
using dck_pihole2influx.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Optional;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class ConfigurationUtilsUnitTest
    {

        private readonly IConfigurationUtils _configurationUtils;

        public ConfigurationUtilsUnitTest()
        {
            _configurationUtils = new ConfigurationUtils();
        }
        
        
        [TestMethod, Description("Try to parse an Option<string> Some to a valid number and returns the real value")]
        public void ReturnValidWhenSome_TryParseValueFromString()
        {
            var result = _configurationUtils.TryParseValueFromString(Option.Some<string>("1"), 100);
            Assert.AreEqual(1,result);
        }

        [TestMethod, Description("Try to parse an Option<string> Some to a valid number and returns the default if it is not parseable")]
        public void ReturnDefaultWhenSome_TryParseValueFromString()
        {
            var result = _configurationUtils.TryParseValueFromString(Option.Some("a"), 100);
            Assert.AreEqual(100,100);
        }

        [TestMethod, Description("Try to parse an Option<string> None to a valid number and returns the default")]
        public void ReturnDefaultWhenNone_TryParseValueFromString()
        {
            var result = _configurationUtils.TryParseValueFromString(Option.None<string>(), 100);
            Assert.AreEqual(100,100);
        }

        [TestMethod, Description("Try to read an environment variable and return the value as Option.Some<string> if it is existing")]
        public void ReturnSome_ReadEnvironmentVariable()
        {
            Environment.SetEnvironmentVariable("test_value1", "a");
            var result = _configurationUtils.ReadEnvironmentVariable("test_value1");
            Assert.AreEqual(Option.Some<string>("a"), result);
            Environment.SetEnvironmentVariable("test_value1",null);
        }

        [TestMethod, Description("Try to read an environment variable and return the value as Option.None<string> if it is not existing")]
        public void ReturnNone_ReadEnvironmentVariable()
        {
            var result = _configurationUtils.ReadEnvironmentVariable("test_value2");
            Assert.AreEqual(Option.None<string>(), result);
        }
    }
}