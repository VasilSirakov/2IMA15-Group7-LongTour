using Util.DataStructures.BST;
using System;
using System.Collections.Generic;
using Assets.Scripts.Util.Algorithms.LongTour;

namespace Util.Algorithms.LongTour
{
    public class IntersectionSweepLine<E, T>
        where E : IntersectionSweepEvent, ISweepEvent<T>, IComparable<E>, IEquatable<E>
        where T : IComparable<T>, IEquatable<T>
    {

        public void FindIntersection(IEnumerable<E> segments)
        {
            SweepLine<E, T> sweepLine = new SweepLine<E, T>();
            IEnumerable<E> events = CreateEvents(segments);
            sweepLine.InitializeEvents(events);
            sweepLine.VerticalSweep(HandleEvent);
            // TODO: return first intersection?
        }

        public IEnumerable<E> CreateEvents(IEnumerable<E> segments)
        {
            throw new NotImplementedException();
        }

        public void HandleEvent(IBST<E> events, IBST<T> status, E ev)
        {
            if (ev.IsStart) {

            } else if (ev.IsEnd) {

            } else if (ev.IsIntersection)
            {

            } else
            {
                throw new Exception("Invalid event type");
            }
        }
    }
}
