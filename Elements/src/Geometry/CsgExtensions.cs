using System;
using System.Collections.Generic;
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

            Tessellate(csgs, buffers, mergeVertices, modifyVertexAttributes);
            return buffers;
        }

        /// <summary>
        /// Triangulate a collection of CSGs and pack the triangulated data into
        /// a supplied buffers object. 
        /// </summary>
        internal static void Tessellate(Csg.Solid[] csgs,
                                        IGraphicsBuffers buffers,
                                        bool mergeVertices = false,
                                        Func<(Vector3, Vector3, UV, Color), (Vector3, Vector3, UV, Color)> modifyVertexAttributes = null)
        {
            (Vector3 U, Vector3 V) basis;

            var allVertices = new List<(Vector3 position, Vector3 normal, UV uv, Color color)>();
            foreach (var csg in csgs)
            {
                foreach (var p in csg.Polygons)
                {
                    var vertexIndices = new ushort[p.Vertices.Count];

                    // Anything with 3 vertices is a triangle. Manually 
                    // tessellate triangles. For everything else, use 
                    // the tessellator.
                    if (p.Vertices.Count == 3)
                    {
                        var a = p.Vertices[0].Pos.ToElementsVector();
                        var b = p.Vertices[1].Pos.ToElementsVector();
                        var c = p.Vertices[2].Pos.ToElementsVector();
                        basis = ComputeBasisAndNormalForTriangle(a, b, c, out Vector3 n);

                        for (var i = 0; i < p.Vertices.Count; i++)
                        {
                            var v = p.Vertices[i];
                            var uu = basis.U.Dot(v.Pos.X, v.Pos.Y, v.Pos.Z);
                            var vv = basis.V.Dot(v.Pos.X, v.Pos.Y, v.Pos.Z);
                            vertexIndices[i] = (ushort)GetOrCreateVertex(new Vector3(v.Pos.X, v.Pos.Y, v.Pos.Z),
                                                                         new Vector3(n.X, n.Y, n.Z),
                                                                         new UV(uu, vv),
                                                                         allVertices,
                                                                         mergeVertices);
                        }

                        // First triangle
                        buffers.AddIndex(vertexIndices[0]);
                        buffers.AddIndex(vertexIndices[1]);
                        buffers.AddIndex(vertexIndices[2]);
                    }
                    else if (p.Vertices.Count > 3)
                    {
                        var tess = new Tess();
                        tess.NoEmptyPolygons = true;
                        tess.AddContour(p.Vertices.ToContourVertices());

                        tess.Tessellate(WindingRule.Positive, LibTessDotNet.Double.ElementType.Polygons, 3);

                        if (tess.ElementCount == 0)
                        {
                            continue;
                        }
                        // We pick the first triangle from the tesselator,
                        // instead of the first three vertices, which are not guaranteed to be
                        // wound correctly.
                        var a = tess.Vertices[tess.Elements[0]].ToElementsVector();
                        var b = tess.Vertices[tess.Elements[1]].ToElementsVector();
                        var c = tess.Vertices[tess.Elements[2]].ToElementsVector();

                        basis = ComputeBasisAndNormalForTriangle(a, b, c, out Vector3 n);

                        for (var i = 0; i < tess.Vertices.Length; i++)
                        {
                            var v = tess.Vertices[i];
                            var uu = basis.U.Dot(v.Position.X, v.Position.Y, v.Position.Z);
                            var vv = basis.V.Dot(v.Position.X, v.Position.Y, v.Position.Z);

                            vertexIndices[i] = (ushort)GetOrCreateVertex(new Vector3(v.Position.X, v.Position.Y, v.Position.Z),
                                                                         new Vector3(n.X, n.Y, n.Z),
                                                                         new UV(uu, vv),
                                                                         allVertices,
                                                                         mergeVertices);

                        }

                        for (var k = 0; k < tess.Elements.Length; k++)
                        {
                            var index = vertexIndices[tess.Elements[k]];
                            buffers.AddIndex(index);
                        }
                    }
                }
            }

            foreach (var v in allVertices)
            {
                if (modifyVertexAttributes != null)
                {
                    var mod = modifyVertexAttributes(v);
                    buffers.AddVertex(mod.Item1, mod.Item2, mod.Item3, mod.Item4);
                }
                else
                {
                    buffers.AddVertex(v.Item1, v.Item2, v.Item3);
                }
            }
        }

        private static (Vector3 U, Vector3 V) ComputeBasisAndNormalForTriangle(Vector3 a, Vector3 b, Vector3 c, out Vector3 n)
        {
            var tmp = (b - a).Unitized();
            n = tmp.Cross(c - a).Unitized();
            var basis = n.ComputeDefaultBasisVectors();
            return basis;
        }

        private static int GetOrCreateVertex(Vector3 position,
                                             Vector3 normal,
                                             UV uv,
                                             List<(Vector3 position, Vector3 normal, UV uv, Color color)> pts,
                                             bool mergeVertices)
        {
            if (mergeVertices)
            {
                var index = pts.FindIndex(p =>
                {
                    return p.position.IsAlmostEqualTo(position) && p.normal.AngleTo(normal) < 45.0;
                });
                if (index != -1)
                {
                    return index;
                }
            }

            pts.Add((position, normal, uv, default(Color)));
            return pts.Count - 1;
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

        private static Vertex ToElementsVertex(this Csg.Vertex v)
        {
            return new Vertex(v.Pos.ToElementsVector());
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

        private static Vector3 ToElementsVector(this ContourVertex v)
        {
            return new Vector3(v.Position.X, v.Position.Y, v.Position.Z);
        }

        private static ContourVertex[] ToContourVertices(this List<Csg.Vertex> vertices)
        {
            var result = new ContourVertex[vertices.Count];
            for (var i = 0; i < vertices.Count; i++)
            {
                result[i] = new ContourVertex() { Position = new Vec3() { X = vertices[i].Pos.X, Y = vertices[i].Pos.Y, Z = vertices[i].Pos.Z }, Data = vertices[i].Tex };
            }
            return result;
        }

        private static Csg.Vector3D OfCsgVector3(this Transform transform, Csg.Vector3D p)
        {
            var m = transform.Matrix;
            return new Csg.Vector3D(
                p.X * m.m11 + p.Y * m.m21 + p.Z * m.m31 + m.tx,
                p.X * m.m12 + p.Y * m.m22 + p.Z * m.m32 + m.ty,
                p.X * m.m13 + p.Y * m.m23 + p.Z * m.m33 + m.tz
            );
        }

        private static bool IsAlmostEqualTo(this Csg.Vector3D target, Csg.Vector3D v)
        {
            if (Math.Abs(target.X - v.X) < Vector3.EPSILON &&
                Math.Abs(target.Y - v.Y) < Vector3.EPSILON &&
                Math.Abs(target.Z - v.Z) < Vector3.EPSILON)
            {
                return true;
            }
            return false;
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