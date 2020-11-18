using System.Collections.Generic;
using System.Threading.Tasks;
using dck_pihole2influx.Configuration;
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
        private WriteApiAsync _writeApiAsync;

        public void Connect(ConfigurationItems configuration)
        {
            var connectionString = $"http://{configuration.InfluxDbHost}:{configuration.InfluxDbPort}";
            _influxDbClientFactory = InfluxDBClientFactory.CreateV1(connectionString, configuration.InfluxDbUsername,
                configuration.InfluxDbPassword.ToCharArray(), configuration.InfluxDbName, "autogen");

            _writeApiAsync = _influxDbClientFactory.GetWriteApiAsync();
        }

        public Task WriteMeasurementsAsync<T>(List<T> measurements) where T : IBaseMeasurement
        {
            return _writeApiAsync.WriteMeasurementsAsync(WritePrecision.S, measurements);
        }

        public void DisposeConnector()
        {
            _influxDbClientFactory.Dispose();
        }
    }
}