using System.Linq;
using Optional;
using Serilog;
using Serilog.Core;
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
                .ForContext<T>();
        }
    }
}