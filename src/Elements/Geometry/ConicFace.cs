using System;
using System.Collections.Generic;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    /// <summary>
    /// A face define by start and end arcs.
    /// </summary>
    public class ConicFace: IFace
    {
        private Arc _start;
        private Arc _end;

        /// <summary>
        /// The type of the element.
        /// Used during deserialization to disambiguate derived types.
        /// </summary>
        [JsonProperty("type", Order = -100)]
        public string Type
        {
            get { return this.GetType().FullName.ToLower(); }
        }

        /// <summary>
        /// Construct a ConicFace.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public ConicFace(Arc start, Arc end)
        {
            this._start = start;
            this._end = end;
        }

        /// <summary>
        /// The Vertices of the ConicFace
        /// </summary>
        public Vector3[] Vertices
        {
            get
            {
                return new []{this._start.Start, this._start.End, this._end.Start, this._end.End};
            }
        }

        /// <summary>
        /// The edges of the ConicFace.
        /// </summary>
        public ICurve[] Edges
        {
            get
            {
                var a = new Line(this._start.Start, this._end.Start);
                var b = new Line(this._start.End, this._end.End);
                return new ICurve[]{this._start, b, this._end, a};
            }
        }

        /// <summary>
        /// Compute the Mesh for this Face.
        /// </summary>
        public void Tessellate(Mesh mesh)
        {
            throw new NotImplementedException();
        }
    }
}