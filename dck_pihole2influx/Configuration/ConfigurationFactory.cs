using static dck_pihole2influx.Configuration.ConfigurationUtils;

namespace dck_pihole2influx.Configuration
{
    public class ConfigurationFactory
    {
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
            this.Configuration = new Configuration(
                ReadEnvironmentVariable(PiholeIpOrHostName).ValueOr(Configuration.DefaultPiholeHostOrIp),
                TryParseValueFromString(ReadEnvironmentVariable(PiholePort), Configuration.DefaultPiholePort),
                ReadEnvironmentVariable(InfluxDbIpOrHostName).ValueOr(Configuration.DefaultInfluxDbHostOrIp),
                TryParseValueFromString(ReadEnvironmentVariable(InfluxDbPort), Configuration.DefaultInfluxDbPort),
                ReadEnvironmentVariable(InfluxDbDatabaseName).ValueOr(Configuration.DefaultInfluxDbDatabaseName),
                ReadEnvironmentVariable(InfluxDbUserName).ValueOr(Configuration.DefaultInfluxDbUserName),
                ReadEnvironmentVariable(InfluxDbPassword).ValueOr(Configuration.DefaultInfluxDbPassword)
            );
        }
    }
}