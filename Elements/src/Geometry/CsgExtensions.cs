using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry.Solids;
using LibTessDotNet.Double;

namespace Elements.Geometry
{
    internal static class CsgExtensions
    {
        /// <summary>
        /// Write the csg into a mesh.
        /// </summary>
        internal static void Tessellate(this Csg.Solid csg, ref Mesh mesh, Transform transform = null, Color color = default(Color))
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
        internal static void Tessellate(this Csg.Solid csg, out byte[] vertexBuffer,
            out byte[] indexBuffer, out byte[] normalBuffer, out byte[] colorBuffer, out byte[] uvBuffer,
            out double[] vmax, out double[] vmin, out double[] nmin, out double[] nmax,
            out float[] cmin, out float[] cmax, out ushort imin, out ushort imax, out double[] uvmin, out double[] uvmax)
        {
            // Initialize everything
            var vertices = new List<byte>();
            var normals = new List<byte>();
            var indices = new List<byte>();
            var uvs = new List<byte>();

            // Vertex colors are not used in this context currently.
            var colors = new List<byte>();
            cmin = new float[0];
            cmax = new float[0];

            vmax = new double[3] { double.MinValue, double.MinValue, double.MinValue };
            vmin = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            nmin = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            nmax = new double[3] { double.MinValue, double.MinValue, double.MinValue };

            uvmin = new double[2] { double.MaxValue, double.MaxValue };
            uvmax = new double[2] { double.MinValue, double.MinValue };

            imax = ushort.MinValue;
            imin = 0;

            ushort iCursor = 0;

            (Vector3 U, Vector3 V) basis;

            foreach (var p in csg.Polygons)
            {
                if (p.Vertices.Count == 3)
                {
                    // It's just a triangle. Add it directly.
                    var a = p.Vertices[0].Pos.ToElementsVector();
                    var b = p.Vertices[1].Pos.ToElementsVector();
                    var c = p.Vertices[2].Pos.ToElementsVector();
                    basis = ComputeBasisAndNormalForTriangle(a, b, c, out Vector3 n);
                    for (var i = 0; i < p.Vertices.Count; i++)
                    {
                        var v = p.Vertices[i];
                        WriteVertex(v.Pos.ToElementsVector(),
                                    n,
                                    vertices,
                                    normals,
                                    uvs,
                                    basis,
                                    ref vmax,
                                    ref vmin,
                                    ref nmax,
                                    ref nmin,
                                    ref uvmax,
                                    ref uvmin);
                    }
                    indices.AddRange(BitConverter.GetBytes(iCursor));
                    indices.AddRange(BitConverter.GetBytes((ushort)(iCursor + 1)));
                    indices.AddRange(BitConverter.GetBytes((ushort)(iCursor + 2)));
                    imax = Math.Max(imax, (ushort)(iCursor + 2));
                    imin = Math.Min(imin, (ushort)(iCursor));
                    iCursor = (ushort)(imax + 1);
                }
                if (p.Vertices.Count == 4)
                {
                    // Triangulate into two triangles.
                    var a = p.Vertices[0].Pos.ToElementsVector();
                    var b = p.Vertices[1].Pos.ToElementsVector();
                    var c = p.Vertices[2].Pos.ToElementsVector();
                    var d = p.Vertices[3].Pos.ToElementsVector();
                    basis = ComputeBasisAndNormalForTriangle(a, b, c, out Vector3 n);
                    for (var i = 0; i < p.Vertices.Count; i++)
                    {
                        var v = p.Vertices[i];
                        WriteVertex(v.Pos.ToElementsVector(),
                                    n,
                                    vertices,
                                    normals,
                                    uvs,
                                    basis,
                                    ref vmax,
                                    ref vmin,
                                    ref nmax,
                                    ref nmin,
                                    ref uvmax,
                                    ref uvmin);
                    }

                    // Triangle 1
                    indices.AddRange(BitConverter.GetBytes(iCursor));
                    indices.AddRange(BitConverter.GetBytes((ushort)(iCursor + 1)));
                    indices.AddRange(BitConverter.GetBytes((ushort)(iCursor + 2)));

                    // Triangle 2
                    indices.AddRange(BitConverter.GetBytes(iCursor));
                    indices.AddRange(BitConverter.GetBytes((ushort)(iCursor + 2)));
                    indices.AddRange(BitConverter.GetBytes((ushort)(iCursor + 3)));

                    imax = Math.Max(imax, (ushort)(iCursor + 3));
                    imin = Math.Min(imin, (ushort)(iCursor));
                    iCursor = (ushort)(imax + 1);
                }
                else if (p.Vertices.Count > 4)
                {
                    var tess = new Tess();
                    tess.NoEmptyPolygons = true;
                    tess.AddContour(p.Vertices.ToContourVertices());

                    tess.Tessellate(WindingRule.Positive, LibTessDotNet.Double.ElementType.Polygons, 3);

                    if (tess.ElementCount == 0)
                    {
                        continue;
                    }

                    var a = tess.Vertices[tess.Elements[0]].Position.ToVector3();
                    var b = tess.Vertices[tess.Elements[1]].Position.ToVector3();
                    var c = tess.Vertices[tess.Elements[2]].Position.ToVector3();
                    var tmp = (b - a).Unitized();
                    var n = tmp.Cross(c - a).Unitized();

                    // Calculate the texture space basis vectors
                    // from the first triangle. This is acceptable
                    // for planar faces.
                    // TODO: Update this when we support non-planar faces.
                    // https://gamedev.stackexchange.com/questions/172352/finding-texture-coordinates-for-plane
                    basis = n.ComputeDefaultBasisVectors();

                    for (var j = 0; j < tess.Vertices.Length; j++)
                    {
                        var v = tess.Vertices[j];
                        var pos = v.Position.ToVector3();
                        WriteVertex(pos,
                                    n,
                                    vertices,
                                    normals,
                                    uvs,
                                    basis,
                                    ref vmax,
                                    ref vmin,
                                    ref nmax,
                                    ref nmin,
                                    ref uvmax,
                                    ref uvmin);
                    }

                    for (var k = 0; k < tess.Elements.Length; k++)
                    {
                        var t = tess.Elements[k];
                        var index = (ushort)(t + iCursor);
                        indices.AddRange(BitConverter.GetBytes(index));
                        imax = Math.Max(imax, index);
                        imin = Math.Min(imin, index);
                    }

                    iCursor = (ushort)(imax + 1);
                }
            }

            vertexBuffer = vertices.ToArray();
            normalBuffer = normals.ToArray();
            indexBuffer = indices.ToArray();
            uvBuffer = uvs.ToArray();
            colorBuffer = colors.ToArray();
        }

