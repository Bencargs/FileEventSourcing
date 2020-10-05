using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileEvents
{
	public class EventSource : IEventSource
	{
		private readonly IFileProvider _fileProvider;
		private readonly Dictionary<string, string> _repositories = 
			new Dictionary<string, string>();

		public EventSource(IFileProvider fileProvider)
		{
			_fileProvider = fileProvider;
		}

		public async Task CreateRepository(string path)
		{
			var eventsPath = $"{path}.events";
			_repositories[path] = eventsPath;
			
			if (!_fileProvider.Exists(eventsPath))
				_fileProvider.Create(eventsPath);

			if (_fileProvider.IsEmpty(eventsPath))
				await Initialise(path);
		}

		public async Task Update(string path, UpdateEvent updateEvent)
		{
			var eventsPath = _repositories[path];
			if (TrySerializeChanges(updateEvent, out var changes))
			{
				var compressionTask = changes.Compress();
				var fileLockTask = _fileProvider.GetFilelock(eventsPath);
				await Task.WhenAll(compressionTask, fileLockTask);
				await _fileProvider.AppendTextAsync(eventsPath, compressionTask.Result);
			}
		}

		public async Task<Document> Rebuild(string path, int? bookmark = null)
		{
			var eventsPath = _repositories[path];
			bookmark ??= int.MaxValue;

			var document = new Document { Data = new MemoryStream() };
			await _fileProvider.GetFilelock(path);
			var fileEvents = await _fileProvider.ReadLinesAsync(eventsPath);
			foreach (var line in fileEvents.Take(bookmark.Value))
			{
				var decompressed = await line.Decompress();
				var updateEvent = Protobuf.Deserialize<UpdateEvent>(decompressed);
				await document.Apply(updateEvent);
			}

			return document;
		}

		private async Task Initialise(string path)
		{
			var i = 0;
			var updateEvent = new UpdateEvent();
			await _fileProvider.GetFilelock(path);
			await foreach (var b in _fileProvider.ReadAsync(path))
			{
				updateEvent.WriteByte(i++, b);
			}
			await Update(path, updateEvent);
		}

		private bool TrySerializeChanges(UpdateEvent updateEvent, out string changes)
		{
			changes = null;
			if (!updateEvent.Updates.Any() && updateEvent.Deletion == null)
				return false;

			changes = Protobuf.Serialize(updateEvent);
			return true;
		}
	}
}
