namespace ShpToWkt.Types
{
    public record RecordHeader
    {
        public int Number { get; init; }

        public int Length { get; init; }

        public ShapeTypes Type { get; init; }
    }
}
