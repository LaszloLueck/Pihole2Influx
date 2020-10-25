using System;
using System.Threading.Tasks;
using dck_pihole2influx.Logging;
using dck_pihole2influx.Scheduler;

namespace dck_pihole2influx
{
    class Program
    {
        private static readonly IMySimpleLogger Log = MySimpleLoggerImpl<Program>.GetLogger();

        protected Program()
        {
        }


        static async Task Main(string[] args)
        {
            Log.Info("starting app!");
            Log.Info("Build up the scheduler");
            ISchedulerFactory schedulerFactory =
                new CustomSchedulerFactory<SchedulerJob>("job1", "group1", "trigger1", 10);
            await schedulerFactory.RunScheduler();

            Log.Info("App is in running state!");
            await Task.Delay(-1);

            await schedulerFactory.ShutdownScheduler();
            Environment.Exit(1);
        }
    }
}