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
        // TODO remove this when we have a logging system with more granular control over logging levels.
        // Switch this to true to see the tessellation progress of all elements.
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static bool LOG_TESSELATION = false;

        /// <summary>
        /// Triangulate a collection of CSGs and pack the triangulated data into
        /// a supplied buffers object.
        /// </summary>
        internal static T Tessellate<T>(IEnumerable<ITessellationTargetProvider> providers,
                                        bool mergeVertices = false,
                                        Func<(Vector3, Vector3, UV, Color?), (Vector3, Vector3, UV, Color?)> modifyVertexAttributes = null) where T : IGraphicsBuffers
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
            // Console.WriteLine($"tess: {sw.ElapsedMilliseconds}ms for tessellation.");
            // sw.Restart();

            // Pre-allocate a buffer big enough to hold all the tessellations
            var buffer = (IGraphicsBuffers)Activator.CreateInstance(typeof(T));
            buffer.Initialize(tesses.Sum(tess => tess.VertexCount), tesses.Sum(tess => tess.Elements.Length));

            PackTessellationsIntoBuffers(tesses, buffer, modifyVertexAttributes);

            return (T)buffer;
        }

        private static void PackTessellationsIntoBuffers(List<Tess> tesses,
                                              IGraphicsBuffers buffers,
                                              Func<(Vector3, Vector3, UV, Color?), (Vector3, Vector3, UV, Color?)> modifyVertexAttributes)
        {
            // The vertex map enables us to re-use vertices. Csgs and solid faces
            // create vertices which store the face id, the vertex id, and for csgs, the uv.
            // We can use the tag as the key to lookup the index of the vertex to avoid re-creating it.
            var vertexMap = new Dictionary<(uint tag, uint faceId, uint solidId), ushort>();
            var vertices = new List<(Vector3 position, Vector3 normal, UV uv, Color? color)>();
            var indices = new List<ushort>();

            var tessOffset = 0;
            uint index = 0;

            foreach (var tess in tesses)
            {
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
                    // data to match vertices.

                    var (uv, tag, faceId, solidId) = ((UV uv, uint tag, uint faceId, uint solidId))v.Data;

                    if (vertexMap.ContainsKey((tag, faceId, solidId)))
                    {
                        Debug.WriteLineIf(LOG_TESSELATION, $"Reusing vertex (tag:{tag},faceId:{faceId},solidId:{solidId}");
                        // Reference an existing vertex from csg
                        indices.Add(vertexMap[(tag, faceId, solidId)]);
                        continue;
                    }
                    else if (vertexMap.ContainsKey((index, 0, 0)))
                    {
                        Debug.WriteLineIf(LOG_TESSELATION, $"Reusing vertex (tag:{tag},faceId:{faceId},solidId:{solidId}");
                        // Reference an existing vertex created
                        // earlier here.
                        indices.Add(vertexMap[(index, 0, 0)]);
                        continue;
                    }
                    else
                    {
                        // Create a new vertex.
                        var v1 = new Vector3(v.Position.X, v.Position.Y, v.Position.Z);
                        Color? c1 = null;

                        // Solid faces won't have UV coordinates.
                        if (uv == default)
                        {
                            var uu = U.Dot(v1);
                            var vv = V.Dot(v1);
                            uv = new UV(uu, vv);
                        }

                        if (modifyVertexAttributes != null)
                        {
                            var mod = modifyVertexAttributes((v1, n, uv, c1));
                            vertices.Add((mod.Item1, mod.Item2, mod.Item3, mod.Item4));
                        }
                        else
                        {
                            vertices.Add((v1, n, uv, c1));
                        }
                        Debug.WriteLineIf(LOG_TESSELATION, $"Adding vertex (tag:{tag},faceId:{faceId}):{index}");
                        indices.Add((ushort)index);
                        vertexMap.Add((tag, faceId, solidId), (ushort)index);
                        newVerts++;
                        index++;
                    }
                }
                tessOffset += newVerts;
                Debug.WriteLineIf(LOG_TESSELATION, $"----------{tessOffset}");
            }

            buffers.AddIndices(indices);
            buffers.AddVertices(vertices);
        }

        private static Vector3 ToElementsVector(this ContourVertex v)
        {
            return new Vector3(v.Position.X, v.Position.Y, v.Position.Z);
        }

        internal static (Vector3 U, Vector3 V) ComputeBasisAndNormalForTriangle(Vector3 a, Vector3 b, Vector3 c, out Vector3 n)
        {
            var tmp = (b - a).Unitized();
            n = tmp.Cross((c - a).Unitized()).Unitized();
            var basis = n.ComputeDefaultBasisVectors();
            return basis;
        }
    }
}