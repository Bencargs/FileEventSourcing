using McMaster.Extensions.CommandLineUtils;
using System.IO;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication<Program>();
            app.HelpOption("-?|--help");
            var filePathOption = app.Option<string>(
                "-f|--file",
                "Input file to track changes",
                CommandOptionType.SingleValue)
                .IsRequired();

            app.Command("quit", (command) =>
            {
                command.HelpOption("-?|--help");

                command.OnExecute(() =>
                {
                    System.Console.WriteLine("Stopped change tracking.");
                    //sourceControl.Close();
                });
            });

            app.Command("add", (command) =>
            {
                command.HelpOption("-?|--help");

                command.OnExecute(() =>
                {
                    var filepath = Path.GetFullPath(filePathOption.Value());
                    //sourceControl.Add(filepath);
                    System.Console.WriteLine($"Added {filepath} to change tracking.");
                });
            });

            app.Command("preview", (command) =>
            {
                command.HelpOption("-?|--help");
                var bookmarkOption = command.Option(
                    "-v|--version",
                    "Version of file to create preview.",
                    CommandOptionType.SingleValue);

                var outputOption = command.Option(
                    "-o|--output",
                    "Filepath of where to save version preview.",
                    CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    var filepath = Path.GetFullPath(filePathOption.Value());
                    var outputPath = Path.GetFullPath(outputOption.Value());
                    var bookmark = int.Parse(bookmarkOption.Value());
                    //var preview = sourceControl.Preview(filepath, bookmark);
                    //preview.Save(outputPath);

                    System.Console.WriteLine($"Preview created at {outputPath} for file {filepath} at version {bookmark}.");
                });
            });
            app.Execute(args);
        }
    }
}
