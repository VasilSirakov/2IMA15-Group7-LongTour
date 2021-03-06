using Util.DataStructures.BST;
using System;
using System.Collections.Generic;
using Util.Geometry;
using UnityEngine;
using Util.Geometry.Graph;

namespace Util.Algorithms.LongTour
{
    public class IntersectionSweepLine<E, T>
        where E : IntersectionSweepEvent, ISweepEvent<T>, IComparable<E>, IEquatable<E>
        where T : IComparable<T>, IEquatable<T>
    {
        public List<Edge> intersected;

        public List<LineSegment> CreateSegments(IEnumerable<Edge> edges)
        {
            var segments = new List<LineSegment>();
            foreach (Edge e in edges)
            {
                segments.Add(new LineSegment(e.Start.Pos, e.End.Pos));
            }
            return segments;
        }

        public List<Edge> FindIntersection(IEnumerable<Edge> edges)
        {
            intersected = null;
            var sweepLine = new SweepLine<IntersectionSweepEvent, IntersectionStatusItem>();
            List<LineSegment> segments = CreateSegments(edges);
            IEnumerable<E> events = CreateEvents(segments) as IEnumerable<E>;
            sweepLine.InitializeEvents(events);
            sweepLine.VerticalSweep(HandleEvent);
            return intersected;
        }

        public void AddEvents(LineSegment segment, List<IntersectionSweepEvent> events)
        {
            Vector2 point1 = segment.Point1;
            Vector2 point2 = segment.Point2;

            var ev1 = new IntersectionSweepEvent(point1, false, false, segment, null as IntersectionSweepEvent);
            var ev2 = new IntersectionSweepEvent(point2, false, false, segment, ev1);
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
                bool prevFound = status.FindNextSmallest(ev.StatusItem, out prev);
                bool nextFound = status.FindNextBiggest(ev.StatusItem, out next);

                if (prevFound)
                {
                    LineSegment otherSegment = prev.SweepEvent.Segment;
                    Vector2? intersection = ev.Segment.IntersectProper(otherSegment);
                    if (intersection != null)
                    {
                        events.Insert(new IntersectionSweepEvent(intersection.Value, false, false,
                            ev.Segment, otherSegment));
                    }
                }

                if (nextFound)
                {
                    LineSegment otherSegment = next.SweepEvent.Segment;
                    Vector2? intersection = ev.Segment.IntersectProper(otherSegment);
                    if (intersection != null)
                    {
                        events.Insert(new IntersectionSweepEvent(intersection.Value, false, false,
                            ev.Segment, otherSegment));
                    }
                }
            }
            else if (ev.IsEnd)
            {
                ev = ev.OtherEvent;
                if (ev.StatusItem == null)
                {
                    return;
                }

                IntersectionStatusItem prev, next;
                bool prevFound = status.FindNextSmallest(ev.StatusItem, out prev);
                bool nextFound = status.FindNextBiggest(ev.StatusItem, out next);

                status.Delete(ev.StatusItem);

                if (nextFound && prevFound)
                {
                    LineSegment segment = prev.SweepEvent.Segment;
                    LineSegment otherSegment = next.SweepEvent.Segment;
                    Vector2? intersection = segment.IntersectProper(otherSegment);
                    if (intersection != null)
                    {
                        events.Insert(new IntersectionSweepEvent(intersection.Value, false, false,
                            segment, otherSegment));
                    }
                }
            }
            else if (ev.IsIntersection)
            {
                // stop on first intersection
                intersected = new List<Edge>
                {
                    new Edge(new Vertex(ev.Segment.Point1), new Vertex(ev.Segment.Point2)),
                    new Edge(new Vertex(ev.OtherSegment.Point1), new Vertex(ev.OtherSegment.Point2))
                };
                events.Clear();
                status.Clear();
            }
            else
            {
                throw new Exception("Invalid event type");
            }
        }
    }
}
