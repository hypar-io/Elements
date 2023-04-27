using System;
using System.Collections.Generic;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    /// <summary>
    /// A bounded curve.
    /// </summary>
    public abstract class BoundedCurve : Curve, IBoundedCurve
    {
        /// <summary>
        /// The start of the curve.
        /// </summary>
        public virtual Vector3 Start { get; protected set; }

        /// <summary>
        /// The end of the curve.
        /// </summary>
        public virtual Vector3 End { get; protected set; }

        /// <summary>
        /// The domain of the curve.
        /// </summary>
        [JsonIgnore]
        public virtual Domain1d Domain => new Domain1d(0, Length());

        /// <summary>
        /// Get the bounding box for this curve.
        /// </summary>
        /// <returns>A bounding box for this curve.</returns>
        public abstract BBox3 Bounds();

        /// <summary>
        /// Calculate the length of the curve.
        /// </summary>
        public abstract double Length();

        /// <summary>
        /// Calculate the length of the curve between two parameters.
        /// </summary>
        public abstract double Length(double start, double end);

        /// <summary>
        /// The mid point of the curve.
        /// </summary>
        public virtual Vector3 Mid()
        {
            return PointAt(this.Domain.Mid());
        }

        /// <summary>
        /// Should the curve be considered closed for rendering?
        /// Curves marked true will use LINE_LOOP mode for rendering.
        /// Curves marked false will use LINE_STRIP for rendering.
        /// </summary>
        public virtual bool IsClosedForRendering => false;

        /// <summary>
        /// Get a collection of transforms which represent frames along this curve.
        /// </summary>
        /// <param name="startSetbackDistance">The offset parameter from the start of the curve.</param>
        /// <param name="endSetbackDistance">The offset parameter from the end of the curve.</param>
        /// <param name="additionalRotation">An additional rotation of the frame at each point.</param>
        /// <returns>A collection of transforms.</returns>
        public virtual Transform[] Frames(double startSetbackDistance = 0.0,
                                          double endSetbackDistance = 0.0,
                                          double additionalRotation = 0.0)
        {
            var parameters = GetSubdivisionParameters(startSetbackDistance, endSetbackDistance);
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
        /// Create a polyline through a set of points along the curve.
        /// </summary>
        /// <param name="divisions">The number of divisions of the curve.</param>
        /// <returns>A polyline.</returns>
        public virtual Polyline ToPolyline(int divisions = 10)
        {
            var pts = new List<Vector3>(divisions + 1);
            var step = this.Domain.Length / divisions;
            for (var t = 0; t <= divisions; t++)
            {
                pts.Add(PointAt(t * step));
            }
            return new Polyline(pts);
        }

        internal GraphicsBuffers ToGraphicsBuffers()
        {
            return this.RenderVertices().ToGraphicsBuffers();
        }

        /// <summary>
        /// Convert a bounded curve to a model curve.
        /// </summary>
        /// <param name="c">The bounded curve to convert.</param>
        public static implicit operator ModelCurve(BoundedCurve c) => new ModelCurve(c);

        /// <summary>
        /// Get parameters to be used to find points along the curve for visualization.
        /// </summary>
        /// <param name="startSetbackDistance">An optional setback from the start of the curve.</param>
        /// <param name="endSetbackDistance">An optional setback from the end of the curve.</param>
        /// <returns>A collection of parameter values.</returns>
        public abstract double[] GetSubdivisionParameters(double startSetbackDistance = 0, double endSetbackDistance = 0);

        /// <summary>
        /// Get a point along the curve at parameter u.
        /// </summary>
        /// <param name="u">A parameter along the curve between 0.0 and 1.0.</param>
        /// <returns>A point along the curve at parameter u.</returns>
        public Vector3 PointAtNormalized(double u)
        {
            if (u < 0 || u > 1)
            {
                throw new ArgumentOutOfRangeException($"The parameter {u} must be between 0.0 and 1.0.");
            }
            return PointAt(u.MapToDomain(this.Domain));
        }

        /// <summary>
        /// Get a transform whose XY plane is perpendicular to the curve, and whose
        /// positive Z axis points along the curve.
        /// </summary>
        /// <param name="u">The parameter along the curve between 0.0 and 1.0.</param>
        /// <returns>A transform.</returns>
        public Transform TransformAtNormalized(double u)
        {
            if (u < 0 || u > 1)
            {
                throw new ArgumentOutOfRangeException($"The parameter {u} must be between 0.0 and 1.0.");
            }
            return TransformAt(u.MapToDomain(this.Domain));
        }


        /// <summary>
        /// Get a collection of vertices used to render the curve.
        /// </summary>
        internal IList<Vector3> RenderVertices()
        {
            var parameters = GetSubdivisionParameters();
            var vertices = new List<Vector3>();
            foreach (var p in parameters)
            {
                vertices.Add(PointAt(p));
            }
            return vertices;
        }
    }
}