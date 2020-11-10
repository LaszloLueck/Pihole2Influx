using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using dck_pihole2influx.Configuration;
using dck_pihole2influx.Logging;
using dck_pihole2influx.StatObjects;
using dck_pihole2influx.Transport.InfluxDb;
using dck_pihole2influx.Transport.InfluxDb.Measurements;
using dck_pihole2influx.Transport.Telnet;
using Optional;
using Quartz;

namespace dck_pihole2influx.Scheduler
{
    public class SchedulerJob : IJob
    {
        private static readonly IMySimpleLogger Log = MySimpleLoggerImpl<SchedulerJob>.GetLogger();

        private static readonly Configuration.Configuration ConfigurationFactory =
            new ConfigurationFactory(new ConfigurationUtils()).Configuration;

        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Run(async () =>
            {
                Log.Info("Use the following parameter for connections:");
                Log.Info($"Pihole host: {ConfigurationFactory.PiholeHostOrIp}");
                Log.Info($"Pihole telnet port: {ConfigurationFactory.PiholeTelnetPort}");
                Log.Info($"InfluxDb host: {ConfigurationFactory.InfluxDbHostOrIp}");
                Log.Info($"InfluxDb port: {ConfigurationFactory.InfluxDbPort}");
                Log.Info($"InfluxDb database name: {ConfigurationFactory.InfluxDbDatabaseName}");
                Log.Info($"InfluxDb user name: {ConfigurationFactory.InfluxDbUserName}");
                Log.Info(
                    $"InfluxDb password is {(ConfigurationFactory.InfluxDbPassword.Length == 0 ? "not set" : "set")}");
                Log.Info(
                    $"Connect to Pihole and process data with {ConfigurationFactory.ConcurrentRequestsToPihole} parallel process(es).");
                Log.Info("Connect to pihole and get stats");

                InfluxConnectionFactory influxConnector =
                    new InfluxDbConnector().GetInfluxDbConnection();
                influxConnector.Connect(ConfigurationFactory);

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
                            var resultString = await worker.GetJsonObjectFromDictionaryAsync(true);
                            Log.Info(resultString);

                            switch (worker)
                            {
                                case TopClientsConverter topClientsConverter:
                                    var items = CalculateMeasurementsTopClients(topClientsConverter.DictionaryOpt);
                                    influxConnector.WriteMeasurements(items);
                                    break;
                                default:
                                    Log.Warning($"No conversion for Type {worker.GetType().FullName} available");
                                    break;
                            }
                        }

                        await telnetClient.WriteCommand(PiholeCommands.Quit);
                        telnetClient.ClientDispose();
                    });
                    await t;
                    mutex.Release();
                });
                await Task.WhenAll(enumerable);
                influxConnector.DisposeConnector();
            });
        }

        private static List<MeasurementTopClient> CalculateMeasurementsTopClients(
            Option<ConcurrentDictionary<string, IBaseResult>> dictOpt)
        {
            return dictOpt.Map(dic =>
            {
                return (from tuple in dic select tuple).Select(tpl =>
                {
                    var convValue = (DoubleStringOutputElement) tpl.Value;
                    return new MeasurementTopClient()
                    {
                        ClientIp = convValue.IpAddress, HostName = convValue.HostName.ValueOr(""),
                        Position = convValue.Position + 1, Count = convValue.Count, Time = DateTime.Now
                    };
                });
            }).ValueOr(new List<MeasurementTopClient>()).ToList();
        }
    }
}