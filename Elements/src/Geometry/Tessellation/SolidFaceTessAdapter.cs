using Elements.Geometry.Solids;
using LibTessDotNet.Double;

namespace Elements.Geometry.Tessellation
{
    /// <summary>
    /// An object which provides a tessellation for a solid.
    /// </summary>
    internal class SolidTessAdapter : ITessAdapter
    {
        private readonly Solid solid;
        private readonly Transform transform;

        /// <summary>
        /// Construct a solid tessellation adapter.
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="transform"></param>
        public SolidTessAdapter(Solid solid, Transform transform = null)
        {
            this.solid = solid;
            this.transform = transform;
        }

        public Tess GetTess()
        {
            var tess = new Tess
            {
                NoEmptyPolygons = true
            };
            foreach (var face in solid.Faces.Values)
            {
                tess.AddContour(face.Outer.ToContourVertexArray(transform));

                if (face.Inner != null)
                {
                    foreach (var loop in face.Inner)
                    {
                        tess.AddContour(loop.ToContourVertexArray(transform));
                    }
                }
            }

            tess.Tessellate(WindingRule.Positive, ElementType.Polygons, 3);
            return tess;
        }

        public bool RequiresTessellation()
        {
            return true;
        }
    }

    /// <summary>
    /// An object which provides a tessellation for a solid face.
    /// </summary>
    internal class SolidFaceTessAdapter : ITessAdapter
    {
        private readonly Face face;
        private readonly Transform transform;

        /// <summary>
        /// Construct a SolidFaceTessAdaptor.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="transform"></param>
        public SolidFaceTessAdapter(Face face, Transform transform = null)
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
}