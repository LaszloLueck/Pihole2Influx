using System.Threading.Tasks;

namespace dck_pihole2influx.Scheduler
{
    public interface ISchedulerFactory
    {
        Task ShutdownScheduler();

        Task RunScheduler();
    }
}