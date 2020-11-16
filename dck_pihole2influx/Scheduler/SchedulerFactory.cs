using System;
using System.Threading.Tasks;
using dck_pihole2influx.Configuration;
using dck_pihole2influx.Logging;
using Quartz;
using Quartz.Impl;

namespace dck_pihole2influx.Scheduler
{
    public class CustomSchedulerFactory<T> : ISchedulerFactory where T : class, IJob
    {
        private static readonly IMySimpleLogger Log = MySimpleLoggerImpl<CustomSchedulerFactory<T>>.GetLogger();
        private readonly string _jobName;
        private readonly string _groupName;
        private readonly string _triggerName;
        private IScheduler _scheduler;
        private readonly StdSchedulerFactory _factory;
        private readonly ConfigurationItems _configurationItems;

        public CustomSchedulerFactory(string jobName, string groupName, string triggerName, ConfigurationItems configurationItems)
        {
            Log.Info("Generate Scheduler with Values: ");
            Log.Info($"JobName: {jobName}");
            Log.Info($"GroupName: {groupName}");
            Log.Info($"TriggerName: {triggerName}");
            Log.Info($"RepeatInterval: {configurationItems.PiholeHost} s");
            _jobName = jobName;
            _groupName = groupName;
            _triggerName = triggerName;
            _configurationItems = configurationItems;
            _factory = new StdSchedulerFactory();
        }

        public async Task RunScheduler()
        {
            Log.Info("Initialize the scheduler.");
            await BuildScheduler();
            await StartScheduler();
            await ScheduleJob();
        }

        private async Task BuildScheduler()
        {
            Log.Info("Build Scheduler");
            _scheduler = await _factory.GetScheduler();
        }

        private IJobDetail GetJob()
        {
            Log.Info("Get Job");
            return JobBuilder
                .Create<T>()
                .WithIdentity(_jobName, _groupName)
                .Build();
        }

        private ITrigger GetTrigger()
        {
            Log.Info("Get Trigger");
            var dto = new DateTimeOffset(DateTime.Now).AddSeconds(5);
            Log.Info($"current time: {new DateTimeOffset(DateTime.Now)}");
            Log.Info($"trigger start: {dto}");
            return TriggerBuilder
                .Create()
                .WithIdentity(_triggerName, _groupName)
                .StartAt(dto)
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(_configurationItems.RunsEvery).RepeatForever())
                .Build();
        }

        private async Task StartScheduler()
        {
            Log.Info("Start Scheduler");
            await _scheduler.Start();
        }

        private async Task ScheduleJob()
        {
            var job = GetJob();
            var trigger = GetTrigger();
            job.JobDataMap.Put("configuration", _configurationItems);
            Log.Info("Schedule Job");
            await _scheduler.ScheduleJob(job, trigger);
        }

        public async Task ShutdownScheduler()
        {
            Log.Info("Shutdown Scheduler");
            await _scheduler.Shutdown();
        }
    }
}