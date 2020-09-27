using ProtoBuf;
using System.Collections.Generic;

namespace FileEvents
{
	[ProtoContract]
	public class Changeset
	{
		[ProtoMember(1)]
		public int Offset { get; set; }
		[ProtoMember(2)]
		public List<byte> Values { get; set; }

		public Changeset()
		{
			Values = new List<byte>();
		}

		public Changeset(int offset, byte value)
		{
			Offset = offset;
			Values = new List<byte> { value };
		}
	}
}
