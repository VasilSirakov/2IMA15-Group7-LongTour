namespace LongTour
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using General.Menu;
    using General.Model;
    using General.Controller;
    using System.Linq;
    using System;
    using Util.Geometry.Graph;
    using Util.Geometry.Polygon;
    using Util.Algorithms.Polygon;
    using Util.Geometry;

    public class TourController : MonoBehaviour, IController
    {
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
        private List<TourLevel> m_levels;
        [SerializeField]
        private string m_victoryScene;

        //list of removable instantiated game objects
        private List<GameObject> instantObjects = new List<GameObject>();

        //Graph info
        protected IGraph m_graph;
        protected TourPoint[] m_tourPoints;

        protected int m_levelCounter = 0;    

        // Start is called before the first frame update
        void Start()
        {
            Clear();
            InitLevel();

            //Compute Tour
            //TODO computer tour
        }

        //Updates handled by SegmentMaker

        /// <summary>
        /// Initialize the level
        /// </summary>
        public void InitLevel()
        {
            // clear old level
            Clear();

            // check if all levels are solved
            if (m_levelCounter >= m_levels.Count)
            {
                SceneManager.LoadScene(m_victoryScene);
                return;
            }

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

            CheckSolution();
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
        }

        /// <summary>
        /// Removes a segment from the graph
        /// </summary>
        /// <param name="a_segment"></param>
        public void RemoveSegment(TourSegment a_segment)
        {
            m_graph.RemoveEdge(a_segment.Edge);
            CheckSolution();
        }

        /// <summary>
        /// Checks whether there is a solution of sufficient quality present
        /// </summary>
        public void CheckSolution()
        {
            if (CheckTour())
            {
                m_advanceButton.Enable();
            }
            else
            {
                m_advanceButton.Disable();
            }
        }

        private bool CheckTour()
        {
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

            // Compare length of user tour to heuristic tour. If shorter, tour is invalid.
            // TODO: Integrate heuristic algorithm output. 
            float tourWeight = this.m_graph.Edges.Sum(e => e.Length);
            if (tourWeight < -1)
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
        }

        /// <summary>
        /// Clears all edges to reset the level
        /// </summary>
        public void ClearEdges()
        {
            Clear();
            InitLevel();
        }
    }
}
