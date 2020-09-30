//using FileEvents;
//using NSubstitute;
//using NSubstitute.Core.Events;
//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;

//namespace FileEventTests
//{
//    public class RebuildTests
//    {
//        [Test]
//        public void RevertAddition()
//        {
//            var fileProvider = Substitute.For<IFileProvider>();
//            fileProvider.Read().returns file as bytes
//            filprovider .eradlines.returns evenyts as IEnumerable<string>
//            fileprov .appendtext does events add line

//            var target = new EventSource(fileProvider);

//            var i = 0;
//            var evt = new UpdateEvent();
//            foreach (var b in Encoding.UTF8.GetBytes("testString"))
//            {
//                evt.WriteByte(i++, b);
//            }

//            target.Update("", evt);



//            //var file = "";
//            //var events = new List<string>();
//            //var fileWatcher = Substitute.For<IFileSystemWatcher>();
//            //using var target = CreateSourceControl(fileWatcher, () => file, events);
//            //target.Add("");

//            //file = "One";
//            //fileWatcher.Changed += CreateFileUpdatedEvent();
//            //file += $"Two";
//            //fileWatcher.Changed += CreateFileUpdatedEvent();
//            //file += $"Three";
//            //fileWatcher.Changed += CreateFileUpdatedEvent();
//            //var actual = target.Preview("", 2);

//            //var content = Encoding.UTF8.GetString(((MemoryStream)actual.Data).ToArray());
//            //Assert.AreEqual("OneTwo", content);
//        }

//        [Test]
//        public void RevertModification()
//        {
//            var file = "";
//            var events = new List<string>();
//            var fileWatcher = Substitute.For<IFileSystemWatcher>();
//            using var target = CreateSourceControl(fileWatcher, () => file, events);
//            target.Add("");

//            file = $"OneTwoThree";
//            fileWatcher.Changed += CreateFileUpdatedEvent();
//            file = $"OneOneThree";
//            fileWatcher.Changed += CreateFileUpdatedEvent();
//            file = $"TwoTwoThree";
//            fileWatcher.Changed += CreateFileUpdatedEvent();
//            var actual = target.Preview("", 2);

//            var content = Encoding.UTF8.GetString(((MemoryStream) actual.Data).ToArray());
//            Assert.AreEqual("OneOneThree", content);
//        }

//        [Test]
//        public void RevertDeletion()
//        {
//            var file = "";
//            var events = new List<string>();
//            var fileWatcher = Substitute.For<IFileSystemWatcher>();
//            using var target = CreateSourceControl(fileWatcher, () => file, events);
//            target.Add("");

//            file = $"OneTwoThree";
//            fileWatcher.Changed += CreateFileUpdatedEvent();
//            file = $"OneTwo";
//            fileWatcher.Changed += CreateFileUpdatedEvent();
//            file = $"One";
//            fileWatcher.Changed += CreateFileUpdatedEvent();
//            var actual = target.Preview("", 2);

//            var content = Encoding.UTF8.GetString(((MemoryStream)actual.Data).ToArray());
//            Assert.AreEqual("OneTwo", content);
//        }

//        private DelegateEventWrapper<FileChangedEventHandler> CreateFileUpdatedEvent() =>
//            Raise.Event<FileChangedEventHandler>("");

//        private SourceControl CreateSourceControl(
//            IFileSystemWatcher fileWatcher,
//            Func<string> readFile, 
//            List<string> events)
//        {
//            var fileProvider = CreateFileProvider(readFile, events);
            
//            var eventStore = Substitute.For<IEventSource>();
//            eventStore.Rebuild(Arg.Any<string>(), null).Returns(x =>
//            {
//                var contents = events.SelectMany(e => Encoding.UTF8.GetBytes(e).Select(b => b));
//                return new Document
//                {
//                    Data = new MemoryStream(contents.ToArray())
//                };
//            });
//            eventStore.When(x => x.Update(Arg.Any<string>(), Arg.Any<UpdateEvent>()))
//                .Do(x =>
//                {
//                    //var update = x.ArgAt<UpdateEvent>(1);
//                    //events.Add(update);
//                });
//            return new SourceControl(fileProvider, eventStore, fileWatcher);
//        }

//        private IFileProvider CreateFileProvider(Func<string> readFile, List<string> events)
//        {
//            var fileProvider = Substitute.For<IFileProvider>();
//            fileProvider.Exists(Arg.Any<string>()).Returns(true);
//            fileProvider.Read(Arg.Any<string>()).Returns(Encoding.UTF8.GetBytes(readFile()));
//            fileProvider.Read(Arg.Any<Stream>()).Returns(events.SelectMany(e => Encoding.UTF8.GetBytes(e).Select(b => b)));

//            return fileProvider;
//        }
//    }
//}
