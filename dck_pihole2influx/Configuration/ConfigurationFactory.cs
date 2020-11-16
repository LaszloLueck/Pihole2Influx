using System;
using CheckWebsiteStatus.Configuration;
using dck_pihole2influx.Logging;
using Optional;

namespace dck_pihole2influx.Configuration
{
    public class ConfigurationFactory : IConfigurationFactory
    {
        private static readonly IMySimpleLogger Log = MySimpleLoggerImpl<ConfigurationFactory>.GetLogger();

        public Option<string> ReadEnvironmentVariableString(string value)
        {
            //Put some sugar here to tell why the container stops.
            return Environment.GetEnvironmentVariable(value).SomeNotNull().Match(
                some: Option.Some,
                none: () =>
                {
                    Log.Info($"No entry found for environment variable {value}");
                    return Option.None<string>();
                }
            );
        }

        public Option<int> ReadEnvironmentVariableInt(string value)
        {
            return Environment.GetEnvironmentVariable(value).SomeNotNull().Match(
                some: variable => int.TryParse(variable, out var intVariable)
                    ? Option.Some(intVariable)
                    : LogAndReturnNone(value, variable),
                none: () =>
                {
                    Log.Warning($"No entry found for environment variable {value}");
                    return Option.None<int>();
                }
            );
        }

        private Option<int> LogAndReturnNone(string envName, string value)
        {
            Log.Warning($"Cannot convert value {value} for env variable {envName}");
            return Option.None<int>();
        }
    }
}