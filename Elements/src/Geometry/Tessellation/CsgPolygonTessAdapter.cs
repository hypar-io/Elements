using LibTessDotNet.Double;

namespace Elements.Geometry.Tessellation
{
    /// <summary>
    /// An object which provides a tessellation for a csg polygon.
    /// </summary>
    internal class CsgPolygonTessAdapter : ITessAdapter
    {
        private readonly Csg.Polygon polygon;
        private readonly int faceId;

        /// <summary>
        /// Construct a CsgPolygonTessAdaptor.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="faceId"></param>
        public CsgPolygonTessAdapter(Csg.Polygon polygon, int faceId)
        {
            this.polygon = polygon;
            this.faceId = faceId;
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
            tess.AddContour(polygon.Vertices.ToContourVertexArray(faceId));

            tess.Tessellate(WindingRule.Positive, ElementType.Polygons, 3);
            return tess;
        }
    }
}