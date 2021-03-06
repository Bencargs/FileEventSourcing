﻿using FileEvents;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileEventTests
{
    public class SourceControlTests
    {
        [Test]
        public void AddsFileToSourceColtrol()
        {
            var fileProvider = Substitute.For<IFileProvider>();
            fileProvider.Exists(Arg.Any<string>()).Returns(true);
            var eventSource = Substitute.For<IEventSource>();
            var watcher = Substitute.For<IFileSystemWatcher>();
            var target = new SourceControl(fileProvider, eventSource, watcher);
            
            target.Add("path");

            watcher.Received().Monitor("path");
            eventSource.Received().CreateRepository("path");
        }

        [Test]
        public async Task DeterminesDeltaOnAddition()
        {
            var readModel = "A";
            var file = "AB";
            var fileProvider = Substitute.For<IFileProvider>();
            fileProvider.Exists(Arg.Any<string>()).Returns(true);
            fileProvider.ReadAsync(Arg.Any<string>()).Returns(Encoding.UTF8.GetBytes(file).ToAsyncEnumerable());
            fileProvider.ReadAsync(Arg.Any<Stream>()).Returns(Encoding.UTF8.GetBytes(readModel).ToAsyncEnumerable());
            var eventSource = Substitute.For<IEventSource>();
            eventSource.Rebuild(Arg.Any<string>()).Returns(new Document());
            var watcher = Substitute.For<IFileSystemWatcher>();
            var target = new SourceControl(fileProvider, eventSource, watcher);
            await target.Add("path");

            watcher.Changed += Raise.Event<FileChangedEventHandler>("path");

            var actual = eventSource.ReceivedCalls().Last().GetArguments().Last();
            actual.Should().BeEquivalentTo(new UpdateEvent
            {
                Updates = new List<Changeset>
                {
                    new Changeset
                    {
                        Offset = 1,
                        Values = Encoding.UTF8.GetBytes("B").ToList()
                    }
                }
            });
        }

        [Test]
        public async Task DeterminesDeltaOnModification()
        {
            var file = "BB";
            var readModel = "AB";
            var fileProvider = Substitute.For<IFileProvider>();
            fileProvider.Exists(Arg.Any<string>()).Returns(true);
            fileProvider.ReadAsync(Arg.Any<string>()).Returns(Encoding.UTF8.GetBytes(file).ToAsyncEnumerable());
            fileProvider.ReadAsync(Arg.Any<Stream>()).Returns(Encoding.UTF8.GetBytes(readModel).ToAsyncEnumerable());
            var eventSource = Substitute.For<IEventSource>();
            eventSource.Rebuild(Arg.Any<string>()).Returns(new Document());
            var watcher = Substitute.For<IFileSystemWatcher>();
            var target = new SourceControl(fileProvider, eventSource, watcher);
            await target.Add("path");

            watcher.Changed += Raise.Event<FileChangedEventHandler>("path");

            var actual = eventSource.ReceivedCalls().Last().GetArguments().Last();
            actual.Should().BeEquivalentTo(new UpdateEvent
            {
                Updates = new List<Changeset>
                {
                    new Changeset
                    {
                        Offset = 0,
                        Values = Encoding.UTF8.GetBytes("B").ToList()
                    }
                }
            });
        }

        [Test]
        public async Task DeterminesDeltaOnMultipleModification()
        {
            var file = "AXCYZ";
            var readModel = "ABCDE";
            var fileProvider = Substitute.For<IFileProvider>();
            fileProvider.Exists(Arg.Any<string>()).Returns(true);
            fileProvider.ReadAsync(Arg.Any<string>()).Returns(Encoding.UTF8.GetBytes(file).ToAsyncEnumerable());
            fileProvider.ReadAsync(Arg.Any<Stream>()).Returns(Encoding.UTF8.GetBytes(readModel).ToAsyncEnumerable());
            var eventSource = Substitute.For<IEventSource>();
            eventSource.Rebuild(Arg.Any<string>()).Returns(new Document());
            var watcher = Substitute.For<IFileSystemWatcher>();
            var target = new SourceControl(fileProvider, eventSource, watcher);
            await target.Add("path");

            watcher.Changed += Raise.Event<FileChangedEventHandler>("path");

            var actual = eventSource.ReceivedCalls().Last().GetArguments().Last();
            actual.Should().BeEquivalentTo(new UpdateEvent
            {
                Updates = new List<Changeset>
                {
                    new Changeset
                    {
                        Offset = 1,
                        Values = Encoding.UTF8.GetBytes("X").ToList()
                    },
                    new Changeset
                    {
                        Offset = 3,
                        Values = Encoding.UTF8.GetBytes("YZ").ToList()
                    }
                }
            });
        }

        [Test]
        public async Task DeterminesDeltaOnDeletion()
        {
            var file = "A";
            var readModel = "AB";
            var fileProvider = Substitute.For<IFileProvider>();
            fileProvider.Exists(Arg.Any<string>()).Returns(true);
            fileProvider.ReadAsync(Arg.Any<string>()).Returns(Encoding.UTF8.GetBytes(file).ToAsyncEnumerable());
            fileProvider.ReadAsync(Arg.Any<Stream>()).Returns(Encoding.UTF8.GetBytes(readModel).ToAsyncEnumerable());
            var eventSource = Substitute.For<IEventSource>();
            eventSource.Rebuild(Arg.Any<string>()).Returns(new Document());
            var watcher = Substitute.For<IFileSystemWatcher>();
            var target = new SourceControl(fileProvider, eventSource, watcher);
            await target.Add("path");

            watcher.Changed += Raise.Event<FileChangedEventHandler>("path");

            var actual = eventSource.ReceivedCalls().Last().GetArguments().Last();
            actual.Should().BeEquivalentTo(new UpdateEvent
            {
                Deletion = 1
            });
        }
    }
}
