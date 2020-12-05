using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using dck_pihole2influx.Configuration;
using dck_pihole2influx.Logging;
using dck_pihole2influx.Transport.InfluxDb.Measurements;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Optional;

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
        private WriteApiAsync _writeApi;

        public Option<bool> Connect(ConfigurationItems configuration)
        {
            try
            {
                var connectionString = $"http://{configuration.InfluxDbHost}:{configuration.InfluxDbPort}";
                Log.Info(connectionString);
                _influxDbClientFactory = InfluxDBClientFactory.CreateV1(connectionString,
                    configuration.InfluxDbUsername,
                    configuration.InfluxDbPassword.ToCharArray(), configuration.InfluxDbName, "autogen");
                _writeApi = _influxDbClientFactory.GetWriteApiAsync();
                
                return Option.Some(true);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error while connecting to influxdb!");
                return Option.None<bool>();
            }
        }
        
        

        public Task WriteMeasurementsAsync<T>(List<T> measurements) where T : IBaseMeasurement
        {
            return Task.Run(() =>
            {
                try
                {
                    if(!_writeApi.WriteMeasurementsAsync(WritePrecision.S, measurements).Wait(500)) throw new TimeoutException("Exception while writing data to influxdb");
                }
                catch (TimeoutException webException)
                {
                    Log.Error(webException, "An error occured");
                }
            });
        }

        public void DisposeConnector()
        {
            _influxDbClientFactory.Dispose();
        }
    }
}