using System.Collections.Generic;
using Elements.Geometry.Interfaces;
using Elements.Serialization.JSON;

namespace Elements.Geometry
{
    [JsonInheritanceAttribute("Elements.Geometry.Line", typeof(Line))]
    [JsonInheritanceAttribute("Elements.Geometry.Arc", typeof(Arc))]
    [JsonInheritanceAttribute("Elements.Geometry.Polyline", typeof(Polyline))]
    [JsonInheritanceAttribute("Elements.Geometry.Polygon", typeof(Polygon))]
    [JsonInheritanceAttribute("Elements.Geometry.Bezier", typeof(Bezier))]
    public abstract partial class Curve : ICurve
    {
        /// <summary>
        /// The minimum chord length allowed for subdivision of the curve.
        /// A lower MinimumChordLength results in smoother curves.
        /// </summary>
        public static double MinimumChordLength = 0.1;

        /// <summary>
        /// Get the bounding box for this curve.
        /// </summary>
        /// <returns>A bounding box for this curve.</returns>
        public abstract BBox3 Bounds();

        /// <summary>
        /// Get a collection of transforms which represent frames along this curve.
        /// </summary>
        /// <param name="startSetback">The offset from the start of the curve.</param>
        /// <param name="endSetback">The offset from the end of the curve.</param>
        /// <returns>A collection of transforms.</returns>
        public virtual Transform[] Frames(double startSetback = 0.0, double endSetback = 0.0)
        {
            var parameters = GetSampleParameters(startSetback, endSetback);
            var transforms = new Transform[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                transforms[i] = TransformAt(parameters[i]);
            }
            return transforms;
        }

        /// <summary>
        /// Calculate the length of the curve.
        /// </summary>
        public abstract double Length();

        /// <summary>
        /// Get a point along the curve at parameter u.
        /// </summary>
        /// <param name="u"></param>
        /// <returns>A point on the curve at parameter u.</returns>
        public abstract Vector3 PointAt(double u);

        /// <summary>
        /// Get a transform whose XY plane is perpendicular to the curve, and whose
        /// positive Z axis points along the curve.
        /// </summary>
        /// <param name="u">The parameter along the Line, between 0.0 and 1.0, at which to calculate the Transform.</param>
        /// <returns>A transform.</returns>
        public abstract Transform TransformAt(double u);

        internal virtual double[] GetSampleParameters(double startSetback = 0.0, double endSetback = 0.0)
        {
            return new[] { 0.0, 1.0 - endSetback };
        }

        /// <summary>
        /// A list of vertices used to render the curve.
        /// </summary>
        internal abstract IList<Vector3> RenderVertices();
    }
}