using Optional;
using static dck_pihole2influx.Configuration.ConfigurationUtils;

namespace dck_pihole2influx.Configuration
{
    public class ConfigurationFactory
    {
        public static readonly string PiholeIpOrHostName = "PIHOLEHOST";
        public static readonly string PiholePort = "PIHOLEPORT";
        public static readonly string InfluxDbIpOrHostName = "INFLUXDBHOST";
        public static readonly string InfluxDbPort = "INFLUXDBPORT";
        public static readonly string InfluxDbDatabaseName = "INFLUXDBNAME";
        public static readonly string InfluxDbUserName = "INFLUXDBUSERNAME";
        public static readonly string InfluxDbPassword = "INFLUXDBPASSWORD";

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