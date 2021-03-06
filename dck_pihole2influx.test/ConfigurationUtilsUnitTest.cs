using System;
using dck_pihole2influx.Configuration;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Optional;
using Optional.Unsafe;

namespace dck_pihole2influx.test
{
    [TestClass]
    public class ConfigurationUtilsUnitTest
    {

        private readonly IConfigurationFactory _configurationFactory;

        public ConfigurationUtilsUnitTest()
        {
            _configurationFactory = new ConfigurationFactory();
        }
        
        
        [TestMethod, Description("try to convert a string from a not existing env var and return none")]
        public void ReturnNoneWhenEnvVarStringNotExists()
        {
            _configurationFactory.ReadEnvironmentVariableString(EnvEntries.INFLUXDBHOST).Should().Be(Option.None<string>());
        }

        [TestMethod, Description("try to convert an int from a not existing env var and return none")]
        public void ReturnNoneWhenEnvVarIntNotExists()
        {
            _configurationFactory.ReadEnvironmentVariableInt(EnvEntries.PIHOLEPORT).Should().Be(Option.None<int>());
        }

        [TestMethod,
         Description("try to convert an existing environment variable and return none if the type cannot be converted")]
        public void ReturnNoneWhenTypeConversionNotMatched()
        {
            Environment.SetEnvironmentVariable(EnvEntries.PIHOLEPORT.ToString(), "abc");
            _configurationFactory.ReadEnvironmentVariableInt(EnvEntries.PIHOLEPORT).Should().Be(Option.None<int>());
            Environment.SetEnvironmentVariable(EnvEntries.PIHOLEPORT.ToString(), null);
        }

        [TestMethod, Description("Check if ConfigurationItems is complete")]
        public void CheckIfConfigurationItemsIsWellformed(){
            var testee = new ConfigurationItems("piholehost", 123, "influxdbhost", 234, "influxdbname", "influxdbusername", "influxdbpassword", 10, 4);

            testee.PiholeHost.Should().Be("piholehost");
            testee.PiholePort.Should().Be(123);
            testee.RunsEvery.Should().Be(10);
            testee.ConcurrentRequestsToPihole.Should().Be(4);
            testee.InfluxDbHost.Should().Be("influxdbhost");
            testee.InfluxDbName.Should().Be("influxdbname");
            testee.InfluxDbPassword.Should().Be("influxdbpassword");
            testee.InfluxDbPort.Should().Be(234);
            testee.InfluxDbUsername.Should().Be("influxdbusername");

        }

