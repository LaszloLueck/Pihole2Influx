using InfluxDB.Client;

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
        public InfluxDBClient Connect(Configuration.Configuration configuration)
        {
            var connectionString = $"http://{configuration.InfluxDbHostOrIp}:{configuration.InfluxDbPort}";
            return InfluxDBClientFactory.Create(connectionString, "");
        }
    }
    
    
}