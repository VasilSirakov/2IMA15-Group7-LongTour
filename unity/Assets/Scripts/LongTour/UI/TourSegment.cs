namespace LongTour
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry;
    using Util.Geometry.Graph;

    /// <summary>
    /// Handles interaction with tour segments to remove them
    /// </summary>
    public class TourSegment : MonoBehaviour
    {
        //The edge this segment corresponds to
        public Edge Edge { get; internal set; }

        private TourController m_gameController;

        void Awake()
        {
            //Find the game controller
            m_gameController = FindObjectOfType<TourController>();
        }

        void OnMouseUpAsButton()
        {
            //destroy the road object
            m_gameController.RemoveSegment(this);
            Destroy(gameObject);
        }
    }
}