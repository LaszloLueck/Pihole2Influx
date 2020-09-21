using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using dck_pihole2influx.Logging;
using Quartz;
using Quartz.Impl;
using Serilog;

namespace dck_pihole2influx.Scheduler
{
    public class SchedulerFactory<T> : ISchedulerFactory where T: IJob
    {
        private static readonly ILogger _log = LoggingFactory<SchedulerFactory<T>>.CreateLogging();
        private readonly string _jobName;
        private readonly string _groupName;
        private readonly string _triggerName;
        private readonly int _repeatIntervalInSeconds;
        private readonly IScheduler _scheduler;
        
        public SchedulerFactory(string jobName, string groupName, string triggerName, int repeatIntervalInSeconds)
        {
            _log.Information("Generate Scheduler with Values: ");
            
            _jobName = jobName;
            _groupName = groupName;
            _triggerName = triggerName;
            _repeatIntervalInSeconds = repeatIntervalInSeconds;
            StdSchedulerFactory factory = new StdSchedulerFactory();
            _scheduler = factory.GetScheduler().Result;
        }

        public IJobDetail GetJob()
        {
            return JobBuilder
                .Create<T>()
                .WithIdentity(_jobName, _groupName)
                .Build();
        }

        public ITrigger GetTrigger()
        {
            return TriggerBuilder
                .Create()
                .WithIdentity(_triggerName, _groupName)
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(_repeatIntervalInSeconds).RepeatForever())
                .Build();
        }
        
        public async Task StartScheduler()
        {
            await _scheduler.Start();
        }

        public async Task<DateTimeOffset> ScheduleJob(IJobDetail jobDetail, ITrigger trigger)
        {
            return await _scheduler.ScheduleJob(jobDetail, trigger);
        }

        public async Task ShutdownScheduler()
        {
            await _scheduler.Shutdown();
        }
    }
}