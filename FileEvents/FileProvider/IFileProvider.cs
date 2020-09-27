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
        FileStream OpenRead(string path);
        FileStream OpenWrite(string path);
        StreamWriter AppendText(string path);
        string GetDirectoryName(string path);
        string GetFileName(string path);
        IEnumerable<string> ReadLines(string path);
    }
}
