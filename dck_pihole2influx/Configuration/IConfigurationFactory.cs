using Optional;

namespace dck_pihole2influx.Configuration
{
    public interface IConfigurationFactory
    {
        Option<string> ReadEnvironmentVariableString(EnvEntries value);
        Option<int> ReadEnvironmentVariableInt(EnvEntries value);
    }
}