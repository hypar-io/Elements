using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using LibTessDotNet.Double;

namespace Elements.Geometry
{
    /// <summary>
    /// A CSG solid.
    /// This type provides a thing wrapper around Frank Krueger's CSG library
    /// which is a port of OpenSCGCad's csg.js: https://github.com/praeclarum/Csg.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/CsgTests.cs?name=example)]
    /// </example>
    public class CSG : ITessellate
    {
        private Csg.Solid _csg;

        /// <summary>
        /// Construct a CSG.
        /// </summary>
        public CSG()
        {
            this._csg = new Csg.Solid();
        }

        /// <summary>
        /// Construct a CSG from a solid.
        /// </summary>
        /// <param name="solid">The solid which defines the CSG.</param>
        /// <param name="transform"></param>
        public CSG(Elements.Geometry.Solids.Solid solid, Transform transform = null)
        {
            _csg = solid.TessellateAsCSG(transform);
        }

        /// <summary>
        /// Construct a CSG from a mesh.
        /// </summary>
        /// <param name="mesh">The mesh which defines the CSG.</param>
        /// <param name="transform"></param>
        public CSG(Elements.Geometry.Mesh mesh, Transform transform = null)
        {
            _csg = mesh.TessellateAsCSG(transform);
        }

        /// <summary>
        /// Union this csg with the provided solid.
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="transform"></param>
        public void Union(Elements.Geometry.Solids.Solid solid, Transform transform = null)
        {
            _csg = Csg.Solids.Union(_csg, solid.TessellateAsCSG());
        }

        /// <summary>
        /// Difference this csg with the provided solid.
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="transform"></param>
        public void Difference(Elements.Geometry.Solids.Solid solid, Transform transform = null)
        {
            _csg = Csg.Solids.Difference(_csg, solid.TessellateAsCSG(transform));
        }

        /// <summary>
        /// Difference this csg with the provided csg.
        /// </summary>
        /// <param name="csg"></param>
        public void Difference(CSG csg)
        {
            _csg = Csg.Solids.Difference(csg._csg);
        }

        /// <summary>
        /// Intersect this csg with the provided solid.
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="transform"></param>
        public void Intersect(Elements.Geometry.Solids.Solid solid, Transform transform = null)
        {
            _csg = Csg.Solids.Intersection(_csg, solid.TessellateAsCSG(transform));
        }

        /// <summary>
        /// Implement ITessellate
        /// </summary>
        public void Tessellate(ref Mesh mesh, Transform transform = null, Color color = default(Color))
        {
            foreach (var p in _csg.Polygons)
            {
                p.AddToMesh(ref mesh);
            }
            mesh.ComputeNormals();
        }

