using System.Collections.Generic;

namespace FileEvents
{
    public interface IEventSource
    {
        void CreateRepository(string path);
        Document Rebuild(string path, int? bookmark = null);
        void Update(string path, UpdateEvent updateEvent);
    }
}
