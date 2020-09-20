using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace dck_pihole2influx.Logging
{
    public static class LoggingFactory<T>
    {

        public static ILogger CreateLogging()
        {
            return new LoggerConfiguration()
                .WriteTo
                .Console(theme: AnsiConsoleTheme.Code,
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
                .CreateLogger()
                .ForContext<T>();
        }
        
    }
}