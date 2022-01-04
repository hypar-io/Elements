using LibTessDotNet.Double;

namespace Elements.Geometry.Tessellation
{
    /// <summary>
    /// An object which provides a tessellation for a csg polygon.
    /// </summary>
    internal class CsgPolygonTessAdapter : ITessAdapter
    {
        private readonly Csg.Polygon polygon;

        /// <summary>
        /// Construct a CsgPolygonTessAdaptor.
        /// </summary>
        /// <param name="polygon"></param>
        public CsgPolygonTessAdapter(Csg.Polygon polygon)
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
}