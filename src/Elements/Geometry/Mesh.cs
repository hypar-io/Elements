using Elements.Geometry.Interfaces;
using LibTessDotNet.Double;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

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
        private double[] m_c_max = new double[4]{double.MinValue, double.MinValue, double.MinValue, double.MaxValue};
        private double[] m_c_min = new double[4]{double.MinValue, double.MinValue, double.MinValue, double.MinValue};

        private List<float> m_vertex_colors = new List<float>();

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
        /// <returns></returns>
        public double[] NMin
        {
            get{return m_n_min;}
        }

        /// <summary>
        /// The maximum color.
        /// </summary>
        /// <returns></returns>
        public double[] CMax
        {
            get{return m_c_max;}
        }

        /// <summary>
        /// The minimum color.
        /// </summary>
        /// <returns></returns>
        public double[] CMin
        {
            get{return m_c_min;}
        }

        /// <summary>
        /// The maximum index.
        /// </summary>
        /// <returns></returns>
        public ushort IMax
        {
            get{return m_index_max;}
        }

        /// <summary>
        /// The minimum index.
        /// </summary>
        /// <returns></returns>
        public ushort IMin
        {
            get{return m_index_min;}
        }

        /// <summary>
        /// The vertices of the mesh.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<double> Vertices
        {
            get{return m_vertices;}
        }

        /// <summary>
        /// The normals of the mesh.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<double> Normals
        {
            get{return m_normals;}
        }
        
        /// <summary>
        /// The indices of the mesh.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ushort> Indices
        {
            get{return m_indices;}
        }

        /// <summary>
        /// The vertex colors of the mesh.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<float> VertexColors
        {
            get{return m_vertex_colors;}
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

        internal static Tess TessFromPolygons(Polygon[] polygons)
        {
            var tess = new Tess();
            tess.NoEmptyPolygons = true;

            foreach(var p in polygons)
            {
                AddContour(tess, p);
            }

            return tess;
        }

        internal static void AddContour(Tess tess, Polygon p)
        {
            var numPoints = p.Vertices.Length;
            var contour = new ContourVertex[numPoints];
            for(var i=0; i<numPoints; i++)
            {
                var v = p.Vertices[i];
                contour[i].Position = new Vec3{X=v.X, Y=v.Y, Z=v.Z};
            }

            tess.AddContour(contour);
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
VMax:{string.Join(",", m_v_max.Select(v=>v.ToString()))}, 
VMin:{string.Join(",", m_v_min.Select(v=>v.ToString()))}, 
NMax:{string.Join(",", m_n_max.Select(v=>v.ToString()))}, 
NMin:{string.Join(",", m_n_min.Select(v=>v.ToString()))}, 
IMax:{m_index_max}, 
IMin:{m_index_min}";
        }

        /// <summary>
        /// Create a ruled loft between sections.
        /// </summary>
        /// <param name="sections"></param>
        public static Mesh Loft(IList<Polygon> sections)
        {
            var mesh = new Elements.Geometry.Mesh();

            for(var i=0; i<sections.Count; i++)
            {
                var p1 = sections[i];
                var p2 = i == sections.Count-1 ? sections[0] : sections[i+1];

                for(var j=0; j<p1.Vertices.Length; j++)
                {
                    var j1 = j == p1.Vertices.Length - 1 ? 0 : j+1;
                    var v1 = p1.Vertices[j];
                    var v2 = p1.Vertices[j1];
                    var v3 = p2.Vertices[j1];
                    var v4 = p2.Vertices[j];
                    mesh.AddQuad(new []{v1,v2,v3,v4});
                }
            }

            return mesh;
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