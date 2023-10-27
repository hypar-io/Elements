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
        /// The minimum chord length allowed for subdivision of the curve. A smaller MinimumChordLength results in smoother curves. For polylines and polygons this parameter will have no effect.
        /// </summary>
        /// TODO: This should not live here. Curve resolution for rendering should
        /// live in the rendering code. Unfortunately, we current build BREPs using
        /// this subdivision. When we have non-planar BREP surfaces, we will be able
        /// to decouple BREPs and tessellation and this setting can be passed into
        /// the glTF serializer.
        public const double DefaultMinimumChordLength = 0.1;

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
        public abstract double ArcLength(double start, double end);

        /// <inheritdoc/>
        public virtual Vector3 Mid()
        {
            return PointAt(this.Domain.Mid());
        }

        /// <summary>
        /// Should the curve be considered closed for rendering?
        /// Curves marked true will use LINE_LOOP mode for rendering.
        /// Curves marked false will use LINE_STRIP for rendering.
        /// </summary>
        [JsonIgnore]
        public virtual bool IsClosedForRendering => false;

        /// <inheritdoc/>
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
        /// Create a polyline through a set of 10 segments along the curve.
        /// </summary>
        /// <returns>A polyline.</returns>
        public virtual Polyline ToPolyline()
        {
            return ToPolyline(10);
        }

        /// <summary>
        /// Create a polyline through a set of points along the curve.
        /// </summary>
        /// <param name="divisions">The number of divisions of the curve.</param>
        /// <returns>A polyline.</returns>
        public virtual Polyline ToPolyline(int divisions)
        {
            var pts = new List<Vector3>(divisions + 1);
            var step = this.Domain.Length / divisions;
            for (var t = this.Domain.Min; t < this.Domain.Max; t += step)
            {
                pts.Add(PointAt(t));
            }

            // We don't go all the way to the end parameter, and
            // add it here explicitly because rounding errors can
            // cause small imprecision which accumulates to make
            // the final parameter slightly more/less than the actual
            // end parameter.
            pts.Add(PointAt(this.Domain.Max));
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

        /// <inheritdoc/>
        public abstract double[] GetSubdivisionParameters(double startSetbackDistance = 0,
                                                          double endSetbackDistance = 0);

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