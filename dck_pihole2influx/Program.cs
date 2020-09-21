using System.Threading.Tasks;
using dck_pihole2influx.Logging;
using dck_pihole2influx.Scheduler;
using Quartz;
using Quartz.Impl;
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
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder
                .Create<SchedulerJob>()
                .WithIdentity("job1", "group1")
                .Build();

            ITrigger trigger = TriggerBuilder
                .Create()
                .WithIdentity("trigger1", "group1")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);

            //The app runs inside of a docker-container, so let them never finish while the container runs.
            await Task.Delay(-1);
            
            await scheduler.Shutdown();
        }
    }
}