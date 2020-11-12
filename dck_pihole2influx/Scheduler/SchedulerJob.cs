using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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

                //throttle the amount of concurrent telnet-requests to pihole.
                //if it is not set per env-var, the default is 1 (one request per time). 
                var mutex = new SemaphoreSlim(ConfigurationFactory.ConcurrentRequestsToPihole);
                InfluxConnectionFactory influxConnector =
                    new InfluxDbConnector().GetInfluxDbConnection();
                influxConnector.Connect(ConfigurationFactory);
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
                            await telnetClient.WriteCommand(PiholeCommands.Quit);
                            telnetClient.ClientDispose();

                            var t = await worker.Convert(result).ContinueWith(task =>
                            {
                                switch (worker)
                                {
                                    case TopClientsConverter topClientsConverter:
                                        var items = CalculateMeasurementsTopClients(topClientsConverter.DictionaryOpt);
                                        influxConnector.WriteMeasurements(items);
                                        break;
                                    case DbStatsConverter dbStatsConverter:
                                        var dbStatsItems = CalculateDbStatsInfo(dbStatsConverter.DictionaryOpt);
                                        influxConnector.WriteMeasurements(dbStatsItems);
                                        break;
                                    case QueryTypesConverter queryTypesConverter:
                                        var queryTypesItems =
                                            CalculateQueryTypesInfo(queryTypesConverter.DictionaryOpt);
                                        influxConnector.WriteMeasurements(queryTypesItems);
                                        break;
                                    case CacheInfoConverter cacheInfoConverter:
                                        var cacheInfoItems = CalculateCacheInfo(cacheInfoConverter.DictionaryOpt);
                                        influxConnector.WriteMeasurements(cacheInfoItems);
                                        break;
                                    case OvertimeConverter overtimeConverter:
                                        var overTimeItems = CalculateOvertime(overtimeConverter.DictionaryOpt);
                                        influxConnector.WriteMeasurements(overTimeItems);
                                        break;
                                    case StatsConverter statsConverter:
                                        var statsItems = CalculateStats(statsConverter.DictionaryOpt);
                                        influxConnector.WriteMeasurements(statsItems);
                                        break;
                                     case ForwardDestinationsConverter forwardDestinationsConverter:
                                         var forwardDestinationItems =
                                             CalculateForwardDestinations(forwardDestinationsConverter.DictionaryOpt);
                                         influxConnector.WriteMeasurements(forwardDestinationItems);
                                         break;
                                    default:
                                        Log.Warning($"No conversion for Type {worker.GetType().FullName} available");
                                        break;
                                }

                                return task;
                            });

                            await Task.WhenAll(t);
                        }
                    });
                    await t;
                    influxConnector.DisposeConnector();
                    mutex.Release();
                });
                await Task.WhenAll(enumerable);
            });
        }



        private static List<MeasurementStats> CalculateStats(Option<ConcurrentDictionary<string, IBaseResult>> dicOpt)
        {
            return dicOpt.Map(dic =>
            {
                var domainsBeingBlocked = ((PrimitiveResultInt) dic[StatsConverter.DomainsBeingBlocked]).Value;
                var dnsQueriesToday = ((PrimitiveResultInt) dic[StatsConverter.DnsQueriesToday]).Value;
                var adsBlockedToday = ((PrimitiveResultInt) dic[StatsConverter.AdsBlockedToday]).Value;
                var adsPercentageToday = ((PrimitiveResultFloat) dic[StatsConverter.AdsPercentageToday]).Value;
                var uniqueDomain = ((PrimitiveResultInt) dic[StatsConverter.UniqueDomains]).Value;
                var queriesForwarded = ((PrimitiveResultInt) dic[StatsConverter.QueriesForwarded]).Value;
                var queriesCached = ((PrimitiveResultInt) dic[StatsConverter.QueriesCached]).Value;
                var clientsEverSeen = ((PrimitiveResultInt) dic[StatsConverter.ClientsEverSeen]).Value;
                var uniqueClients = ((PrimitiveResultInt) dic[StatsConverter.UniqueClients]).Value;
                var status = ((PrimitiveResultString) dic[StatsConverter.Status]).Value;

                var returnValue = new MeasurementStats()
                {
                    DomainsBeingBlocked = domainsBeingBlocked,
                    DnsQueriesToday = dnsQueriesToday,
                    AdsBlockedToday = adsBlockedToday,
                    AdsPercentageToday = adsPercentageToday,
                    UniqueDomains = uniqueDomain,
                    QueriesForwarded = queriesForwarded,
                    QueriesCached = queriesCached,
                    ClientsEverSeen = clientsEverSeen,
                    UniqueClients = uniqueClients,
                    Status = status
                };
                
                return new List<MeasurementStats>(){returnValue};

            }).ValueOr(new List<MeasurementStats>()).ToList();
        }
        
        private static List<MeasurementForwardDestinations> CalculateForwardDestinations(
            Option<ConcurrentDictionary<string, IBaseResult>> dictOpt)
        {
            return dictOpt.Map(dic =>
            {
                return (from tuple in dic select tuple).Select(tuple =>
                {
                    var convValue = (DoubleOutputNumberedElement) tuple.Value;
                    return new MeasurementForwardDestinations()
                    {
                        IpOrHostName = convValue.IpOrHost, Percentage = convValue.Count, Position = convValue.Position,
                        Time = DateTime.Now
                    };
                });
            }).ValueOr(new List<MeasurementForwardDestinations>()).ToList();
        }

        private static List<MeasurementOvertime> CalculateOvertime(
            Option<ConcurrentDictionary<string, IBaseResult>> dicOpt)
        {
            return dicOpt.Map(dic =>
            {
                return (from tuple in dic select tuple).Select(tuple =>
                {
                    var convValue = (OvertimeOutputElement) tuple.Value;
                    var convertedDateTime = DateTimeOffset.FromUnixTimeSeconds(convValue.TimeStamp).DateTime;
                    return new MeasurementOvertime{BlockValue = convValue.BlockValue, PermitValue = convValue.PermitValue, Time= convertedDateTime.Add(TimeSpan.FromSeconds(2))};

                });
            }).ValueOr(new List<MeasurementOvertime>()).OrderBy(e => e.Time).ToList();
        }
        
        private static List<MeasurementCacheInfo> CalculateCacheInfo(
            Option<ConcurrentDictionary<string, IBaseResult>> dicOpt)
        {
            return dicOpt.Map(dic =>
            {
                var cacheSize = ((PrimitiveResultInt) dic[CacheInfoConverter.CacheSize]).Value;
                var cacheLiveFreed = ((PrimitiveResultInt) dic[CacheInfoConverter.CacheLiveFreed]).Value;
                var cacheInserted = ((PrimitiveResultInt) dic[CacheInfoConverter.CacheInserted]).Value;

                var returnValue = new MeasurementCacheInfo()
                {
                    CacheInserted = cacheInserted,
                    CacheSize = cacheSize,
                    CacheLiveFreed = cacheLiveFreed
                };
                return new List<MeasurementCacheInfo>{returnValue};

            }).ValueOr(new List<MeasurementCacheInfo>()).ToList();
        }
        
        private static List<MeasurementQueryType> CalculateQueryTypesInfo(
            Option<ConcurrentDictionary<string, IBaseResult>> dicOpt)
        {
            return dicOpt.Map(dic =>
            {
                return (from tuple in dic select tuple).Select(kv =>
                {
                    var confValue = (StringDecimalOutput) kv.Value;
                    return new MeasurementQueryType() {DnsType = confValue.Key, Value = confValue.Value};
                });
            }).ValueOr(new List<MeasurementQueryType>()).ToList();
        }

        private static List<MeasurementDbStats> CalculateDbStatsInfo(
            Option<ConcurrentDictionary<string, IBaseResult>> dicOpt)
        {
            return dicOpt.Map(dic =>
            {
                var entriesInDb = ((PrimitiveResultLong) dic[DbStatsConverter.QueriesInDatabase]).Value;
                var databaseFileSizeAsString = ((PrimitiveResultString) dic[DbStatsConverter.DatabaseFileSize]).Value;
                var databaseVersion = ((PrimitiveResultString) dic[DbStatsConverter.SqLiteVersion]).Value;
                var databaseFileSizeAsStringCut = databaseFileSizeAsString.Replace("MB", "").TrimEnd().TrimStart();
                var databaseFileSize = double.TryParse(databaseFileSizeAsStringCut, NumberStyles.Number, CultureInfo.InvariantCulture, out var doubleValue)
                    ? doubleValue
                    : 0;

                var retValue = new MeasurementDbStats()
                {
                    DatabaseEntries = entriesInDb, DatabaseFileSize = databaseFileSize,
                    DatabaseVersion = databaseVersion, Time = DateTime.Now
                };


                return new List<MeasurementDbStats> {retValue};
            }).ValueOr(new List<MeasurementDbStats>()).ToList();
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