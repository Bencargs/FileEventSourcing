using System.IO;

namespace FileEvents
{
    public class Document
    {
        public Stream Data { get; set; }

        public void Apply(UpdateEvent updateEvent)
        {
            foreach (var update in updateEvent.Updates)
            {
                var data = update.Values.ToArray();
                Data.Position = update.Offset;
                Data.Write(data, 0, data.Length);
            }
            if (updateEvent.Deletion.HasValue)
            {
                Data.SetLength(updateEvent.Deletion.Value);
            }
        }
    }
}
