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
                container.GetService<IFileSystemWatcher>(), 
                filename);
            while (true)
            {
                Thread.Sleep(500);
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            var container = new ServiceCollection();
            container.AddSingleton<IFileProvider>(new WindowsFileProvider());
            container.AddSingleton<IFileSystemWatcher>(new WindowsFileSystemWatcher());

            return container.BuildServiceProvider();
        }
    }
}
