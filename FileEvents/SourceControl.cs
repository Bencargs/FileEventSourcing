using System;
using System.IO;

namespace FileEvents
{
	public class SourceControl : ISourceControl, IDisposable
	{
		private readonly IEventSource _eventStore;
		private readonly IFileProvider _fileProvider;
		private readonly IFileSystemWatcher _watcher;

		public SourceControl(
			IFileProvider fileProvider, 
			IEventSource eventStore, 
			IFileSystemWatcher watcher)
		{
			_fileProvider = fileProvider;
			_eventStore = eventStore;
			_watcher = watcher;
			_watcher.Changed += OnChanged;
		}

		public void Add(string path)
		{
			if (!_fileProvider.Exists(path))
				throw new FileNotFoundException(path);
			
			_eventStore.CreateRepository(path);
			_watcher.Monitor(path);
		}

		public Document Preview(string path, int bookmark) =>
			_eventStore.Rebuild(path, bookmark);

		private void OnChanged(string path)
		{
			var previous = _eventStore.Rebuild(path);
			var updateEvent = Compare(path, previous);
			_eventStore.Update(path, updateEvent);
		}

		private UpdateEvent Compare(string path, Document previous)
		{
			var i = 0;
			var updateEvent = new UpdateEvent();
			using var currentData = _fileProvider.Read(path).GetEnumerator();
			using var previousData = _fileProvider.Read(previous.Data).GetEnumerator();
			while (currentData.MoveNext())
			{
				if (!previousData.MoveNext())
				{
					// Addition
					updateEvent.WriteByte(i, currentData.Current);
				}
				else if (currentData.Current != previousData.Current)
				{
					// Modification
					updateEvent.WriteByte(i, currentData.Current);
				}
				i++;
			}
			if (previousData.MoveNext())
			{
				// Deletion
				updateEvent.Deletion = i;
			}

			return updateEvent;
		}

		public void Dispose()
		{
			_watcher?.Dispose();
		}
	}
}
