using System;
using System.Threading.Tasks;
using dck_pihole2influx.Configuration;
using dck_pihole2influx.Logging;
using dck_pihole2influx.Transport.InfluxDb;
using dck_pihole2influx.Transport.Telnet;
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
        private readonly InfluxConnectionFactory _influxConnectionFactory;
        private readonly TelnetClientFactory _telnetClientFactory;

        public CustomSchedulerFactory(string jobName, string groupName, string triggerName, ConfigurationItems configurationItems, InfluxConnectionFactory influxConnectionFactory, TelnetClientFactory telnetClientFactory)
        {
            Task.Run(async() =>
            {
                await Log.InfoAsync("Generate Scheduler with Values: ");
                await Log.InfoAsync($"JobName: {jobName}");
                await Log.InfoAsync($"GroupName: {groupName}");
                await Log.InfoAsync($"TriggerName: {triggerName}");
                await Log.InfoAsync($"RepeatInterval: {configurationItems.RunsEvery} s");
            });
            _jobName = jobName;
            _groupName = groupName;
            _triggerName = triggerName;
            _configurationItems = configurationItems;
            _influxConnectionFactory = influxConnectionFactory;
            _telnetClientFactory = telnetClientFactory;
            _factory = new StdSchedulerFactory();
        }

        public async Task RunScheduler()
        {
            await Log.InfoAsync("Initialize the scheduler.");
            await BuildScheduler();
            await StartScheduler();
            await ScheduleJob();
        }

        private async Task BuildScheduler()
        {
            await Log.InfoAsync("Build Scheduler");
            _scheduler = await _factory.GetScheduler();
        }

        private IJobDetail GetJob()
        {
            return JobBuilder
                .Create<T>()
                .WithIdentity(_jobName, _groupName)
                .Build();
        }

        private ITrigger GetTrigger()
        {
            var dto = new DateTimeOffset(DateTime.Now).AddSeconds(5);
            return TriggerBuilder
                .Create()
                .WithIdentity(_triggerName, _groupName)
                .StartAt(dto)
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(_configurationItems.RunsEvery).RepeatForever())
                .Build();
        }

        private async Task StartScheduler()
        {
            await Log.InfoAsync("Start Scheduler");
            await _scheduler.Start();
        }

        private async Task ScheduleJob()
        {
            var job = GetJob();
            var trigger = GetTrigger();
            job.JobDataMap.Put("configuration", _configurationItems);
            job.JobDataMap.Put("influxConnectionFactory", _influxConnectionFactory);
            job.JobDataMap.Put("telnetClientFactory", _telnetClientFactory);
            await Log.InfoAsync("Schedule Job");
            await _scheduler.ScheduleJob(job, trigger);
        }

        public async Task ShutdownScheduler()
        {
            await Log.InfoAsync("Shutdown Scheduler");
            await _scheduler.Shutdown();
        }
    }
}