using Elements.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// ID of this Vertex.
        /// </summary>
        public ulong Id { get; internal set; }

        /// <summary>
        /// Find edge between this Vertex and Vertex with given ID.
        /// </summary>
        /// <param name="otherId">Id of other vertex.</param>
        /// <returns>Edge between this and Vertex with given ID. Null if not found.</returns>
        public Edge GetEdge(ulong otherId)
        {
            if (otherId == this.Id)
            {
                return null;
            }
            return Edges.Where(e => e.StartId == otherId || e.EndId == otherId).FirstOrDefault();
        }

        /// <summary>
        /// All Edges connected to this Vertex.
        /// </summary>
        [JsonIgnore]
        public HashSet<Edge> Edges = new HashSet<Edge>();

        internal Vertex(ulong id, Vector3 point)
        {
            Id = id;
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
