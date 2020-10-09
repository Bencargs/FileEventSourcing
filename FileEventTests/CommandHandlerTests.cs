using FileEvents;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FileEventTests
{
    public class CommandHandlerTests
    {
        [Test]
        public async Task AddCommandHandlesValidCommand()
        {
            var sourceControl = Substitute.For<ISourceControl>();
            var target = new AddCommandHandler(sourceControl);
            dynamic command = new System.Dynamic.ExpandoObject();
            command.Type = "Add";
            command.Path = "Path";

            await HandleCommand(target, command);

            await sourceControl.Received().Add("Path");
        }

        [Test]
        public async Task AddCommandIgnoresInvalidCommand()
        {
            var sourceControl = Substitute.For<ISourceControl>();
            var target = new AddCommandHandler(sourceControl);
            dynamic command = new System.Dynamic.ExpandoObject();
            command.Type = "Invalid";

            await HandleCommand(target, command);

            await sourceControl.DidNotReceive().Add(Arg.Any<string>());
        }

        [Test]
        public async Task PreviewCommandHandlesValidCommand()
        {
            using var file = new TemporaryFile();
            var sourceControl = Substitute.For<ISourceControl>();
            sourceControl.Preview(Arg.Any<string>(), Arg.Any<int>()).Returns(new Document { Data = new MemoryStream() });
            var target = new PreviewCommandHandler(sourceControl);
            dynamic command = new System.Dynamic.ExpandoObject();
            command.Type = "Preview";
            command.Path = file.Fullname;
            command.Bookmark = "4";
            command.Destination = "Destination";

            await HandleCommand(target, command);

            await sourceControl.Received().Preview(file.Fullname, 4);
        }

        [Test]
        public async Task PreviewCommandIgnoresInvalidCommand()
        {
            var sourceControl = Substitute.For<ISourceControl>();
            var target = new PreviewCommandHandler(sourceControl);
            dynamic command = new System.Dynamic.ExpandoObject();
            command.Type = "Invalid";

            await HandleCommand(target, command);

            await sourceControl.DidNotReceive().Preview(Arg.Any<string>(), Arg.Any<int>());
        }

        private async Task HandleCommand(ICommandHandler target, dynamic command)
        {
            if (!target.CanHandle(command))
                return;

            await target.Execute(command);
        }
    }
}
