using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FileEvents
{
    public class WindowsFileProvider : IFileProvider
    {
        public StreamWriter AppendText(string path)
        {
            return File.AppendText(path);
        }

        public void Copy(string source, string dest)
        {
            File.Copy(source, dest, true);
        }

        public void Create(string path)
        {
            File.Create(path).Close();
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public void GetFilelock(string path)
        {
            var file = new FileInfo(path);
            while (IsFileLocked(file))
            {
                Thread.Sleep(Constants.FileLockTimeout);
            }
        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public bool IsEmpty(string path)
        {
            return new FileInfo(path).Length == 0;
        }

        public FileStream OpenRead(string path)
        {
            return File.OpenRead(path);
        }

        public FileStream OpenWrite(string path)
        {
            return File.OpenWrite(path);
        }

        public IEnumerable<string> ReadLines(string path)
        {
            GetFilelock(path);
            return File.ReadLines(path);
        }

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
