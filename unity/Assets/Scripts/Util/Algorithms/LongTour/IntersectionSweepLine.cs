using Util.DataStructures.BST;
using System;
using System.Collections.Generic;
using Assets.Scripts.Util.Algorithms.LongTour;
using Util.Geometry;
using UnityEngine;

namespace Util.Algorithms.LongTour
{
    public class IntersectionSweepLine<E, T>
        where E : IntersectionSweepEvent, ISweepEvent<T>, IComparable<E>, IEquatable<E>
        where T : IComparable<T>, IEquatable<T>
    {

        public void FindIntersection(IEnumerable<LineSegment> segments)
        {
            var sweepLine = new SweepLine<IntersectionSweepEvent, IntersectionStatusItem>();
            IEnumerable<E> events = CreateEvents(segments) as IEnumerable<E>;
            sweepLine.InitializeEvents(events);
            sweepLine.VerticalSweep(HandleEvent);
            // TODO: return first intersection?
        }

        public void AddEvents(LineSegment segment, List<IntersectionSweepEvent> events)
        {
            Vector2 point1 = segment.Point1;
            Vector2 point2 = segment.Point2;

            var ev1 = new IntersectionSweepEvent(point1, null, false, false);
            var ev2 = new IntersectionSweepEvent(point2, ev1, false, false);
            ev1.OtherEvent = ev2;

            if (point1.Equals(point2))
            {
                return;
            }

            if (ev1.CompareTo(ev2) < 1)
            {
                ev1.IsStart = true;
                ev2.IsEnd = true;
            }
            else
            {
                ev1.IsEnd = true;
                ev2.IsStart = true;
            }

            events.Add(ev1);
            events.Add(ev2);
        }

        public IEnumerable<IntersectionSweepEvent> CreateEvents(IEnumerable<LineSegment> segments)
        {
            var events = new List<IntersectionSweepEvent>();
            foreach (LineSegment segment in segments)
            {
                AddEvents(segment, events);
            }
            return events;
        }

        public void HandleEvent(IBST<IntersectionSweepEvent> events, IBST<IntersectionStatusItem> status,
            IntersectionSweepEvent ev)
        {
            if (ev.IsStart)
            {
                ev.StatusItem = new IntersectionStatusItem(ev);
                status.Insert(ev.StatusItem);

                IntersectionStatusItem prev, next;
                var prevFound = status.FindNextSmallest(ev.StatusItem, out prev);
                var nextFound = status.FindNextBiggest(ev.StatusItem, out next);

                if (prevFound)
                {
                    // TODO: check for intersection
                }

                if (nextFound)
                {
                    // TODO: check for intersection
                }
            }
            else if (ev.IsEnd)
            {
                ev = ev.OtherEvent;

                IntersectionStatusItem prev, next;
                var prevFound = status.FindNextSmallest(ev.StatusItem, out prev);
                var nextFound = status.FindNextBiggest(ev.StatusItem, out next);

                status.Delete(ev.StatusItem);

                if (nextFound && prevFound)
                {
                    // TODO: check for intersection
                }
            }
            else if (ev.IsIntersection)
            {

            }
            else
            {
                throw new Exception("Invalid event type");
            }
        }
    }
}
