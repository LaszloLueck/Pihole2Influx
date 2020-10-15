using System;
using System.Threading.Tasks;
using dck_pihole2influx.Logging;
using dck_pihole2influx.Scheduler;
using Serilog;

namespace dck_pihole2influx
{
    class Program
    {
        private static readonly ILogger Log = LoggingFactory<Program>.CreateLogging();

        protected Program()
        {
        }


        static async Task Main(string[] args)
        {
            Log.Information("starting app!");
            Log.Information("Build up the scheduler");
            ISchedulerFactory schedulerFactory =
                new CustomSchedulerFactory<SchedulerJob>("job1", "group1", "trigger1", 10);
            await schedulerFactory.RunScheduler();

            Log.Information("App is in running state!");
            await Task.Delay(-1);

            await schedulerFactory.ShutdownScheduler();
            Environment.Exit(1);
        }
    }
}