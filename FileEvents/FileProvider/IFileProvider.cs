using System.Collections.Generic;
using System.IO;

namespace FileEvents
{
    public interface IFileProvider
    {
        void Copy(string source, string dest);
        bool Exists(string path);
        bool IsEmpty(string path);
        void GetFilelock(string path);
        void Create(string path);
        IEnumerable<byte> Read(string path);
        IEnumerable<byte> Read(Stream stream);
        Stream OpenWrite(string path);
        void AppendText(string path, string line);
        string GetDirectoryName(string path);
        string GetFileName(string path);
        IEnumerable<string> ReadLines(string path);
    }
}
