using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ShpToWkt
{
    internal static class WkbHelper
    {
        public static async Task Write(this Stream stream, Types.RecordHeader header, int count = 1)
        {
            stream.Write((byte)WkbByteOrder.BigEndian);

            var type = header.Type switch
            {
                ShapeTypes.Point => WkbShapeTypes.Point,
                ShapeTypes.Polyline => count == 1 ? WkbShapeTypes.LineString : WkbShapeTypes.MultiLineString,
                ShapeTypes.Polygon => WkbShapeTypes.Polygon,
                ShapeTypes.MultiPoint => WkbShapeTypes.MultiPoint,

                _ => throw new NotSupportedException($"Shape type '{header.Type}' is not supported yet!")
            };
            await stream.Write((uint)type);
        }

        public static async Task Write(this Stream stream, Types.Point point)
        {
            await stream.Write(point.Latitude);
            await stream.Write(point.Longitude);
        }

        public static async Task Write(this Stream stream, IEnumerable<Types.Point> ring)
        {
            await stream.Write(Convert.ToUInt32(ring.Count()));

            foreach (var item in ring)
            {
                await stream.Write(item);
            }
        }

        public static async Task Write(this Stream stream, Records.PointRecord record)
        {
            await stream.Write(record.Header);
            await stream.Write(record.X);
            await stream.Write(record.Y);
        }

        public static async Task Write(this Stream stream, Records.PolyLineRecord record)
        {
            await stream.Write(record.Header, record.Parts.Length);

            if (record.Parts.Length > 1)
            {
                foreach (var item in record.Rings)
                {
                    stream.Write((byte)WkbByteOrder.BigEndian);
                    await stream.Write((uint)WkbShapeTypes.LineString);
                    await stream.Write(item);
                }
            }
            else
            {
                await stream.Write(record.Points);
            }
        }

        public static async Task Write(this Stream stream, Records.PolygonRecord record)
        {
            await stream.Write(record.Header);
            await stream.Write((uint)record.Parts.Length);

            foreach (var item in record.Rings)
            {
                await stream.Write(item);
            }
        }

        public static async Task Write(this Stream stream, Records.MultiPointRecord record)
        {
            await stream.Write(record.Header);
            await stream.Write((uint)record.Points.Length);

            foreach (var item in record.Points)
            {
                stream.Write((byte)WkbByteOrder.BigEndian);
                await stream.Write((uint)WkbShapeTypes.Point);
                await stream.Write(item);
            }
        }
    }
}
