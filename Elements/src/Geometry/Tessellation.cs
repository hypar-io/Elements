using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Elements.Geometry.Solids;
using LibTessDotNet.Double;
[assembly: InternalsVisibleTo("Hypar.Elements.Tests")]

namespace Elements.Geometry
{
    /// <summary>
    /// An object which provides tessellation targets.
    /// </summary>
    internal interface ITessellationTargetProvider
    {
        /// <summary>
        /// Get the tessellation targets.
        /// </summary>
        IEnumerable<ITtessAdator> GetTessellationTargets();
    }

    /// <summary>
    /// An object which provides tessellation targets for a solid.
    /// </summary>
    internal class SolidTesselationTargetProvider : ITessellationTargetProvider
    {
        private readonly Solid solid;
        private readonly Transform transform;

        /// <summary>
        /// Construct a SolidTesselationTargetProvider.
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="transform"></param>
        public SolidTesselationTargetProvider(Solid solid, Transform transform = null)
        {
            this.solid = solid;
            this.transform = transform;
        }

        /// <summary>
        /// Get the tessellation targets.
        /// </summary>
        public IEnumerable<ITtessAdator> GetTessellationTargets()
        {
            foreach (var f in solid.Faces.Values)
            {
                yield return new SolidFaceTessAdaptor(f, transform);
            }
        }
    }

    /// <summary>
    /// An object which provides tessellation targets for a csg solid.
    /// </summary>
    internal class CsgTessellationTargetProvider : ITessellationTargetProvider
    {
        private readonly Csg.Solid csg;

        /// <summary>
        /// Construct a CsgTessellationTargetProvider.
        /// </summary>
        /// <param name="csg"></param>
        public CsgTessellationTargetProvider(Csg.Solid csg)
        {
            this.csg = csg;
        }

        /// <summary>
        /// Get the tessellation targets.
        /// </summary>
        public IEnumerable<ITtessAdator> GetTessellationTargets()
        {
            foreach (var p in csg.Polygons)
            {
                yield return new CsgPolygonTessAdaptor(p);
            }
        }
    }

    /// <summary>
    /// An object which creates a tessellation for a tessellation target.
    /// </summary>
    internal interface ITtessAdator
    {
        /// <summary>
        /// Does this target require tessellation?
        /// </summary>
        bool RequiresTessellation();

        /// <summary>
        /// Get the tessellation.
        /// </summary>
        Tess GetTess();
    }

    /// <summary>
    /// An object which provides a tessellation for a csg polygon.
    /// </summary>
    internal class CsgPolygonTessAdaptor : ITtessAdator
    {
        private readonly Csg.Polygon polygon;

        /// <summary>
        /// Construct a CsgPolygonTessAdaptor.
        /// </summary>
        /// <param name="polygon"></param>
        public CsgPolygonTessAdaptor(Csg.Polygon polygon)
        {
            this.polygon = polygon;
        }

        /// <summary>
        /// Get the tessellation.
        /// </summary>
        public Tess GetTess()
        {
            var tess = new Tess
            {
                NoEmptyPolygons = true
            };
            tess.AddContour(polygon.Vertices.ToContourVertices());

            tess.Tessellate(WindingRule.Positive, ElementType.Polygons, 3);
            return tess;
        }

        /// <summary>
        /// Does this target require tessellation?
        /// </summary>
        public bool RequiresTessellation()
        {
            return polygon.Vertices.Count > 3;
        }
    }

    /// <summary>
    /// An object which provides a tessellation for a solid face.
    /// </summary>
    internal class SolidFaceTessAdaptor : ITtessAdator
    {
        private readonly Face face;
        private readonly Transform transform;

        /// <summary>
        /// Construct a SolidFaceTessAdaptor.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="transform"></param>
        public SolidFaceTessAdaptor(Face face, Transform transform = null)
        {
            this.face = face;
            this.transform = transform;
        }

        /// <summary>
        /// Get the tessellation.
        /// </summary>
        public Tess GetTess()
        {
            var tess = new Tess
            {
                NoEmptyPolygons = true
            };
            tess.AddContour(face.Outer.ToContourVertexArray(transform));

            if (face.Inner != null)
            {
                foreach (var loop in face.Inner)
                {
                    tess.AddContour(loop.ToContourVertexArray(transform));
                }
            }

            tess.Tessellate(WindingRule.Positive, ElementType.Polygons, 3);
            return tess;
        }

        /// <summary>
        /// Does this target require tessellation?
        /// </summary>
        public bool RequiresTessellation()
        {
            return true;
        }
    }

    /// <summary>
    /// Methods for the tessellation of various objects.
    /// </summary>
    internal static class Tessellation
    {
        /// <summary>
        /// Triangulate a collection of CSGs and pack the triangulated data into
        /// a supplied buffers object. 
        /// </summary>
        internal static void Tessellate(IEnumerable<ITessellationTargetProvider> providers,
                                        IGraphicsBuffers buffers,
                                        bool mergeVertices = false,
                                        Func<(Vector3, Vector3, UV, Color), (Vector3, Vector3, UV, Color)> modifyVertexAttributes = null)
        {
            var allVertices = new List<(Vector3 position, Vector3 normal, UV uv, Color color)>();
            foreach (var provider in providers)
            {
                foreach (var target in provider.GetTessellationTargets())
                {
                    TessellatePolygon(target.GetTess(), buffers, allVertices, mergeVertices);
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
                    buffers.AddVertex(v.position, v.normal, v.uv);
                }
            }
        }

        private static void TessellatePolygon(Tess tess,
                                              IGraphicsBuffers buffers,
                                              List<(Vector3 position, Vector3 normal, UV uv, Color color)> allVertices,
                                              bool mergeVertices = false)
        {
            if (tess.ElementCount == 0)
            {
                return;
            }

            var vertexIndices = new ushort[tess.Vertices.Length];

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