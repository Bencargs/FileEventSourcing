using FileEvents;
using NUnit.Framework;
using System.Threading.Tasks;

namespace FileEventTests
{
    public class WindowsFileSystemWatcherTests
    {
        [Test]
        public async Task PreventsDuplicateEvents()
        {
            var eventCount = 0;
            using (var file = new TemporaryFile())
            {
                var fileProvider = new WindowsFileProvider();
                var target = new WindowsFileSystemWatcher(fileProvider);
                target.Monitor(file.Fullname);
                target.Changed += (e) => { eventCount++; };

                file.Append("test");
                file.Append("test");
            }
            await Task.Delay(1000); // Gives the OS enough time to dispatch the event
            Assert.AreEqual(1, eventCount);
        }

        [Test]
        public async Task AllowsMultipleEvents()
        {
            var eventCount = 0;
            using (var file = new TemporaryFile())
            {
                var fileProvider = new WindowsFileProvider();
                var target = new WindowsFileSystemWatcher(fileProvider);
                target.Monitor(file.Fullname);
                target.Changed += (e) => { eventCount++; };

                file.Append("test");
                await Task.Delay(500);
                file.Append("test");
            }
            await Task.Delay(1000); // Gives the OS enough time to dispatch the event
            Assert.AreEqual(2, eventCount);
        }
    }
}
