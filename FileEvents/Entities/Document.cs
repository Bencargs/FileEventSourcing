using System.IO;
using System.Threading.Tasks;

namespace FileEvents
{
    public class Document
    {
        public Stream Data { get; set; } = new MemoryStream();

        public async Task Apply(UpdateEvent updateEvent)
        {
            foreach (var update in updateEvent.Updates)
            {
                var data = update.Values.ToArray();
                Data.Position = update.Offset;
                await Data.WriteAsync(data, 0, data.Length);
            }
            if (updateEvent.Deletion.HasValue)
            {
                Data.SetLength(updateEvent.Deletion.Value);
            }
        }

        public async Task Save(string path)
        {
            using var fileStream = File.Create(path);
            Data.Position = 0;
            await Data.CopyToAsync(fileStream);
        }
    }
}
