using FileEvents;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace FileEventTests
{
    public class EventFileTests
    {
        [Test]
        public void CreatesEventsFile()
        {
            var fileProvider = new WindowsFileProvider();
            var eventStore = new EventSource(fileProvider);
            using var fileWatcher = new WindowsFileSystemWatcher(fileProvider);
            using var file = new TemporaryFile();
            
            file.Append("InitialText");
            using var target = new SourceControl(fileProvider, eventStore, fileWatcher);
            target.Add(file.Fullname);

            Assert.IsTrue(fileProvider.Exists($"{file.Fullname}.events"));
            Assert.IsFalse(fileProvider.IsEmpty($"{file.Fullname}.events"));
        }

        [Test]
        public async Task AppendsEvents()
        {
            var fileProvider = new WindowsFileProvider();
            var eventStore = new EventSource(fileProvider);
            using var fileWatcher = new WindowsFileSystemWatcher(fileProvider);
            using var file = new TemporaryFile();

            file.Append("InitialText");
            using var target = new SourceControl(fileProvider, eventStore, fileWatcher);
            target.Add(file.Fullname);
            file.Append("AdditionalText");

            await Task.Delay(10000);
            var lines = fileProvider.ReadLines($"{file.Fullname}.events").Count();
            Assert.AreEqual(2, lines);
        }
    }
}