using System.Threading.Tasks;
using dck_pihole2influx.Configuration;
using dck_pihole2influx.Logging;
using Quartz;
using Serilog;

namespace dck_pihole2influx.Scheduler
{
    public class SchedulerJob : IJob
    {
        private static readonly ILogger Log = LoggingFactory<SchedulerJob>.CreateLogging();

        private static readonly Configuration.Configuration _configurationFactory =
            new ConfigurationFactory(new ConfigurationUtils()).Configuration;

        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Run(() =>
            {
                Log.Information("Use the following parameter for connections:");
                Log.Information($"Pihole host: {_configurationFactory.PiholeHostOrIp}");
                Log.Information($"Pihole telnet port: {_configurationFactory.PiholeTelnetPort}");
                Log.Information($"InfluxDb host: {_configurationFactory.InfluxDbHostOrIp}");
                Log.Information($"InfluxDb port: {_configurationFactory.InfluxDbPort}");
                Log.Information($"InfluxDb database name: {_configurationFactory.InfluxDbDatabaseName}");
                Log.Information($"InfluxDb user name: {_configurationFactory.InfluxDbUserName}");
                Log.Information(
                    $"InfluxDb password is {(_configurationFactory.InfluxDbPassword.Length == 0 ? "not set" : "set")}");
            });
        }
    }
}