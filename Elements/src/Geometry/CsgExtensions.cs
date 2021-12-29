using Elements.Geometry.Tessellation;
using System;
using System.Collections.Generic;
using System.Linq;
using LibTessDotNet.Double;

namespace Elements.Geometry
{
    internal static class CsgExtensions
    {
        /// <summary>
        /// Write the csg into a mesh.
        /// </summary>
        internal static void Tessellate(this Csg.Solid csg, ref Mesh mesh)
        {
            foreach (var p in csg.Polygons)
            {
                p.AddToMesh(ref mesh);
            }
            mesh.ComputeNormals();
        }

        /// <summary>
        /// Triangulate this csg and pack the triangulated data into buffers
        /// appropriate for use with gltf.
        /// </summary>
        internal static GraphicsBuffers Tessellate(this Csg.Solid csg,
                                                   bool mergeVertices = false,
                                                   Func<(Vector3, Vector3, UV, Color), (Vector3, Vector3, UV, Color)> modifyVertexAttributes = null)
        {
            return Tessellate(new[] { csg }, mergeVertices, modifyVertexAttributes);
        }

        /// <summary>
        /// Triangulate a collection of CSGs and pack the triangulated data into
        /// buffers appropriate for use with gltf. 
        /// </summary>
        internal static GraphicsBuffers Tessellate(this Csg.Solid[] csgs,
                                                   bool mergeVertices = false,
                                                   Func<(Vector3, Vector3, UV, Color), (Vector3, Vector3, UV, Color)> modifyVertexAttributes = null)
        {
            var buffers = new GraphicsBuffers();

            Tessellation.Tessellation.Tessellate(csgs.Select(csg => new CsgTessellationTargetProvider(csg)),
                                    buffers,
                                    mergeVertices,
                                    modifyVertexAttributes);
            return buffers;
        }

        internal static Csg.Matrix4x4 ToMatrix4x4(this Transform transform)
        {
            var m = transform.Matrix;
            return new Csg.Matrix4x4(new[]{
                m.m11, m.m12, m.m13, 0.0,
                m.m21, m.m22, m.m23, 0.0,
                m.m31, m.m32, m.m33, 0.0,
                m.tx, m.ty, m.tz, 1.0
            });
        }

        internal static Csg.Solid ToCsg(this Mesh mesh)
        {
            var vertices = new List<Csg.Vertex>();
            foreach (var v in mesh.Vertices)
            {
                var vv = new Csg.Vertex(v.Position.ToCsgVector3(), v.UV.ToCsgVector2());
                vertices.Add(vv);
            }

            var polygons = new List<Csg.Polygon>();
            foreach (var t in mesh.Triangles)
            {
                var a = vertices[t.Vertices[0].Index];
                var b = vertices[t.Vertices[1].Index];
                var c = vertices[t.Vertices[2].Index];
                var polygon = new Csg.Polygon(new List<Csg.Vertex>() { a, b, c });
                polygons.Add(polygon);
            }

            return Csg.Solid.FromPolygons(polygons);
        }

