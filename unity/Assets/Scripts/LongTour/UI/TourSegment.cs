using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util.Geometry;

public class TourSegment : MonoBehaviour
{
    public LineSegment Segment { get; set; }
    private TourController m_gameController;

    void Awake()
    {
        m_gameController = FindObjectOfType<TourController>();
    }

    void OnMouseUpAsButton()
    {
        //destroy the road object
        m_gameController.RemoveSegment(this);
        Destroy(gameObject);
    }
}
