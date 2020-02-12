using Autodesk.Revit.DB;
using Elements.Geometry;

namespace Hypar.Revit 
{
    public static class RevitExtensions {
        public static Vector3 ToVector3(this XYZ xyz) {
            return new Vector3(xyz.X, xyz.Y, xyz.Z);
        }       
    }
}