using System;
using dck_pihole2influx.Logging;
using Optional;
using Serilog;

namespace dck_pihole2influx.Configuration
{
    public class ConfigurationUtils : IConfigurationUtils
    {
        private static readonly ILogger Log = LoggingFactory<ConfigurationUtils>.CreateLogging();
        public Option<string> ReadEnvironmentVariable(string value)
        {
            return Environment.GetEnvironmentVariable(value).SomeNotNull();
        }
        
        public int TryParseValueFromString(Option<string> value, int defaultValue)
        {
            return value.Match(
                some: innerValue =>
                {
                    int parsedValue = int.TryParse(innerValue, out parsedValue) ? parsedValue : defaultValue;
                    return parsedValue;
                },
                none: () =>
                {
                    return defaultValue;
                });
        }
        
        
    }
}