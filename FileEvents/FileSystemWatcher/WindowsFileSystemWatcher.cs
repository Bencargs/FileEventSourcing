using System;
using System.Collections.Concurrent;
using System.IO;

namespace FileEvents
{
    public class WindowsFileSystemWatcher : IFileSystemWatcher
    {
        // Windows FileSystemWatcher is raises multiple events, best practice is to wrap it in a cache
        private readonly ConcurrentDictionary<string, DateTime> _memoryCache = new ConcurrentDictionary<string, DateTime>();
        private readonly TimeSpan Timeout = Constants.FileEventTimeout;
        private FileSystemWatcher _watcher;

        public event FileSystemEventHandler Changed;

        public void Initialise(string directory, string path)
        {
            _watcher = new FileSystemWatcher(directory)
            {
                Filter = path,
                EnableRaisingEvents = true,
            };
            _watcher.Changed += OnChanged;
        }

        private bool HasFileEventFiredRecently(string filename)
        {
            var hasFileEventRecently = false;
            var currentTime = DateTime.Now;
            if (_memoryCache.TryGetValue(filename, out var lastEventTime))
            {
                var timeSinceLastEvent = currentTime - lastEventTime;
                hasFileEventRecently = timeSinceLastEvent < Timeout;
            }
            _memoryCache[filename] = currentTime;

            return hasFileEventRecently;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (HasFileEventFiredRecently(e.FullPath))
                return;

            Changed?.Invoke(this, e);
        }

        public void Dispose()
        {
            _watcher?.Dispose();
        }
    }
}
