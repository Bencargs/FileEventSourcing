using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileEvents
{
    public class WindowsFileProvider : IFileProvider
    {
        public async Task AppendTextAsync(string path, string line) =>
            await File.AppendAllLinesAsync(path, new[] { line });
        
        public void Create(string path) =>
            File.Create(path).Close();

        public bool Exists(string path) =>
            File.Exists(path);

        public string GetDirectoryName(string path) =>
            Path.GetDirectoryName(path);

        public string GetFileName(string path) =>
            Path.GetFileName(path);

        public bool IsEmpty(string path) =>
            new FileInfo(path).Length == 0;

        public async IAsyncEnumerable<byte> ReadAsync(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            await foreach (var b in ReadAsync(stream))
            {
                yield return b;
            }
        }

        public async IAsyncEnumerable<byte> ReadAsync(Stream stream)
        {
            int bytesRead;
            var buffer = new byte[Constants.FileBufferSize];
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                foreach (var b in buffer.Take(bytesRead))
                {
                    yield return b;
                }
            }
        }

        public async Task<string[]> ReadLinesAsync(string path)
            => await File.ReadAllLinesAsync(path);

        public async Task GetFilelock(string path)
        {
            var file = new FileInfo(path);
            while (IsFileLocked(file))
            {
                await Task.Delay(Constants.FileLockTimeout);
            }
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
