using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using General.Menu;
using General.Model;
using System.Linq;
using Util.Geometry.Polygon;
using Util.Algorithms.Polygon;
using Util.Geometry;

public class TourController : MonoBehaviour
{
    public LineRenderer m_line;

    [SerializeField]
    private GameObject m_roadMeshPrefab;
    [SerializeField]
    private ButtonContainer m_advanceButton;

    internal TourPoint m_firstPoint;
    internal TourPoint m_secondPoint;
    internal bool m_locked;

    private List<TourPoint> m_points;
    private HashSet<LineSegment> m_segments;

    // Start is called before the first frame update
    void Start()
    {
        //get unity objects
        m_points = FindObjectsOfType<TourPoint>().ToList();
        m_segments = new HashSet<LineSegment>();

        //Compute Tour
        //TODO computer tour

        //disable advance button
        m_advanceButton.Disable();
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
}
