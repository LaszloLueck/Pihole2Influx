using System;
using System.Threading.Tasks;
using Quartz;

namespace dck_pihole2influx.Scheduler
{
    public interface ISchedulerFactory
    {
        Task StartScheduler();

        Task BuildScheduler();

        Task ScheduleJob();

        Task ShutdownScheduler();
    }
}