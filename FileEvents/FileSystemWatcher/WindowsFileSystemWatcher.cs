using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace FileEvents
{
    public class WindowsFileSystemWatcher : IFileSystemWatcher
    {
        // Windows FileSystemWatcher is raises multiple events, best practice is to wrap it in a cache
        private readonly Dictionary<string, FileSystemWatcher> _watchers =
            new Dictionary<string, FileSystemWatcher>();
        private readonly ConcurrentDictionary<string, DateTime> _lastEvent = 
            new ConcurrentDictionary<string, DateTime>();
        private readonly TimeSpan Timeout = Constants.FileEventTimeout;
        private readonly IFileProvider _fileProvider;

        public event FileChangedEventHandler Changed;

        public WindowsFileSystemWatcher(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public void Monitor(string path)
        {
            var directory = _fileProvider.GetDirectoryName(path);
            var file = _fileProvider.GetFileName(path);
            var watcher = new FileSystemWatcher(directory)
            {
                Filter = file,
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
            watcher.Changed += OnChanged;

            _watchers[path] = watcher;
        }

        private bool HasEventFiredRecently(string filename)
        {
            var hasFileEventRecently = false;
            var currentTime = DateTime.Now;
            if (_lastEvent.TryGetValue(filename, out var lastEventTime))
            {
                var timeSinceLastEvent = currentTime - lastEventTime;
                hasFileEventRecently = timeSinceLastEvent < Timeout;
            }
            _lastEvent[filename] = currentTime;

            return hasFileEventRecently;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (HasEventFiredRecently(e.FullPath))
                return;

            Changed?.Invoke(e.FullPath);
        }

        public void Dispose()
        {
            foreach (var w in _watchers)
            {
                w.Value?.Dispose();
            }
        }
    }
}
