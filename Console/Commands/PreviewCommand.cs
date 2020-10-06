namespace Console
{
    public class PreviewCommand : ICommand
    {
        public string Type { get; } = "Preview";
        public string Path { get; }
        public int Bookmark { get; }
        public string Destination { get; }

        public PreviewCommand(string path, int bookmark, string destination)
        {
            Path = path;
            Bookmark = bookmark;
            Destination = destination;
        }
    }
}
