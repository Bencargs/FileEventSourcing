using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
			Rebuild(preview, sequence);
		}

		/// <summary>
		/// Preforms a projection until most recent event
		/// </summary>
		/// <returns></returns>
		private IEnumerable<byte> CreatePreviousVersion()
		{
			using var stream = new MemoryStream();
			Rebuild(stream, int.MaxValue);

			stream.Position = 0;
			for (int i = stream.ReadByte(); i != -1; i = stream.ReadByte())
				yield return (byte)i;
		}

		private void Rebuild(Stream stream, int sequence)
		{
			foreach (var change in _eventStore.GetChangesets(sequence))
			{
				foreach (var update in change.Updates)
				{
					var content = update.Values.ToArray();
					stream.Position = update.Offset;
					stream.Write(content, 0, content.Length);
				}
				if (change.Deletion != null)
				{
					stream.SetLength(change.Deletion.Value);
				}
			}
		}

		private IFileSystemWatcher CreateFileWatcher(IFileSystemWatcher watcher)
		{
			var directory = _fileProvider.GetDirectoryName(_path);
			var filename = _fileProvider.GetFileName(_path);
			watcher.Initialise(directory, filename);
			watcher.Changed += OnChanged;

			return watcher;
		}

		private EventStore CreateEventStore()
		{
			var eventStore = new EventStore(_fileProvider, _path);
			if (eventStore.IsEmpty)
			{
				var i = 0;
				using var current = ReadFile(_path).GetEnumerator();
				while (current.MoveNext())
				{
					eventStore.WriteByte(i, current.Current);
					i++;
				}
				eventStore.Save();
			}
			return eventStore;
		}

		private void OnChanged(object source, FileSystemEventArgs e)
		{
			var i = 0;
			using var current = ReadFile(e.FullPath).GetEnumerator();

			var prev = CreatePreviousVersion().ToArray();
			var test = Encoding.UTF8.GetString(prev);
			using var previous = CreatePreviousVersion().GetEnumerator();
			while (current.MoveNext())
			{
				if (!previous.MoveNext())
				{
					// Addition
					_eventStore.WriteByte(i, current.Current);
				}
				else if (current.Current != previous.Current)
				{
					// Modification
					_eventStore.WriteByte(i, current.Current);
				}
				i++;
			}
			if (previous.MoveNext())
			{
				// Deletion
				_eventStore.Truncate(i);
			}
			_eventStore.Save();
		}

		private IEnumerable<byte> ReadFile(string path)
		{
			_fileProvider.GetFilelock(path);
			return _fileProvider.Read(path);
		}

		public void Dispose()
		{
			_watcher?.Dispose();
		}
	}
}
