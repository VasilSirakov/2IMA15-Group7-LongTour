using System;
using UnityEngine;
using Util.Geometry;
using Util.Math;

namespace Util.Algorithms.LongTour
{
    public class IntersectionSweepEvent : ISweepEvent<IntersectionStatusItem>, IComparable<IntersectionSweepEvent>, IEquatable<IntersectionSweepEvent>
    {
        public IntersectionSweepEvent(Vector2 point, bool isStart, bool isEnd, LineSegment segment,
            IntersectionSweepEvent otherEvent)
        {
            Point = new Vector2D(point);
            IsStart = isStart;
            IsEnd = isEnd;
            Segment = segment;
            OtherEvent = otherEvent;
        }

        public IntersectionSweepEvent(Vector2 point, bool isStart, bool isEnd, LineSegment segment,
             LineSegment otherSegment)
        {
            Point = new Vector2D(point);
            IsStart = isStart;
            IsEnd = isEnd;
            Segment = segment;
            OtherSegment = otherSegment;
        }

        public LineSegment Segment { get; set; }

        public LineSegment OtherSegment { get; set; }  // use in intersection events

        public Vector2D Point { get; set; }

        public Vector2 Pos
        {
            get { return Point.Vector2;  }
        }

        public IntersectionStatusItem StatusItem { get; set; }

        public bool IsStart { get; set; }

        public bool IsEnd{ get; set; }

        public bool IsIntersection
        {
            get { return !IsStart && !IsEnd; }
        }

        public IntersectionSweepEvent OtherEvent { get; set; }  // other endpoint event

        public bool Below(Vector2D point)
        {
            return IsStart
                ? MathUtil.SignedArea(Point, OtherEvent.Point, point) > 0
                : MathUtil.SignedArea(OtherEvent.Point, Point, point) > 0;
        }

        public int CompareTo(IntersectionSweepEvent other)
        {
            if (Equals(other))
            {
                return 0;
            }
            if (Point.x > other.Point.x)
            {
                return 1;
            }

            if (Point.x < other.Point.x)
            {
                return -1;
            }

            if (!Point.y.Equals(other.Point.y))
            {
                return Point.y > other.Point.y ? 1 : -1;
            }

            if (IsStart != other.IsStart)
            {
                return IsStart ? 1 : -1;
            }
            if (Segment != other.Segment)
            {
                return Below(other.OtherEvent.Point) ? -1 : 1;
            }
            return 0;
        }

        public bool Equals(IntersectionSweepEvent other)
        {
            return this == other;
        }
    }
}
