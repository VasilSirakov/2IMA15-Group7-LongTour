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
    using Util.Geometry.Polygon;
    using Util.Algorithms.Polygon;
    using Util.Geometry;

    public class TourController : MonoBehaviour, IController
    {
        public LineRenderer m_line;

        [SerializeField]
        private GameObject m_roadMeshPrefab;
        [SerializeField]
        private GameObject m_pointPrefab;
        [SerializeField]
        private ButtonContainer m_advanceButton;
        [SerializeField]
        private ButtonContainer m_resetButton;

        [SerializeField]
        private List<TourLevel> m_levels;
        [SerializeField]
        private string m_victoryScene;

        internal TourPoint m_firstPoint;
        internal TourPoint m_secondPoint;
        internal bool m_locked;

        private List<TourPoint> m_points;
        private HashSet<LineSegment> m_segments;

        private List<GameObject> instantObjects;
            
        protected int m_levelCounter = 0;    

        // Start is called before the first frame update
        void Start()
        {
            //get unity objects
            m_points = new List<TourPoint>();
            m_segments = new HashSet<LineSegment>();
            instantObjects = new List<GameObject>();

            InitLevel();

            //Compute Tour
            //TODO computer tour
        }

        // Update is called once per frame
        void Update()
        {
            if (m_locked && !Input.GetMouseButton(0))
            {
                //create road
                AddSegment(m_firstPoint, m_secondPoint);
            }
            else if (Input.GetMouseButton(0))
            {
                //update road endpoint
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward);
                m_line.SetPosition(1, pos);
            }

            //clear road creation variables
            if ((m_locked && !Input.GetMouseButton(0)) || Input.GetMouseButtonUp(0))
            {
                m_locked = false;
                m_firstPoint = null;
                m_secondPoint = null;
                m_line.enabled = false;
            }
        }

        public void InitLevel()
        {
            if (m_levelCounter >= m_levels.Count)
            {
                SceneManager.LoadScene(m_victoryScene);
                return;
            }

            // clear old level
            Clear();

            //initialize points
            foreach (var point in m_levels[m_levelCounter].Points)
            {
                var obj = Instantiate(m_pointPrefab, point, Quaternion.identity) as GameObject;
                obj.transform.parent = this.transform;
                instantObjects.Add(obj);
            }

            // make vertex list
            m_points = FindObjectsOfType<TourPoint>().ToList();

            // computer long tour
            //TODO implement solution 
            //m_solutionTour;

            m_resetButton.Enable();

            //m_advanceButton.Disable();
        }

        public void AdvanceLevel()
        {
            // increase level index
            m_levelCounter++;
            InitLevel();
        }

        public void AddSegment(TourPoint a_point1, TourPoint a_point2)
        {
            var segment = new LineSegment(a_point1.Pos, a_point2.Pos);

            // dont add double segments (also checking the reverse)
            if (m_segments.Contains(segment) || m_segments.Contains(new LineSegment(a_point2.Pos, a_point1.Pos)))
            {
                return;
            }

            m_segments.Add(segment);

            //instantiate new road mesh
            var roadmesh = Instantiate(m_roadMeshPrefab, Vector3.forward, Quaternion.identity) as GameObject;
            roadmesh.transform.parent = this.transform;
            instantObjects.Add(roadmesh);

            roadmesh.GetComponent<TourSegment>().Segment = segment;

            var roadmeshScript = roadmesh.GetComponent<ReshapingMesh>();
            roadmeshScript.CreateNewMesh(a_point1.transform.position, a_point2.transform.position);

            CheckSolution();
        }

        public void RemoveSegment(TourSegment a_segment)
        {
            m_segments.Remove(a_segment.Segment);
            CheckSolution();
        }

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
            //TODO: check tour against some rules / heuristic algo
            return true;
        }

        ///<summary>
        ///Clears tour and relevant game objects
        /// </summary>
        private void Clear()
        {
            //TODO: clear tour solution
            //m_solutionTour = null;
            m_points.Clear();
            m_segments.Clear();

            //destroy game objects created in level
            foreach (var obj in instantObjects)
            {
                // destory the objects
                DestroyImmediate(obj);
            }
        }

        /// <summary>
        /// Clears all edges to reset the level
        /// </summary>
        public void ClearEdges()
        {
            InitLevel();
        }
    }
}
