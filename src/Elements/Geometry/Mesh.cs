using LibTessDotNet.Double;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Elements.Geometry
{
    /// <summary>
    /// An indexed mesh used for storing data for gltf.
    /// </summary>
    public class Mesh
    {
        private List<double> m_vertices = new List<double>();
        private List<double> m_normals = new List<double>();
        private List<ushort> m_indices = new List<ushort>();
        private List<float> m_colors = new List<float>();

        private ushort m_index_max;
        private ushort m_index_min;
        
        private double[] m_v_max = new double[3]{double.MinValue, double.MinValue, double.MinValue};
        private double[] m_v_min= new double[3]{double.MaxValue, double.MaxValue, double.MaxValue};
        private double[] m_n_max = new double[3]{double.MinValue, double.MinValue, double.MinValue};
        private double[] m_n_min = new double[3]{double.MaxValue, double.MaxValue, double.MaxValue};

        private float[] m_c_min = new float[]{float.MaxValue, float.MaxValue, float.MaxValue};
        private float[] m_c_max = new float[]{float.MinValue, float.MinValue, float.MinValue};

        /// <summary>
        /// The maximum vertex.
        /// </summary>
        public double[] VMax
        {
            get{return m_v_max;}
        }

        /// <summary>
        /// The minimum vertex.
        /// </summary>
        public double[] VMin
        {
            get{return m_v_min;}
        }

        /// <summary>
        /// The maximum normal.
        /// </summary>
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
        /// The minimum color.
        /// </summary>
        public float[] CMin
        {
            get{return m_c_min;}
        }

        /// <summary>
        /// The maximum color.
        /// </summary>
        public float[] CMax
        {
            get{return m_c_max;}
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
        /// The colors of the mesh.
        /// </summary>
        public List<float> Colors
        {
            get{return m_colors;}
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
        /// <param name="colors">An array containing floats of the form [r,g,b,r,g,b...]</param>
        public Mesh(double[] vertices, double[] normals, ushort[] indices, float[] colors)
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
        /// <param name="an"></param>
        /// <param name="bn"></param>
        /// <param name="cn"></param>
        /// <param name="colorizer">A function used to determine the color of the triangle. The function's sole argument is the normal of the triangle.</param>
        internal void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 an = null, Vector3 bn = null, Vector3 cn = null, Func<Vector3,Color> colorizer = null)
        {
            // Calculate the face normal
            var v1 = b - a;
            var v2 = c - a;
            var n = v1.Cross(v2).Normalized();
            if(Double.IsNaN(n.X) || Double.IsNaN(n.Y) || Double.IsNaN(n.Z))
            {
                Debug.WriteLine("Degenerate triangle found.");
                return;
            }

            var color = colorizer(n);
            
            AddVertex(a, an, color);
            AddVertex(b, bn, color);
            AddVertex(c, cn, color);
        }

        internal int AddVertex(Vector3 v, Vector3 n, Color c = null)
        {
            var vArr = v.ToArray();
            var nArr = n.ToArray();
            
            this.m_vertices.AddRange(vArr);
            this.m_normals.AddRange(nArr);

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

            if(c != null)
            {
                var cArr = c.ToArray();
                this.m_colors.AddRange(cArr);
                m_c_max[0] = Math.Max(m_c_max[0], cArr[0]);
                m_c_max[1] = Math.Max(m_c_max[1], cArr[1]);
                m_c_max[2] = Math.Max(m_c_max[2], cArr[2]);
                m_c_min[0] = Math.Min(m_c_min[0], cArr[0]);
                m_c_min[1] = Math.Min(m_c_min[1], cArr[1]);
                m_c_min[2] = Math.Min(m_c_min[2], cArr[2]);
            }

            var start = (ushort)m_indices.Count;
            this.m_indices.Add(start);
            m_index_max = Math.Max(m_index_max, start);
            m_index_min = Math.Min(m_index_min, start);

            return (this.m_vertices.Count/3)-1;
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

            var start = (ushort)m_indices.Count;
            this.m_indices.AddRange(new[]{start,(ushort)(start+1),(ushort)(start+2),(ushort)(start),(ushort)(start+2),(ushort)(start+3)});
            m_index_max = Math.Max(m_index_max, start);
            m_index_max = Math.Max(m_index_max, (ushort)(start+1));
            m_index_max = Math.Max(m_index_max, (ushort)(start+2));
            m_index_max = Math.Max(m_index_max, (ushort)(start+3));
            m_index_min = Math.Min(m_index_min, start);
            m_index_min = Math.Min(m_index_min, (ushort)(start+1));
            m_index_min = Math.Min(m_index_min, (ushort)(start+2));
            m_index_min = Math.Min(m_index_min, (ushort)(start+3));
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