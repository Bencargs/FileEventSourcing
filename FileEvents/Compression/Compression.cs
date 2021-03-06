﻿using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace FileEvents
{
    public static class Compression
    {
		public static async Task<string> Compress(this byte[] source)
		{
			var stream = new MemoryStream();
			using (var zip = new GZipStream(stream, CompressionMode.Compress, true))
			{
				await zip.WriteAsync(source, 0, source.Length);
			}

			stream.Position = 0;
			var compressedData = new byte[stream.Length];
			await stream.ReadAsync(compressedData, 0, compressedData.Length);

			var output = new byte[compressedData.Length + 4];
			Buffer.BlockCopy(compressedData, 0, output, 4, compressedData.Length);
			Buffer.BlockCopy(BitConverter.GetBytes(source.Length), 0, output, 0, 4);
			return Convert.ToBase64String(output);
		}

		public static async Task<byte[]> Decompress(this string source)
		{
			byte[] buffer = Convert.FromBase64String(source);
			using var stream = new MemoryStream();
			int dataLength = BitConverter.ToInt32(buffer, 0);
			await stream.WriteAsync(buffer, 4, buffer.Length - 4);

			var output = new byte[dataLength];

			stream.Position = 0;
			using (var zip = new GZipStream(stream, CompressionMode.Decompress))
			{
				await zip.ReadAsync(output, 0, output.Length);
			}

			return output;
		}
	}
}
