using System;
using System.Collections.Generic;
using System.Linq;
using Elements;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A unique edge in a cell complex.
    /// </summary>
    public abstract class EdgeBase : ChildBase<Line>
    {
        /// <summary>
        /// ID of first vertex
        /// </summary>
        public ulong StartVertexId;

        /// <summary>
        /// ID of second vertex
        /// </summary>
        public ulong EndVertexId;

        /// <summary>
        /// Create an EdgeBase (just calls CellChild constructor)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cellComplex"></param>
        /// <returns></returns>
        protected EdgeBase(ulong id, CellComplex cellComplex) : base(id, cellComplex) { }

        /// <summary>
        /// Get the geometry for this Edge
        /// </summary>
        /// <returns></returns>
        public override Line GetGeometry()
        {
            return new Line(
                this.CellComplex.GetVertex(this.StartVertexId).Value,
                this.CellComplex.GetVertex(this.EndVertexId).Value
            );
        }

        /// <summary>
        /// Shortest distance to a given point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public override double DistanceTo(Vector3 point)
        {
            return point.DistanceTo(this.GetGeometry());
        }
    }
}