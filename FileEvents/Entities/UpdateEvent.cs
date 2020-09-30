using ProtoBuf;
using System.Collections.Generic;
using System.Linq;

namespace FileEvents
{
    [ProtoContract]
    public class UpdateEvent
    {
        [ProtoMember(1)]
        public List<Changeset> Updates { get; set; } = new List<Changeset>();
        [ProtoMember(2)]
        public long? Deletion { get; set; }

        public void WriteByte(int offset, byte value)
        {
            // If contigious region, append to previous changeset
            var latest = Updates.LastOrDefault();
            if (latest != null && IsContigious(offset, latest))
            {
                latest.Values.Add(value);
                return;
            }

            Updates.Add(new Changeset(offset, value));
        }

        private bool IsContigious(int offset, Changeset latest) => offset == latest.Offset + latest.Values.Count;

    }
}
