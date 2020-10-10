using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileEvents
{
	public class EventSource : IEventSource
	{
		private readonly IRepository _repository;
		private readonly IFileProvider _fileProvider;
		private readonly Dictionary<string, string> _repositories = 
			new Dictionary<string, string>();

		public EventSource(IRepository repository, IFileProvider fileProvider)
		{
			_repository = repository;
			_fileProvider = fileProvider;
		}

		public async Task CreateRepository(string path)
		{
			var eventsPath = $"{path}.events";
			_repositories[path] = eventsPath;

			if (!_fileProvider.Exists(eventsPath))
				await _repository.Create(eventsPath);

			if (await _repository.IsEmpty(eventsPath))
				await Initialise(path);
		}

		public async Task Update(string path, UpdateEvent updateEvent)
		{
			var eventsPath = _repositories[path];
			if (TrySerializeChanges(updateEvent, out var changes))
			{
				var compressed = await changes.Compress();
				await _repository.AddRecordAsync(eventsPath, compressed);
			}
		}

		public async Task<Document> Rebuild(string path, int? bookmark = null)
		{
			var eventsPath = _repositories[path];
			bookmark ??= int.MaxValue;

			var document = new Document();
			await foreach (var record in _repository.ReadRecordsAsync(eventsPath).Take(bookmark.Value))
			{
				var decompressed = await record.Decompress();
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

		private bool TrySerializeChanges(UpdateEvent updateEvent, out byte[] changes)
		{
			changes = null;
			if (!updateEvent.Updates.Any() && updateEvent.Deletion == null)
				return false;

			changes = Protobuf.Serialize(updateEvent);
			return true;
		}
	}
}
