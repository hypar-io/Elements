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
        /// <param name="ac"></param>
        /// <param name="bc"></param>
        /// <param name="cc"></param>
        public void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Color ac = null, Color bc = null, Color cc = null)
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

        /// <summary>
        /// Add a triangle to the mesh.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="c"></param>
        public void AddTriangle(IList<Vector3> v, Color[] c = null)
        {
            if(c != null)
            {
                AddTriangle(v[0], v[1], v[2], c[0], c[1], c[2]);
            }
            else
            {
                AddTriangle(v[0], v[1], v[2]);
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

        /// <summary>
        /// Add two triangles to the mesh by splitting a rectangular region in two.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="colors"></param>
        public void AddQuad(IList<Vector3> vertices, Color[] colors = null)
        {
            var v1 = vertices[1] - vertices[0];
            var v2 = vertices[2] - vertices[0];
            var n1 = v1.Cross(v2).Normalized();
            if(Double.IsNaN(n1.X) || Double.IsNaN(n1.Y) || Double.IsNaN(n1.Z))
            {
                Console.WriteLine("Degenerate triangle found.");
                return;
            }

            if(colors != null)
            {
                AddVertex(vertices[0], n1, colors[0]);
                AddVertex(vertices[1], n1, colors[1]);
                AddVertex(vertices[2], n1, colors[2]);
                AddVertex(vertices[3], n1, colors[3]);
            }
            else
            {
                AddVertex(vertices[0], n1);
                AddVertex(vertices[1], n1);
                AddVertex(vertices[2], n1);
                AddVertex(vertices[3], n1);
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

        /// <summary>
        /// Add a tessellated face to the mesh.
        /// </summary>
        /// <param name="perimeter">A Polygon representing the perimeter of the face.</param>
        /// <param name="voids">A collection of Polygons representing voids in the face.</param>
        /// <param name="transform">A Transform to apply to all vertices of the supplied perimeter.</param>
        /// <param name="reversed">A flag indicating whether the winding of the Polygons should be reversed.</param>
        public void AddTesselatedFace(Polygon perimeter, IList<Polygon> voids, Transform transform, bool reversed = false)
        {
            var tess = new Tess();

            AddContour(tess, perimeter, reversed);

            if(voids != null)
            {
                foreach(var p in voids)
                {
                    AddContour(tess, p, reversed);
                }
            }
            
            tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);

            for(var i=0; i<tess.ElementCount; i++)
            {
                var a = tess.Vertices[tess.Elements[i * 3]].Position.ToVector3();
                var b = tess.Vertices[tess.Elements[i * 3 + 1]].Position.ToVector3();
                var c = tess.Vertices[tess.Elements[i * 3 + 2]].Position.ToVector3();
                AddTriangle(transform.OfPoint(a), transform.OfPoint(b), transform.OfPoint(c));
            }
        }

        private void AddContour(Tess tess, Polygon p, bool reversed = false)
        {
            var numPoints = p.Vertices.Count;
            var contour = new ContourVertex[numPoints];
            for(var i=0; i<numPoints; i++)
            {
                var v = p.Vertices[i];
                contour[i].Position = new Vec3{X=v.X, Y=v.Y, Z=v.Z};
            }
            tess.AddContour(contour, reversed ? ContourOrientation.Clockwise : ContourOrientation.CounterClockwise);
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
        /// Add a Plane to the Model.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="length"></param>
        public static Mesh Plane(double width, double length)
        {
            var vertices = new[]{-width,-length,0.0,width,-length,0.0,width,length,0.0,-width,length,0.0};
            var normals = new[]{0.0,0.0,1.0,0.0,0.0,1.0,0.0,0.0,1.0,0.0,0.0,1.0};
            var indices = new ushort[]{0,1,2,0,2,3};

            var mesh = new Hypar.Geometry.Mesh(vertices, normals, indices);
            return mesh;
        }

        /// <summary>
        /// Extrude a Polyon.
        /// </summary>
        /// <param name="perimeter">The Polygon to extrude.</param>
        /// <param name="voids">A collection of Polygons representing voids in the extrusion.</param> 
        /// <param name="height">The height of the extrusion.</param>
        /// <param name="capped">A flag indicating whether the extrusion should be capped.</param>
        /// <returns></returns>
        public static Mesh Extrude(Polygon perimeter, double height, IList<Polygon> voids = null, bool capped=true)
        {
            var mesh = new Hypar.Geometry.Mesh();

            var perimeters = new List<Polygon>();
            perimeters.Add(perimeter);
            if(voids != null)
            {
                perimeters.AddRange(voids);
            }

            foreach(var boundary in perimeters)
            {
                var verts = boundary.Vertices;

                for(var i=0; i<verts.Count; i++)
                {
                    Vector3 a;
                    Vector3 b;

                    if(i == verts.Count-1)
                    {
                        a = verts[i];
                        b = verts[0];
                    }
                    else
                    {
                        a = verts[i];
                        b = verts[i+1];
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
                mesh.AddTesselatedFace(perimeter, voids, new Transform());
                mesh.AddTesselatedFace(perimeter, voids, new Transform(new Vector3(0,0,height)), true);
            }

            return mesh;
        }

        /// <summary>
        /// Extrude a Polygon along a Line.
        /// </summary>
        /// <param name="line">The Line along which to extrude.</param>
        /// <param name="perimeter">A Polygon to extrude.</param>
        /// <param name="voids">A collection of Polygons representing voids in the extrusion.</param>
        /// <param name="capped">A flag indicating whether the extrusion should be capped.</param>
        /// <returns></returns>
        public static Mesh ExtrudeAlongLine(Line line, Polygon perimeter, IList<Polygon> voids = null, bool capped=true)
        {
            var mesh = new Hypar.Geometry.Mesh();

            var tStart = line.GetTransform(0.0);
            var tEnd = line.GetTransform(1.0);

            ExtrudePolygon(ref mesh, perimeter, tStart, tEnd);

            if(voids != null)
            {
                foreach(var p in voids)
                {
                    ExtrudePolygon(ref mesh, p, tStart, tEnd, true);
                }
            }

            if(capped)
            {
                mesh.AddTesselatedFace(perimeter, voids, tStart);
                mesh.AddTesselatedFace(perimeter, voids, tEnd, true);
            }

            return mesh;
        }

        private static void ExtrudePolygon(ref Mesh mesh, Polygon p, Transform tStart, Transform tEnd, bool reverse = false)
        {
            var start = tStart.OfPolygon(p);
            var end = tEnd.OfPolygon(p);

            for(var i=0; i<start.Vertices.Count; i++)
            {
                Vector3 a, b, c, d;

                if(i == start.Vertices.Count-1)
                {
                    a = start.Vertices[i];
                    b = start.Vertices[0];
                    c = end.Vertices[0];
                    d = end.Vertices[i];
                }
                else
                {
                    a = start.Vertices[i];
                    b = start.Vertices[i+1];
                    c = end.Vertices[i+1];
                    d = end.Vertices[i];
                }

                if(reverse)
                {
                    mesh.AddQuad(new []{a,d,c,b});
                }
                else
                {
                    mesh.AddQuad(new []{a,b,c,d});
                }
            }
        }

        /// <summary>
        /// Create a ruled loft between sections.
        /// </summary>
        /// <param name="sections"></param>
        public static Mesh Loft(IList<Polygon> sections)
        {
            var mesh = new Hypar.Geometry.Mesh();

            for(var i=0; i<sections.Count; i++)
            {
                var p1 = sections[i];
                var p2 = i == sections.Count-1 ? sections[0] : sections[i+1];

                for(var j=0; j<p1.Vertices.Count; j++)
                {
                    var j1 = j == p1.Vertices.Count - 1 ? 0 : j+1;
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