using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using dxf = netDxf.Entities;

namespace Elements.DXF.Extensions
{
    public static class ToDxfExtensions
    {
        public static dxf.Polyline ToDxf(this Polyline polyline)
        {
            IEnumerable<netDxf.Vector3> vertices = polyline.Vertices.Select(v => v.ToDxf());
            var dxf = new dxf.Polyline(vertices, true);
            return dxf;
        }

        public static netDxf.Vector3 ToDxf(this Vector3 vector3)
        {
            return new netDxf.Vector3(vector3.X, vector3.Y, vector3.Z);
        }
    }
}