using System;
using System.Threading.Tasks;
using dck_pihole2influx.Logging;
using dck_pihole2influx.Scheduler;
using Quartz.Logging;
using Serilog;

namespace dck_pihole2influx
{
    class Program
    {
        private static readonly ILogger _Log = LoggingFactory<Program>.CreateLogging();
        
        static async Task Main(string[] args)
        {
            _Log.Information("starting app!");
            _Log.Information("Build up the scheduler");
            ISchedulerFactory schedulerFactory = new CustomSchedulerFactory<SchedulerJob>("job1", "group1", "trigger1", 10);
            await schedulerFactory.RunScheduler();

            _Log.Information("App is in running state!");
            await Task.Delay(-1);

            await schedulerFactory.ShutdownScheduler();
            Environment.Exit(1);
        }
    }
}