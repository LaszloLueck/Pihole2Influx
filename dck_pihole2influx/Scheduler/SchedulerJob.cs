using System.Linq;
using System.Threading;
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
                
                //throttle the amount of concurrent telnet-requests to pihole.
                //if it is not set per env-var, the default is 1 (one request per time). 
                var mutex = new SemaphoreSlim(ConfigurationFactory.ConcurrentRequestsToPihole);

                var enumerable = Workers.GetJobsToDo().Select(async worker =>
                {
                    await mutex.WaitAsync();
                    var t = Task.Run(async () =>
                    {
                        IConnectedTelnetClient telnetClient =
                            new ConnectedTelnetClient(ConfigurationFactory.PiholeHostOrIp,
                                ConfigurationFactory.PiholeTelnetPort);
                        if (telnetClient.IsConnected())
                        {
                            if (ConfigurationFactory.PiholeUser.Length > 0 &&
                                ConfigurationFactory.PiholePassword.Length > 0)
                            {
                                await telnetClient.LoginOnTelnet(ConfigurationFactory.PiholeUser,
                                    ConfigurationFactory.PiholePassword);
                            }

                            await telnetClient.WriteCommand(worker.GetPiholeCommand());
                            var result = await telnetClient.ReadResult(worker.GetTerminator());
                            await worker.Convert(result);
                            var resultString = await worker.GetJsonFromObjectAsync(true);
                            Log.Information($"UGU: {resultString}");
                        }

                        await telnetClient.WriteCommand(PiholeCommands.Quit);
                        telnetClient.Dispose();
                    });
                    await t;
                    mutex.Release();
                });
                await Task.WhenAll(enumerable);
            });
        }
    }
}