        /// <summary>
        /// Triangulate this solid and pack the triangulated data into buffers
        /// appropriate for use with gltf.
        /// </summary>
        public void Tessellate(out byte[] vertexBuffer,
            out byte[] indexBuffer, out byte[] normalBuffer, out byte[] colorBuffer, out byte[] uvBuffer,
            out double[] vmax, out double[] vmin, out double[] nmin, out double[] nmax,
            out float[] cmin, out float[] cmax, out ushort imin, out ushort imax, out double[] uvmin, out double[] uvmax)
        {

            var tessellations = new Tess[this._csg.Polygons.Count];

            var fi = 0;
            foreach (var p in this._csg.Polygons)
            {
                var tess = new Tess();
                tess.NoEmptyPolygons = true;
                tess.AddContour(p.Vertices.ToContourVertices());

                tess.Tessellate(WindingRule.Positive, LibTessDotNet.Double.ElementType.Polygons, 3);
                tessellations[fi] = tess;
                fi++;
            }

            var floatSize = sizeof(float);
            var ushortSize = sizeof(ushort);

            var vertexCount = tessellations.Sum(t => t.VertexCount);
            var indexCount = tessellations.Sum(t => t.Elements.Length);

            vertexBuffer = new byte[vertexCount * floatSize * 3];
            normalBuffer = new byte[vertexCount * floatSize * 3];
            indexBuffer = new byte[indexCount * ushortSize];
            uvBuffer = new byte[vertexCount * floatSize * 2];

            // Vertex colors are not used in this context currently.
            colorBuffer = new byte[0];
            cmin = new float[0];
            cmax = new float[0];

            vmax = new double[3] { double.MinValue, double.MinValue, double.MinValue };
            vmin = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            nmin = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            nmax = new double[3] { double.MinValue, double.MinValue, double.MinValue };

            // TODO: Set this properly when solids get UV coordinates.
            uvmin = new double[2] { 0, 0 };
            uvmax = new double[2] { 0, 0 };

            imax = ushort.MinValue;
            imin = ushort.MaxValue;

            var vi = 0;
            var ii = 0;
            var uvi = 0;

            var iCursor = 0;

            for (var i = 0; i < tessellations.Length; i++)
            {
                var tess = tessellations[i];
                if (tess.ElementCount == 0)
                {
                    continue;
                }
                var a = tess.Vertices[tess.Elements[0]].Position.ToVector3();
                var b = tess.Vertices[tess.Elements[1]].Position.ToVector3();
                var c = tess.Vertices[tess.Elements[2]].Position.ToVector3();
                var n = (b - a).Cross(c - a).Unitized();

                for (var j = 0; j < tess.Vertices.Length; j++)
                {
                    var v = tess.Vertices[j];

                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Position.X), 0, vertexBuffer, vi, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Position.Y), 0, vertexBuffer, vi + floatSize, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Position.Z), 0, vertexBuffer, vi + 2 * floatSize, floatSize);

                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)n.X), 0, normalBuffer, vi, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)n.Y), 0, normalBuffer, vi + floatSize, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)n.Z), 0, normalBuffer, vi + 2 * floatSize, floatSize);

                    // TODO: Update Solids to use something other than UV = {0,0}.
                    System.Buffer.BlockCopy(BitConverter.GetBytes(0f), 0, uvBuffer, uvi, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(0f), 0, uvBuffer, uvi + floatSize, floatSize);

                    uvi += 2 * floatSize;
                    vi += 3 * floatSize;

                    vmax[0] = Math.Max(vmax[0], v.Position.X);
                    vmax[1] = Math.Max(vmax[1], v.Position.Y);
                    vmax[2] = Math.Max(vmax[2], v.Position.Z);
                    vmin[0] = Math.Min(vmin[0], v.Position.X);
                    vmin[1] = Math.Min(vmin[1], v.Position.Y);
                    vmin[2] = Math.Min(vmin[2], v.Position.Z);

                    nmax[0] = Math.Max(nmax[0], n.X);
                    nmax[1] = Math.Max(nmax[1], n.Y);
                    nmax[2] = Math.Max(nmax[2], n.Z);
                    nmin[0] = Math.Min(nmin[0], n.X);
                    nmin[1] = Math.Min(nmin[1], n.Y);
                    nmin[2] = Math.Min(nmin[2], n.Z);

                    // uvmax[0] = Math.Max(uvmax[0], 0);
                    // uvmax[1] = Math.Max(uvmax[1], 0);
                    // uvmin[0] = Math.Min(uvmin[0], 0);
                    // uvmin[1] = Math.Min(uvmin[1], 0);
                }

                for (var k = 0; k < tess.Elements.Length; k++)
                {
                    var t = tess.Elements[k];
                    var index = (ushort)(t + iCursor);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(index), 0, indexBuffer, ii, ushortSize);
                    imax = Math.Max(imax, index);
                    imin = Math.Min(imin, index);
                    ii += ushortSize;
                }

                iCursor = imax + 1;
            }
        }
    }

    internal static class CsgExtensions
    {
        internal static void AddToMesh(this Csg.Polygon p, ref Mesh mesh)
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

                var v1 = mesh.AddVertex(a, uva.ToUV());
                var v2 = mesh.AddVertex(b, uvb.ToUV());
                var v3 = mesh.AddVertex(c, uvc.ToUV());
                mesh.AddTriangle(v1, v2, v3);
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

        internal static ContourVertex[] ToContourVertices(this List<Csg.Vertex> vertices)
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

        internal static Csg.Solid TessellateAsCSG(this Solid solid, Transform transform = null)
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

                var vertices = new List<Csg.Vertex>();
                for (var i = 0; i < tess.ElementCount; i++)
                {
                    var a = tess.Vertices[tess.Elements[i * 3]].ToCsgVector3();
                    var b = tess.Vertices[tess.Elements[i * 3 + 1]].ToCsgVector3();
                    var c = tess.Vertices[tess.Elements[i * 3 + 2]].ToCsgVector3();

                    if (transform != null)
                    {
                        a = transform.OfCsgVector3(a);
                        b = transform.OfCsgVector3(b);
                        c = transform.OfCsgVector3(c);
                    }

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
                    if (av == null)
                    {
                        av = new Csg.Vertex(a, new Csg.Vector2D());
                        vertices.Add(av);
                    }
                    if (bv == null)
                    {
                        bv = new Csg.Vertex(b, new Csg.Vector2D());
                        vertices.Add(bv);
                    }
                    if (cv == null)
                    {
                        cv = new Csg.Vertex(c, new Csg.Vector2D());
                        vertices.Add(cv);
                    }

                    // TODO: Add texture coordinates.
                    var p = new Csg.Polygon(new List<Csg.Vertex>() { av, bv, cv });
                    polygons.Add(p);
                }
            }
            return Csg.Solid.FromPolygons(polygons);
        }

        internal static Csg.Solid TessellateAsCSG(this Mesh mesh, Transform transform = null)
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
    }
}