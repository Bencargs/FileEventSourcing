namespace Console
{
    public class AddCommand : ICommand
    {
        public string Type { get; } = "Add";
        public string Path { get; }

        public AddCommand(string path)
        {
            Path = path;
        }
    }
}
