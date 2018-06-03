using System.Collections;
using System.Collections.Generic;

namespace Hypar.Geometry
{
    public class Polygon2
    {
        private List<Vector2> m_vertices = new List<Vector2>();

        public IEnumerable<Vector2> Vertices
        {
            get{return m_vertices;}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices">A CCW wound set of vertices.</param>
        public Polygon2(IEnumerable<Vector2> vertices)
        {
            m_vertices.AddRange(vertices);
        }

        public Polygon2 Reversed()
        {
            var verts = new List<Vector2>(m_vertices);
            verts.Reverse();
            return new Polygon2(verts);
        }

        public override string ToString()
        {
            return string.Join(",", this.Vertices);
        }

        public IEnumerable<Line> Explode()
        {
            var lines = new List<Line>();
            for(var i=0; i<m_vertices.Count; i++)
            {
                var a = m_vertices[i];
                Vector2 b;
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
    }
}