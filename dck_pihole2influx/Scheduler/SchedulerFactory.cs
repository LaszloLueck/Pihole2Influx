using System;
using System.Threading.Tasks;
using dck_pihole2influx.Logging;
using Quartz;
using Quartz.Impl;
using Serilog;

namespace dck_pihole2influx.Scheduler
{
    public class CustomSchedulerFactory<T> : ISchedulerFactory where T : class, IJob
    {
        private static readonly ILogger _Log = LoggingFactory<CustomSchedulerFactory<T>>.CreateLogging();
        private readonly string _jobName;
        private readonly string _groupName;
        private readonly string _triggerName;
        private readonly int _repeatIntervalInSeconds;
        private IScheduler _scheduler;
        private readonly StdSchedulerFactory _factory;

        public CustomSchedulerFactory(string jobName, string groupName, string triggerName, int repeatIntervalInSeconds)
        {
            _Log.Information("Generate Scheduler with Values: ");
            _Log.Information($"JobName: {jobName}");
            _Log.Information($"GroupName: {groupName}");
            _Log.Information($"TriggerName: {triggerName}");
            _Log.Information($"RepeatInterval: {repeatIntervalInSeconds} s");
            _jobName = jobName;
            _groupName = groupName;
            _triggerName = triggerName;
            _repeatIntervalInSeconds = repeatIntervalInSeconds;
            _factory = new StdSchedulerFactory();
        }

        public async Task RunScheduler()
        {
            await BuildScheduler();
            await StartScheduler();
            await ScheduleJob();
        }

        private async Task BuildScheduler()
        {
            _Log.Information("Build Scheduler");
            _scheduler = await _factory.GetScheduler();
        }

        private IJobDetail GetJob()
        {
            _Log.Information("Get Job");
            return JobBuilder
                .Create<T>()
                .WithIdentity(_jobName, _groupName)
                .Build();
        }

        private ITrigger GetTrigger()
        {
            _Log.Information("Get Trigger");
            var dto = new DateTimeOffset(DateTime.Now).AddSeconds(5);
            _Log.Information($"current time: {new DateTimeOffset(DateTime.Now)}");
            _Log.Information($"trigger start: {dto}");
            return TriggerBuilder
                .Create()
                .WithIdentity(_triggerName, _groupName)
                .StartAt(dto)
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(_repeatIntervalInSeconds).RepeatForever())
                .Build();
        }

        private async Task StartScheduler()
        {
            _Log.Information("Start Scheduler");
            await _scheduler.Start();
        }

        private async Task ScheduleJob()
        {
            var job = GetJob();
            var trigger = GetTrigger();
            _Log.Information("Schedule Job");
            await _scheduler.ScheduleJob(job, trigger);
        }

        public async Task ShutdownScheduler()
        {
            _Log.Information("Shutdown Scheduler");
            await _scheduler.Shutdown();
        }
    }
}