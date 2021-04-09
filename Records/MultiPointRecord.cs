namespace ShpToWkt.Records
{
    public record MultiPointRecord : NullRecord
    {
        public Types.Box BoundingBox { get; init; }

        public Types.Point[] Points { get; init; }
    }
}
