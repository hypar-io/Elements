using LibTessDotNet.Double;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Hypar.Geometry
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

        private List<double> m_vertex_colors = new List<double>();

        private ushort m_current_vertex_index = 0;

        public double[] VMax
        {
            get{return m_v_max;}
        }
        public double[] VMin
        {
            get{return m_v_min;}
        }
        public double[] NMax
        {
            get{return m_n_max;}
        }
        public double[] NMin
        {
            get{return m_n_min;}
        }
        public double[] CMax
        {
            get{return m_c_max;}
        }
        public double[] CMin
        {
            get{return m_c_min;}
        }

        public ushort IMax
        {
            get{return m_index_max;}
        }

        public ushort IMin
        {
            get{return m_index_min;}
        }

        public IEnumerable<double> Vertices
        {
            get{return m_vertices;}
        }

        public IEnumerable<double> Normals
        {
            get{return m_normals;}
        }

        public IEnumerable<ushort> Indices
        {
            get{return m_indices;}
        }

        public IEnumerable<double> VertexColors
        {
            get{return m_vertex_colors;}
        }

        public Mesh()
        {
            // An empty mesh.
        }

        public Mesh(double[] vertices, double[] normals, ushort[] indices)
        {
            this.m_vertices.AddRange(vertices);
            this.m_normals.AddRange(normals);
            this.m_indices.AddRange(indices);
        }

        public void AddTri(Vector3 a, Vector3 b, Vector3 c, Color ac = null, Color bc = null, Color cc = null)
        {
            var v1 = b - a;
            var v2 = c - a;
            var n1 = v1.Cross(v2).Normalized();
            if(Double.IsNaN(n1.X) || Double.IsNaN(n1.Y) || Double.IsNaN(n1.Z))
            {
                Debug.WriteLine("Degenerate triangle found.");
                return;
            }

            if(ac != null && bc != null && cc != null)
            {
                AddVertex(a, n1, ac);
                AddVertex(b, n1, bc);
                AddVertex(c, n1, cc);
            }
            else
            {
                AddVertex(a, n1);
                AddVertex(b, n1);
                AddVertex(c, n1);
            }

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

        public void AddTri(Vector3[] v, Color[] c = null)
        {
            if(c != null)
            {
                AddTri(v[0], v[1], v[2], c[0], c[1], c[2]);
            }
            else
            {
                AddTri(v[0], v[1], v[2]);
            }
        }

        private void AddVertex(Vector3 v, Vector3 n, Color c = null)
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

            if(c != null)
            {
                var cArr = c.ToArray();
                this.m_vertex_colors.AddRange(c.ToArray());

                m_c_max[0] = Math.Max(m_c_max[0], cArr[0]);
                m_c_max[1] = Math.Max(m_c_max[1], cArr[1]);
                m_c_max[2] = Math.Max(m_c_max[2], cArr[2]);
                m_c_max[3] = Math.Max(m_c_max[3], cArr[3]);
                m_c_min[0] = Math.Min(m_c_min[0], cArr[0]);
                m_c_min[1] = Math.Min(m_c_min[1], cArr[1]);
                m_c_min[2] = Math.Min(m_c_min[2], cArr[2]);
                m_c_min[3] = Math.Min(m_c_min[3], cArr[3]);
            }
        }

        public void AddQuad(Vector3[] v, Color[] c = null)
        {
            var v1 = v[1] - v[0];
            var v2 = v[2] - v[0];
            var n1 = v1.Cross(v2).Normalized();
            if(Double.IsNaN(n1.X) || Double.IsNaN(n1.Y) || Double.IsNaN(n1.Z))
            {
                Console.WriteLine("Degenerate triangle found.");
                return;
            }

            if(c != null)
            {
                AddVertex(v[0], n1, c[0]);
                AddVertex(v[1], n1, c[1]);
                AddVertex(v[2], n1, c[2]);
                AddVertex(v[3], n1, c[3]);
            }
            else
            {
                AddVertex(v[0], n1);
                AddVertex(v[1], n1);
                AddVertex(v[2], n1);
                AddVertex(v[3], n1);
            }

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

        public void AddTesselatedFace(IEnumerable<Polyline> perimeter, double height=0.0, bool reverse = false)
        {
            var tess = new Tess();

            foreach(var b in perimeter)
            {
                var numPoints = b.Vertices.Count();
                var contour = new ContourVertex[numPoints];
                for(var i=0; i<numPoints; i++)
                {
                    var v = b.Vertices.ElementAt(i);
                    contour[i].Position = new Vec3{X=v.X, Y=v.Y, Z=height};
                }
                tess.AddContour(contour, reverse?ContourOrientation.Clockwise:ContourOrientation.CounterClockwise);
            }
            tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);
            for(var i=0; i<tess.ElementCount; i++)
            {
                var a = tess.Vertices[tess.Elements[i * 3]].Position.ToVector3();
                var b = tess.Vertices[tess.Elements[i * 3 + 1]].Position.ToVector3();
                var c = tess.Vertices[tess.Elements[i * 3 + 2]].Position.ToVector3();
                AddTri(a,b,c);
            }
        }

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
        /// Add a Plane to the Model.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <param name="materialId"></param>
        public static Mesh Plane(double width, double length)
        {
            var vertices = new[]{-width,-length,0.0,width,-length,0.0,width,length,0.0,-width,length,0.0};
            var normals = new[]{0.0,0.0,1.0,0.0,0.0,1.0,0.0,0.0,1.0,0.0,0.0,1.0};
            var indices = new ushort[]{0,1,2,0,2,3};

            var mesh = new Hypar.Geometry.Mesh(vertices, normals, indices);
            return mesh;
        }

        public static Mesh Extrude(IEnumerable<Polyline> perimeters, double height, bool capped=true)
        {
            var mesh = new Hypar.Geometry.Mesh();

            foreach(var boundary in perimeters)
            {
                for(var i=0; i<boundary.Vertices.Count(); i++)
                {
                    Vector3 a;
                    Vector3 b;

                    if(i == boundary.Vertices.Count()-1)
                    {
                        a = boundary.Vertices.ElementAt(i);
                        b = boundary.Vertices.ElementAt(0);
                    }
                    else
                    {
                        a = boundary.Vertices.ElementAt(i);
                        b = boundary.Vertices.ElementAt(i+1);
                    }

                    var v1 = new Vector3(a.X, a.Y, 0.0);
                    var v2 = new Vector3(b.X, b.Y, 0.0);
                    var v3 = new Vector3(b.X, b.Y, height);
                    var v4 = new Vector3(a.X, a.Y, height);
                    mesh.AddQuad(new []{v1,v2,v3,v4});
                }
            }

            if(capped)
            {
                mesh.AddTesselatedFace(perimeters);
                mesh.AddTesselatedFace(perimeters, height, true);
            }

            return mesh;
        }

        public static Mesh ExtrudeAlongLine(Line line, IEnumerable<Polyline> perimeters, bool capped=true)
        {
            var height = line.Length();
            return Mesh.Extrude(perimeters, height, capped);
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