using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileEvents
{
    public interface IEventSource
    {
        Task CreateRepository(string path);
        Task<Document> Rebuild(string path, int? bookmark = null);
        Task Update(string path, UpdateEvent updateEvent);
    }
}
