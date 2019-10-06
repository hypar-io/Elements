using Elements.Geometry.Interfaces;
using Elements.Serialization.JSON;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    [JsonInheritanceAttribute("Elements.Geometry.Line", typeof(Line))]
    [JsonInheritanceAttribute("Elements.Geometry.Arc", typeof(Arc))]
    [JsonInheritanceAttribute("Elements.Geometry.Polyline", typeof(Polyline))]
    [JsonInheritanceAttribute("Elements.Geometry.Polygon", typeof(Polygon))]
    public abstract partial class Curve : ICurve
    {
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
        public abstract Transform[] Frames(double startSetback = 0, double endSetback = 0);
        
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
    }
}