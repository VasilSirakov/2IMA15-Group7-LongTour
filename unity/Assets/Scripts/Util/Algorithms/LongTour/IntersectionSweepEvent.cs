using System;
using UnityEngine;
using Util.Algorithms;
using Util.Algorithms.Polygon;

namespace Assets.Scripts.Util.Algorithms.LongTour
{
    class IntersectionSweepEvent : ISweepEvent<StatusItem>, IComparable<IntersectionSweepEvent>, IEquatable<IntersectionSweepEvent>
    {
        public Vector2 Pos => throw new NotImplementedException();

        public StatusItem StatusItem => throw new NotImplementedException();

        public bool IsStart => throw new NotImplementedException();

        public bool IsEnd => throw new NotImplementedException();

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
