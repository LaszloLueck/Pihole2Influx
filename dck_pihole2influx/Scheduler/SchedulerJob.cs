using System.Threading.Tasks;
using dck_pihole2influx.Configuration;
using dck_pihole2influx.Logging;
using dck_pihole2influx.StatObjects;
using dck_pihole2influx.Transport.Telnet;
using Quartz;
using Serilog;

namespace dck_pihole2influx.Scheduler
{
    public class SchedulerJob : IJob
    {
        private static readonly ILogger Log = LoggingFactory<SchedulerJob>.CreateLogging();

        private static readonly Configuration.Configuration ConfigurationFactory =
            new ConfigurationFactory(new ConfigurationUtils()).Configuration;
        
        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Run(async () =>
            {
                Log.Information("Use the following parameter for connections:");
                Log.Information($"Pihole host: {ConfigurationFactory.PiholeHostOrIp}");
                Log.Information($"Pihole telnet port: {ConfigurationFactory.PiholeTelnetPort}");
                Log.Information($"InfluxDb host: {ConfigurationFactory.InfluxDbHostOrIp}");
                Log.Information($"InfluxDb port: {ConfigurationFactory.InfluxDbPort}");
                Log.Information($"InfluxDb database name: {ConfigurationFactory.InfluxDbDatabaseName}");
                Log.Information($"InfluxDb user name: {ConfigurationFactory.InfluxDbUserName}");
                Log.Information(
                    $"InfluxDb password is {(ConfigurationFactory.InfluxDbPassword.Length == 0 ? "not set" : "set")}");

                Log.Information("Connect to pihole and get stats");

                var telnetClient = new TelnetClient("192.168.1.10",
                    ConfigurationFactory.PiholeTelnetPort, ConfigurationFactory.PiholeUser,
                    ConfigurationFactory.PiholePassword);
                
                var result = await telnetClient.ConnectAndReceiveData(TelnetUtils.PiholeCommands.CACHEINFO);

                result.MatchSome(value =>
                {
                    var obj = new CacheInfo(value);
                    obj.AsJsonOpt.MatchSome(m =>
                    {
                        Log.Information($"R: {m}");
                    });
                });

                // var result = await telnetClient.ConnectAndReceiveData(TelnetUtils.PiholeCommands.TOPDOMAINS);
                // result.Match(
                //     some: s => Log.Information($"RESULT: {s}"),
                //     none: () => Log.Error("Scheisse")
                // );

            });
        }
    }
}