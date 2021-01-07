﻿namespace LongTour
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Util.Geometry.Graph;

    public class TourPoint : MonoBehaviour
    {
        public Vertex Vertex { get; private set; }
        public Vector2 Pos { get { return Vertex.Pos; } }

        private SegmentMaker m_segmentMaker;

        //Use this for initialization
        void Awake()
        {
            //create vertex at point position
            Vertex = new Vertex(transform.position);

            // find segmentmaker class to have user interaction
            m_segmentMaker = FindObjectOfType<SegmentMaker>();
            if (m_segmentMaker == null)
            {
                throw new InvalidProgramException("Road builder cannot be found");
            }

        }

        void OnMouseDown()
        {
            m_segmentMaker.MouseDown(this);
        }

        void OnMouseEnter()
        {
            m_segmentMaker.MouseEnter(this);
        }

        void OnMouseExit()
        {
            m_segmentMaker.MouseExit(this);
        }

    }
}
