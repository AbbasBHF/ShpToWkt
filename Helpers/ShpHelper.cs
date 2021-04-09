using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ShpToWkt
{
    internal static class ShpHelper
    {
        public const double NoData = 0;

        public static async Task<Types.RecordHeader> ReadRecordHeader(this Stream stream)
            => new Types.RecordHeader
            {
                Number = await stream.ReadInt(),
                Length = await stream.ReadInt(),
                Type = (ShapeTypes)await stream.ReadInt(false)
            };

        public static async Task<Records.PointRecord> ReadPointRecord(this Stream stream, Types.RecordHeader header)
            => new Records.PointRecord
            {
                Header = header,
                X = await stream.ReadDouble(),
                Y = await stream.ReadDouble()
            };

        public static async Task<Records.MultiPointRecord> ReadMultiPointRecord(this Stream stream, Types.RecordHeader header)
            => new Records.MultiPointRecord
            {
                Header = header,
                BoundingBox = await stream.ReadBox(),
                Points = await stream.ReadPoints()
            };

        public static async Task<Records.PolyLineRecord> ReadPolyLineRecord(this Stream stream, Types.RecordHeader header)
        {
            var box = await stream.ReadBox();
            var parts = await stream.ReadInt(false);
            var points = await stream.ReadInt(false);

            return new Records.PolyLineRecord
            {
                Header = header,
                BoundingBox = box,
                Parts = await stream.ReadIntArray(parts),
                Points = await stream.ReadPoints(points)
            };
        }

        public static async Task<Records.PolygonRecord> ReadPolygonRecord(this Stream stream, Types.RecordHeader header)
        {
            var box = await stream.ReadBox();
            var parts = await stream.ReadInt(false);
            var points = await stream.ReadInt(false);

            return new Records.PolygonRecord
            {
                Header = header,
                BoundingBox = box,
                Parts = await stream.ReadIntArray(parts),
                Points = await stream.ReadPoints(points)
            };
        }

        public static async Task<Records.NullRecord> ReadRecord(this Stream stream)
        {
            var header = await stream.ReadRecordHeader();

            return header.Type switch
            {
                ShapeTypes.Null => new Records.NullRecord { Header = header },

                ShapeTypes.Point => await stream.ReadPointRecord(header),
                ShapeTypes.Polyline => await stream.ReadPolyLineRecord(header),
                ShapeTypes.Polygon => await stream.ReadPolygonRecord(header),
                ShapeTypes.MultiPoint => await stream.ReadMultiPointRecord(header),

                _ => throw new NotSupportedException($"Shape type '{header.Type}' is not supported yet!")
            };
        }

        public static async Task<Types.ShpFile> ReadShpFile(this Stream stream)
        {
            var code = await stream.ReadInt();
            stream.Seek(sizeof(int) * 5, SeekOrigin.Current);
            var length = await stream.ReadInt();
            var version = await stream.ReadInt(false);
            stream.Seek(sizeof(int), SeekOrigin.Current);
            var box = await stream.ReadBox();
            var records = new List<Records.NullRecord>();

            while (stream.Position <= stream.Length)
            {
                if (await stream.ReadDouble() != NoData)
                {
                    stream.Seek(sizeof(double) * -1, SeekOrigin.Current);
                    break;
                }
            }

            while (stream.Position <= stream.Length)
            {
                records.Add(await stream.ReadRecord());
            }

            return new Types.ShpFile
            {
                Code = code,
                Length = length,
                Version = version,
                BoudingBox = box,
                Records = records.ToArray()
            };
        }
    }
}