        private static (Vector3 U, Vector3 V) ComputeBasisAndNormalForTriangle(Vector3 a, Vector3 b, Vector3 c, out Vector3 n)
        {
            var tmp = (b - a).Unitized();
            n = tmp.Cross(c - a).Unitized();
            var basis = n.ComputeDefaultBasisVectors();
            return basis;
        }

        private static void WriteVertex(Vector3 pos,
                                        Vector3 n,
                                        List<byte> vertices,
                                        List<byte> normals,
                                        List<byte> uvs,
                                        (Vector3 U, Vector3 V) basis,
                                        ref double[] vmax,
                                        ref double[] vmin,
                                        ref double[] nmax,
                                        ref double[] nmin,
                                        ref double[] uvmax,
                                        ref double[] uvmin)
        {
            vertices.AddRange(BitConverter.GetBytes((float)pos.X));
            vertices.AddRange(BitConverter.GetBytes((float)pos.Y));
            vertices.AddRange(BitConverter.GetBytes((float)pos.Z));

            normals.AddRange(BitConverter.GetBytes((float)n.X));
            normals.AddRange(BitConverter.GetBytes((float)n.Y));
            normals.AddRange(BitConverter.GetBytes((float)n.Z));

            var uu = basis.U.Dot(pos);
            var vv = basis.V.Dot(pos);
            uvs.AddRange(BitConverter.GetBytes((float)uu));
            uvs.AddRange(BitConverter.GetBytes((float)vv));

            vmax[0] = Math.Max(vmax[0], pos.X);
            vmax[1] = Math.Max(vmax[1], pos.Y);
            vmax[2] = Math.Max(vmax[2], pos.Z);
            vmin[0] = Math.Min(vmin[0], pos.X);
            vmin[1] = Math.Min(vmin[1], pos.Y);
            vmin[2] = Math.Min(vmin[2], pos.Z);

            nmax[0] = Math.Max(nmax[0], n.X);
            nmax[1] = Math.Max(nmax[1], n.Y);
            nmax[2] = Math.Max(nmax[2], n.Z);
            nmin[0] = Math.Min(nmin[0], n.X);
            nmin[1] = Math.Min(nmin[1], n.Y);
            nmin[2] = Math.Min(nmin[2], n.Z);

            uvmax[0] = Math.Max(uvmax[0], uu);
            uvmax[1] = Math.Max(uvmax[1], vv);
            uvmin[0] = Math.Min(uvmin[0], uu);
            uvmin[1] = Math.Min(uvmin[1], vv);
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

        internal static Csg.Solid ToCsg(this Solid solid)
        {
            var polygons = new List<Csg.Polygon>();

            foreach (var f in solid.Faces.Values)
            {
                var tess = new Tess();
                tess.NoEmptyPolygons = true;

                tess.AddContour(f.Outer.ToContourVertexArray(f));

                if (f.Inner != null)
                {
                    foreach (var loop in f.Inner)
                    {
                        tess.AddContour(loop.ToContourVertexArray(f));
                    }
                }

                tess.Tessellate(WindingRule.Positive, LibTessDotNet.Double.ElementType.Polygons, 3);

                Vector3 e1 = new Vector3();
                Vector3 e2 = new Vector3();

                var vertices = new List<Csg.Vertex>();
                for (var i = 0; i < tess.ElementCount; i++)
                {
                    var a = tess.Vertices[tess.Elements[i * 3]].ToCsgVector3();
                    var b = tess.Vertices[tess.Elements[i * 3 + 1]].ToCsgVector3();
                    var c = tess.Vertices[tess.Elements[i * 3 + 2]].ToCsgVector3();

                    Csg.Vertex av = null;
                    Csg.Vertex bv = null;
                    Csg.Vertex cv = null;

                    // Merge vertices.
                    foreach (var v in vertices)
                    {
                        if (v.Pos.IsAlmostEqualTo(a))
                        {
                            av = v;
                        }

                        if (v.Pos.IsAlmostEqualTo(b))
                        {
                            bv = v;
                        }

                        if (v.Pos.IsAlmostEqualTo(c))
                        {
                            cv = v;
                        }
                    }
                    if (i == 0)
                    {
                        var n = f.Plane().Normal;
                        e1 = n.Cross(n.IsParallelTo(Vector3.XAxis) ? Vector3.YAxis : Vector3.XAxis).Unitized();
                        e2 = n.Cross(e1).Unitized();
                    }
                    if (av == null)
                    {
                        var avv = new Vector3(a.X, a.Y, a.Z);
                        av = new Csg.Vertex(a, new Csg.Vector2D(e1.Dot(avv), e2.Dot(avv)));
                        vertices.Add(av);
                    }
                    if (bv == null)
                    {
                        var bvv = new Vector3(b.X, b.Y, b.Z);
                        bv = new Csg.Vertex(b, new Csg.Vector2D(e1.Dot(bvv), e2.Dot(bvv)));
                        vertices.Add(bv);
                    }
                    if (cv == null)
                    {
                        var cvv = new Vector3(c.X, c.Y, c.Z);
                        cv = new Csg.Vertex(c, new Csg.Vector2D(e1.Dot(cvv), e2.Dot(cvv)));
                        vertices.Add(cv);
                    }

                    var p = new Csg.Polygon(new List<Csg.Vertex>() { av, bv, cv });
                    polygons.Add(p);
                }
            }
            return Csg.Solid.FromPolygons(polygons);
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
                // Don't tesselate unless we need to.

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
                // Don't tesselate unless we need to.

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

        private static Csg.Vector3D ToCsgVector3(this ContourVertex v)
        {
            return new Csg.Vector3D(v.Position.X, v.Position.Y, v.Position.Z);
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

        private static Csg.Vector3D ToCsgVector3(this Vector3 v)
        {
            return new Csg.Vector3D(v.X, v.Y, v.Z);
        }

        private static Csg.Vector2D ToCsgVector2(this UV uv)
        {
            return new Csg.Vector2D(uv.U, uv.V);
        }

    }
}