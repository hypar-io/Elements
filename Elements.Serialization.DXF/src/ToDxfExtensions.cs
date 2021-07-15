using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using IxMilia.Dxf;
using IxMilia.Dxf.Entities;

namespace Elements.Serialization.DXF.Extensions
{
    /// <summary>
    /// Extension methods for converting Element geometric primitives into DXF Entities and objects.
    /// </summary>
    public static class ToDxfExtensions
    {

        /// <summary>
        /// Convert to a DXF Polyline entity.
        /// </summary>
        public static DxfPolyline ToDxf(this Polyline polyline)
        {
            var vertices = polyline.Vertices.Select(v => v.ToDxf());
            var dxf = new DxfPolyline(vertices);
            dxf.IsClosed = polyline is Polygon;
            return dxf;
        }

        /// <summary>
        /// Convert to a DXF Vector3.
        /// </summary>
        public static DxfVertex ToDxf(this Vector3 vector3)
        {
            return new DxfVertex(new DxfPoint(vector3.X, vector3.Y, vector3.Z));
        }
    }
}