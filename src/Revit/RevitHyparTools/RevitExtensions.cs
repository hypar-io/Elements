using Autodesk.Revit.DB;
using Elements.Geometry;

namespace RevitHyparTools 
{
    public static class RevitExtensions {
        public static Vector3 ToVec3(this XYZ xyz) {
            return new Vector3(xyz.X, xyz.Y, xyz.Z);
        }       
    }
}