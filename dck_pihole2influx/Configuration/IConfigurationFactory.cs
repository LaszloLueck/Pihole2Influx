using Optional;

namespace CheckWebsiteStatus.Configuration
{
    public interface IConfigurationFactory
    {
        Option<string> ReadEnvironmentVariableString(string value);
        Option<int> ReadEnvironmentVariableInt(string value);
    }
}