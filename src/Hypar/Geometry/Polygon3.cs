using System.Collections.Generic;
using System;
using System.Linq;

namespace Hypar.Geometry
{
    /// <summary>
    /// A planar polyon in 3D.
    /// </summary>
    public class Polygon3
    {
        private List<Vector3> m_vertices = new List<Vector3>();

        /// <summary>
        /// The vertices of the polygon.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Vector3> Vertices
        {
            get{return m_vertices;}
        }

        /// <summary>
        /// Construct a Polygon3 from a collection of vertices.
        /// </summary>
        /// <param name="vertices">A CCW wound set of vertices.</param>
        public Polygon3(IEnumerable<Vector3> vertices)
        {
            m_vertices.AddRange(vertices);
        }

        /// <summary>
        /// Reverse the direction of a Polygon3.
        /// </summary>
        /// <returns>Returns a new Polygon3 with opposite winding.</returns>
        public Polygon3 Reversed()
        {
            var verts = new List<Vector3>(m_vertices);
            verts.Reverse();
            return new Polygon3(verts);
        }

        public override string ToString()
        {
            return string.Join(",", this.Vertices);
        }
        
        /// <summary>
        /// Explode a Polygon3 into a collection of Lines.
        /// </summary>
        /// <returns>A collection of Lines.</returns>
        public IEnumerable<Line> Explode()
        {
            var lines = new List<Line>();
            for(var i=0; i<m_vertices.Count; i++)
            {
                var a = m_vertices[i];
                Vector3 b;
                if(i == m_vertices.Count-1)
                {
                    b = m_vertices[0];
                }
                else
                {
                    b = m_vertices[i+1];
                }
                
                lines.Add(new Line(new Vector3(a),new Vector3(b)));
            }
            return lines;
        }

        public Vector3[] ToArray()
        {
            return this.m_vertices.ToArray();
        }

        /// <summary>
        /// Compute the normal of the Polygon using the first 3 vertices to define a plane.
        /// </summary>
        /// <returns></returns>
        public Vector3 Normal()
        {
            var a = (this.m_vertices[2] - this.m_vertices[1]).Normalized();
            var b = (this.m_vertices[0] - this.m_vertices[1]).Normalized();

            return a.Cross(b);
        }

        public Line Segment(int i)
        {
            if(this.m_vertices.Count <= i)
            {
                throw new Exception($"The specified index is greater than the number of segments.");
            }

            var a = this.m_vertices[i];
            var b = this.m_vertices[i+1];
            return new Line(a,b);
        }
    }
}