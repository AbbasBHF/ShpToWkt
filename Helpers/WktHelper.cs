using System;
using System.Collections.Generic;
using System.Linq;

namespace ShpToWkt
{
    internal static class WktHelper
    {
        public static string ToWkt(this Types.Point point, (ConvertTypes type, char? zoneLetter, int? zoneNumber)? convert = null)
        {
            if (convert != null)
            {
                if (convert?.type == ConvertTypes.ToLatLng)
                {
                    point = point.ToLatLng(convert?.zoneNumber ?? 40, convert?.zoneLetter ?? 'N');
                }
                else if (convert?.type == ConvertTypes.ToUtm)
                {
                    var p = point.ToUtm();
                    point = new Types.Point(p.easting, p.northing);
                }
            }

            return $"{point.Latitude} {point.Longitude}";
        }

        public static string ToWkt(this IEnumerable<Types.Point> points, (ConvertTypes type, char? zoneLetter, int? zoneNumber)? convert = null)
            => string.Join(",", points.Select(x => x.ToWkt(convert)));

        public static string ToWkt(this IEnumerable<IEnumerable<Types.Point>> rings, (ConvertTypes type, char? zoneLetter, int? zoneNumber)? convert = null)
            => string.Join(",", rings.Select(x => $"({x.ToWkt(convert)})"));

        public static string ToWkt(this Records.PointRecord record, (ConvertTypes type, char? zoneLetter, int? zoneNumber)? convert = null)
            => $"POINT ({record.Coordinate.ToWkt(convert)})";

        public static string ToWkt(this Records.PolyLineRecord record, (ConvertTypes type, char? zoneLetter, int? zoneNumber)? convert = null)
        {
            if (record.Parts.Length > 1)
            {
                return $"MULTILINESTRING ({record.Rings.ToWkt(convert)})";
            }

            return $"LINESTRING ({record.Points.ToWkt(convert)})";
        }

        public static string ToWkt(this Records.PolygonRecord record, (ConvertTypes type, char? zoneLetter, int? zoneNumber)? convert = null)
            => $"POLYGON ({record.Rings.ToWkt(convert)})";

        public static string ToWkt(this Records.MultiPointRecord record, (ConvertTypes type, char? zoneLetter, int? zoneNumber)? convert = null)
            => $"MULTIPOINT ({record.Points.ToWkt(convert)})";

        public static string ToWkt(this IEnumerable<Records.PointRecord> records, bool flatten = true, (ConvertTypes type, char? zoneLetter, int? zoneNumber)? convert = null)
            => records.Count() == 0 ?
                "MULTIPOINT EMPTY" :
                records.Count() == 1 ?
                records.ElementAt(0).ToWkt(convert) :
                flatten ?
                $"MULTIPOINT ({string.Join(",", records.Select(x => x.Coordinate.ToWkt(convert)))})" :
                $"GEOMETRYCOLLECTION ({string.Join(",", records.Select(x => x.ToWkt(convert)))})";

        public static string ToWkt(this IEnumerable<Records.PolyLineRecord> records, bool flatten = true, (ConvertTypes type, char? zoneLetter, int? zoneNumber)? convert = null)
            => records.Count() == 0 ?
                "MULTILINESTRING EMPTY" :
                records.Count() == 1 ?
                records.ElementAt(0).ToWkt(convert) :
                flatten ?
                $"MULTILINESTRING ({string.Join(",", records.Select(x => $"({x.Rings.ToWkt(convert)})"))})" :
                $"GEOMETRYCOLLECTION ({string.Join(",", records.Select(x => x.ToWkt(convert)))})";

        public static string ToWkt(this IEnumerable<Records.PolygonRecord> records, bool flatten = true, (ConvertTypes type, char? zoneLetter, int? zoneNumber)? convert = null)
            => records.Count() == 0 ?
                "MULTIPOLYGON EMPTY" :
                records.Count() == 1 ?
                records.ElementAt(0).ToWkt(convert) :
                flatten ?
                $"MULTIPOLYGON ({string.Join(",", records.Select(x => $"({x.Rings.ToWkt(convert)})"))})" :
                $"GEOMETRYCOLLECTION ({string.Join(",", records.Select(x => x.ToWkt(convert)))})";

        public static string ToWkt(this IEnumerable<Records.MultiPointRecord> records, bool flatten = true, (ConvertTypes type, char? zoneLetter, int? zoneNumber)? convert = null)
            => flatten ?
                $"MULTIPOINT ({records.SelectMany(x => x.Points).ToWkt(convert)})" :
                $"GEOMETRYCOLLECTION ({string.Join(",", records.Select(x => x.ToWkt(convert)))})";

        public static string ToWkt(this Types.ShpFile file, bool flatten = true, (ConvertTypes type, char? zoneLetter, int? zoneNumber)? convert = null)
        {
            if (file.ShapeTypes.Length > 1)
            {
                throw new NotSupportedException("Multi shape type shp files is not supported yet!");
            }

            if (file.Records.Count() == 0 && !flatten)
            {
                return "GEOMETRYCOLLECTION EMPTY";
            }

            return file.ShapeTypes[0] switch
            {
                ShapeTypes.Null => "POINT EMPTY",

                ShapeTypes.Point => file.Records.OfType<Records.PointRecord>().ToWkt(flatten, convert),
                ShapeTypes.Polyline => file.Records.OfType<Records.PolyLineRecord>().ToWkt(flatten, convert),
                ShapeTypes.Polygon => file.Records.OfType<Records.PolygonRecord>().ToWkt(flatten, convert),
                ShapeTypes.MultiPoint => file.Records.OfType<Records.MultiPointRecord>().ToWkt(flatten, convert),

                _ => throw new NotSupportedException($"Shape type '{file.ShapeTypes[0]}' is not supported yet!")
            };
        }
    }
}
