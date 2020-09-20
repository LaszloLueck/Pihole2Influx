using System.IO;
using System.Threading.Tasks;
using dck_pihole2influx.Configuration;
using log4net;
using log4net.Config;

namespace dck_pihole2influx
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        static async Task Main(string[] args)
        {
            //BasicConfigurator.Configure();
            XmlConfigurator.Configure(new FileInfo("LoggerConfig.xml"));
            
            
            Logger.Info("starting app!");
            
            RunThings();

            await Task.CompletedTask;
        }

        private static void RunThings()
        {
            var configuration = new ConfigurationFactory().Configuration;
            Logger.Info("Use the following parameter for connections:");
            Logger.Info($"Pihole host: {configuration.PiholeHostOrIp}");
            Logger.Info($"Pihole telnet port: {configuration.PiholeTelnetPort}");
            Logger.Info($"InfluxDb host: {configuration.InfluxDbHostOrIp}");
            Logger.Info($"InfluxDb port: {configuration.InfluxDbPort}");
            Logger.Info($"InfluxDb database name: {configuration.InfluxDbDatabaseName}");
            Logger.Info($"InfluxDb user name: {configuration.InfluxDbUserName}");
            Logger.Info($"InfluxDb password is {(configuration.InfluxDbPassword.Length == 0 ? "not set" : "set")}");

            //Console.WriteLine("Use the following parameter for connections:");
            //Console.WriteLine($"Pihole host: {configuration.PiholeHostOrIp}");
            //Console.WriteLine($"Pihole telnet port: {configuration.PiholeTelnetPort}");
            //Console.WriteLine($"InfluxDb host: {configuration.InfluxDbHostOrIp}");
            //Console.WriteLine($"InfluxDb port: {configuration.InfluxDbPort}");
            //Console.WriteLine($"InfluxDb database name: {configuration.InfluxDbDatabaseName}");
            //Console.WriteLine($"InfluxDb user name: {configuration.InfluxDbUserName}");
            //Console.WriteLine("InfluxDb password is {0}",
            //    configuration.InfluxDbPassword.Length == 0 ? "not set" : "set");
        }
    }
}