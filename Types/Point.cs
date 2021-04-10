namespace ShpToWkt.Types
{
    public record Point
    {
        public Point(double longitude, double latitude)
            => (Longitude, Latitude) = (longitude, latitude);

        public double Latitude { get; init; }
        public double Longitude { get; init; }

        public static implicit operator Point((double lng, double lat) tuple)
            => new Point(tuple.lng, tuple.lat);

        public void Deconstruct(out double longitude, out double latitude)
            => (longitude, latitude) = (Longitude, Latitude);
    }
}
