using Util.DataStructures.BST;
using System;
using System.Collections.Generic;

namespace Util.Algorithms.LongTour
{
    public class IntersectionSweepLine<E, T>
        where E : ISweepEvent<T>, IComparable<E>, IEquatable<E>
        where T : IComparable<T>, IEquatable<T>
    {

        public void FindIntersection(IEnumerable<E> items)
        {
            SweepLine<E, T> sweepLine = new SweepLine<E, T>();
            // TODO: sort events
            sweepLine.InitializeEvents(items);
            sweepLine.VerticalSweep(HandleEvent);
            // TODO: return first intersection?
        }

        public void HandleEvent(IBST<E> events, IBST<T> status, E ev)
        {
            throw new NotImplementedException();
        }
    }
}
