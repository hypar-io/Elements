using System.Collections.Generic;
using Elements.Geometry.Interfaces;

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
        public virtual Vector3 End { get; protected set;}

        /// <summary>
        /// Get the bounding box for this curve.
        /// </summary>
        /// <returns>A bounding box for this curve.</returns>
        public abstract BBox3 Bounds();

        /// <summary>
        /// Calculate the length of the curve.
        /// </summary>
        public abstract double Length();

        /// </summary>
        /// The mid point of the curve.
        /// </summary>
        public virtual Vector3 Mid()
        {
            return PointAt(0.5);
        }

        /// <summary>
        /// A list of vertices used to render the curve.
        /// </summary>
        internal abstract IList<Vector3> RenderVertices();

        internal GraphicsBuffers ToGraphicsBuffers()
        {
            return this.RenderVertices().ToGraphicsBuffers();
        }

        /// <summary>
        /// Convert a bounded curve to a model curve.
        /// </summary>
        /// <param name="c">The bounded curve to convert.</param>
        public static implicit operator ModelCurve(BoundedCurve c) => new ModelCurve(c);
    }
}