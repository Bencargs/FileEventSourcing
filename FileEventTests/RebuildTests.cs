using FileEvents;
using NSubstitute;
using NSubstitute.Core.Events;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileEventTests
{
    public class RebuildTests
    {
        [Test]
        public void RevertAddition()
        {
            var file = "";
            var events = new List<string>();
            var preview = new MemoryStream();
            var fileProvider = CreateFileProvider(() => file, events, preview);
            var fileWatcher = Substitute.For<IFileSystemWatcher>();
            using var target = new SourceControl(fileProvider, fileWatcher, "");

            file = "One";
            fileWatcher.Changed += CreateFileUpdatedEvent();
            file += $"Two";
            fileWatcher.Changed += CreateFileUpdatedEvent();
            file += $"Three";
            fileWatcher.Changed += CreateFileUpdatedEvent();
            target.ApplyChange(2);

            var content = Encoding.UTF8.GetString(preview.ToArray());
            Assert.AreEqual("OneTwo", content);
        }

        [Test]
        public void RevertModification()
        {
            var file = "";
            var events = new List<string>();
            var preview = new MemoryStream();

            var fileWatcher = Substitute.For<IFileSystemWatcher>();
            var fileProvider = CreateFileProvider(() => file, events, preview);
            using var target = new SourceControl(fileProvider, fileWatcher, "");

            file = $"OneTwoThree";
            fileWatcher.Changed += CreateFileUpdatedEvent();
            file = $"OneOneThree";
            fileWatcher.Changed += CreateFileUpdatedEvent();
            file = $"TwoTwoThree";
            fileWatcher.Changed += CreateFileUpdatedEvent();
            target.ApplyChange(2);

            var content = Encoding.UTF8.GetString(preview.ToArray());
            Assert.AreEqual("OneOneThree", content);
        }

        [Test]
        public void RevertDeletion()
        {
            var file = "";
            var events = new List<string>();
            var preview = new MemoryStream();

            var fileWatcher = Substitute.For<IFileSystemWatcher>();
            var fileProvider = CreateFileProvider(() => file, events, preview);
            using var target = new SourceControl(fileProvider, fileWatcher, "");

            file = $"OneTwoThree";
            fileWatcher.Changed += CreateFileUpdatedEvent();
            file = $"OneTwo";
            fileWatcher.Changed += CreateFileUpdatedEvent();
            file = $"One";
            fileWatcher.Changed += CreateFileUpdatedEvent();
            target.ApplyChange(2);

            var content = Encoding.UTF8.GetString(preview.ToArray());
            Assert.AreEqual("OneTwo", content);
        }

        private DelegateEventWrapper<FileSystemEventHandler> CreateFileUpdatedEvent() =>
            Raise.Event<FileSystemEventHandler>(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, "", ""));

        private IFileProvider CreateFileProvider(Func<string> readFile, List<string> events, Stream preview)
        {
            var fileProvider = Substitute.For<IFileProvider>();
            fileProvider.Exists(Arg.Any<string>()).Returns(true);
            fileProvider.IsEmpty(Arg.Any<string>()).Returns(false);
            fileProvider.When(x => x.AppendText(Arg.Any<string>(), Arg.Any<string>())).Do(x =>
            {
                var update = x.ArgAt<string>(1);
                events.Add(update);
            });
            fileProvider.ReadLines(Arg.Any<string>()).Returns(events);
            fileProvider.Read(Arg.Any<string>()).Returns(x =>
            {
                var fileData = readFile();
                return Encoding.UTF8.GetBytes(fileData);
            });
            fileProvider.OpenWrite(Arg.Any<string>()).Returns(x =>
            {
                return preview;
            });

            return fileProvider;
        }
    }
}
