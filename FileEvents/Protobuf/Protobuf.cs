using ProtoBuf;
using System.IO;

namespace FileEvents
{
    public static class Protobuf
    {
		public static byte[] Serialize<T>(T source)
		{
			using var ms = new MemoryStream();
			Serializer.Serialize(ms, source);
			return ms.ToArray();
		}

		public static T Deserialize<T>(byte[] source)
		{
			using var ms = new MemoryStream(source);
			return Serializer.Deserialize<T>(ms);
		}
	}
}
