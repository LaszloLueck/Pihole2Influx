using System.Threading.Tasks;
using dck_pihole2influx.Logging;
using dck_pihole2influx.Scheduler;
using Serilog;

namespace dck_pihole2influx
{
    class Program
    {
        private static readonly ILogger _log = LoggingFactory<Program>.CreateLogging();

        static async Task Main(string[] args)
        {
            _log.Information("starting app!");
            _log.Information("Build up the scheduler");
            ISchedulerFactory schedulerFactory = new MySchedulerFactory<SchedulerJob>("job1", "group1", "trigger1", 10);
            await schedulerFactory.BuildScheduler();
            await schedulerFactory.StartScheduler();
            await schedulerFactory.ScheduleJob();

            _log.Information("App is in running state!");
            await Task.Delay(-1);

            await schedulerFactory.ShutdownScheduler();
            System.Environment.Exit(1);
        }
    }
}