using System.Collections.Generic;
using System.Linq;

namespace ShpToWkt.Records
{
    public record PolyLineRecord : MultiPointRecord
    {
        public int[] Parts { get; init; }

        public IEnumerable<Types.Point[]> Rings
        {
            get
            {
                for (var i = 0; i < Parts.Length; i++)
                {
                    var from = Parts[i];
                    var to = Parts.Length > i + 1 ? Parts[i + 1] : int.MaxValue;
                    yield return Points.Skip(from).Take(to - from).ToArray();
                }
            }
        }
    }
}
