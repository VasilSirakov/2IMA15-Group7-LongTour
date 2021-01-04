namespace LongTour
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using General.Menu;
    using General.Model;
    using General.Controller;
    using System.Linq;
    using System;
    using Util.Geometry.Graph;
    using Util.Geometry.Polygon;
    using Util.Algorithms.Polygon;
    using Util.Geometry;
    using Util.Algorithms.LongTour.Heuristic;
    using Util.Algorithms.LongTour;

    public class TourController : MonoBehaviour, IController
    {
        public Material m_roadMaterial;
        public Material m_intersectedMaterial;

        [SerializeField]
        private GameObject m_roadMeshPrefab;
        [SerializeField]
        private GameObject m_pointPrefab;
        [SerializeField]
        private ButtonContainer m_advanceButton;
        [SerializeField]
        private ButtonContainer m_resetButton;
        [SerializeField]
        private ButtonContainer m_backButton;
        [SerializeField]
        private ButtonContainer m_giveUpButton;
        [SerializeField]
        private Text m_scoreText;

        [SerializeField]
        private List<TourLevel> m_levels;
        [SerializeField]
        private string m_victoryScene;


        //list of removable instantiated game objects
        private List<GameObject> instantObjects = new List<GameObject>();

        private Heuristic m_heuristic;
        private List<LineSegment> heuristicTour;
        private float heuristicTourLength;

        private IntersectionSweepLine<IntersectionSweepEvent, IntersectionStatusItem> m_sweepline;
  
        //Graph info
        protected IGraph m_graph;
        protected TourPoint[] m_tourPoints;

        protected int m_levelCounter = 0;    

        // Start is called before the first frame update
        void Start()
        {
            InitLevel();
        }

        //Updates handled by SegmentMaker

        /// <summary>
        /// Initialize the level
        /// </summary>
        public void InitLevel()
        {
            // check if all levels are solved
            if (m_levelCounter >= m_levels.Count)
            {
                SceneManager.LoadScene(m_victoryScene);
                return;
            }

            m_sweepline = new IntersectionSweepLine<IntersectionSweepEvent, IntersectionStatusItem>();

            //initialize points
            foreach (var point in m_levels[m_levelCounter].Points)
            {
                var obj = Instantiate(m_pointPrefab, point, Quaternion.identity);
                obj.transform.parent = this.transform;
                instantObjects.Add(obj);
            }

            // make vertex list
            m_tourPoints = FindObjectsOfType<TourPoint>();

            //init empty graph
            if (m_graph != null)
            {
                m_graph.Clear();
            }
            m_graph = new AdjacencyListGraph(m_tourPoints.Select(go => go.Vertex));

            var vertices = m_tourPoints.Select(go => new Vertex(go.Pos));

            m_heuristic = new Heuristic(vertices);
            heuristicTour = m_heuristic.GetResultingTour();
            heuristicTourLength = heuristicTour.Sum(x => x.Magnitude);

            CheckSolution();
            UpdateTextField();
        }

        /// <summary>
        /// Updates the text of the textfield
        /// </summary>
        private void UpdateTextField()
        {
            string text;
            float tourWeight = this.m_graph.Edges.Sum(e => e.Length);

            text = "Your current tour has length: " + tourWeight.ToString("0.##");
            text += "\nThe heuristic length to beat: " + heuristicTourLength.ToString("0.##");
            m_scoreText.text = text;
        }

        public void AdvanceLevel()
        {
            // increase level index
            m_levelCounter++;
            Clear();
            InitLevel();
        }

        public void AddSegment(TourPoint a_point1, TourPoint a_point2)
        {
            // Dont add edge to itself or double edges
            if (a_point1 == a_point2 || m_graph.ContainsEdge(a_point1.Vertex, a_point2.Vertex))
            {
                return;
            }

            //instantiate new road mesh object
            var roadmesh = Instantiate(m_roadMeshPrefab, Vector3.forward, Quaternion.identity) as GameObject;
            roadmesh.transform.parent = this.transform;
            roadmesh.name = $"mesh({a_point1.Vertex}, {a_point2.Vertex})";
            
            //remember segment for destoryal later
            instantObjects.Add(roadmesh);
            
            //create road mesh
            var roadmeshScript = roadmesh.GetComponent<ReshapingMesh>();
            roadmeshScript.CreateNewMesh(a_point1.transform.position, a_point2.transform.position);

            //create road edge
            var edge = m_graph.AddEdge(a_point1.Vertex, a_point2.Vertex);

            //error check
            if (edge == null)
            {
                throw new InvalidOperationException("Edge could not be added to graph");
            }

            //link edge to segment
            roadmesh.GetComponent<TourSegment>().Edge = edge;

            //check the solution
            CheckSolution();
            UpdateTextField();
        }

        /// <summary>
        /// Removes a segment from the graph
        /// </summary>
        /// <param name="a_segment"></param>
        public void RemoveSegment(TourSegment a_segment)
        {
            m_graph.RemoveEdge(a_segment.Edge);
            CheckSolution();
            UpdateTextField();
        }

        /// <summary>
        /// Checks whether there is a solution of sufficient quality present
        /// </summary>
        public void CheckSolution()
        {
            if (CheckTour())
            {
                m_advanceButton.Enable();
                m_giveUpButton.Disable();
            }
            else
            {
                m_giveUpButton.Enable();
                m_advanceButton.Disable();
            }
        }

        /// <summary>
        /// Checks whether the tour is valid and long enough
        /// </summary>
        /// <returns>true if tour is valid and long enough</returns>
        private bool CheckTour()
        {
            // Reset mesh colours to original (in case intersection has been removed)
            foreach (GameObject mesh in instantObjects.Where(x => x != null && x.name.Contains("mesh")))
            {
                var renderer = mesh.GetComponent<MeshRenderer>();
                renderer.material = m_roadMaterial;
            }
            // Perform plane sweep to find first interesection
            // Perform before other checks to show intersection as it happens
            List<Edge> intersected = m_sweepline.FindIntersection(m_graph.Edges);
            if (intersected != null)
            {
                // Find existing roadmesh for intersected edges and change material
                foreach (Edge e in intersected)
                {
                    var roadmesh = instantObjects
                        .Find(x => x != null && (x.name == $"mesh({e.Start}, {e.End})"
                                                || x.name == $"mesh({e.End}, {e.Start})"));
                    if (roadmesh != null)
                    {
                        var renderer = roadmesh.GetComponent<MeshRenderer>();
                        renderer.material = m_intersectedMaterial;
                    }
                } 
                return false;
            }

            // Check if the number of input edges is correct.
            if (this.m_graph.EdgeCount != this.m_levels[m_levelCounter].Points.Count - 1)
            {
                return false;
            }

            // Check if the degree of a vertex is either 1 or 2. 1 is when the vertex is the start point
            // or end point of the tour. 2 is for all other vertices.
            var vertices = this.m_levels[m_levelCounter].Points
                .Select(x => new Vertex(x));
            foreach (var vertex in vertices)
            {
                int degreeOfVertex = 
                    this.m_graph.Edges.Count(e => e.Start.Equals(vertex)) +
                    this.m_graph.Edges.Count(e => e.End.Equals(vertex));

                if (degreeOfVertex == 0 || degreeOfVertex > 2)
                {
                    return false;
                }
            }

            // Compare length of user tour to heuristic tour. If shorter, the tour is not good enough.
            float tourWeight = this.m_graph.Edges.Sum(e => e.Length);
            if (tourWeight < heuristicTourLength)
            {
                return false;
            }

            // All other checks have been passed. Tour is valid.
            return true;
        }

        ///<summary>
        ///Clears tour and relevant game objects
        /// </summary>
        private void Clear()
        {
            //clear the graph if it exists
            if (m_graph != null) m_graph.Clear();

            //Destroy the objects related to the graph
            foreach (var obj in instantObjects)
            {
                // destory the objects
                DestroyImmediate(obj);
            }
            instantObjects.Clear();
            m_tourPoints = null;
            m_scoreText.text = "";
        }

        /// <summary>
        /// Clears all edges to reset the level
        /// </summary>
        public void ClearEdges()
        {
            Clear();
            InitLevel();
        }

        /// <summary>
        /// Gives the heuristic solution to the problem if the player gives up
        /// </summary>
        public void GiveUp()
        {
            Clear();
            InitLevel();
            foreach (var lineSegment in heuristicTour)
            {
                Vertex endpoint1 = new Vertex(lineSegment.Point1);
                Vertex endpoint2 = new Vertex(lineSegment.Point2);

                Vector2 pos1 = endpoint1.Pos;
                Vector2 pos2 = endpoint2.Pos;

                // Dont add edge to itself or double edges
                if (endpoint1 == endpoint2 || m_graph.ContainsEdge(endpoint1, endpoint2))
                {
                    return;
                }

                //instantiate new road mesh object
                var roadmesh = Instantiate(m_roadMeshPrefab, Vector3.forward, Quaternion.identity) as GameObject;
                roadmesh.transform.parent = this.transform;

                //remember segment for destoryal later
                instantObjects.Add(roadmesh);

                //create road mesh
                var roadmeshScript = roadmesh.GetComponent<ReshapingMesh>();
                roadmeshScript.CreateNewMesh(pos1, pos2);

                //create road edge
                var edge = m_graph.AddEdge(endpoint1, endpoint2);

                //error check
                if (edge == null)
                {
                    throw new InvalidOperationException("Edge could not be added to graph");
                }

                //link edge to segment
                roadmesh.GetComponent<TourSegment>().Edge = edge;
            }
            //check the solution
            CheckSolution();
            UpdateTextField();
        }
    }
}
