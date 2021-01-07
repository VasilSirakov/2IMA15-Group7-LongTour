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
        private IGraph m_graph;
        protected TourPoint[] m_tourPoints;
        protected TourSegment[] m_segments;

        protected int m_levelCounter = 0;
        protected readonly float m_pointRadius = 0.6f;

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
           
            // check if all levels are solved then go to endless mode
            if (m_levelCounter >= m_levels.Count)
            {
                int amntOfPoints = m_levelCounter;
                if (m_levelCounter >= 14)
                {
                    amntOfPoints = 14;
                }
                Camera.main.orthographicSize = 4 * (1 + m_pointRadius );

                var height = Camera.main.orthographicSize * 0.9f;
                var width = height * Camera.main.aspect;
                List<Vector2> positions = InitEndlessLevelPositions(amntOfPoints, width, height);
                foreach(var position in positions)
                {
                    GameObject obj;
                    obj = Instantiate(m_pointPrefab, position, Quaternion.identity);
                    obj.transform.parent = this.transform;
                    instantObjects.Add(obj);
                }
            }
            else
            {
                //initialize points
                foreach (var point in m_levels[m_levelCounter].Points)
                {
                    var obj = Instantiate(m_pointPrefab, point, Quaternion.identity);
                    obj.transform.parent = this.transform;
                    instantObjects.Add(obj);
                }
            }


            m_sweepline = new IntersectionSweepLine<IntersectionSweepEvent, IntersectionStatusItem>();


            // make vertex list
            m_tourPoints = FindObjectsOfType<TourPoint>();

            //init empty graph
            m_graph = new AdjacencyListGraph(m_tourPoints.Select(go => go.Vertex));

            var vertices = m_tourPoints.Select(go => new Vertex(go.Pos));

            m_heuristic = new Heuristic(vertices);
            heuristicTour = m_heuristic.GetResultingTour();
            heuristicTourLength = heuristicTour.Sum(x => x.Magnitude);

            CheckSolution();
            UpdateTextField();
        }

        /// <summary>
        /// Generates new coordinates randomly for endless levels
        /// </summary>
        /// <param name="count"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private List<Vector2> InitEndlessLevelPositions(int count, float width, float height)
        {
            var result = new List<Vector2>();
            while (result.Count < count)
            {
                //find uniform random position centered around (0,0) within width and height
                //taking into account point radius
                var xpos = UnityEngine.Random.Range(-width / 2, width / 2 );
                var ypos = UnityEngine.Random.Range(-height / 2, width / 2);
                var pos = new Vector2(xpos, ypos);

                //don't add if too close to another point
                if (!result.Exists(r => Vector2.Distance(r,pos) < 2 *  m_pointRadius))
                {
                    result.Add(pos);
                }
            }
          
            return result;
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

        /// <summary>
        /// Clears the solved level and initialize the next level
        /// </summary>
        public void AdvanceLevel()
        {
            // increase level index
            m_levelCounter++;
            Clear();
            InitLevel();
        }

        /// <summary>
        /// Add a road tour segment between 2 points
        /// </summary>
        /// <param name="a_point1"></param>
        /// <param name="a_point2"></param>
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
            bool isTourCorrect = true;
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
                isTourCorrect = false; ;
            }

            // Check if the degree of a vertex is either 1 or 2. 1 is when the vertex is the start point
            // or end point of the tour. 2 is for all other vertices.
            var vertices = new List<Vertex>();
            foreach( var tourPoint in m_tourPoints)
            {
                vertices.Add(tourPoint.Vertex);
            }
            foreach (var vertex in vertices)
            {
                int degreeOfVertex = 
                    this.m_graph.Edges.Count(e => e.Start.Equals(vertex)) +
                    this.m_graph.Edges.Count(e => e.End.Equals(vertex));

                if (degreeOfVertex == 0 || degreeOfVertex > 2)
                {
                    var tooManyEdges = this.m_graph.EdgesOf(vertex);
                    foreach (Edge e in tooManyEdges)
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
                    isTourCorrect = false;
                }
            }

            // Compare length of user tour to heuristic tour. If shorter, the tour is not good enough.
            float tourWeight = this.m_graph.Edges.Sum(e => e.Length);
            if (tourWeight < heuristicTourLength)
            {
                isTourCorrect = false;
            }

            // Check if the number of input edges is correct.
            if (this.m_graph.EdgeCount != m_tourPoints.Length - 1)
            {
                isTourCorrect = false;
            }

            // All other checks have been passed. Tour is valid.
            return isTourCorrect;
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
            //destroy all instant objects(both the vertices and edges)
            foreach (var obj in instantObjects)
            {
                // destory the objects
                DestroyImmediate(obj);
            }
            instantObjects.Clear();

            //Reload all the vertices into the instant objects
            foreach (var point in m_tourPoints)
            {
                var obj = Instantiate(m_pointPrefab, point.Pos, Quaternion.identity);
                obj.transform.parent = this.transform;
                instantObjects.Add(obj);
            }
            //remove all edges in the underlying graph
            this.m_graph.RemoveAllEdges(this.m_graph.Edges);
        }

        /// <summary>
        /// Gives the heuristic solution to the problem if the player gives up
        /// </summary>
        public void GiveUp()
        {
            ClearEdges();
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
