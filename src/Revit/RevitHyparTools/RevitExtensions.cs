using System.Linq;
using Autodesk.Revit.DB;
using Elements.Geometry;

namespace Hypar.Revit 
{
    public static class RevitExtensions {
        public static Vector3 ToVector3(this XYZ xyz) {
            return new Vector3(xyz.X, xyz.Y, xyz.Z);
        }       
        public static Polygon ToPolygon(this CurveLoop cL)
        {
            return new Polygon(cL.Select(l => l.GetEndPoint(0).ToVector3()).ToList());
        }
    }
}