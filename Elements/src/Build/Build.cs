using System.Collections.Generic;
using Elements.Geometry;
using Elements.Serialization.glTF;

namespace Elements
{
    public static class Build
    {
        private static Model _model = new Model();
        private static List<string> _errors = new List<string>();

        public static List<string> Errors => _errors;

        public static CircleBuilder Circle()
        {
            var builder = new CircleBuilder(_errors);
            return builder;
        }

        public static LineBuilder Line()
        {
            var builder = new LineBuilder(_errors);
            return builder;
        }

        public static ModelCurveBuilder ModelCurve()
        {
            var builder = new ModelCurveBuilder(_model, _errors);
            return builder;
        }

        public static PointAtCoordinatesBuilder Point()
        {
            var builder = new PointAtCoordinatesBuilder(_errors);
            return builder;
        }

        public static PointAlongCurveBuilder PointAlongCurve(Curve curve)
        {
            var builder = new PointAlongCurveBuilder(curve, _errors);
            return builder;
        }

        public static BeamBuilder Beam()
        {
            var builder = new BeamBuilder(_model, _errors);
            return builder;
        }

        public static void ToGltf(string path)
        {
            _model.ToGlTF(path);
        }
    }
}