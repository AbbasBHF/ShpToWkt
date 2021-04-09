namespace ShpToWkt.Records
{
    public record PolyLineRecord : MultiPointRecord
    {
        public int[] Parts { get; init; }
    }
}
