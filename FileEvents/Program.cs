using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace FileEvents
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = ConfigureServices();

            var filename = @"C:\Temp\test\a.txt";
            using var sourceControl = new SourceControl(
                container.GetService<IFileProvider>(),
                container.GetService<IEventSource>(),
                container.GetService<IFileSystemWatcher>());
            sourceControl.Add(filename);
            while (true)
            {
                Thread.Sleep(500);
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            // todo: file system watcher requires a directory to initialise
            // refactor with a dictionary of watcher per directory?

            var container = new ServiceCollection();
            container.AddSingleton<IFileProvider, WindowsFileProvider>();
            container.AddSingleton<IFileSystemWatcher, WindowsFileSystemWatcher>();
            container.AddSingleton<IEventSource, EventSource>();

            return container.BuildServiceProvider();
        }
    }
}
