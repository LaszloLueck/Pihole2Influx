using System;
using System.Collections.Generic;
using System.Linq;
using Optional;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace dck_pihole2influx.Logging
{
    public static class LoggingFactory<T> where T : class
    {

        public static ILogger CreateLogging()
        {
            return new LoggerConfiguration()
                .WriteTo
                .Console(theme: AnsiConsoleTheme.Code,
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u4}] {SourceContext} {Message:lj}{NewLine}{Exception}")
                .CreateLogger()
                .ForContext(Constants.SourceContextPropertyName, ShortenNamespaces(3));
        }

        private static string ShortenNamespaces(int shortenSize)
        {
            var x = typeof(T);
            var classPathArray = x.FullName.Some().ValueOr("")?.Split(".");
            var resString = "";
            if (classPathArray == null || classPathArray.Length <= 0) return resString;
            var lastClassName = classPathArray.TakeLast(1).Single();
            var restPathArray = classPathArray.Reverse().Skip(1);

            var resArray = restPathArray.Select(c =>
            {
                var sSize = c.Length < shortenSize ? c.Length : shortenSize; 
                return c.Substring(0, sSize);
            }).Reverse();
            resString = string.Join(".", resArray) + $".{lastClassName}";

            return resString;
        }

    }
}