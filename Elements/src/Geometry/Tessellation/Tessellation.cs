using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LibTessDotNet.Double;
[assembly: InternalsVisibleTo("Hypar.Elements.Tests")]

namespace Elements.Geometry.Tessellation
{
    /// <summary>
    /// Methods for the tessellation of various objects.
    /// </summary>
    internal static class Tessellation
    {
        /// <summary>
        /// Triangulate a collection of CSGs and pack the triangulated data into
        /// a supplied buffers object. 
        /// </summary>
        internal static T Tessellate<T>(IEnumerable<ITessellationTargetProvider> providers,
                                        Func<(Vector3, Vector3, UV, Color), (Vector3, Vector3, UV, Color)> modifyVertexAttributes = null) where T : IGraphicsBuffers
        {

            // Gather all the tessellations
            var tesses = new List<Tess>();
            foreach (var provider in providers)
            {
                foreach (var target in provider.GetTessellationTargets())
                {
                    tesses.Add(target.GetTess());
                }
            }

            // Pre-allocate a buffer big enough to hold all the tessellations
            var buffers = (IGraphicsBuffers)Activator.CreateInstance(typeof(T));
            buffers.Initialize(tesses.Sum(tess => tess.VertexCount), tesses.Sum(tess => tess.Elements.Length));

            ushort indexOffset = 0;
            foreach (var tess in tesses)
            {
                PackTessellationIntoBuffers(tess, buffers, modifyVertexAttributes, ref indexOffset);
            }

            return (T)buffers;
        }

        private static void PackTessellationIntoBuffers(Tess tess,
                                              IGraphicsBuffers buffers,
                                              Func<(Vector3, Vector3, UV, Color), (Vector3, Vector3, UV, Color)> modifyVertexAttributes,
                                              ref ushort indexOffset)
        {
            if (tess.ElementCount == 0)
            {
                return;
            }

            // We pick the first triangle from the tesselator,
            // instead of the first three vertices, which are not guaranteed to be
            // wound correctly.
            var a = tess.Vertices[tess.Elements[0]].ToElementsVector();
            var b = tess.Vertices[tess.Elements[1]].ToElementsVector();
            var c = tess.Vertices[tess.Elements[2]].ToElementsVector();

            // Calculate the texture space basis vectors
            // from the first triangle. This is acceptable
            // for planar faces.
            // TODO: Update this when we support non-planar faces.
            // https://gamedev.stackexchange.com/questions/172352/finding-texture-coordinates-for-plane
            var (U, V) = ComputeBasisAndNormalForTriangle(a, b, c, out Vector3 n);

            for (var i = 0; i < tess.Vertices.Length; i++)
            {
                var v = tess.Vertices[i];
                var uu = U.Dot(v.Position.X, v.Position.Y, v.Position.Z);
                var vv = V.Dot(v.Position.X, v.Position.Y, v.Position.Z);

                var v1 = new Vector3(v.Position.X, v.Position.Y, v.Position.Z);
                var uv1 = new UV(uu, vv);
                var c1 = default(Color);

                if (modifyVertexAttributes != null)
                {
                    var mod = modifyVertexAttributes((v1, n, uv1, c1));
                    buffers.AddVertex(mod.Item1, mod.Item2, mod.Item3, mod.Item4);
                }
                else
                {
                    buffers.AddVertex(v1, n, uv1, c1);
                }
            }

            for (var k = 0; k < tess.Elements.Length; k++)
            {
                var index = (ushort)(tess.Elements[k] + indexOffset);
                buffers.AddIndex(index);
            }

            indexOffset += (ushort)tess.Vertices.Length;
        }

        private static Vector3 ToElementsVector(this ContourVertex v)
        {
            return new Vector3(v.Position.X, v.Position.Y, v.Position.Z);
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

        internal static (Vector3 U, Vector3 V) ComputeBasisAndNormalForTriangle(Vector3 a, Vector3 b, Vector3 c, out Vector3 n)
        {
            var tmp = (b - a).Unitized();
            n = tmp.Cross(c - a).Unitized();
            var basis = n.ComputeDefaultBasisVectors();
            return basis;
        }
    }
}