using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using dxf = netDxf.Entities;

namespace Elements.DXF.Extensions
{
    /// <summary>
    /// Extension methods for converting Element geometric primitives into DXF Entities and objects.
    /// </summary>
    public static class ToDxfExtensions
    {
        /// <summary>
        /// Convert to a DXF Polyline entity.
        /// </summary>
        public static dxf.Polyline ToDxf(this Polyline polyline)
        {
            IEnumerable<netDxf.Vector3> vertices = polyline.Vertices.Select(v => v.ToDxf());
            var dxf = new dxf.Polyline(vertices, true);
            return dxf;
        }

        /// <summary>
        /// Convert to a DXF Vector3.
        /// </summary>
        public static netDxf.Vector3 ToDxf(this Vector3 vector3)
        {
            return new netDxf.Vector3(vector3.X, vector3.Y, vector3.Z);
        }
    }
}