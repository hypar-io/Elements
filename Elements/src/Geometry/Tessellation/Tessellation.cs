using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var buffer = (IGraphicsBuffers)Activator.CreateInstance(typeof(T));
            buffer.Initialize(tesses.Sum(tess => tess.VertexCount), tesses.Sum(tess => tess.Elements.Length));

            PackTessellationsIntoBuffers(tesses, buffer, modifyVertexAttributes);

            return (T)buffer;
        }

        private static void PackTessellationsIntoBuffers(List<Tess> tesses,
                                              IGraphicsBuffers buffers,
                                              Func<(Vector3, Vector3, UV, Color), (Vector3, Vector3, UV, Color)> modifyVertexAttributes)
        {
            // The vertex map enables us to re-use vertices. Csgs only return
            // polygons with vertices that have a 'tag'. We can use the tag as
            // the key to lookup the index of the vertex to avoid re-creating it.
            // For direct solid tesselation, we don't have a tag but we do have
            // the tesselation's element index, which we use as the key when
            // writing solid tesselations.
            var vertexMap = new Dictionary<int, ushort>();
            var tessOffset = 0;
            var index = 0;

            foreach (var tess in tesses)
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

                var newVerts = 0;

                for (var i = 0; i < tess.Elements.Length; i++)
                {
                    var localIndex = tess.Elements[i];
                    var v = tess.Vertices[localIndex];
                    var tessIndex = tessOffset + localIndex;

                    // This is an optimization to use pre-existing csg vertex 
                    // data to match vertices. For solid tesselations, this
                    // information won't exist, so we'll skip to the block below.
                    if (v.Data != null)
                    {
                        var (uv, tag) = ((UV uv, int tag))v.Data;
                        if (vertexMap.ContainsKey(tag))
                        {
                            Debug.WriteLine($"Resuing vertex {tag}");
                            // Reference an existing vertex from csg
                            buffers.AddIndex(vertexMap[tag]);
                            continue;
                        }
                        else if (vertexMap.ContainsKey(index))
                        {
                            Debug.WriteLine($"Resuing vertex {tag}");
                            // Reference an existing vertex created
                            // earlier here.
                            buffers.AddIndex(vertexMap[index]);
                            continue;
                        }
                        else
                        {
                            // Create a new vertex.
                            var v1 = new Vector3(v.Position.X, v.Position.Y, v.Position.Z);
                            var c1 = default(Color);

                            if (modifyVertexAttributes != null)
                            {
                                var mod = modifyVertexAttributes((v1, n, uv, c1));
                                buffers.AddVertex(mod.Item1, mod.Item2, mod.Item3, mod.Item4);
                            }
                            else
                            {
                                buffers.AddVertex(v1, n, uv, c1);
                            }
                            Debug.WriteLine($"Adding vertex {tag}:{index}");
                            buffers.AddIndex((ushort)index);
                            vertexMap.Add(tag, (ushort)index);
                            newVerts++;
                            index++;
                        }
                    }
                    else
                    {
                        if (vertexMap.ContainsKey(tessIndex))
                        {
                            Debug.WriteLine($"Resuing vertex {tessIndex}");
                            buffers.AddIndex((ushort)tessIndex);
                            continue;
                        }
                        else
                        {
                            // Create the new vertex.
                            var v1 = new Vector3(v.Position.X, v.Position.Y, v.Position.Z);
                            var c1 = default(Color);
                            var uu = U.Dot(v.Position.X, v.Position.Y, v.Position.Z);
                            var vv = V.Dot(v.Position.X, v.Position.Y, v.Position.Z);
                            var uv = new UV(uu, vv);

                            if (modifyVertexAttributes != null)
                            {
                                var mod = modifyVertexAttributes((v1, n, uv, c1));
                                buffers.AddVertex(mod.Item1, mod.Item2, mod.Item3, mod.Item4);
                            }
                            else
                            {
                                buffers.AddVertex(v1, n, uv, c1);
                            }
                            Debug.WriteLine($"Adding vertex {tessIndex}:{index}");
                            buffers.AddIndex((ushort)index);
                            vertexMap.Add(tessIndex, (ushort)index);
                            newVerts++;
                            index++;
                        }
                    }
                }
                tessOffset += newVerts;
                Debug.WriteLine($"----------{tessOffset}");
            }
        }

        private static Vector3 ToElementsVector(this ContourVertex v)
        {
            return new Vector3(v.Position.X, v.Position.Y, v.Position.Z);
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