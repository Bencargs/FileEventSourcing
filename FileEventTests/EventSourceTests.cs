using FileEvents;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileEventTests
{
    public class EventSourceTests
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

        [Test]
        public void DeterminesDeltaOnAddition()
        {
            var updateEvent = CreateUpdateEvent(new[] { (0, "ABC") });
            var fileProvider = Substitute.For<IFileProvider>();
            var target = new EventSource(fileProvider);
            target.CreateRepository("path");
            target.Update("path", updateEvent);

            var expected = Protobuf.Serialize(updateEvent).Compress();
            fileProvider.Received().AppendText("path.events", expected);
        }

        [Test]
        public void RebuildsPreviousVersion()
        {
            var file = "";
            var fileProvider = CreateFileProvider(file);
            var target = new EventSource(fileProvider);
            target.CreateRepository("path");
            target.Update("path", CreateUpdateEvent(new[] { (0, "A") }));
            target.Update("path", CreateUpdateEvent(new[] { (0, "B") }));
            target.Update("path", CreateUpdateEvent(new[] { (0, "C") }));

            var actual = target.Rebuild("path", 2);

            Assert.AreEqual(actual.Data, new MemoryStream(Encoding.UTF8.GetBytes("B")));
        }

        [Test]
        public void HandlesAddition()
        {
            var file = "";
            var fileProvider = CreateFileProvider(file);
            var target = new EventSource(fileProvider);
            target.CreateRepository("path");
            target.Update("path", CreateUpdateEvent(new[] { (0, "A") }));
            target.Update("path", CreateUpdateEvent(new[] { (1, "B") }));

            var actual = target.Rebuild("path");

            Assert.AreEqual(actual.Data, new MemoryStream(Encoding.UTF8.GetBytes("AB")));
        }

        [Test]
        public void HandlesDeletion()
        {
            var file = "";
            var fileProvider = CreateFileProvider(file);
            var target = new EventSource(fileProvider);
            target.CreateRepository("path");
            target.Update("path", CreateUpdateEvent(new[] { (0, "AB") }));
            target.Update("path", CreateUpdateEvent(new[] { (0, "B") }, 1));

            var actual = target.Rebuild("path");

            Assert.AreEqual(actual.Data, new MemoryStream(Encoding.UTF8.GetBytes("B")));
        }

        private static IFileProvider CreateFileProvider(string file)
        {
            var fileProvider = Substitute.For<IFileProvider>();
            fileProvider.When(x => x.AppendText(Arg.Any<string>(), Arg.Any<string>()))
            .Do(x =>
            {
                file += $"{x.ArgAt<string>(1)}{Environment.NewLine}";
            });
            fileProvider.ReadLines(Arg.Any<string>()).Returns(x =>
            {
                return ReadLines(file);
            });
            return fileProvider;
        }

        private static IEnumerable<string> ReadLines(string file)
        {
            using var sr = new StringReader(file);
            while (sr.Peek() > 0)
            {
                yield return sr.ReadLine();
            }
        }

        private UpdateEvent CreateUpdateEvent(IEnumerable<(int offset, string value)> changes, int? length = null) =>
            new UpdateEvent
            {
                Updates = changes.Select(x => new Changeset
                {
                    Offset = x.offset,
                    Values = Encoding.UTF8.GetBytes(x.value).ToList()
                }).ToList(),
                Deletion = length
            };
    }
}