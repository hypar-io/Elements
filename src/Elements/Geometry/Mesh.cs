using LibTessDotNet.Double;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Elements.Geometry
{
    /// <summary>
    /// An indexed mesh.
    /// </summary>
    public class Mesh
    {
        private List<double> m_vertices = new List<double>();
        private List<double> m_normals = new List<double>();
        private List<ushort> m_indices = new List<ushort>();

        private ushort m_index_max;
        private ushort m_index_min;
        
        private double[] m_v_max = new double[3]{double.MinValue, double.MinValue, double.MinValue};
        private double[] m_v_min= new double[3]{double.MaxValue, double.MaxValue, double.MaxValue};
        private double[] m_n_max = new double[3]{double.MinValue, double.MinValue, double.MinValue};
        private double[] m_n_min = new double[3]{double.MaxValue, double.MaxValue, double.MaxValue};


        private ushort m_current_vertex_index = 0;

        /// <summary>
        /// The maximum vertex.
        /// </summary>
        /// <returns></returns>
        public double[] VMax
        {
            get{return m_v_max;}
        }

        /// <summary>
        /// The minimum vertex.
        /// </summary>
        /// <returns></returns>
        public double[] VMin
        {
            get{return m_v_min;}
        }

        /// <summary>
        /// The maximum normal.
        /// </summary>
        /// <returns></returns>
        public double[] NMax
        {
            get{return m_n_max;}
        }

        /// <summary>
        /// The minimum normal.
        /// </summary>
        public double[] NMin
        {
            get{return m_n_min;}
        }

        /// <summary>
        /// The maximum index.
        /// </summary>
        public ushort IMax
        {
            get{return m_index_max;}
        }

        /// <summary>
        /// The minimum index.
        /// </summary>
        public ushort IMin
        {
            get{return m_index_min;}
        }

        /// <summary>
        /// The vertices of the mesh.
        /// </summary>
        public List<double> Vertices
        {
            get{return m_vertices;}
        }

        /// <summary>
        /// The normals of the mesh.
        /// </summary>
        public List<double> Normals
        {
            get{return m_normals;}
        }
        
        /// <summary>
        /// The indices of the mesh.
        /// </summary>
        public List<ushort> Indices
        {
            get{return m_indices;}
        }

        /// <summary>
        /// Construct an empty mesh.
        /// </summary>
        public Mesh()
        {
            // An empty mesh.
        }

        /// <summary>
        /// Construct a mesh from vertices, normals, and indices.
        /// </summary>
        /// <param name="vertices">An array containing doubles of the form [x1, y1, z1, x2, y2, z2...].</param>
        /// <param name="normals">An array containing doubles of the form [nx1, ny1, nz1, nx2, ny2, nz2...]</param>
        /// <param name="indices">An array containing integers of the form [0, 1, 2, 0, 2, 3...].</param>
        public Mesh(double[] vertices, double[] normals, ushort[] indices)
        {
            this.m_vertices.AddRange(vertices);
            this.m_normals.AddRange(normals);
            this.m_indices.AddRange(indices);
        }

        /// <summary>
        /// Add a triangle to the mesh.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="n"></param>
        internal void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 n = null)
        {
            if(n == null)
            {
                var v1 = b - a;
                var v2 = c - a;
                n = v1.Cross(v2).Normalized();
                if(Double.IsNaN(n.X) || Double.IsNaN(n.Y) || Double.IsNaN(n.Z))
                {
                    Debug.WriteLine("Degenerate triangle found.");
                    return;
                }
            }

            AddVertex(a, n);
            AddVertex(b, n);
            AddVertex(c, n);

            var start = m_current_vertex_index;
            this.m_indices.AddRange(new []{start,(ushort)(start+1),(ushort)(start+2)});
            m_index_max = Math.Max(m_index_max, start);
            m_index_max = Math.Max(m_index_max, (ushort)(start+1));
            m_index_max = Math.Max(m_index_max, (ushort)(start+2));
            m_index_min = Math.Min(m_index_min, start);
            m_index_min = Math.Min(m_index_min, (ushort)(start+1));
            m_index_min = Math.Min(m_index_min, (ushort)(start+2));
            m_current_vertex_index += 3;
        }

        /// <summary>
        /// Add a triangle to the mesh.
        /// </summary>
        /// <param name="v"></param>
        internal void AddTriangle(IList<Vector3> v)
        {
            AddTriangle(v[0], v[1], v[2]);
        }

        internal void AddVertex(Vector3 v, Vector3 n)
        {
            var vArr = v.ToArray();
            var nArr = n.ToArray();
        
            this.m_vertices.AddRange(v.ToArray());
            this.m_normals.AddRange(n.ToArray());
            
            m_v_max[0] = Math.Max(m_v_max[0], vArr[0]);
            m_v_max[1] = Math.Max(m_v_max[1], vArr[1]);
            m_v_max[2] = Math.Max(m_v_max[2], vArr[2]);
            m_v_min[0] = Math.Min(m_v_min[0], vArr[0]);
            m_v_min[1] = Math.Min(m_v_min[1], vArr[1]);
            m_v_min[2] = Math.Min(m_v_min[2], vArr[2]);

            m_n_max[0] = Math.Max(m_n_max[0], nArr[0]);
            m_n_max[1] = Math.Max(m_n_max[1], nArr[1]);
            m_n_max[2] = Math.Max(m_n_max[2], nArr[2]);
            m_n_min[0] = Math.Min(m_n_min[0], nArr[0]);
            m_n_min[1] = Math.Min(m_n_min[1], nArr[1]);
            m_n_min[2] = Math.Min(m_n_min[2], nArr[2]);
        }

        /// <summary>
        /// Add two triangles to the mesh by splitting a rectangular region in two.
        /// </summary>
        /// <param name="vertices"></param>
        internal void AddQuad(IList<Vector3> vertices)
        {
            var v1 = vertices[1] - vertices[0];
            var v2 = vertices[2] - vertices[0];
            var n1 = v1.Cross(v2).Normalized();
            if(Double.IsNaN(n1.X) || Double.IsNaN(n1.Y) || Double.IsNaN(n1.Z))
            {
                Console.WriteLine("Degenerate triangle found.");
                Console.WriteLine(v1);
                Console.WriteLine(v2);
                return;
            }

            AddVertex(vertices[0], n1);
            AddVertex(vertices[1], n1);
            AddVertex(vertices[2], n1);
            AddVertex(vertices[3], n1);

            var start = m_current_vertex_index;
            this.m_indices.AddRange(new[]{start,(ushort)(start+1),(ushort)(start+2),(ushort)(start),(ushort)(start+2),(ushort)(start+3)});
            m_index_max = Math.Max(m_index_max, start);
            m_index_max = Math.Max(m_index_max, (ushort)(start+1));
            m_index_max = Math.Max(m_index_max, (ushort)(start+2));
            m_index_max = Math.Max(m_index_max, (ushort)(start+3));
            m_index_min = Math.Min(m_index_min, start);
            m_index_min = Math.Min(m_index_min, (ushort)(start+1));
            m_index_min = Math.Min(m_index_min, (ushort)(start+2));
            m_index_min = Math.Min(m_index_min, (ushort)(start+3));
            m_current_vertex_index += 4;
        }

        /// <summary>
        /// Get a string representation of the mesh.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $@"
Vertices:{m_vertices.Count/3}, 
Normals:{m_normals.Count/3}, 
Indices:{m_indices.Count}, 
VMax:{string.Join(",", m_v_max)}, 
VMin:{string.Join(",", m_v_min)}, 
NMax:{string.Join(",", m_n_max)}, 
NMin:{string.Join(",", m_n_min)}, 
IMax:{m_index_max}, 
IMin:{m_index_min}";
        }
    }

    internal static class TessExtensions
    {
        internal static Vector3 ToVector3(this Vec3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }
}