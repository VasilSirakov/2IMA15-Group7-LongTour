namespace LongTour
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SegmentMaker : MonoBehaviour
    {
        public Material m_roadMaterial;

        //Set pointers for two points 
        private TourPoint m_firstPoint = null;
        private TourPoint m_secondPoint = null;

        // is the end of an edge attatched to the cursor or a second point
        private bool m_attatched = false;

        private LineRenderer m_edge;
        private TourController m_tourController;

        //Initialize
        void Awake()
        {
            m_edge = GetComponent<LineRenderer>();
            m_edge.material = m_roadMaterial;
            m_edge.startWidth = 0.3f;
            m_edge.endWidth = 0.3f;
            m_tourController = FindObjectOfType<TourController>();
        }

        // Update is called once per frame
        void Update()
        {
            if (m_attatched)
            {
                if (!Input.GetMouseButton(0))
                {
                    // create segment
                    m_tourController.AddSegment(m_firstPoint, m_secondPoint);
                    //clear edge line
                    Clear();
                }
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    //add forward vector to create distance to the camera
                    var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward);
                    m_edge.SetPosition(1, pos);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    //clear current line
                    Clear();
                }
            }
        }
        /// <summary>
        /// Handles a mouse down event on the given point
        /// </summary>
        /// <param name="a_target"></param>
        public void MouseDown(TourPoint a_target)
        {
            //enalbe line drawing
            m_edge.enabled = true;

            //set first point
            m_firstPoint = a_target;

            //update edge line start
            m_edge.SetPosition(0, a_target.transform.position);
        }

        /// <summary>
        /// Handles a mouse event on a settlement
        /// </summary>
        /// <param name="a_target"></param>
        public void MouseEnter(TourPoint a_target)
        {
            //don't do anything if no startint point has been selected
            if (m_firstPoint == null) return;

            //add lock to target point
            m_attatched = true;
            m_secondPoint = a_target;

            // update edge line end to target point
            m_edge.SetPosition(1, a_target.transform.position);
        }

        /// <summary>
        /// Handles a mouse exit event for a given point
        /// </summary>
        /// <param name="a_target"></param>
        public void MouseExit(TourPoint a_target)
        {
            //do nothing if settlement was not the current target
            if (a_target != m_secondPoint)
            {
                return;
            }

            //remove the attatchement lock and the target settlement
            m_attatched = false;
            m_secondPoint = null;

            //update edge to mouse position
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward);
            m_edge.SetPosition(1, pos);
        }

        void Clear()
        {
            //remove attatchment and points
            m_attatched = false;
            m_firstPoint = null;
            m_secondPoint = null;

            //disable edge drawer
            m_edge.enabled = false;
        }
    }
}