using System;
using System.IO;
using System.Threading.Tasks;

namespace ShpToWkt
{
    internal static class StreamHelper
    {
        public static async Task<byte[]> Read(this Stream stream, int length)
        {
            var buffer = new byte[length];
            await stream.ReadAsync(buffer, 0, length);

            return buffer;
        }

        public static async Task<byte[]> Read(this Stream stream, int length, bool bigEndian = true)
        {
            var buffer = await stream.Read(length);

            if (BitConverter.IsLittleEndian == bigEndian)
            {
                Array.Reverse(buffer);
            }

            return buffer;
        }

        public static async Task<int> ReadInt(this Stream stream, bool bigEndian = true)
            => BitConverter.ToInt32(await stream.Read(sizeof(int), bigEndian));

        public static async Task<double> ReadDouble(this Stream stream, bool bigEndian = false)
            => BitConverter.ToDouble(await stream.Read(sizeof(double), bigEndian));
    }
}
