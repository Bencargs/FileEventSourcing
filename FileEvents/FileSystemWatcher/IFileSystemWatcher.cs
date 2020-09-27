using System;
using System.IO;

namespace FileEvents
{
    public interface IFileSystemWatcher : IDisposable
    {
        void Initialise(string directory, string path);
        event FileSystemEventHandler Changed;
    }
}
