using ProtoBuf;
using System.Collections.Generic;

namespace FileEvents
{
    [ProtoContract]
    public class UpdateEvent
    {
        [ProtoMember(1)]
        public List<Changeset> Updates { get; set; } = new List<Changeset>();
        [ProtoMember(2)]
        public long? Deletion { get; set; }
    }
}
