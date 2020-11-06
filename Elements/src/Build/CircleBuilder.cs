using System.Collections.Generic;
using Elements.Geometry;

namespace Elements
{
    public class CircleBuilder : BuilderBase
    {
        private double _radius = 1.0;
        private Vector3 _origin = Vector3.Origin;

        public CircleBuilder(List<string> errors) : base(errors) { }

        public Circle Build()
        {
            return new Circle(_origin, _radius);
        }

        public CircleBuilder OfRadius(double radius)
        {
            if (radius <= 0)
            {
                _errors.Add($"The provided radius, {radius}, was less than zero. Using a default radius of 1.0");
            }
            else
            {
                _radius = radius;
            }

            return this;
        }

        public CircleBuilder AtOrigin(Vector3 origin)
        {
            _origin = origin;
            return this;
        }
    }
}