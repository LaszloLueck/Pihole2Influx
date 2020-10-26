using System;

namespace dck_pihole2influx.Logging
{
    public interface IMySimpleLogger
    {
        void Info(string message);
        void Error(Exception ex, string message);

        void Warning(string message);

    }


    public class MySimpleConsoleLogger<T> : IMySimpleLogger
    {
        public void Info(string message)
        {
            Console.WriteLine($"{DateTime.Now} :: {typeof(T).Name} : {message}");
        }

        public void Error(Exception ex, string message)
        {
            var defaultConsoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Info(message);
            Console.WriteLine($"{DateTime.Now} :: {typeof(T).Name} : {ex.Message}");
            Console.WriteLine($"{DateTime.Now} :: {typeof(T).Name} : {ex.StackTrace}");
            Console.ForegroundColor = defaultConsoleColor;
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