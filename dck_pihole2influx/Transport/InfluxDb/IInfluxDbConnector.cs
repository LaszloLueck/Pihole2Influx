using System;
using System.Collections.Generic;
using dck_pihole2influx.Logging;
using dck_pihole2influx.Transport.InfluxDb.Measurements;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace dck_pihole2influx.Transport.InfluxDb
{
    public interface IInfluxDbConnector
    {
        InfluxConnectionFactory GetInfluxDbConnection();
    }

    public class InfluxDbConnector : IInfluxDbConnector
    {
        public InfluxConnectionFactory GetInfluxDbConnection()
        {
            return new InfluxConnectionFactory();
        }
    }

    public class InfluxConnectionFactory
    {
        private static readonly IMySimpleLogger Log = MySimpleLoggerImpl<InfluxConnectionFactory>.GetLogger();

        private InfluxDBClient _influxDbClientFactory;
        
        public void Connect(Configuration.Configuration configuration)
        {
            var connectionString = $"http://{configuration.InfluxDbHostOrIp}:{configuration.InfluxDbPort}";
            _influxDbClientFactory = InfluxDBClientFactory.CreateV1(connectionString, configuration.InfluxDbUserName,
                configuration.InfluxDbPassword.ToCharArray(), configuration.InfluxDbDatabaseName, "autogen");
            _influxDbClientFactory.GetWriteApi().EventHandler += (sender, eventArgs) =>
            {
                switch (eventArgs)
                {
                    case WriteSuccessEvent @event:
                        Log.Info(@event.LineProtocol);
                        break;
                    case WriteErrorEvent @eventError:
                        Log.Error(@eventError.Exception, "Error while writing to influxDb");
                        break;
                }
            };

        }

        public void WriteMeasurements<T>(List<T> measurements) where T:IBaseMeasurement
        {
            _influxDbClientFactory.GetWriteApi().WriteMeasurements(WritePrecision.S, measurements);
        }

        public void DisposeConnector()
        {
            _influxDbClientFactory.GetWriteApi().Flush();
            _influxDbClientFactory.Dispose();
        }
        
    }
}