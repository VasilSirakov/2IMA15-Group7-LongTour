using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Util.Geometry;
using Util.Geometry.Graph;
using Util.Math;

namespace Util.Algorithms.LongTour.Heuristic
{
    public class HeuristicAlgorithm
    {
        private readonly List<Vertex> inputVertices;
        // TODO: Not a fan of field mutation in a method such as GetTourLineSegments.
        // Will leave it for now because it is fine but if we have time, the recursion can be improved
        // to return a list of line segments instead of a boolean.
        private readonly List<LineSegment> tourSegments;

        public HeuristicAlgorithm(List<Vertex> vertices)
        {
            this.inputVertices = vertices.OrderBy(x => x.Pos.x).ToList();
            this.tourSegments = new List<LineSegment>();
        }

        public IEnumerable<LineSegment> GetResultingTour()
        {
            // In case this function is called more than once on the same instance of this class.
            this.tourSegments.Clear();

            // TODO: Determine which vertex to start from.
            bool tourExists = this.GetTourSegments(this.inputVertices[0], new Stack<Vertex>());
            if (!tourExists)
            {
                throw new Exception("No valid tour found.");
            }

            return this.tourSegments;
        }

        private bool GetTourSegments(Vertex current, Stack<Vertex> alreadyVisited)
        {
            // Exit condition. When we have 'n' vertices and 'n-1' edges we must exit with true, which means
            // a tour has been found.
            if (this.tourSegments.Count == this.inputVertices.Count - 1)
            {
                return true;
            }

            // Get all potential segments that can be added regardless of whether they are legal (intersect anything
            // so far). Mark current vertex as visited.
            alreadyVisited.Push(current);
            var potentialSegments = this.GetPotentialLineSegmentsForVertex(current, alreadyVisited);

            foreach (var segment in potentialSegments)
            {
                // Check if segment is legal (does not intersect any other segments so far, and does not overlap any
                // of the other input vertices except its start and end point.
                bool legal = this.IsSegmentLegal(segment);
                if (legal)
                {
                    this.tourSegments.Add(segment);
                    // If you added an edge to the tour, e.g. {(1,1),(2,2)}, make vertex (2,2) your next starting point.
                    var nextVertex = new Vertex(segment.Point2);
                    bool validTourExists = this.GetTourSegments(nextVertex, alreadyVisited);
                    if (validTourExists)
                        return true;
                    else
                        this.tourSegments.Remove(segment);
                }
            }

            alreadyVisited.Pop();
            return false;
        }

        private List<LineSegment> GetPotentialLineSegmentsForVertex(Vertex from, Stack<Vertex> alreadyVisited)
        {
            return this.inputVertices
                    .Except(alreadyVisited)
                    // TODO: Check which 'order by' to use.
                    .Select(to => new LineSegment(from.Pos, to.Pos))
                    .OrderByDescending(segment => segment.Magnitude)
                    // .OrderBy(segment => segment.Magnitude)
                    // .OrderBy(to => Guid.NewGuid())
                    .ToList();
        }

        private bool IsSegmentLegal(LineSegment segmentToAdd)
        {
            if (this.IntersectsAnySoFar(segmentToAdd))
                return false;

            if (this.OverlapsAnyVertex(segmentToAdd))
                return false;
           
            return true;
        }

        private bool OverlapsAnyVertex(LineSegment candidateSegment)
        {
            var except = new List<Vertex>
            {
                new Vertex(candidateSegment.Point1),
                new Vertex(candidateSegment.Point2)
            };

            // Added this just in case. Whenever a new segment is added between 2 vertices, and another vertex lies
            // on it. Then if you add a third edge to this other vertex, it will not count it as 'intersecting and thus
            // the output of the algorithm would be wrong.
            return this.inputVertices
                .Except(except)
                .Any(vertex => candidateSegment.IsOnSegment(vertex.Pos));
        }

        private bool IntersectsAnySoFar(LineSegment candidateSegment)
        {
            return this.tourSegments.Any(
                segment => LineSegment.IntersectProper(segment, candidateSegment) == null
                    ? false
                    : true);
        }
    }
}