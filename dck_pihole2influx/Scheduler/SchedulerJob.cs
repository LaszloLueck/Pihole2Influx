using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

                var enumerable = Workers.GetJobsToDo().Select(async worker =>
                {
                    var t = Task.Run(async () => { 
                        IConnectedTelnetClient telnetClient = new ConnectedTelnetClient(ConfigurationFactory.PiholeHostOrIp, ConfigurationFactory.PiholeTelnetPort);
                        if (telnetClient.IsConnected())
                        {
                            if (ConfigurationFactory.PiholeUser.Length > 0 && ConfigurationFactory.PiholePassword.Length > 0)
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
                });
                await Task.WhenAll(enumerable);
            });
        }
    }
}