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
                                                   Func<(Vector3, Vector3, UV, Color?), (Vector3, Vector3, UV, Color?)> modifyVertexAttributes = null)
        {
            return Tessellate(new[] { csg }, mergeVertices, modifyVertexAttributes);
        }

        /// <summary>
        /// Triangulate a collection of CSGs and pack the triangulated data into
        /// buffers appropriate for use with gltf. 
        /// </summary>
        internal static GraphicsBuffers Tessellate(this Csg.Solid[] csgs,
                                                   bool mergeVertices = false,
                                                   Func<(Vector3, Vector3, UV, Color?), (Vector3, Vector3, UV, Color?)> modifyVertexAttributes = null)
        {
            var providers = new List<CsgTessellationTargetProvider>();
            uint solidId = 0;
            foreach (var csg in csgs)
            {
                providers.Add(new CsgTessellationTargetProvider(csg, solidId));
                solidId++;
            }
            var buffers = Tessellation.Tessellation.Tessellate<GraphicsBuffers>(providers,
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
                var av = mesh.FindOrCreateVertex(a.Pos.ToElementsVector(), a.Tag, a.Tex.ToUV(), n);
                var bv = mesh.FindOrCreateVertex(b.Pos.ToElementsVector(), b.Tag, b.Tex.ToUV(), n);
                var cv = mesh.FindOrCreateVertex(c.Pos.ToElementsVector(), c.Tag, c.Tex.ToUV(), n);

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
                var av = mesh.FindOrCreateVertex(a.Pos.ToElementsVector(), a.Tag, a.Tex.ToUV(), n);
                var bv = mesh.FindOrCreateVertex(b.Pos.ToElementsVector(), b.Tag, b.Tex.ToUV(), n);
                var cv = mesh.FindOrCreateVertex(c.Pos.ToElementsVector(), c.Tag, c.Tex.ToUV(), n);
                var dv = mesh.FindOrCreateVertex(d.Pos.ToElementsVector(), d.Tag, d.Tex.ToUV(), n);

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
                var tess = new Tess
                {
                    NoEmptyPolygons = true
                };

                tess.AddContour(p.Vertices.ToContourVertices());
                tess.Tessellate(WindingRule.Positive, ElementType.Polygons, 3);

                for (var i = 0; i < tess.ElementCount; i++)
                {
                    var t1 = tess.Vertices[tess.Elements[i * 3]];
                    var t2 = tess.Vertices[tess.Elements[i * 3 + 1]];
                    var t3 = tess.Vertices[tess.Elements[i * 3 + 2]];

                    var a = t1.Position.ToVector3();
                    var b = t2.Position.ToVector3();
                    var c = t3.Position.ToVector3();

                    var dataA = ((Csg.Vector2D, int))t1.Data;
                    var dataB = ((Csg.Vector2D, int))t2.Data;
                    var dataC = ((Csg.Vector2D, int))t3.Data;

                    var v1 = mesh.FindOrCreateVertex(a, dataA.Item2, dataA.Item1.ToUV(), n);
                    var v2 = mesh.FindOrCreateVertex(b, dataB.Item2, dataB.Item1.ToUV(), n);
                    var v3 = mesh.FindOrCreateVertex(c, dataC.Item2, dataC.Item1.ToUV(), n);

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
                result[i] = new ContourVertex() { Position = new Vec3() { X = vertices[i].Pos.X, Y = vertices[i].Pos.Y, Z = vertices[i].Pos.Z }, Data = (vertices[i].Tex, vertices[i].Tag) };
            }
            return result;
        }

        internal static Vector3 ToVector3(this Csg.Vector3D v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        internal static Csg.Vector3D ToCsgVector3(this Vector3 v)
        {
            return new Csg.Vector3D(v.X, v.Y, v.Z);
        }

        private static Csg.Vector2D ToCsgVector2(this UV uv)
        {
            return new Csg.Vector2D(uv.U, uv.V);
        }

        internal static bool IsCoplanar(this Csg.Plane csgPlane, Plane plane)
        {
            var dot = Math.Abs(csgPlane.Normal.ToVector3().Dot(plane.Normal));
            return dot.ApproximatelyEquals(1) && csgPlane.W.ApproximatelyEquals(plane.Origin.DistanceTo(Vector3.Origin));
        }

        internal static Polygon Project(this Csg.Polygon poly, Plane plane)
        {
            return new Polygon(poly.Vertices.Select(vtx => vtx.Pos.ToVector3().Project(plane)).ToList());
        }

        internal static bool IsBehind(this Csg.Plane csgPlane, Plane plane)
        {
            var p = (csgPlane.Normal * csgPlane.W).ToVector3();
            return plane.SignedDistanceTo(p) < 0;
        }

        /// <summary>
        /// Convert an array of CSG vertices to an array of tessellation
        /// vertices, storing the texture coordinates and the tag in the data.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="faceId"></param>
        /// <param name="solidId"></param>
        internal static ContourVertex[] ToContourVertexArray(this IList<Csg.Vertex> vertices, uint faceId, uint solidId = 0)
        {
            var contour = new ContourVertex[vertices.Count];
            for (var i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];

                var cv = new ContourVertex
                {
                    Position = new Vec3 { X = v.Pos.X, Y = v.Pos.Y, Z = v.Pos.Z },
                    Data = (v.Tex.ToUV(), (uint)v.Tag, faceId, solidId)
                };
                contour[i] = cv;
            }
            return contour;
        }

        internal static Csg.Vector3D ToCsgVector3(this Vec3 v)
        {
            return new Csg.Vector3D(v.X, v.Y, v.Z);
        }
    }
}