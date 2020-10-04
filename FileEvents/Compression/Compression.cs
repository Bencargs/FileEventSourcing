using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace FileEvents
{
    public static class Compression
    {
		public static string Compress(this string text)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(text);
			var stream = new MemoryStream();
			using (var zip = new GZipStream(stream, CompressionMode.Compress, true))
			{
				zip.Write(buffer, 0, buffer.Length);
			}

			stream.Position = 0;
			var compressedData = new byte[stream.Length];
			stream.Read(compressedData, 0, compressedData.Length);

			var output = new byte[compressedData.Length + 4];
			Buffer.BlockCopy(compressedData, 0, output, 4, compressedData.Length);
			Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, output, 0, 4);
			return Convert.ToBase64String(output);
		}

		public static string Decompress(this string source)
		{
			byte[] buffer = Convert.FromBase64String(source);
			using (var stream = new MemoryStream())
			{
				int dataLength = BitConverter.ToInt32(buffer, 0);
				stream.Write(buffer, 4, buffer.Length - 4);

				var output = new byte[dataLength];

				stream.Position = 0;
				using (var zip = new GZipStream(stream, CompressionMode.Decompress))
				{
					zip.Read(output, 0, output.Length);
				}

				return Encoding.UTF8.GetString(output);
			}
		}
	}
}
