using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileEvents
{
    class Program
    {
        static async Task Main(string[] _)
        {
            await new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(LoadSettings())
                        .AddSingleton<IFileProvider, WindowsFileProvider>()
                        .AddSingleton<IFileSystemWatcher, WindowsFileSystemWatcher>()
                        .AddSingleton<IEventSource, EventSource>()
                        .AddSingleton<ISourceControl, SourceControl>()
                        .AddSingleton<IHostedService, ChangeTrackingService>();
                }).RunConsoleAsync();
        }

        private static IConfiguration LoadSettings()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"{AppDomain.CurrentDomain.BaseDirectory}/appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            return configuration;
        }
    }
}
