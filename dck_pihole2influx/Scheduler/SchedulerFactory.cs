using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using dck_pihole2influx.Configuration;
using dck_pihole2influx.Logging;
using Quartz;
using Quartz.Impl;
using Serilog;

namespace dck_pihole2influx.Scheduler
{
    public class MySchedulerFactory<T> : ISchedulerFactory where T: class, IJob
    {
        private static readonly ILogger _log = LoggingFactory<MySchedulerFactory<T>>.CreateLogging();
        private readonly string _jobName;
        private readonly string _groupName;
        private readonly string _triggerName;
        private readonly int _repeatIntervalInSeconds;
        private IScheduler _scheduler;
        private readonly StdSchedulerFactory _factory;

        public MySchedulerFactory(string jobName, string groupName, string triggerName, int repeatIntervalInSeconds)
        {
            _log.Information("Generate Scheduler with Values: ");
            _log.Information($"JobName: {jobName}");
            _log.Information($"GroupName: {groupName}");
            _log.Information($"TriggerName: {triggerName}");
            _log.Information($"RepeatInterval: {repeatIntervalInSeconds} s");
            _jobName = jobName;
            _groupName = groupName;
            _triggerName = triggerName;
            _repeatIntervalInSeconds = repeatIntervalInSeconds;
            _factory = new StdSchedulerFactory();
        }

        public async Task BuildScheduler()
        {
            _log.Information("Build Scheduler");
            _scheduler = await _factory.GetScheduler();
        }

        private IJobDetail GetJob()
        {
            _log.Information("Get Job");
            return JobBuilder
                .Create<T>()
                .WithIdentity(_jobName, _groupName)
                .Build();
        }

        private ITrigger GetTrigger()
        {
            _log.Information("Get Trigger");
            var dto = new DateTimeOffset(DateTime.Now).AddSeconds(5);
            _log.Information($"current time: {new DateTimeOffset(DateTime.Now)}");
            _log.Information($"trigger start: {dto}");
            return TriggerBuilder
                .Create()
                .WithIdentity(_triggerName, _groupName)
                .StartAt(dto)
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(_repeatIntervalInSeconds).RepeatForever())
                .Build();
        }

        public async Task StartScheduler()
        {
            _log.Information("Start Scheduler");
            await _scheduler.Start();
        }

        public async Task ScheduleJob()
        {
            var job = GetJob();
            var trigger = GetTrigger();
            _log.Information("Schedule Job");
            await _scheduler.ScheduleJob(job, trigger);
        }

        public async Task ShutdownScheduler()
        {
            _log.Information("Shutdown Scheduler");
            await _scheduler.Shutdown();
        }
    }
}