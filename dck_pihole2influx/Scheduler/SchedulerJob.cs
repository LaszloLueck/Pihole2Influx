using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using dck_pihole2influx.Configuration;
using dck_pihole2influx.Logging;
using dck_pihole2influx.StatObjects;
using dck_pihole2influx.Transport.InfluxDb;
using dck_pihole2influx.Transport.Telnet;
using Quartz;

namespace dck_pihole2influx.Scheduler
{
    public class SchedulerJob : IJob
    {
        private static readonly IMySimpleLogger Log = MySimpleLoggerImpl<SchedulerJob>.GetLogger();

        public async Task Execute(IJobExecutionContext context)
        {
            var configuration = (ConfigurationItems) context.JobDetail.JobDataMap["configuration"];
            
            await Task.Run(async () =>
            {
                Log.Info("Use the following parameter for connections:");
                Log.Info($"Pihole host: {configuration.PiholeHost}");
                Log.Info($"Pihole telnet port: {configuration.PiholePort }");
                Log.Info($"InfluxDb host: {configuration.InfluxDbHost}");
                Log.Info($"InfluxDb port: {configuration.InfluxDbPort}");
                Log.Info($"InfluxDb database name: {configuration.InfluxDbName}");
                Log.Info($"InfluxDb user name: {configuration.InfluxDbUsername}");
                Log.Info(
                    $"InfluxDb password is {(configuration.InfluxDbPassword.Length == 0 ? "not set" : "set")}");
            
                Log.Info(
                    $"Connect to Pihole and process data with {configuration.ConcurrentRequestsToPihole} parallel process(es).");
                Log.Info("Connect to pihole and get stats");

                //throttle the amount of concurrent telnet-requests to pihole.
                //if it is not set per env-var, the default is 1 (one request per time). 
                var mutex = new SemaphoreSlim(configuration.ConcurrentRequestsToPihole);
                var influxConnector =
                    new InfluxDbConnector().GetInfluxDbConnection();
                influxConnector.Connect(configuration);
                var enumerable = Workers.GetJobsToDo().Select(async worker =>
                {
                    await mutex.WaitAsync();
                    var t = Task.Run(async () =>
                    {
                        IConnectedTelnetClient telnetClient =
                            new ConnectedTelnetClient(configuration.PiholeHost,
                                configuration.PiholePort);
                        if (telnetClient.IsConnected())
                        {
                            if (configuration.PiholeUser.Length > 0 &&
                                configuration.PiholePassword.Length > 0)
                            {
                                await telnetClient.LoginOnTelnet(configuration.PiholeUser,
                                    configuration.PiholePassword);
                            }

                            await telnetClient.WriteCommand(worker.GetPiholeCommand());
                            var result = await telnetClient.ReadResult(worker.GetTerminator());
                            await telnetClient.WriteCommand(PiholeCommands.Quit);
                            telnetClient.ClientDispose();

                            await worker.Convert(result);

                            var measurements = await worker.CalculateMeasurementData();
                            await influxConnector.WriteMeasurementsAsync(measurements);

                            Log.Info($"Finished Worker <{worker.GetType().Name}>");
                        }
                    });
                    await t;
                    mutex.Release();
                });
                await Task.WhenAll(enumerable);
                influxConnector.DisposeConnector();
            });
        }
    }
}