        private static void AddToMesh(this Csg.Polygon p, ref Mesh mesh)
        {
            var n = p.Plane.Normal.ToElementsVector();

            if (p.Vertices.Count == 3)
            {
                // Don't tessellate unless we need to.

                var a = p.Vertices[0];
                var b = p.Vertices[1];
                var c = p.Vertices[2];
                var av = mesh.AddVertex(a.Pos.ToElementsVector(), a.Tex.ToUV(), n, merge: true);
                var bv = mesh.AddVertex(b.Pos.ToElementsVector(), b.Tex.ToUV(), n, merge: true);
                var cv = mesh.AddVertex(c.Pos.ToElementsVector(), c.Tex.ToUV(), n, merge: true);

                var t = new Triangle(av, bv, cv);
                if (!t.HasDuplicatedVertices(out Vector3 _))
                {
                    mesh.AddTriangle(t);
                }
            }
            else if (p.Vertices.Count == 4)
            {
                // Don't tessellate unless we need to.

                var a = p.Vertices[0];
                var b = p.Vertices[1];
                var c = p.Vertices[2];
                var d = p.Vertices[3];
                var av = mesh.AddVertex(a.Pos.ToElementsVector(), a.Tex.ToUV(), n, merge: true);
                var bv = mesh.AddVertex(b.Pos.ToElementsVector(), b.Tex.ToUV(), n, merge: true);
                var cv = mesh.AddVertex(c.Pos.ToElementsVector(), c.Tex.ToUV(), n, merge: true);
                var dv = mesh.AddVertex(d.Pos.ToElementsVector(), d.Tex.ToUV(), n, merge: true);

                var t = new Triangle(av, bv, cv);
                if (!t.HasDuplicatedVertices(out Vector3 _))
                {
                    mesh.AddTriangle(t);
                }

                var t1 = new Triangle(av, cv, dv);
                if (!t1.HasDuplicatedVertices(out Vector3 _))
                {
                    mesh.AddTriangle(t1);
                }
            }
            else
            {
                // Polygons coming back from Csg can have an arbitrary number
                // of vertices. We need to retessellate the returned polygon.
                var tess = new Tess();
                tess.NoEmptyPolygons = true;

                tess.AddContour(p.Vertices.ToContourVertices());

                tess.Tessellate(WindingRule.Positive, LibTessDotNet.Double.ElementType.Polygons, 3);
                for (var i = 0; i < tess.ElementCount; i++)
                {
                    var a = tess.Vertices[tess.Elements[i * 3]].Position.ToVector3();
                    var b = tess.Vertices[tess.Elements[i * 3 + 1]].Position.ToVector3();
                    var c = tess.Vertices[tess.Elements[i * 3 + 2]].Position.ToVector3();

                    var uva = (Csg.Vector2D)tess.Vertices[tess.Elements[i * 3]].Data;
                    var uvb = (Csg.Vector2D)tess.Vertices[tess.Elements[i * 3 + 1]].Data;
                    var uvc = (Csg.Vector2D)tess.Vertices[tess.Elements[i * 3 + 2]].Data;

                    var v1 = mesh.AddVertex(a, uva.ToUV(), n, merge: true);
                    var v2 = mesh.AddVertex(b, uvb.ToUV(), n, merge: true);
                    var v3 = mesh.AddVertex(c, uvc.ToUV(), n, merge: true);

                    var t = new Triangle(v1, v2, v3);
                    if (!t.HasDuplicatedVertices(out Vector3 _))
                    {
                        mesh.AddTriangle(t);
                    }
                }
            }
        }

        private static Vector3 ToElementsVector(this Csg.Vector3D v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        private static UV ToUV(this Csg.Vector2D uv)
        {
            return new UV(uv.X, uv.Y);
        }

        internal static Csg.Vector3D ToCsgVector3(this ContourVertex v)
        {
            return new Csg.Vector3D(v.Position.X, v.Position.Y, v.Position.Z);
        }

        internal static ContourVertex[] ToContourVertices(this List<Csg.Vertex> vertices)
        {
            var result = new ContourVertex[vertices.Count];
            for (var i = 0; i < vertices.Count; i++)
            {
                result[i] = new ContourVertex() { Position = new Vec3() { X = vertices[i].Pos.X, Y = vertices[i].Pos.Y, Z = vertices[i].Pos.Z }, Data = vertices[i].Tex };
            }
            return result;
        }

        internal static Csg.Vector3D ToCsgVector3(this Vector3 v)
        {
            return new Csg.Vector3D(v.X, v.Y, v.Z);
        }

        private static Csg.Vector2D ToCsgVector2(this UV uv)
        {
            return new Csg.Vector2D(uv.U, uv.V);
        }
    }
}