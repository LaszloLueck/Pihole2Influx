using System;
using System.Collections.Generic;
using dck_pihole2influx.Transport.InfluxDb.Measurements;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;

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
        private InfluxDBClient _influxDbClientFactory;
        private Configuration.Configuration _configuration;

        public void Connect(Configuration.Configuration configuration)
        {
            _configuration = configuration;
            var connectionString = $"http://{configuration.InfluxDbHostOrIp}:{configuration.InfluxDbPort}";
            _influxDbClientFactory = InfluxDBClientFactory.CreateV1(connectionString, configuration.InfluxDbUserName,
                configuration.InfluxDbPassword.ToCharArray(), configuration.InfluxDbDatabaseName, "autogen");
        }


        public void MeasureTopClients(List<MeasurementTopClient> topClients)
        {
            _influxDbClientFactory.GetWriteApi().WriteMeasurements(WritePrecision.S, topClients);
            _influxDbClientFactory.GetWriteApi().Flush();
        }

        public void DisposeConnector()
        {
            _influxDbClientFactory.Dispose();
        }
        
    }
}