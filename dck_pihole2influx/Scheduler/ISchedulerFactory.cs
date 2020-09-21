using System;
using System.Threading.Tasks;
using Quartz;

namespace dck_pihole2influx.Scheduler
{
    public interface ISchedulerFactory
    {
        Task StartScheduler();

        Task<DateTimeOffset> ScheduleJob(IJobDetail jobDetail, ITrigger trigger);

        IJobDetail GetJob();

        ITrigger GetTrigger();

        Task ShutdownScheduler();
    }
}