using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DdManager.Sensor.NetworkInformation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DdManager.Sensor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Any(a => String.Equals("--export", a, StringComparison.OrdinalIgnoreCase))) {
                await ExportAsync();
                return;
            }

            if (args.Length != 0) {
                PrintHelp();
                return;
            }

            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo
                .Console()
                .WriteTo
                .File(
                    @"logs\log.txt",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10485760,
                    retainedFileCountLimit: 5
                )
                .CreateLogger();

            try {
                await CreateHostBuilder(args).Build().RunAsync();
            } catch (Exception e) {
                Log.Logger.Error(e, "Windows Service encounter unexpected exception");
                throw;
            }
        }

        private static async Task ExportAsync()
        {
            ServiceProvider provider = new ServiceCollection()
                .AddLogging(c => c.AddConsole())
                .BuildServiceProvider();

            ILogger logger = provider.GetService<ILoggerFactory>().CreateLogger<Program>();
            TcpConnectionExporter exporter = new(logger);
            await exporter.ExecuteAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSerilog()
                .ConfigureServices((hostContext, services) => {
                    IConfiguration configuration = hostContext.Configuration;

                    services.Configure<SensorOptions>(configuration.GetSection("Sensor"));

                    services.AddApplicationInsightsTelemetryWorkerService();

                    services.AddSingleton<ITcpConnectionReporter, InsightsTcpConnectionReporter>();
                    services.AddSingleton<ITcpConnectionReporter, LoggerTcpConnectionReporter>();

                    services.AddHostedService<TcpConnectionSensor>();
                });

        private static void PrintHelp()
        {
            Console.WriteLine("--export".PadLeft(15) + "  To export ephemeral port connections as CSV");
            Console.WriteLine("[blank]".PadLeft(15) + "  To print ephemeral ports check every 1 min and print to console");
            Console.WriteLine("sc.exe create [<serviceName>] [binPath= <binaryPathName>]");
        }
    }
}