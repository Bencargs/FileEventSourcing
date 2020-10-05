using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FileEvents
{
    public interface IFileProvider
    {
        bool Exists(string path);
        bool IsEmpty(string path);
        void Create(string path);
        IAsyncEnumerable<byte> ReadAsync(string path);
        IAsyncEnumerable<byte> ReadAsync(Stream stream);
        Task AppendTextAsync(string path, string line);
        string GetDirectoryName(string path);
        string GetFileName(string path);
        Task GetFilelock(string path);
        Task<string[]> ReadLinesAsync(string path);
    }
}