        [TestMethod, Description("build the complete configuration-object")]
        public void ReturnSomeConfigurationIfAllParametersAreSet()
        {
            Environment.SetEnvironmentVariable(EnvEntries.RUNSEVERY.ToString(), "10");
            Environment.SetEnvironmentVariable(EnvEntries.PIHOLEHOST.ToString(), "piholehost");
            Environment.SetEnvironmentVariable(EnvEntries.PIHOLEPORT.ToString(), "123");
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBHOST.ToString(), "influxdbhost");
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBNAME.ToString(), "influxdbname");
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBPORT.ToString(), "234");
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBPASSWORD.ToString(), "influxdbpassword");
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBUSERNAME.ToString(), "influxdbusername");
            Environment.SetEnvironmentVariable(EnvEntries.CONCURRENTREQUESTSTOPIHOLE.ToString(), "1");


            var expected = new ConfigurationItems("piholehost", 123, "influxdbhost", 234, "influxdbname",
                "influxdbusername", "influxdbpassword",  10, 1);

            var testeeOpt = new ConfigurationBuilder(_configurationFactory).GetConfiguration();
            
            Option.Some(expected).Should().BeEquivalentTo(testeeOpt);

            var testee = testeeOpt.ValueOrDefault();

            testee.PiholeHost.Should().Be(expected.PiholeHost);
            testee.PiholePort.Should().Be(expected.PiholePort);
            testee.RunsEvery.Should().Be(expected.RunsEvery);
            testee.InfluxDbHost.Should().Be(expected.InfluxDbHost);
            testee.InfluxDbName.Should().Be(expected.InfluxDbName);
            testee.InfluxDbPassword.Should().Be(expected.InfluxDbPassword);
            testee.InfluxDbPort.Should().Be(expected.InfluxDbPort);
            testee.InfluxDbUsername.Should().Be(expected.InfluxDbUsername);
            testee.ConcurrentRequestsToPihole.Should().Be(expected.ConcurrentRequestsToPihole);

            Environment.SetEnvironmentVariable(EnvEntries.RUNSEVERY.ToString(), null);
            Environment.SetEnvironmentVariable(EnvEntries.PIHOLEHOST.ToString(), null);
            Environment.SetEnvironmentVariable(EnvEntries.PIHOLEPORT.ToString(), null);
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBHOST.ToString(), null);
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBNAME.ToString(), null);
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBPORT.ToString(), null);
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBPASSWORD.ToString(), null);
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBUSERNAME.ToString(), null);
            Environment.SetEnvironmentVariable(EnvEntries.CONCURRENTREQUESTSTOPIHOLE.ToString(), null);

        }

        [TestMethod, Description("check if we could handle empty strings")]
        public void ReturnNoneIfTheEnvironmentVariableIsEmpty()
        {
            Environment.SetEnvironmentVariable(EnvEntries.RUNSEVERY.ToString(), "10");
            Environment.SetEnvironmentVariable(EnvEntries.PIHOLEHOST.ToString(), "piholehost");
            Environment.SetEnvironmentVariable(EnvEntries.PIHOLEPORT.ToString(), "123");
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBHOST.ToString(), "influxdbhost");
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBNAME.ToString(), "influxdbname");
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBPORT.ToString(), "234");
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBPASSWORD.ToString(), string.Empty);
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBUSERNAME.ToString(), string.Empty);
            Environment.SetEnvironmentVariable(EnvEntries.CONCURRENTREQUESTSTOPIHOLE.ToString(), "1");
            
            var expected = new ConfigurationItems("piholehost", 123, "influxdbhost", 234, "influxdbname",
                string.Empty, string.Empty, 10, 1);

            var testeeOpt = new ConfigurationBuilder(_configurationFactory).GetConfiguration();
            
            Option.Some(expected).Should().BeEquivalentTo(testeeOpt);

            var testee = testeeOpt.ValueOrDefault();

            testee.PiholeHost.Should().Be(expected.PiholeHost);
            testee.PiholePort.Should().Be(expected.PiholePort);
            testee.RunsEvery.Should().Be(expected.RunsEvery);
            testee.InfluxDbHost.Should().Be(expected.InfluxDbHost);
            testee.InfluxDbName.Should().Be(expected.InfluxDbName);
            testee.InfluxDbPassword.Should().Be(expected.InfluxDbPassword);
            testee.InfluxDbPort.Should().Be(expected.InfluxDbPort);
            testee.InfluxDbUsername.Should().Be(expected.InfluxDbUsername);
            testee.ConcurrentRequestsToPihole.Should().Be(expected.ConcurrentRequestsToPihole);

            Environment.SetEnvironmentVariable(EnvEntries.RUNSEVERY.ToString(), null);
            Environment.SetEnvironmentVariable(EnvEntries.PIHOLEHOST.ToString(), null);
            Environment.SetEnvironmentVariable(EnvEntries.PIHOLEPORT.ToString(), null);
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBHOST.ToString(), null);
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBNAME.ToString(), null);
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBPORT.ToString(), null);
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBPASSWORD.ToString(), null);
            Environment.SetEnvironmentVariable(EnvEntries.INFLUXDBUSERNAME.ToString(), null);
            Environment.SetEnvironmentVariable(EnvEntries.CONCURRENTREQUESTSTOPIHOLE.ToString(), null);
            
        }
        
        
        
    }
}