using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileEvents
{
    public interface IRepository
    {
        Task Create(string path);
        Task<bool> IsEmpty(string path);
        Task AddRecordAsync(string path, string content);
        IAsyncEnumerable<string> ReadRecordsAsync(string path);
    }
}
