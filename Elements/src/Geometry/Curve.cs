using System.Collections.Generic;
using Elements.Geometry.Interfaces;
using Elements.Serialization.JSON;
using System.Text.Json.Serialization;

namespace Elements.Geometry
{
    /// <summary>
    /// The abstract base class for all curves.
    /// </summary>
    [JsonConverter(typeof(ElementConverter<Curve>))]
    public abstract partial class Curve : ICurve, ITransformable<Curve>
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
        /// <param name="startSetback">The offset parameter from the start of the curve.</param>
        /// <param name="endSetback">The offset parameter from the end of the curve.</param>
        /// <param name="additionalRotation">An additional rotation of the frame at each point.</param>
        /// <returns>A collection of transforms.</returns>
        public virtual Transform[] Frames(double startSetback = 0.0,
                                          double endSetback = 0.0,
                                          double additionalRotation = 0.0)
        {
            var parameters = GetSampleParameters(startSetback, endSetback);
            var transforms = new Transform[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                transforms[i] = TransformAt(parameters[i]);
                if (additionalRotation != 0.0)
                {
                    transforms[i].RotateAboutPoint(transforms[i].Origin, transforms[i].ZAxis, additionalRotation);
                }
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


        /// <summary>
        /// Create a polyline through a set of points along the curve.
        /// </summary>
        /// <param name="divisions">The number of divisions of the curve.</param>
        /// <returns>A polyline.</returns>
        public virtual Polyline ToPolyline(int divisions = 10)
        {
            var pts = new List<Vector3>(divisions + 1);
            for (var t = 0; t <= divisions; t++)
            {
                pts.Add(PointAt(t * 1.0 / divisions));
            }
            return new Polyline(pts);
        }

        internal virtual double[] GetSampleParameters(double startSetback = 0.0, double endSetback = 0.0)
        {
            return new[] { startSetback, 1.0 - endSetback };
        }

        /// <summary>
        /// A list of vertices used to render the curve.
        /// </summary>
        internal abstract IList<Vector3> RenderVertices();

        /// <summary>
        /// Construct a transformed copy of this Curve.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public abstract Curve Transformed(Transform transform);

        /// <summary>
        /// Implicitly convert a curve to a ModelCurve Element.
        /// </summary>
        /// <param name="c">The curve to convert.</param>
        public static implicit operator ModelCurve(Curve c) => new ModelCurve(c);

        internal GraphicsBuffers ToGraphicsBuffers()
        {
            return this.RenderVertices().ToGraphicsBuffers();
        }
    }
}