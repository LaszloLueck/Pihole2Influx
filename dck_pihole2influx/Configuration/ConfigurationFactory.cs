using System;
using dck_pihole2influx.Logging;
using Serilog;

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
        public const string PiholeUser = "PIHOLEUSER";
        public const string PihoePassword = "PIHOLEPASSWORD";
        public const string ConcurrentRequestsToPihole = "CONCURRENTREQUESTSTOPIHOLE";

        public readonly Configuration Configuration;

        private readonly IConfigurationUtils _configurationUtils;
        private static readonly ILogger Log = LoggingFactory<ConfigurationFactory>.CreateLogging();

        public ConfigurationFactory(IConfigurationUtils configurationUtils)
        {
            _configurationUtils = configurationUtils;
            this.Configuration = new Configuration(
                GetValueOrDefaultFromEnv<string>(PiholeIpOrHostName, Configuration.DefaultPiholeHostOrIp),
                GetValueOrDefaultFromEnv<int>(PiholePort, Configuration.DefaultPiholePort),
                GetValueOrDefaultFromEnv<string>(InfluxDbIpOrHostName, Configuration.DefaultInfluxDbHostOrIp),
                GetValueOrDefaultFromEnv<int>(InfluxDbPort, Configuration.DefaultInfluxDbPort),
                GetValueOrDefaultFromEnv<string>(InfluxDbDatabaseName, Configuration.DefaultInfluxDbDatabaseName),
                GetValueOrDefaultFromEnv<string>(InfluxDbUserName, Configuration.DefaultInfluxDbUserName),
                GetValueOrDefaultFromEnv<string>(InfluxDbPassword, Configuration.InfluxDbPassword),
                GetValueOrDefaultFromEnv<string>(PiholeUser, Configuration.PiholeUser),
                GetValueOrDefaultFromEnv<string>(PihoePassword, Configuration.PiholePassword),
                GetValueOrDefaultFromEnv<int>(ConcurrentRequestsToPihole, Configuration.DefaultConcurrentRequestsToPihole)
                );
        }

        private T GetValueOrDefaultFromEnv<T>(string envKey, T defaultValue)
        {
            switch (defaultValue)
            {
                case int intValue:
                    return (T) Convert.ChangeType(
                        _configurationUtils.TryParseValueFromString(_configurationUtils.ReadEnvironmentVariable(envKey),
                            intValue), typeof(T));
                case string stringValue:
                    return (T) Convert.ChangeType(_configurationUtils.ReadEnvironmentVariable(envKey).ValueOr(stringValue),
                        typeof(T));
                default:
                    Log.Warning(
                        $"Unidentified type <{typeof(T).FullName}> found. Return default value instead <{defaultValue}>");
                    return (T) Convert.ChangeType(defaultValue, typeof(T));
            }
        }
    }
}