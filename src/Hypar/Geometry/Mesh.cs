using LibTessDotNet.Double;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        private List<int> m_indices = new List<int>();

        private int m_current_vertex_index = 0;

        public IEnumerable<double> Vertices
        {
            get{return m_vertices;}
        }

        public IEnumerable<double> Normals
        {
            get{return m_normals;}
        }

        public IEnumerable<int> Indices
        {
            get{return m_indices;}
        }

        public Mesh()
        {
            // An empty mesh.
        }

        public Mesh(double[] vertices, double[] normals, int[] indices)
        {
            this.m_vertices.AddRange(vertices);
            this.m_normals.AddRange(normals);
            this.m_indices.AddRange(indices);
        }

        public void AddTri(Vector3 a, Vector3 b, Vector3 c)
        {
            var v1 = b - a;
            var v2 = c - a;
            var n1 = v1.Cross(v2).Normalized();
            if(Double.IsNaN(n1.X) || Double.IsNaN(n1.Y) || Double.IsNaN(n1.Z))
            {
                Debug.WriteLine("Degenerate triangle found.");
                return;
            }

            this.m_normals.AddRange(n1.ToArray());
            this.m_normals.AddRange(n1.ToArray());
            this.m_normals.AddRange(n1.ToArray());

            this.m_vertices.AddRange(a.ToArray());
            this.m_vertices.AddRange(b.ToArray());
            this.m_vertices.AddRange(c.ToArray());

            var start = m_current_vertex_index;
            this.m_indices.AddRange(new[]{start,start+1,start+2});
            m_current_vertex_index += 3;
        }

        public void AddTri(Vector3[] v)
        {
            AddTri(v[0], v[1], v[2]);
        }

        public void AddQuad(Vector3[] v)
        {
            var v1 = v[1] - v[0];
            var v2 = v[2] - v[0];
            var n1 = v1.Cross(v2).Normalized();
            if(Double.IsNaN(n1.X) || Double.IsNaN(n1.Y) || Double.IsNaN(n1.Z))
            {
                Console.WriteLine("Degenerate triangle found.");
                return;
            }

            this.m_normals.AddRange(n1.ToArray());
            this.m_normals.AddRange(n1.ToArray());
            this.m_normals.AddRange(n1.ToArray());
            this.m_normals.AddRange(n1.ToArray());

            this.m_vertices.AddRange(v[0].ToArray());
            this.m_vertices.AddRange(v[1].ToArray());
            this.m_vertices.AddRange(v[2].ToArray());
            this.m_vertices.AddRange(v[3].ToArray());

            var start = m_current_vertex_index;
            this.m_indices.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});
            m_current_vertex_index += 4;
        }

        public void AddTesselatedFace(IEnumerable<Polygon2> perimeter, double height=0.0, bool reverse = false)
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
            return $"Vertices:{m_vertices.Count/3}, Normals:{m_normals.Count/3}, Indices:{m_indices.Count}";
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
            var indices = new[]{0,1,2,0,2,3};

            var mesh = new Hypar.Geometry.Mesh(vertices, normals, indices);
            return mesh;
        }

        public static Mesh Extrude(IEnumerable<Polygon2> boundaries, double height, bool capped=true)
        {
            var mesh = new Hypar.Geometry.Mesh();

            foreach(var boundary in boundaries)
            {
                for(var i=0; i<boundary.Vertices.Count(); i++)
                {
                    Vector2 a;
                    Vector2 b;

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
                mesh.AddTesselatedFace(boundaries);
                mesh.AddTesselatedFace(boundaries, height, true);
            }

            return mesh;
        }

        public static Mesh ExtrudeAlongLine(Line line, IEnumerable<Polygon2> boundaries, bool capped=true)
        {
            var height = line.Length();
            return Mesh.Extrude(boundaries, height, capped);
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