using System;
using System.IO;

namespace FileEventTests
{
    public class TemporaryFile : IDisposable
    {
        private string _filename;

        public string Fullname => _filename;
        public string Filename => Path.GetFileName(_filename);
        public string Directory => Path.GetDirectoryName(_filename);

        public TemporaryFile()
        {
            _filename = Path.GetTempFileName();
        }

        public string Read()
        {
            return File.ReadAllText(_filename);
        }

        public void Write(string text)
        {
            File.AppendAllText(_filename, text);
        }

        public void Dispose()
        {
            File.Delete(_filename);
            File.Delete($"{_filename}.events");
            File.Delete($"{_filename}.preview");
        }
    }
}
