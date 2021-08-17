﻿using Elements.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// A unique vertex in a adaptive grid.
    /// Class is forked from CellComplex.Vertex.
    /// </summary>
    public class Vertex
    {
        public Vector3 Point { get; set; }

        public AdaptiveGrid AdaptiveGrid { get; private set; }

        /// <summary>
        /// ID of this child.
        /// </summary>
        public ulong Id { get; internal set; }

        /// <summary>
        /// All Edges connected to this Vertex.
        /// </summary>
        [JsonIgnore]
        public HashSet<Edge> Edges = new HashSet<Edge>();

        internal Vertex(AdaptiveGrid adaptiveGrid, ulong id, Vector3 point)
        {
            Id = id;
            AdaptiveGrid = adaptiveGrid;
            Point = point;
        }

        public override bool Equals(object obj)
        {
            return obj is Vertex vertex &&
                   Point.Equals(vertex.Point);
        }

        public override int GetHashCode()
        {
            return -1396796455 + Point.GetHashCode();
        }
    }
}