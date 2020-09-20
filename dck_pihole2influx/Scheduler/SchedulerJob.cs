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
        
        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Run(() =>
            {
                var configuration = new ConfigurationFactory().Configuration;
                Log.Information("Use the following parameter for connections:");
                Log.Information($"Pihole host: {configuration.PiholeHostOrIp}");
                Log.Information($"Pihole telnet port: {configuration.PiholeTelnetPort}");
                Log.Information($"InfluxDb host: {configuration.InfluxDbHostOrIp}");
                Log.Information($"InfluxDb port: {configuration.InfluxDbPort}");
                Log.Information($"InfluxDb database name: {configuration.InfluxDbDatabaseName}");
                Log.Information($"InfluxDb user name: {configuration.InfluxDbUserName}");
                Log.Information(
                    $"InfluxDb password is {(configuration.InfluxDbPassword.Length == 0 ? "not set" : "set")}");
            });
        }
    }
}