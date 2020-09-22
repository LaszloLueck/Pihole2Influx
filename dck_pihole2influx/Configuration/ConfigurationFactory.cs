﻿namespace dck_pihole2influx.Configuration
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
        public const string PiholeUser = "PIHOLEUSER";
        public const string PihoePassword = "PIHOLEPASSWORD";

        public readonly Configuration Configuration;

        public ConfigurationFactory(IConfigurationUtils configurationUtils)
        {
            this.Configuration = new Configuration(
                configurationUtils.ReadEnvironmentVariable(PiholeIpOrHostName)
                    .ValueOr(Configuration.DefaultPiholeHostOrIp),
                configurationUtils.TryParseValueFromString(configurationUtils.ReadEnvironmentVariable(PiholePort),
                    Configuration.DefaultPiholePort),
                configurationUtils.ReadEnvironmentVariable(InfluxDbIpOrHostName)
                    .ValueOr(Configuration.DefaultInfluxDbHostOrIp),
                configurationUtils.TryParseValueFromString(configurationUtils.ReadEnvironmentVariable(InfluxDbPort),
                    Configuration.DefaultInfluxDbPort),
                configurationUtils.ReadEnvironmentVariable(InfluxDbDatabaseName)
                    .ValueOr(Configuration.DefaultInfluxDbDatabaseName),
                configurationUtils.ReadEnvironmentVariable(InfluxDbUserName)
                    .ValueOr(Configuration.DefaultInfluxDbUserName),
                configurationUtils.ReadEnvironmentVariable(InfluxDbPassword)
                    .ValueOr(Configuration.DefaultInfluxDbPassword),
                configurationUtils.ReadEnvironmentVariable(PiholeUser).ValueOr(Configuration.DefaultPiholeUser),
                configurationUtils.ReadEnvironmentVariable(PihoePassword).ValueOr(Configuration.DefaultPiholePassword)
            );
        }
    }
}