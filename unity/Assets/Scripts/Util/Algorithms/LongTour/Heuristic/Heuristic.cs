using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Util.Geometry;
using Util.Geometry.Graph;

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

        public IEnumerable<LineSegment> GetResult()
        {
            // In case this function is called more than once on the same instance of this class.
            this.tourSegments.Clear();

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
            var potentialSegments = this.GetPotentialLineSegmentsForVertex(current, alreadyVisited);
            alreadyVisited.Push(current);
            
            // If there are no more segments to be added and the tour segments are not yet 'n-1', then report that
            // a tour cannot be found for this subtree of the recursion. Also, remove current vertex from visited.
            if (!potentialSegments.Any())
            {
                alreadyVisited.Pop();
                return false;
            }

            foreach (var segment in potentialSegments)
            {
                // Try to add the segment to the tourSegments list if it is a legal (does not intersect any of the
                // already added segments).
                bool added = this.TryAddSegmentToTour(segment);
                if (added)
                {
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
                    .Where(to => !to.Equals(from))
                    // TODO: Check which 'order by' to use.
                    // .OrderBy(to => this.GetEuclidianDistance(from, to))
                    .OrderByDescending(to => this.GetEuclidianDistance(from, to))
                    // .OrderBy(to => Guid.NewGuid())
                    .Select(to => new LineSegment(from.Pos, to.Pos))
                    .ToList();
        }

        private bool TryAddSegmentToTour(LineSegment segmentToAdd)
        {
            if (this.IsSegmentLegal(segmentToAdd))
            {
                this.tourSegments.Add(segmentToAdd);
                return true;
            }

            return false;
        }

        private bool IsSegmentLegal(LineSegment segmentToAdd)
        {
            if (this.IntersectsAnySoFar(segmentToAdd))
                return false;

            if (this.OverlapsAnyVertex(segmentToAdd))
            {
                return false;
            }
           
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

        private float GetEuclidianDistance(Vertex v1, Vertex v2)
        {
            float x1 = v1.Pos.x;
            float x2 = v2.Pos.x;
            float y1 = v1.Pos.y;
            float y2 = v2.Pos.y;
            return (float)System.Math.Sqrt(System.Math.Pow(x1 - x2, 2) + System.Math.Pow(y1 - y2, 2));
        }
    }
}