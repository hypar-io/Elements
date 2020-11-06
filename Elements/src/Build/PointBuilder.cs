using System.Collections.Generic;
using Elements.Geometry;

namespace Elements
{
    public abstract class PointBuilder : BuilderBase
    {
        public PointBuilder(List<string> errors) : base(errors) { }

        public virtual Vector3 Build()
        {
            return new Vector3();
        }
    }

    public class PointAtCoordinatesBuilder : PointBuilder
    {
        double _x = 0.0;
        double _y = 0.0;
        double _z = 0.0;

        public PointAtCoordinatesBuilder(List<string> errors) : base(errors) { }

        public override Vector3 Build()
        {
            var pt = new Vector3(_x, _y, _z);
            return pt;
        }

        public PointBuilder X(double x)
        {
            _x = x;
            return this;
        }

        public PointBuilder Y(double y)
        {
            _y = y;
            return this;
        }

        public PointBuilder Z(double z)
        {
            _z = z;
            return this;
        }
    }

    public class PointAlongCurveBuilder : PointBuilder
    {
        private double _u = 0.5;
        private Curve _curve;

        public PointAlongCurveBuilder(Curve curve, List<string> errors) : base(errors)
        {
            _curve = curve;
        }

        public override Vector3 Build()
        {
            return _curve.PointAt(_u);
        }

        public PointBuilder AtParameter(double u)
        {
            _u = u;
            return this;
        }
    }
}