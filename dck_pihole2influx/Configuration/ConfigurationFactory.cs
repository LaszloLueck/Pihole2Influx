using System;
using System.Threading.Tasks;
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
                    Task.Run(async () =>
                    {
                        await Log.InfoAsync($"No entry found for environment variable {value}");
                    });
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
                    Task.Run(async () =>
                    {
                        await Log.WarningAsync($"No entry found for environment variable {value}");
                    });
                    return Option.None<int>();
                }
            );
        }

        private static Option<int> LogAndReturnNone(string envName, string value)
        {
            Task.Run(async () =>
            {
                await Log.WarningAsync($"Cannot convert value {value} for env variable {envName}");
            });
            return Option.None<int>();
        }
    }
}