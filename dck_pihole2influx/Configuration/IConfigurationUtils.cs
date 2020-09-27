
using Optional;

namespace dck_pihole2influx.Configuration
{
    public interface IConfigurationUtils
    {
        Option<string> ReadEnvironmentVariable(string value);
        int TryParseValueFromString(Option<string> value, int defaultValue);
    }
}