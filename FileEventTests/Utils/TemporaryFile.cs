using System;
using System.IO;

namespace FileEventTests
{
    public class TemporaryFile : IDisposable
    {
        public string Fullname { get; }
        public string Filename => Path.GetFileName(Fullname);
        public string Directory => Path.GetDirectoryName(Fullname);

        public TemporaryFile()
        {
            Fullname = Path.GetTempFileName();
        }

        public string Read()
        {
            return File.ReadAllText(Fullname);
        }

        public void Write(string text)
        {
            File.WriteAllText(Fullname, text);
        }

        public void Append(string text)
        {
            File.AppendAllText(Fullname, text);
            //File.AppendAllLines(Fullname, new[] { text });
        }

        public void Dispose()
        {
            File.Delete(Fullname);
            File.Delete($"{Fullname}.events");
        }
    }
}
