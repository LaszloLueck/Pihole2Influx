using System;
using System.Threading.Tasks;
using dck_pihole2influx.Configuration;
using dck_pihole2influx.Logging;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Quartz.Impl;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;

namespace dck_pihole2influx
{
    class Test : IJob
    {
        private static readonly ILogger Log = LoggingFactory<Test>.CreateLogging();
        public async Task Execute(IJobExecutionContext context)
        {
            var t = new Task(() =>
            {
                var configuration = new ConfigurationFactory().Configuration;
                Log.Information("Use the following parameter for connections:");
                Log.Information($"Pihole host: {configuration.PiholeHostOrIp}");
                Log.Information($"Pihole telnet port: {configuration.PiholeTelnetPort}");
                Log.Information($"InfluxDb host: {configuration.InfluxDbHostOrIp}");
                Log.Information($"InfluxDb port: {configuration.InfluxDbPort}");
                Log.Information($"InfluxDb database name: {configuration.InfluxDbDatabaseName}");
                Log.Information($"InfluxDb user name: {configuration.InfluxDbUserName}");
                Log.Information(
                    $"InfluxDb password is {(configuration.InfluxDbPassword.Length == 0 ? "not set" : "set")}");
            });

            t.Start();

        }
    }
    
    
    class Program
    {
        private static readonly ILogger Log = LoggingFactory<Program>.CreateLogging();

        static async Task Main(string[] args)
        {
            Log.Information("starting app!");
            Log.Information("Build up the scheduler");
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder
                .Create<Test>()
                .WithIdentity("job1", "group1")
                .Build();

            ITrigger trigger = TriggerBuilder
                .Create()
                .WithIdentity("trigger1", "group1")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
            
            await Task.Delay(-1);
            await scheduler.Shutdown();
        }
        
    }
}