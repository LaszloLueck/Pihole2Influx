using Optional;
using Optional.Linq;

namespace dck_pihole2influx.Configuration
{
   
    public record ConfigurationItems(string PiholeHost, int PiholePort, string InfluxDbHost, int InfluxDbPort, string InfluxDbName, string InfluxDbUsername, string InfluxDbPassword, string PiholeUser, string PiholePassword, int RunsEvery, int ConcurrentRequestsToPihole);

    public enum EnvEntries
    {
        PIHOLEHOST,
        PIHOLEPORT,
        INFLUXDBHOST,
        INFLUXDBPORT,
        INFLUXDBNAME,
        INFLUXDBUSERNAME,
        INFLUXDBPASSWORD,
        PIHOLEUSER,
        PIHOLEPASSWORD,
        CONCURRENTREQUESTSTOPIHOLE,
        RUNSEVERY
    }

    public class ConfigurationBuilder
    {
        private readonly IConfigurationFactory _configurationFactory;

        public ConfigurationBuilder(IConfigurationFactory configurationFactory)
        {
            _configurationFactory = configurationFactory;
        }

        public Option<ConfigurationItems> GetConfiguration()
        {
            return (
                from piholeHost in _configurationFactory.ReadEnvironmentVariableString(EnvEntries.PIHOLEHOST)
                from piholePort in _configurationFactory.ReadEnvironmentVariableInt(EnvEntries.PIHOLEPORT)
                from influxDbHost in _configurationFactory.ReadEnvironmentVariableString(EnvEntries.INFLUXDBHOST) 
                from influxDbPort in _configurationFactory.ReadEnvironmentVariableInt(EnvEntries.INFLUXDBPORT)
                from influxDbName in _configurationFactory.ReadEnvironmentVariableString(EnvEntries.INFLUXDBNAME)
                from influxDbUserName in _configurationFactory.ReadEnvironmentVariableString(EnvEntries.INFLUXDBUSERNAME)
                from influxDbPassword in _configurationFactory.ReadEnvironmentVariableString(EnvEntries.INFLUXDBPASSWORD)
                from piholeUser in _configurationFactory.ReadEnvironmentVariableString(EnvEntries.PIHOLEUSER)
                from piholePassword in _configurationFactory.ReadEnvironmentVariableString(EnvEntries.PIHOLEPASSWORD)
                from parallelity in _configurationFactory.ReadEnvironmentVariableInt(EnvEntries.CONCURRENTREQUESTSTOPIHOLE)

                from runsEvery in _configurationFactory.ReadEnvironmentVariableInt(EnvEntries.RUNSEVERY)
                select new ConfigurationItems(piholeHost, piholePort, influxDbHost, influxDbPort, influxDbName, influxDbUserName, influxDbPassword, piholeUser, piholePassword, runsEvery, parallelity)
            );
        }
        
    }
}