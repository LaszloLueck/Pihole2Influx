using System.Collections.Generic;
using System.Linq;
using dck_pihole2influx.Transport.InfluxDb.Measurements;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
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
        private InfluxDBClient _influxDbClientFactory;
        private WriteApi _writeApi;

        public void Connect(Configuration.Configuration configuration)
        {
            var connectionString = $"http://{configuration.InfluxDbHostOrIp}:{configuration.InfluxDbPort}";
            _influxDbClientFactory = InfluxDBClientFactory.CreateV1(connectionString, configuration.InfluxDbUserName,
                configuration.InfluxDbPassword.ToCharArray(), configuration.InfluxDbDatabaseName, "autogen");
            _writeApi = _influxDbClientFactory.GetWriteApi();
        }

        public void WriteStringRecord<T>(T stringRecord) where T : StringRecordEntry
        {
            var toProcess = $"{stringRecord.Key}={stringRecord.Value}";
            _writeApi.WriteRecord(WritePrecision.S, toProcess);
        }

        public void WriteStringRecords<T>(IEnumerable<T> stringRecords) where T: StringRecordEntry
        {
            var returnList = (from element in stringRecords select $"{element.Key}={element.Value}").ToList();
            _writeApi.WriteRecords(WritePrecision.S, returnList);
        }

        public void WritePoint(PointData point)
        {
            _writeApi.WritePoint(point);
            _writeApi.Flush();
        }
        
        public void WritePoints(List<PointData> points)
        {
            _writeApi.WritePoints(points.ToArray());
            _writeApi.Flush();
        }

        public void WriteMeasurements<T>(List<T> measurements) where T: IBaseMeasurement
        {
            _writeApi.WriteMeasurements(WritePrecision.S, measurements);
            _writeApi.Flush();
        }

        public void DisposeConnector()
        {
            _influxDbClientFactory.Dispose();
        }
        
    }
}