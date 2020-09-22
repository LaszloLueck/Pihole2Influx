using System.Threading.Tasks;
using dck_pihole2influx.Configuration;
using dck_pihole2influx.Logging;
using Quartz;
using Serilog;

namespace dck_pihole2influx.Scheduler
{
    public class SchedulerJob : IJob
    {
        private static readonly ILogger _log = LoggingFactory<SchedulerJob>.CreateLogging();

        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Run(() =>
            {
                var configuration = new ConfigurationFactory(new ConfigurationUtils()).Configuration;
                _log.Information("Use the following parameter for connections:");
                _log.Information($"Pihole host: {configuration.PiholeHostOrIp}");
                _log.Information($"Pihole telnet port: {configuration.PiholeTelnetPort}");
                _log.Information($"InfluxDb host: {configuration.InfluxDbHostOrIp}");
                _log.Information($"InfluxDb port: {configuration.InfluxDbPort}");
                _log.Information($"InfluxDb database name: {configuration.InfluxDbDatabaseName}");
                _log.Information($"InfluxDb user name: {configuration.InfluxDbUserName}");
                _log.Information(
                    $"InfluxDb password is {(configuration.InfluxDbPassword.Length == 0 ? "not set" : "set")}");
            });
        }
    }
}