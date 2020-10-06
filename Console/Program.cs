using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication<Program>();
            app.HelpOption("-?|--help");

            app.Command("add", (command) =>
            {
                command.HelpOption("-?|--help");

                var filePathOption = command.Option(
                    "-f|--file",
                    "Input file to track changes",
                    CommandOptionType.SingleValue)
                    .IsRequired();

                command.OnExecute(() =>
                {
                    var filepath = Path.GetFullPath(filePathOption.Value());
                    var command = new AddCommand(filepath);
                    SendMessage(command);
                    
                    System.Console.WriteLine($"Added {filepath} to change tracking.");
                });
            });

            app.Command("preview", (command) =>
            {
                command.HelpOption("-?|--help");

                var filePathOption = command.Option(
                    "-f|--file",
                    "Input file to track changes",
                    CommandOptionType.SingleValue)
                    .IsRequired();

                var bookmarkOption = command.Option(
                    "-v|--version",
                    "Version of file to create preview.",
                    CommandOptionType.SingleValue)
                    .IsRequired();

                var outputOption = command.Option(
                    "-o|--output",
                    "Filepath of where to save version preview.",
                    CommandOptionType.SingleValue)
                    .IsRequired();

                command.OnExecute(() =>
                {
                    var filepath = Path.GetFullPath(filePathOption.Value());
                    var outputPath = Path.GetFullPath(outputOption.Value());
                    var bookmark = int.Parse(bookmarkOption.Value());
                    var command = new PreviewCommand(filepath, bookmark, outputPath);
                    SendMessage(command);

                    System.Console.WriteLine($"Preview created at {outputPath} for file {filepath} at version {bookmark}.");
                });
            });

            app.OnExecute(() =>
            {
                System.Console.WriteLine("***FileEvents***");
            });

            app.Execute(args);
        }

        private static void SendMessage(ICommand command)
        {
            using var client = new NamedPipeClientStream("FileEvents");
            client.Connect();
            var message = JsonConvert.SerializeObject(command);
            using var writer = new StreamWriter(client);
            writer.WriteLine(message);
        }
    }
}
