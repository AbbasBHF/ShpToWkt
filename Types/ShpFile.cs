using System.Linq;

namespace ShpToWkt.Types
{
    public record ShpFile
    {
        public int Code { get; init; }

        public int Length { get; init; }

        public int Version { get; init; }

        public ShapeTypes[] ShapeTypes
            => Records.Select(x => x.Header.Type)
                .Distinct()
                .ToArray();

        public Box BoudingBox { get; init; }

        public Records.NullRecord[] Records { get; init; }
    }
}
