using System.Collections.Generic;
using System.IO;
using System.Linq;

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

		public void CreateRepository(string path)
		{
			var eventsPath = $"{path}.events";
			_repositories[path] = eventsPath;
			
			if (!_fileProvider.Exists(eventsPath))
				_fileProvider.Create(eventsPath);

			if (_fileProvider.IsEmpty(eventsPath))
				Initialise(path);
		}

		public void Update(string path, UpdateEvent updateEvent)
		{
			var eventsPath = _repositories[path];
			if (TrySerializeChanges(updateEvent, out var changes))
			{
				_fileProvider.GetFilelock(eventsPath);
				_fileProvider.AppendText(eventsPath, changes);
			}
		}

		public Document Rebuild(string path, int? bookmark = null)
		{
			var eventsPath = _repositories[path];
			bookmark ??= int.MaxValue;

			var document = new Document { Data = new MemoryStream() };
			foreach (var line in _fileProvider.ReadLines(eventsPath).Take(bookmark.Value))
			{
				var updateEvent = Protobuf.Deserialize<UpdateEvent>(line);
				document.Apply(updateEvent);
			}
			return document;
		}

		private void Initialise(string path)
		{
			var i = 0;
			var updateEvent = new UpdateEvent();
			using var source = _fileProvider.Read(path).GetEnumerator();
			while (source.MoveNext())
			{
				updateEvent.WriteByte(i, source.Current);
				i++;
			}
			Update(path, updateEvent);
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
