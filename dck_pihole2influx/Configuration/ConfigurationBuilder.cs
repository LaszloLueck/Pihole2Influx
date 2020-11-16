using CheckWebsiteStatus.Configuration;

namespace dck_pihole2influx.Configuration
{
    public class ConfigurationItems
    {
        public readonly string SitemapUrl;
        public readonly int RunsEvery;

        public ConfigurationItems(string sitemapUrl, int runsEvery)
        {
            SitemapUrl = sitemapUrl;
            RunsEvery = runsEvery;
        }
    }

    public enum EnvEntries
    {
        SitemapUrl,
        RunsEvery
    }

    public class ConfigurationBuilder
    {
        private readonly IConfigurationFactory _configurationFactory;

        public ConfigurationBuilder(IConfigurationFactory configurationFactory)
        {
            _configurationFactory = configurationFactory;
        }
    }
}