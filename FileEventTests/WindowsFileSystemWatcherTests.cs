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
                var target = new WindowsFileSystemWatcher();
                target.Initialise(file.Directory, file.Filename);
                target.Changed += (o, e) => { eventCount++; };

                file.Write("test");
                file.Write("test");
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
                var target = new WindowsFileSystemWatcher();
                target.Initialise(file.Directory, file.Filename);
                target.Changed += (o, e) => { eventCount++; };

                file.Write("test");
                await Task.Delay(500);
                file.Write("test");
            }
            await Task.Delay(1000); // Gives the OS enough time to dispatch the event
            Assert.AreEqual(2, eventCount);
        }
    }
}
