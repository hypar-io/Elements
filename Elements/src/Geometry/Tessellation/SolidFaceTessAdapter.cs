using Elements.Geometry.Solids;
using LibTessDotNet.Double;

namespace Elements.Geometry.Tessellation
{
    /// <summary>
    /// An object which provides a tessellation for a solid face.
    /// </summary>
    internal class SolidFaceTessAdapter : ITessAdapter
    {
        private readonly Face face;
        private readonly Transform transform;
        private readonly long faceId;

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
                UsePooling = true,
                NoEmptyPolygons = true
            };

            tess.AddContour(face.Outer.ToContourVertexArray((int)face.Id, transform));

            if (face.Inner != null)
            {
                foreach (var loop in face.Inner)
                {
                    tess.AddContour(loop.ToContourVertexArray((int)face.Id, transform));
                }
            }

            tess.Tessellate(WindingRule.Positive, ElementType.Polygons, 3);
            return tess;
        }
    }
}