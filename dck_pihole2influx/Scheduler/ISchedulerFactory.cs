using System;
using System.Threading.Tasks;
using Quartz;

namespace dck_pihole2influx.Scheduler
{
    public interface ISchedulerFactory
    {
        Task ShutdownScheduler();

        Task RunScheduler();
    }
}