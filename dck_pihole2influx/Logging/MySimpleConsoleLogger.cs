using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace dck_pihole2influx.Logging
{
    public interface IMySimpleLogger
    {
        void Info(string message);
        void Error(Exception ex, string message);

        void Warning(string message);

        Task InfoAsync(string message);

    }


    public class MySimpleConsoleLogger<T> : IMySimpleLogger
    {
        [Obsolete("The synchronous version of Info is deprecated, please use the asynchronous InfoAsync instead")]
        public void Info(string message)
        {
            Console.WriteLine($"{DateTime.Now} :: {typeof(T).Name} : {message}");
        }

        public async Task InfoAsync(string message)
        {
            await Console.Out.WriteLineAsync(message);
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        

        public void Error(Exception ex, string message)
        {
            return Task.Factory.StartNew(static state =>
            {
                var tuple = (Tuple<Exception?, string?>) state!;
                

            }, (ex, message), CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
            
            Task.Run(async () =>
            {
                var defaultConsoleColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                await InfoAsync(message);
                await Console.Out.WriteLineAsync($"{DateTime.Now} :: {typeof(T).Name} : {ex.Message}");
                await Console.Out.WriteLineAsync($"{DateTime.Now} :: {typeof(T).Name} : {ex.StackTrace}");
                Console.ForegroundColor = defaultConsoleColor;
            });
        }

        public void Warning(string message)
        {
            var defaultConsoleColor = Console.ForegroundColor;
            Info(message);
            Console.ForegroundColor = defaultConsoleColor;
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