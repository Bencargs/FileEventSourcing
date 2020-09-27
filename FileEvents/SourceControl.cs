using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileEvents
{
	public class SourceControl : IDisposable
	{
		private readonly string _path;
		private readonly IFileProvider _fileProvider;
		private readonly IFileSystemWatcher _watcher;
		private readonly EventStore _eventStore;

		public SourceControl(IFileProvider fileProvider, IFileSystemWatcher watcher, string path)
		{
			_path = path;
			_fileProvider = fileProvider;
			if (!_fileProvider.Exists(_path))
				throw new FileNotFoundException(_path);

			_eventStore = CreateEventStore();
			_watcher = CreateFileWatcher(watcher);
		}

		public void ApplyChange(int sequence)
		{
			using var preview = _fileProvider.OpenWrite($"{_path}.preview");
			foreach (var change in _eventStore.GetChangesets(sequence))
			{
				var content = change.Values.ToArray();
				preview.Write(content, change.Offset, content.Length);
			}
		}

		/// <summary>
		/// Preforms a projection until most recent event
		/// </summary>
		/// <returns></returns>
		private IEnumerable<byte> CreatePreviousVersion()
		{
			foreach (var change in _eventStore.GetChangesets(int.MaxValue))
			{
				foreach (var b in change.Values)
				{
					yield return b;
				}
			}
		}

		private EventStore CreateEventStore()
		{
			var eventStore = new EventStore(_fileProvider, _path);
			if (eventStore.IsEmpty)
			{
				var i = 0;
				using (var current = ReadFile(_path).GetEnumerator())
				{
					while (current.MoveNext())
					{
						eventStore.WriteByte(i, current.Current);
						i++;
					}
					eventStore.Save();
				}
			}
			return eventStore;
		}

		private IFileSystemWatcher CreateFileWatcher(IFileSystemWatcher watcher)
		{
			var directory = _fileProvider.GetDirectoryName(_path);
			var filename = _fileProvider.GetFileName(_path);
			watcher.Initialise(directory, filename);
			watcher.Changed += OnChanged;

			return watcher;
		}

		private void OnChanged(object source, FileSystemEventArgs e)
		{
			var i = 0;
			using var current = ReadFile(e.FullPath).GetEnumerator();
			using var previous = CreatePreviousVersion().GetEnumerator();
			while (current.MoveNext())
			{
				// Addition or Modification
				if (!previous.MoveNext() || current.Current != previous.Current)
					_eventStore.WriteByte(i, current.Current);
				i++;
			}
			while (previous.MoveNext())
			{
				// todo: Deletion
			}
			_eventStore.Save();
		}

		private IEnumerable<byte> ReadFile(string path)
		{
			int bytesRead;
			var buffer = new byte[Constants.FileBufferSize];

			_fileProvider.GetFilelock(path);
			using var stream = _fileProvider.OpenRead(path);
			while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
			{
				foreach (var b in buffer.Take(bytesRead))
				{
					yield return b;
				}
			}
		}

		public void Dispose()
		{
			_watcher?.Dispose();
		}
	}
}
