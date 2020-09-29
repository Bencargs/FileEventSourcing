using System.Collections.Generic;
using System.Linq;

namespace FileEvents
{
	public class EventStore
	{
		private readonly string _path;
		private readonly IFileProvider _fileProvider;
		private readonly List<Changeset> _updates = new List<Changeset>();
		private long? _deletion;

		public EventStore(IFileProvider fileProvider, string path)
		{
			_fileProvider = fileProvider;
			_path = CreateEventDatabase(path);
		}

		public void WriteByte(int offset, byte value)
		{
			var latest = _updates.LastOrDefault();

			// If contigious region, append to previous changeset
			if (latest != null && IsContigious(offset, latest))
			{
				latest.Values.Add(value);
				return;
			}

			_updates.Add(new Changeset(offset, value));
		}

		public void Truncate(int offset)
		{
			_deletion = offset;
		}

		public void Save()
		{
			if (TrySerializeChanges(out var changes))
			{
				_fileProvider.GetFilelock(_path);
				_fileProvider.AppendText(_path, changes);

				_updates.Clear();
				_deletion = null;
			}
		}

		public bool IsEmpty => _fileProvider.IsEmpty(_path);

		public IEnumerable<UpdateEvent> GetChangesets(int maxSequence)
		{
			foreach (var line in _fileProvider.ReadLines(_path).Take(maxSequence))
			{
				var changeset = Protobuf.Deserialize<UpdateEvent>(line);
				yield return changeset;
			}
		}

		private string CreateEventDatabase(string path)
		{
			var eventsDatabaseFilename = EventDatabase(path);
			if (!_fileProvider.Exists(eventsDatabaseFilename))
				_fileProvider.Create(eventsDatabaseFilename);

			return eventsDatabaseFilename;
		}

		private bool TrySerializeChanges(out string changes)
		{
			changes = null;
			if (!_updates.Any() && _deletion == null)
				return false;

			var updateEvent = new UpdateEvent 
			{ 
				Updates = _updates,
				Deletion = _deletion
			};
			changes = Protobuf.Serialize(updateEvent);
			return true;
		}

		private bool IsContigious(int offset, Changeset latest) => offset == latest.Offset + latest.Values.Count;

		private static string EventDatabase(string path) => $"{path}.events";
	}
}
