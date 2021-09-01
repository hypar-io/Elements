using Elements.Geometry;
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
        /// <summary>
        /// Position of this Vertex in 3D space
        /// </summary>
        public Vector3 Point { get; set; }

        /// <summary>
        /// The AdaptiveGrid that this Vertex belongs to.
        /// </summary>
        public AdaptiveGrid AdaptiveGrid { get; private set; }

        /// <summary>
        /// ID of this Vertex.
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

        /// <summary>
        /// Used to handle comparisons for when we make HashSets of this type.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is Vertex vertex &&
                   Point.Equals(vertex.Point);
        }

        /// <summary>
        /// Used to return a unique identifier for when we make HashSets of this type.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return -1396796455 + Point.GetHashCode();
        }
    }
}
