using FileEvents;
using System;
using System.IO;

namespace FileEventTests
{
    public class TemporaryFile : IDisposable
    {
        public string Fullname { get; }

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
        }

        public void Dispose()
        {
            File.Delete(Fullname);
            File.Delete($"{Fullname}.events");
        }
    }
}
