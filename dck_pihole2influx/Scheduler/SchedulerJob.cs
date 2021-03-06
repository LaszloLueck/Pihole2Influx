using System.Diagnostics;
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
            var influxConnector = (InfluxConnectionFactory) context.JobDetail.JobDataMap["influxConnectionFactory"];
            var telnetClientFactory =
                (IStandardTelnetClientFactory) context.JobDetail.JobDataMap["telnetClientFactory"];

            await Task.Run(async () =>
            {
                await Log.InfoAsync("Use the following parameter for connections:");
                await Log.InfoAsync($"Pihole host: {configuration.PiholeHost}");
                await Log.InfoAsync($"Pihole telnet port: {configuration.PiholePort}");
                await Log.InfoAsync($"InfluxDb host: {configuration.InfluxDbHost}");
                await Log.InfoAsync($"InfluxDb port: {configuration.InfluxDbPort}");
                await Log.InfoAsync($"InfluxDb database name: {configuration.InfluxDbName}");
                await Log.InfoAsync($"InfluxDb user name: {configuration.InfluxDbUsername}");
                await Log.InfoAsync(
                    $"InfluxDb password is {(configuration.InfluxDbPassword.Length == 0 ? "not set" : "set")}");

                await Log.InfoAsync(
                    $"Connect to Pihole and process data with {configuration.ConcurrentRequestsToPihole} parallel process(es).");
                await Log.InfoAsync("Connect to pihole and get stats");

                //only if a connection to influxdb is successfully established, we measure against pihole
                influxConnector.Connect(configuration).MatchSome(async _ =>
                {
                    var cnt = 0;
                    //throttle the amount of concurrent telnet-requests to pihole.
                    //if it is not set per env-var, the default is 1 (one request per time).
                    var mutex = new SemaphoreSlim(configuration.ConcurrentRequestsToPihole);
                    var enumerable = Workers.GetJobsToDo().Select(async worker =>
                    {
                        await mutex.WaitAsync();
                        var inner = cnt++;
                        await Log.InfoAsync(
                            $"Connect Task <{inner}> to Telnet-Host for worker {worker.GetType().Name}");

                        await Task.Run(async () =>
                        {
                            Stopwatch sw = Stopwatch.StartNew();
                            var standardTcpClientImpl = telnetClientFactory.Build();
                            standardTcpClientImpl.Connect(configuration.PiholeHost, configuration.PiholePort).MatchSome(
                                _ =>
                                {
                                    standardTcpClientImpl.WriteCommand(worker.GetPiholeCommand()).MatchSome(_ =>
                                    {
                                        standardTcpClientImpl
                                            .ReceiveDataSync(worker.GetTerminator())
                                            .MatchSome(async result =>
                                            {
                                                standardTcpClientImpl.WriteCommand(PiholeCommands.Quit);
                                                await worker.Convert(result);
                                                var measurements = await worker.CalculateMeasurementData();
                                                await influxConnector.WriteMeasurementsAsync(measurements);
                                            });
                                    });
                                });
                            standardTcpClientImpl.CloseAndDisposeStream();
                            standardTcpClientImpl.CloseAndDisposeTcpClient();
                            sw.Stop();
                            await Log.InfoAsync(
                                $"Finished Task <{inner}> for Worker <{worker.GetType().Name}> in {sw.ElapsedMilliseconds} ms");
                        });
                        mutex.Release();
                    });
                    await Task.WhenAll(enumerable);
                    influxConnector.DisposeConnector();
                });
            });
        }
    }
}