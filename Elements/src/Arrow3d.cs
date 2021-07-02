using System.Collections.Generic;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A 3D linework arrow, derived from ModelCurve.
    /// </summary>
    public class Arrow3d : ModelCurve
    {
        /// <summary>
        /// Construct a 3d Arrow element from a curve.
        /// </summary>
        /// <param name="curve">The curve to which to add an arrowhead.</param>
        /// <param name="arrowLength">The length of the arrow, measured from the tip of the curve.</param>
        /// <param name="arrowWidth">The width of the arrow.</param>
        /// <param name="triangleCount">The number of triangles to draw for the arrow's tip.</param>
        [JsonConstructor]
        public Arrow3d(Curve curve, double arrowLength = 0.4, double arrowWidth = 0.2, int triangleCount = 2) : base(curve)
        {
            Polyline polyline;
            switch (curve)
            {
                case Line line:
                    polyline = line.ToPolyline(1);
                    break;
                case Polyline p:
                    polyline = p;
                    break;
                default:
                    polyline = curve.ToPolyline(20);
                    break;
            }
            var endVertices = new List<Vector3>(polyline.Vertices);
            var endTransform = polyline.TransformAt(1);
            for (int i = 0; i < triangleCount; i++)
            {
                var rotationTransform = new Transform();
                rotationTransform.Rotate(Vector3.ZAxis, ((double)i / triangleCount) * 180);
                rotationTransform.Concatenate(endTransform);
                endVertices.Add(rotationTransform.OfPoint(new Vector3(arrowWidth / 2, 0, arrowLength)));
                endVertices.Add(rotationTransform.OfPoint(new Vector3(-arrowWidth / 2, 0, arrowLength)));
                endVertices.Add(rotationTransform.Origin);
            }
            Curve = new Polyline(endVertices);
        }

        /// <summary>
        /// Construct a 3d Arrow element from a location and a direction.
        /// </summary>
        /// <param name="origin">The point at which the arrow starts.</param>
        /// <param name="direction">The direction of the arrow.</param>
        /// <param name="arrowLength">The length of the arrow, measured from the tip of the curve.</param>
        /// <param name="arrowWidth">The width of the arrow.</param>
        /// <param name="triangleCount">The number of triangles to draw for the arrow's tip.</param>
        public Arrow3d(Vector3 origin, Vector3 direction, double arrowLength = 0.4, double arrowWidth = 0.2, int triangleCount = 2) : this(new Line(origin, origin + direction), arrowLength, arrowWidth, triangleCount)
        {

        }

    }
}