namespace ShpToWkt.Records
{
    public record PointRecord : NullRecord
    {
        public double X { get; init; }

        public double Y { get; init; }
    }
}
