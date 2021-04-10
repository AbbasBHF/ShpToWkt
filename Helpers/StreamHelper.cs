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

        public static async Task<int[]> ReadIntArray(this Stream stream, int count)
        {
            var res = new int[count];

            for (var i = 0; i < count; i++)
            {
                res[i] = await stream.ReadInt(false);
            }

            return res;
        }

        public static async Task<int[]> ReadIntArray(this Stream stream)
            => await stream.ReadIntArray(await stream.ReadInt(false));

        public static async Task<double> ReadDouble(this Stream stream, bool bigEndian = false)
            => BitConverter.ToDouble(await stream.Read(sizeof(double), bigEndian));

        public static async Task<Types.Point> ReadPoint(this Stream stream)
            => new Types.Point(await stream.ReadDouble(), await stream.ReadDouble());

        public static async Task<Types.Point[]> ReadPoints(this Stream stream, int count)
        {
            var res = new Types.Point[count];

            for (var i = 0; i < count; i++)
            {
                res[i] = await stream.ReadPoint();
            }

            return res;
        }

        public static async Task<Types.Point[]> ReadPoints(this Stream stream)
            => await stream.ReadPoints(await stream.ReadInt(false));

        public static async Task<Types.Box> ReadBox(this Stream stream)
            => new Types.Box(await stream.ReadPoint(), await stream.ReadPoint());

        public static async Task Write(this Stream stream, byte[] buffer, bool bigEndian)
        {
            if (BitConverter.IsLittleEndian == bigEndian)
            {
                Array.Reverse(buffer);
            }

            await stream.WriteAsync(buffer);
        }

        public static async Task Write(this Stream stream, int x, bool bigEndian = true)
            => await stream.Write(BitConverter.GetBytes(x), bigEndian);

        public static async Task Write(this Stream stream, double x, bool bigEndian = true)
            => await stream.Write(BitConverter.GetBytes(x), bigEndian);

        public static async Task Write(this Stream stream, uint x, bool bigEndian = true)
            => await stream.Write(BitConverter.GetBytes(x), bigEndian);

        public static void Write(this Stream stream, byte x)
            => stream.WriteByte(x);
    }
}
