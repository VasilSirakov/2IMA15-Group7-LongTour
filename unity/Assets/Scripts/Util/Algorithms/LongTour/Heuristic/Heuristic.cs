using System;
using System.Collections.Generic;
using System.Linq;
using Util.Geometry;
using Util.Geometry.Graph;

namespace Util.Algorithms.LongTour.Heuristic
{
    public class HeuristicAlgorithm
    {
        private readonly IEnumerable<Vertex> vertices;
        private readonly List<LineSegment> heuristicTourSegments;

        public HeuristicAlgorithm(IEnumerable<Vertex> vertices)
        {
            // Just in case ordering them in ascending order of their x coordinates.
            this.vertices = vertices.OrderBy(x => x.Pos.x);
            this.heuristicTourSegments = new List<LineSegment>();
        }

        public IEnumerable<LineSegment> GetTourLineSegments()
        {
            return null;
        }
    }

}