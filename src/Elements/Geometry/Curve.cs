using Elements.Geometry.Interfaces;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    public abstract partial class Curve : ICurve
    {
        /// <summary>
        /// The type of the curve.
        /// Used during deserialization to disambiguate derived types.
        /// </summary>
        [JsonProperty(Order = -100)]
        public string Type
        {
            get { return this.GetType().FullName.ToLower(); }
        }
        
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