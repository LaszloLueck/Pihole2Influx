using System;
using dck_pihole2influx.Logging;

namespace dck_pihole2influx.Configuration
{
    public class ConfigurationFactory2
    {
        public const string PiholeIpOrHostName = "PIHOLEHOST";
        public const string PiholePort = "PIHOLEPORT";
        public const string InfluxDbIpOrHostName = "INFLUXDBHOST";
        public const string InfluxDbPort = "INFLUXDBPORT";
        public const string InfluxDbDatabaseName = "INFLUXDBNAME";
        public const string InfluxDbUserName = "INFLUXDBUSERNAME";
        public const string InfluxDbPassword = "INFLUXDBPASSWORD";
        public const string PiholeUser = "PIHOLEUSER";
        public const string PiholePassword = "PIHOLEPASSWORD";
        public const string ConcurrentRequestsToPihole = "CONCURRENTREQUESTSTOPIHOLE";
        public const string RunsEvery = "RUNSEVERY";

        public readonly Configuration Configuration;

        private readonly IConfigurationUtils _configurationUtils;
        private static readonly IMySimpleLogger Log = MySimpleLoggerImpl<ConfigurationFactory>.GetLogger();

        public ConfigurationFactory2(IConfigurationUtils configurationUtils)
        {
            _configurationUtils = configurationUtils;
            Configuration = new Configuration(
                GetValueOrDefaultFromEnv(PiholeIpOrHostName, Configuration.DefaultPiholeHostOrIp),
                GetValueOrDefaultFromEnv(PiholePort, Configuration.DefaultPiholePort),
                GetValueOrDefaultFromEnv(InfluxDbIpOrHostName, Configuration.DefaultInfluxDbHostOrIp),
                GetValueOrDefaultFromEnv(InfluxDbPort, Configuration.DefaultInfluxDbPort),
                GetValueOrDefaultFromEnv(InfluxDbDatabaseName, Configuration.DefaultInfluxDbDatabaseName),
                GetValueOrDefaultFromEnv(InfluxDbUserName, Configuration.DefaultInfluxDbUserName),
                GetValueOrDefaultFromEnv(InfluxDbPassword, Configuration.DefaultInfluxDbPassword),
                GetValueOrDefaultFromEnv(PiholeUser, Configuration.DefaultPiholeUser),
                GetValueOrDefaultFromEnv(PiholePassword, Configuration.DefaultPiholePassword),
                GetValueOrDefaultFromEnv(ConcurrentRequestsToPihole,
                    Configuration.DefaultConcurrentRequestsToPihole),
                GetValueOrDefaultFromEnv(RunsEvery, Configuration.DefaultRunsEvery)
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
                    return (T) Convert.ChangeType(
                        _configurationUtils.ReadEnvironmentVariable(envKey).ValueOr(stringValue),
                        typeof(T));
                default:
                    Log.Warning(
                        $"Unidentified type <{typeof(T).FullName}> found. Return default value instead.");
                    return (T) Convert.ChangeType(defaultValue, typeof(T));
            }
        }
    }
}