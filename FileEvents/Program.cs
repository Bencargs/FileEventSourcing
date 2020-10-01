using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace FileEvents
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = ConfigureServices();

            using var sourceControl = new SourceControl(
                container.GetService<IFileProvider>(),
                container.GetService<IEventSource>(),
                container.GetService<IFileSystemWatcher>());

            // todo: replace with a background service
            var filename = @"C:\Temp\test\a.txt";
            sourceControl.Add(filename);
            while (true)
            {
                Thread.Sleep(500);
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            var container = new ServiceCollection();
            container.AddSingleton<IFileProvider, WindowsFileProvider>();
            container.AddSingleton<IFileSystemWatcher, WindowsFileSystemWatcher>();
            container.AddSingleton<IEventSource, EventSource>();

            return container.BuildServiceProvider();
        }
    }
}
