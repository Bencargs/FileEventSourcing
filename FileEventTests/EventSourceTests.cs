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
        public async Task CreatesEventsFile()
        {
            var repository = new SqliteRepository();
            var fileProvider = new WindowsFileProvider();
            var eventStore = new EventSource(repository, fileProvider);
            using var fileWatcher = new WindowsFileSystemWatcher(fileProvider);
            using var file = new TemporaryFile();
            
            file.Append("InitialText");
            using var target = new SourceControl(fileProvider, eventStore, fileWatcher);
            await target.Add(file.Fullname);

            Assert.IsTrue(fileProvider.Exists($"{file.Fullname}.events"));
            Assert.IsFalse(fileProvider.IsEmpty($"{file.Fullname}.events"));
        }

        [Test]
        public async Task AppendsEvents()
        {
            var repository = new SqliteRepository();
            var fileProvider = new WindowsFileProvider();
            var eventStore = new EventSource(repository, fileProvider);
            using var fileWatcher = new WindowsFileSystemWatcher(fileProvider);
            using var file = new TemporaryFile();

            file.Append("InitialText");
            using var target = new SourceControl(fileProvider, eventStore, fileWatcher);
            await target.Add(file.Fullname);
            file.Append("AdditionalText");

            await Task.Delay(10000);
            var lines = await repository.ReadRecordsAsync($"{file.Fullname}.events").CountAsync();
            Assert.AreEqual(2, lines);
        }

        [Test]
        public async Task DeterminesDeltaOnAddition()
        {
            var updateEvent = CreateUpdateEvent(new[] { (0, "ABC") });
            var repository = Substitute.For<IRepository>();
            var fileProvider = Substitute.For<IFileProvider>();
            var target = new EventSource(repository, fileProvider);
            await target.CreateRepository("path");
            await target.Update("path", updateEvent);

            var expected = await Protobuf.Serialize(updateEvent).Compress();
            await repository.Received().AddRecordAsync("path.events", expected);
        }

        [Test]
        public async Task RebuildsPreviousVersion()
        {
            var file = "";
            var repository = CreateRepository(file);
            var fileProvider = Substitute.For<IFileProvider>();
            var target = new EventSource(repository, fileProvider);
            await target.CreateRepository("path");
            await target.Update("path", CreateUpdateEvent(new[] { (0, "A") }));
            await target.Update("path", CreateUpdateEvent(new[] { (0, "B") }));
            await target.Update("path", CreateUpdateEvent(new[] { (0, "C") }));

            var actual = await target.Rebuild("path", 2);

            Assert.AreEqual(actual.Data, new MemoryStream(Encoding.UTF8.GetBytes("B")));
        }

        [Test]
        public async Task HandlesAddition()
        {
            var file = "";
            var repository = CreateRepository(file);
            var fileProvider = Substitute.For<IFileProvider>();
            var target = new EventSource(repository, fileProvider);
            await target.CreateRepository("path");
            await target.Update("path", CreateUpdateEvent(new[] { (0, "A") }));
            await target.Update("path", CreateUpdateEvent(new[] { (1, "B") }));

            var actual = await target.Rebuild("path");

            Assert.AreEqual(actual.Data, new MemoryStream(Encoding.UTF8.GetBytes("AB")));
        }

        [Test]
        public async Task HandlesDeletion()
        {
            var file = "";
            var repository = CreateRepository(file);
            var fileProvider = Substitute.For<IFileProvider>();
            var target = new EventSource(repository, fileProvider);
            await target.CreateRepository("path");
            await target.Update("path", CreateUpdateEvent(new[] { (0, "AB") }));
            await target.Update("path", CreateUpdateEvent(new[] { (0, "B") }, 1));

            var actual = await target.Rebuild("path");

            Assert.AreEqual(actual.Data, new MemoryStream(Encoding.UTF8.GetBytes("B")));
        }

        private static IRepository CreateRepository(string file)
        {
            var repository = Substitute.For<IRepository>();
            repository.When(x => x.AddRecordAsync(Arg.Any<string>(), Arg.Any<string>()))
            .Do(x =>
            {
                file += $"{x.ArgAt<string>(1)}{Environment.NewLine}";
            });
            repository.ReadRecordsAsync(Arg.Any<string>()).Returns(x =>
            {
                return ReadLines(file).ToArray().ToAsyncEnumerable();
            });
            return repository;
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