using System;
using System.IO;
using System.Threading.Tasks;

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
			_watcher.Changed += async (path) => await OnChanged(path);
		}

		public async Task Add(string path)
		{
			if (!_fileProvider.Exists(path))
				throw new FileNotFoundException(path);
			
			var repositoryTask = _eventStore.CreateRepository(path);
			_watcher.Monitor(path);
			await repositoryTask;
		}

		public async Task<Document> Preview(string path, int bookmark) =>
			await _eventStore.Rebuild(path, bookmark);

		private async Task OnChanged(string path)
		{
			var previous = await _eventStore.Rebuild(path);
			var updateEvent = await Compare(path, previous);
			await _eventStore.Update(path, updateEvent);
		}

		private async Task<UpdateEvent> Compare(string path, Document previous)
		{
			var i = 0;
			var updateEvent = new UpdateEvent();
			await _fileProvider.GetFilelock(path);
			await using var currentData = _fileProvider.ReadAsync(path).GetAsyncEnumerator();
			await using var previousData = _fileProvider.ReadAsync(previous.Data).GetAsyncEnumerator();
			while (await currentData.MoveNextAsync())
			{
				// Addition || Update
				if (!await previousData.MoveNextAsync() || currentData.Current != previousData.Current)
					updateEvent.WriteByte(i, currentData.Current);

				i++;
			}
			if (await previousData.MoveNextAsync())
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
