using System;
using UnityEngine;
using Util.Algorithms;

namespace Assets.Scripts.Util.Algorithms.LongTour
{
    public class IntersectionSweepEvent : ISweepEvent<IntersectionStatusItem>, IComparable<IntersectionSweepEvent>, IEquatable<IntersectionSweepEvent>
    {
        public IntersectionSweepEvent(Vector2 pos, bool isStart, bool isEnd)
        {
            Pos = pos;
            IsStart = isStart;
            IsEnd = isEnd;
        }
        public Vector2 Pos { get; set; }

        public IntersectionStatusItem StatusItem { get; set; }

        public bool IsStart { get; set; }

        public bool IsEnd{ get; set; }

        public bool IsIntersection
        {
            get { return !IsStart && !IsEnd; }
        }

        public int CompareTo(IntersectionSweepEvent other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(IntersectionSweepEvent other)
        {
            throw new NotImplementedException();
        }
    }
}
