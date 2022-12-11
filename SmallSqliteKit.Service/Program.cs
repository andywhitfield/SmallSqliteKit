using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;
using Serilog.Events;

namespace SmallSqliteKit.Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var logPath = ".";
            if (WindowsServiceHelpers.IsWindowsService())
                logPath = AppContext.BaseDirectory;
            logPath = Path.Combine(logPath, "smallsqlitekit.service.log");

            const string logOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(WindowsServiceHelpers.IsWindowsService() ? LogEventLevel.Debug : LogEventLevel.Verbose)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: logOutputTemplate)
                .WriteTo.File(logPath, LogEventLevel.Verbose, outputTemplate: logOutputTemplate,
                    fileSizeLimitBytes: 10_000_000, rollOnFileSizeLimit: true, shared: true, flushToDiskInterval: TimeSpan.FromSeconds(1))
                .CreateLogger();

            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
                .CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                    .UseKestrel()
                    .UseStartup<Startup>()
                )
                .UseSerilog();
        }
    }
}
