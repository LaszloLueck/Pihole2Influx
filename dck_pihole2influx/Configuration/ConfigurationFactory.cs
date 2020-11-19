using System;
using dck_pihole2influx.Logging;
using Optional;

namespace dck_pihole2influx.Configuration
{
    public class ConfigurationFactory : IConfigurationFactory
    {
        private static readonly IMySimpleLogger Log = MySimpleLoggerImpl<ConfigurationFactory>.GetLogger();

        public Option<string> ReadEnvironmentVariableString(EnvEntries value, bool returnEmptyStringIfNoValue = false)
        {
            //Put some sugar here to tell why the container stops.
            return Environment.GetEnvironmentVariable(value.ToString()).SomeNotNull().Match(
                some: Option.Some,
                none: () =>
                {
                    if (returnEmptyStringIfNoValue)
                        return Option.Some(string.Empty);
                    
                    Log.Info($"No entry found for environment variable {value}");
                    return Option.None<string>();
                }
            );
        }

        public Option<int> ReadEnvironmentVariableInt(EnvEntries value)
        {
            return Environment.GetEnvironmentVariable(value.ToString()).SomeNotNull().Match(
                some: variable => int.TryParse(variable, out var intVariable)
                    ? Option.Some(intVariable)
                    : LogAndReturnNone(value.ToString(), variable),
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