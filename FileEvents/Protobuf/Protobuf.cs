using ProtoBuf;
using System;
using System.IO;

namespace FileEvents
{
    public static class Protobuf
    {
		public static string Serialize<T>(T source)
		{
			using var ms = new MemoryStream();
			Serializer.Serialize(ms, source);
			return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
		}

		public static T Deserialize<T>(string content)
		{
			var bytes = Convert.FromBase64String(content);
			using var ms = new MemoryStream(bytes);
			return Serializer.Deserialize<T>(ms);
		}
	}
}
