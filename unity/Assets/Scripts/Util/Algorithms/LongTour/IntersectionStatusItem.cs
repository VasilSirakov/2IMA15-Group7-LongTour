using System;
using Util.Math;

namespace Assets.Scripts.Util.Algorithms.LongTour
{
    public class IntersectionStatusItem : IComparable<IntersectionStatusItem>, IEquatable<IntersectionStatusItem>
    {
        public IntersectionSweepEvent SweepEvent { get; set; }

        public IntersectionStatusItem(IntersectionSweepEvent sweepEvent)
        {
            SweepEvent = sweepEvent;
        }
 
        public int CompareTo(IntersectionStatusItem other)
        {
            if (Equals(other))
            {
                return 0;
            }

            IntersectionSweepEvent ev = SweepEvent;
            IntersectionSweepEvent otherEv = other.SweepEvent;

            if (ev.Equals(otherEv))
            {
                return 0;
            }

            if (MathUtil.SignedArea(ev.Point, ev.OtherEvent.Point, otherEv.Point) != 0
                || MathUtil.SignedArea(ev.Point, ev.OtherEvent.Point, otherEv.OtherEvent.Point) != 0)
            {
                if (ev.Point.Equals(otherEv.Point))
                {
                    return ev.Below(otherEv.OtherEvent.Point) ? -1 : 1;
                }

                if (ev.Point.x.Equals(otherEv.Point.x))
                {
                    return ev.Point.y < otherEv.Point.y ? -1 : 1;
                }

                if (ev.CompareTo(otherEv) == 1)
                {
                    return !otherEv.Below(ev.Point) ? -1 : 1;
                }

                return ev.Below(otherEv.Point) ? -1 : 1;
            }

            if (ev.Point.Equals(otherEv.Point) && ev.OtherEvent.Point.Equals(otherEv.OtherEvent.Point))
            {
                return 0;
            }

            return ev.CompareTo(otherEv) == 1 ? 1 : -1;
        }

        public bool Equals(IntersectionStatusItem other)
        {
            return this == other;
        }
    }
}
