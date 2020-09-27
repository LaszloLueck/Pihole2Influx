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

                IConnectedTelnetClient telnetClient = new ConnectedTelnetClient(ConfigurationFactory.PiholeHostOrIp, ConfigurationFactory.PiholeTelnetPort);
                if (telnetClient.IsConnected())
                {
                    if (ConfigurationFactory.PiholeUser.Length > 0 && ConfigurationFactory.PiholePassword.Length > 0)
                    {
                        await telnetClient.LoginOnTelnet(ConfigurationFactory.PiholeUser,
                            ConfigurationFactory.PiholePassword);
                    }

                    Parallel.ForEach(Workers.GetJobsToDo(), new ParallelOptions(){MaxDegreeOfParallelism = 4}, async (worker) =>
                    {
                        telnetClient.WriteCommand(worker.GetPiholeCommand());

                        var result = await telnetClient.ReadResult(worker.GetTerminator());

                        worker.Convert(result);

                        worker.GetJsonFromObject(true).Match(
                            some: s => Log.Information($"Receive following result as json: {s}"),
                            none: () => Log.Warning($"Could not convert the resultset to an approriate json. {result}")
                        );
                    });

                    telnetClient.WriteCommand(PiholeCommands.Quit);
                    telnetClient.DisposeClient();
                }
            });
        }
    }
}