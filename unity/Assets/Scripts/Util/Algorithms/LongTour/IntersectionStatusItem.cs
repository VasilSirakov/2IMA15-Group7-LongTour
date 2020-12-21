using System;

namespace Assets.Scripts.Util.Algorithms.LongTour
{
    public class IntersectionStatusItem : IComparable<IntersectionStatusItem>, IEquatable<IntersectionStatusItem>
    {
        public IntersectionSweepEvent SweepEvent { get; set; }

        IntersectionStatusItem(IntersectionSweepEvent sweepEvent)
        {
            SweepEvent = sweepEvent;
        }
 
        public int CompareTo(IntersectionStatusItem other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(IntersectionStatusItem other)
        {
            throw new NotImplementedException();
        }
    }
}
