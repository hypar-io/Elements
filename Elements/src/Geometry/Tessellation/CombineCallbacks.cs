using System;
using System.Threading;
using LibTessDotNet.Double;

namespace Elements.Geometry.Tessellation
{
    /// <summary>
    /// Shared <see cref="CombineCallback"/> factories for tessellators.
    ///
    /// LibTess can synthesize new vertices at contour intersection / T-junction points
    /// during <see cref="Tess.Tessellate(WindingRule, ElementType, int, CombineCallback)"/>.
    /// Without a callback, those synthetic vertices end up with <c>Data == null</c>, which
    /// later trips a <see cref="System.NullReferenceException"/> in
    /// <see cref="Tessellation.PackTessellationsIntoBuffers"/> when it unboxes
    /// <c>v.Data</c> to the expected shape.
    ///
    /// Each callback interpolates the UV from the input vertices, preserves the
    /// faceId/solidId of the first non-null input (these are constant within a single
    /// face's tessellation), and assigns a unique synthetic tag drawn from a process-global
    /// monotonically increasing counter starting in the upper half of the uint range so it
    /// can never collide with the small, sequential tags emitted by the CSG library.
    /// </summary>
    internal static class CombineCallbacks
    {
        // Start synthetic tags in the upper half of the uint range so they cannot
        // collide with Csg.Vertex.Tag values, which are sequentially allocated from 0.
        private static long _dataCombineCounter = 0x80000000L;
        private static long _csgTexTagCombineCounter = 0x90000000L;

        /// <summary>
        /// Combine callback for tessellation paths that attach
        /// <see cref="CsgVertexData"/> to <see cref="ContourVertex.Data"/>.
        /// </summary>
        internal static CombineCallback DataCombine { get; } = CombineData;

        /// <summary>
        /// Combine callback for legacy mesh tessellation paths that store
        /// <c>(Csg.Vector2D tex, int tag)</c> on <see cref="ContourVertex.Data"/>.
        /// </summary>
        internal static CombineCallback CsgTexTagCombine { get; } = CombineCsgTexTag;

        private static object CombineData(Vec3 position, object[] data, double[] weights)
        {
            var uvU = 0.0;
            var uvV = 0.0;
            uint faceId = 0;
            uint solidId = 0;
            bool seenInput = false;

            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] is CsgVertexData t)
                {
                    var w = weights[i];
                    uvU += t.Uv.U * w;
                    uvV += t.Uv.V * w;
                    if (!seenInput)
                    {
                        faceId = t.FaceId;
                        solidId = t.SolidId;
                        seenInput = true;
                    }
                }
            }

            var tag = (uint)Interlocked.Increment(ref _dataCombineCounter);
            return new CsgVertexData(new UV(uvU, uvV), tag, faceId, solidId);
        }

        private static object CombineCsgTexTag(Vec3 position, object[] data, double[] weights)
        {
            var texX = 0.0;
            var texY = 0.0;
            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] is ValueTuple<Csg.Vector2D, int> t)
                {
                    var w = weights[i];
                    texX += t.Item1.X * w;
                    texY += t.Item1.Y * w;
                }
            }

            var tag = (int)Interlocked.Increment(ref _csgTexTagCombineCounter);
            return (new Csg.Vector2D(texX, texY), tag);
        }
    }
}
