using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FileEvents
{
    public class WindowsFileProvider : IFileProvider
    {
        public void AppendText(string path, string line) =>
            File.AppendAllLines(path, new[] { line });
        
        public void Create(string path) =>
            File.Create(path).Close();

        public bool Exists(string path) =>
            File.Exists(path);

        public string GetDirectoryName(string path) =>
            Path.GetDirectoryName(path);

        public void GetFilelock(string path)
        {
            var file = new FileInfo(path);
            while (IsFileLocked(file))
            {
                Thread.Sleep(Constants.FileLockTimeout);
            }
        }

        public string GetFileName(string path) =>
            Path.GetFileName(path);

        public bool IsEmpty(string path) => 
            new FileInfo(path).Length == 0;

        public IEnumerable<byte> Read(string path) =>
            File.ReadAllBytes(path);

        public IEnumerable<byte> Read(Stream stream)
        {
            int bytesRead;
            var buffer = new byte[Constants.FileBufferSize];
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                foreach (var b in buffer.Take(bytesRead))
                {
                    yield return b;
                }
            }
        }

        public IEnumerable<string> ReadLines(string path) =>
            File.ReadLines(path);

        private static bool IsFileLocked(FileInfo file)
        {
            try
            {
                using FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }
    }
}
