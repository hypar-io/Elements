using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Elements.Geometry;

namespace Hypar.Revit
{
    public static class RevitGeometryExtensions
    {
        public static double ToFeet(this double meters)
        {
            return meters * 3.28084;
        }

        public static ColorWithTransparency ToColorWithTransparency(this Elements.Geometry.Color color)
        {
            // TODO: Fix this when we reverse alpha values.
            return new ColorWithTransparency((uint)(color.Red * 255), (uint)(color.Green * 255), (uint)(color.Blue * 255), (uint)((1.0 - color.Alpha) * 255));
        }

        public static XYZ ToXYZ(this Vector3 vector)
        {
            return new XYZ(vector.X, vector.Y, vector.Z);
        }

        public static XYZ ToXYZFeet(this Vector3 vector)
        {
            return new XYZ(vector.X.ToFeet(), vector.Y.ToFeet(), vector.Z.ToFeet());
        }

        public static XYZ ToXYZ(this Vector3 vector, Application app)
        {
            return app.Create.NewXYZ(vector.X.ToFeet(), vector.Y.ToFeet(), vector.Z.ToFeet());
        }

        public static Autodesk.Revit.DB.Line ToRevitLine(this Elements.Geometry.Line line, Application app)
        {
            return Autodesk.Revit.DB.Line.CreateBound(line.Start.ToXYZ(app), line.End.ToXYZ(app));
        }

        public static CurveArray ToRevitCurveArray(this Elements.Geometry.Profile profile, Application app)
        {
            var curveArr = profile.Perimeter.ToRevitCurveArray(app);
            return curveArr;
        }

        public static CurveArray ToRevitCurveArray(this Polygon polygon, Application app)
        {
            var curveArr = new CurveArray();
            foreach (var l in polygon.Segments())
            {
                curveArr.Append(l.ToRevitLine(app));
            }
            return curveArr;
        }

        public static Frame ToFrame(this Elements.Geometry.Transform transform, Application app)
        {
            return new Frame(transform.Origin.ToXYZ(app), transform.XAxis.ToXYZ(app).Normalize(), transform.YAxis.ToXYZ(app).Normalize(), transform.ZAxis.ToXYZ(app).Normalize());
        }
    }
}