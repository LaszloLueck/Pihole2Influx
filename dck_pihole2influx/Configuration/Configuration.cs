namespace dck_pihole2influx.Configuration
{
    public class Configuration
    {
        public const string DefaultPiholeHostOrIp = "127.0.0.1";
        public const int DefaultPiholePort = 4711;
        public const string DefaultInfluxDbHostOrIp = "127.0.0.1";
        public const int DefaultInfluxDbPort = 8086;
        public const string DefaultInfluxDbDatabaseName = "pihole2influx";
        public const string DefaultInfluxDbUserName = "";
        public const string DefaultInfluxDbPassword = "";
        public const string DefaultPiholeUser = "";
        public const string DefaultPiholePassword = "";
        public const int DefaultConcurrentRequestsToPihole = 1;
        public const int DefaultRunsEvery = 60;

        public readonly string PiholeHostOrIp;
        public readonly int PiholeTelnetPort;
        public readonly string InfluxDbHostOrIp;
        public readonly int InfluxDbPort;
        public readonly string InfluxDbDatabaseName;
        public readonly string InfluxDbUserName;
        public readonly string InfluxDbPassword;
        public readonly string PiholeUser;
        public readonly string PiholePassword;
        public readonly int ConcurrentRequestsToPihole;
        public readonly int RunsEvery;

        public Configuration(string piholeHostOrIp, int piholeTelnetPort, string influxDbHostOrIp, int influxDbPort, string influxDbDatabaseName, string influxDbUserName, string influxDbPassword, string piholeUser, string piholePassword, int concurrentRequestsToPihole, int runsEvery)
        {
            PiholeHostOrIp = piholeHostOrIp;
            PiholeTelnetPort = piholeTelnetPort;
            InfluxDbHostOrIp = influxDbHostOrIp;
            InfluxDbPort = influxDbPort;
            InfluxDbDatabaseName = influxDbDatabaseName;
            InfluxDbUserName = influxDbUserName;
            InfluxDbPassword = influxDbPassword;
            PiholeUser = piholeUser;
            PiholePassword = piholePassword;
            ConcurrentRequestsToPihole = concurrentRequestsToPihole;
            RunsEvery = runsEvery;
        }
    }
}