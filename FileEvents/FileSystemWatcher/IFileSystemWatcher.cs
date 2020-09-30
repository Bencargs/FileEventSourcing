using System;

namespace FileEvents
{
    public interface IFileSystemWatcher : IDisposable
    {
        event FileChangedEventHandler Changed;
        void Monitor(string path);
    }
}
