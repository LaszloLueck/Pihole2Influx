using System;
using System.Threading.Tasks;
using dck_pihole2influx.Configuration;
using dck_pihole2influx.Logging;
using dck_pihole2influx.Scheduler;

namespace dck_pihole2influx
{
    class Program
    {
        private static readonly IMySimpleLogger Log = MySimpleLoggerImpl<Program>.GetLogger();

        protected Program()
        {
        }


        static async Task Main(string[] args)
        {
            Log.Info("starting app!");

            Log.Info("Load and build the configuration");

            IConfigurationFactory configurationFactory = new ConfigurationFactory();
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder(configurationFactory);

            var mainTask = configurationBuilder.GetConfiguration().Map(configuration => {
                Task.Run(async () => {
                Log.Info("successfully loaded configuration");
                Log.Info("Build up the scheduler");
                Log.Info($"A: {configuration.RunsEvery}");
                ISchedulerFactory schedulerFactory =
                    new CustomSchedulerFactory<SchedulerJob>("job1", "group1", "trigger1", configuration);
                await schedulerFactory.RunScheduler();
                Log.Info("App is in running state!");
                });
                return Task.Delay(-1);
            }).ValueOr(() => Task.CompletedTask);

            await Task.WhenAll(mainTask);
            Environment.Exit(1);
        }
    }
}