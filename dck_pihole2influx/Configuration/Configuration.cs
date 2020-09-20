namespace dck_pihole2influx.Configuration
{
    public class Configuration
    {
        public static readonly string DefaultPiholeHostOrIp = "127.0.0.1";
        public static readonly int DefaultPiholePort = 4711;
        public static readonly string DefaultInfluxDbHostOrIp = "127.0.0.1";
        public static readonly int DefaultInfluxDbPort = 8086;
        public static readonly string DefaultInfluxDbDatabaseName = "influxdb";
        public static readonly string DefaultInfluxDbUserName = "";
        public static readonly string DefaultInfluxDbPassword = "";

        public readonly string PiholeHostOrIp;
        public readonly int PiholeTelnetPort;
        public readonly string InfluxDbHostOrIp;
        public readonly int InfluxDbPort;
        public readonly string InfluxDbDatabaseName;
        public readonly string InfluxDbUserName;
        public readonly string InfluxDbPassword;

        public Configuration(string piholeHostOrIp, int piholeTelnetPort, string influxDbHostOrIp, int influxDbPort, string influxDbDatabaseName, string influxDbUserName, string influxDbPassword)
        {
            PiholeHostOrIp = piholeHostOrIp;
            PiholeTelnetPort = piholeTelnetPort;
            InfluxDbHostOrIp = influxDbHostOrIp;
            InfluxDbPort = influxDbPort;
            InfluxDbDatabaseName = influxDbDatabaseName;
            InfluxDbUserName = influxDbUserName;
            InfluxDbPassword = influxDbPassword;
        }
    }
}