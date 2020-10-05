using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileEvents
{
    class Program
    {
        public class ChangeTrackingService : IHostedService
        {
            private readonly IConfiguration _configuration;
            private readonly ISourceControl _sourceControl;

            public ChangeTrackingService(
                IConfiguration configuration, 
                ISourceControl sourceControl)
            {
                _configuration = configuration;
                _sourceControl = sourceControl;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                var repositories = _configuration.GetSection("Repositories").Get<string[]>();
                foreach (var repo in repositories)
                    _sourceControl.Add(repo);

                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }

        static async Task Main(string[] args)
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

        private static IServiceProvider ConfigureServices()
        {
            var settings = LoadSettings();
            var container = new ServiceCollection()
                .AddSingleton(settings)
                .AddSingleton<IFileProvider, WindowsFileProvider>()
                .AddSingleton<IFileSystemWatcher, WindowsFileSystemWatcher>()
                .AddSingleton<IEventSource, EventSource>()
                .AddSingleton<ISourceControl, SourceControl>()
                .AddSingleton<IHostedService, ChangeTrackingService>()
                .BuildServiceProvider();

            return container;
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
