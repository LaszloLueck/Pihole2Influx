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
        private static readonly ILogger _Log = LoggingFactory<SchedulerJob>.CreateLogging();

        private static readonly Configuration.Configuration _ConfigurationFactory =
            new ConfigurationFactory(new ConfigurationUtils()).Configuration;
        
        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Run(async () =>
            {
                _Log.Information("Use the following parameter for connections:");
                _Log.Information($"Pihole host: {_ConfigurationFactory.PiholeHostOrIp}");
                _Log.Information($"Pihole telnet port: {_ConfigurationFactory.PiholeTelnetPort}");
                _Log.Information($"InfluxDb host: {_ConfigurationFactory.InfluxDbHostOrIp}");
                _Log.Information($"InfluxDb port: {_ConfigurationFactory.InfluxDbPort}");
                _Log.Information($"InfluxDb database name: {_ConfigurationFactory.InfluxDbDatabaseName}");
                _Log.Information($"InfluxDb user name: {_ConfigurationFactory.InfluxDbUserName}");
                _Log.Information(
                    $"InfluxDb password is {(_ConfigurationFactory.InfluxDbPassword.Length == 0 ? "not set" : "set")}");

                _Log.Information("Connect to pihole and get stats");

                var telnetClient = new TelnetClient("192.168.1.10",
                    _ConfigurationFactory.PiholeTelnetPort, _ConfigurationFactory.PiholeUser,
                    _ConfigurationFactory.PiholePassword);
                
                var result = await telnetClient.ConnectAndReceiveData(TelnetUtils.PiholeCommands.Cacheinfo);

                result.MatchSome(value =>
                {
                    var obj = new CacheInfo(value);
                    obj.AsJsonOpt.MatchSome(m =>
                    {
                        _Log.Information($"R: {m}");
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