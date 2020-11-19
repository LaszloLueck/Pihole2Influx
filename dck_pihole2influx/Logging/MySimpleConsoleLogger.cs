#nullable enable
using System;
using System.Threading.Tasks;

namespace dck_pihole2influx.Logging
{
    public interface IMySimpleLogger
    {
        Task InfoAsync(string? message);

        Task ErrorAsync(Exception? ex, string? message);

        Task WarningAsync(string? message);

    }


    public class MySimpleConsoleLogger<T> : IMySimpleLogger
    {

        public async Task InfoAsync(string? message)
        {
            await Console.Out.WriteLineAsync($"{DateTime.Now} :: {typeof(T).Name} : {message}");
        }

        public Task ErrorAsync(Exception? ex, string? message)
        {
            return Task.Run(async () =>
            {
                var defaultConsoleColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                await InfoAsync(message);
                await Console.Out.WriteLineAsync($"{DateTime.Now} :: {typeof(T).Name} : {ex?.Message}");
                await Console.Out.WriteLineAsync($"{DateTime.Now} :: {typeof(T).Name} : {ex?.StackTrace}");
                Console.ForegroundColor = defaultConsoleColor;
            });
        }

        public Task WarningAsync(string? message)
        {
            return Task.Run(async () =>
            {
                await InfoAsync(message);
            });
        }
    }

    public static class MySimpleLoggerImpl<T> where T : class
    {
        public static IMySimpleLogger GetLogger()
        {
            return new MySimpleConsoleLogger<T>();
        }
    }
    
    
}