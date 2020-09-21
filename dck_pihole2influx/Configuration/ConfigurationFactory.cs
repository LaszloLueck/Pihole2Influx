using static dck_pihole2influx.Configuration.ConfigurationUtils;

namespace dck_pihole2influx.Configuration
{
    public class ConfigurationFactory
    {
        private readonly ConfigurationUtils _configurationUtils;
        
        public const string PiholeIpOrHostName = "PIHOLEHOST";
        public const string PiholePort = "PIHOLEPORT";
        public const string InfluxDbIpOrHostName = "INFLUXDBHOST";
        public const string InfluxDbPort = "INFLUXDBPORT";
        public const string InfluxDbDatabaseName = "INFLUXDBNAME";
        public const string InfluxDbUserName = "INFLUXDBUSERNAME";
        public const string InfluxDbPassword = "INFLUXDBPASSWORD";

        public readonly Configuration Configuration;

        public ConfigurationFactory()
        {
            _configurationUtils = new ConfigurationUtils();
            
            this.Configuration = new Configuration(
                _configurationUtils.ReadEnvironmentVariable(PiholeIpOrHostName).ValueOr(Configuration.DefaultPiholeHostOrIp),
                _configurationUtils.TryParseValueFromString(_configurationUtils.ReadEnvironmentVariable(PiholePort), Configuration.DefaultPiholePort),
                _configurationUtils.ReadEnvironmentVariable(InfluxDbIpOrHostName).ValueOr(Configuration.DefaultInfluxDbHostOrIp),
                _configurationUtils.TryParseValueFromString(_configurationUtils.ReadEnvironmentVariable(InfluxDbPort), Configuration.DefaultInfluxDbPort),
                _configurationUtils.ReadEnvironmentVariable(InfluxDbDatabaseName).ValueOr(Configuration.DefaultInfluxDbDatabaseName),
                _configurationUtils.ReadEnvironmentVariable(InfluxDbUserName).ValueOr(Configuration.DefaultInfluxDbUserName),
                _configurationUtils.ReadEnvironmentVariable(InfluxDbPassword).ValueOr(Configuration.DefaultInfluxDbPassword)
            );
        }
    }
}