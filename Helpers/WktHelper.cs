using System;
using System.Collections.Generic;
using System.Linq;

namespace ShpToWkt
{
    internal static class WktHelper
    {
        public static string ToWkt(this Types.Point point)
            => $"{point.Latitude} {point.Longitude}";

        public static string ToWkt(this IEnumerable<Types.Point> points)
            => string.Join(",", points.Select(ToWkt));

        public static string ToWkt(this IEnumerable<IEnumerable<Types.Point>> rings)
            => string.Join(",", rings.Select(x => $"({x.ToWkt()})"));

        public static string ToWkt(this Records.PointRecord record)
            => $"POINT ({record.X} {record.Y})";

        public static string ToWkt(this Records.PolyLineRecord record)
        {
            if (record.Parts.Length > 1)
            {
                return $"MULTILINESTRING ({record.Rings.ToWkt()})";
            }

            return $"LINESTRING ({record.Points.ToWkt()})";
        }

        public static string ToWkt(this Records.PolygonRecord record)
            => $"POLYGON ({record.Rings.ToWkt()})";

        public static string ToWkt(this Records.MultiPointRecord record)
            => $"MULTIPOINT ({record.Points.ToWkt()})";

        public static string ToWkt(this IEnumerable<Records.PointRecord> records, bool flatten = true)
            => flatten ?
                $"MULTIPOINT ({string.Join(",", records.Select(x => $"{x.X} {x.Y}"))})" :
                $"GEOMETRYCOLLECTION ({string.Join(",", records.Select(ToWkt))})";

        public static string ToWkt(this IEnumerable<Records.PolyLineRecord> records, bool flatten = true)
            => flatten ?
                $"MULTILINESTRING ({string.Join(",", records.Select(x => $"({x.Rings.ToWkt()})"))})" :
                $"GEOMETRYCOLLECTION ({string.Join(",", records.Select(ToWkt))})";

        public static string ToWkt(this IEnumerable<Records.PolygonRecord> records, bool flatten = true)
            => flatten ?
                $"MULTIPOLYGON ({string.Join(",", records.Select(x => $"({x.Rings.ToWkt()})"))})" :
                $"GEOMETRYCOLLECTION ({string.Join(",", records.Select(ToWkt))})";

        public static string ToWkt(this IEnumerable<Records.MultiPointRecord> records, bool flatten = true)
            => flatten ?
                $"MULTIPOINT ({records.SelectMany(x => x.Points).ToWkt()})" :
                $"GEOMETRYCOLLECTION ({string.Join(",", records.Select(ToWkt))})";

        public static string ToWkt(this Types.ShpFile file, bool flatten = true)
        {
            if (file.ShapeTypes.Length > 1) {
                throw new NotSupportedException("Multi shape type shp files is not supported yet!");
            }

            return file.ShapeTypes[0] switch {
                ShapeTypes.Null => "POINT EMPTY",

                ShapeTypes.Point => file.Records.OfType<Records.PointRecord>().ToWkt(flatten),
                ShapeTypes.Polyline => file.Records.OfType<Records.PolyLineRecord>().ToWkt(flatten),
                ShapeTypes.Polygon => file.Records.OfType<Records.PolygonRecord>().ToWkt(flatten),
                ShapeTypes.MultiPoint => file.Records.OfType<Records.MultiPointRecord>().ToWkt(flatten),

                _ => throw new NotSupportedException($"Shape type '{file.ShapeTypes[0]}' is not supported yet!")
            };
        }
    }
}
