using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// A bounded segment of a line.
    /// </summary>
    public class LineSegment : TrimmedCurve<Line>
    {
        /// <summary>
        /// Get the bounding box for this line.
        /// </summary>
        /// <returns>A bounding box for this line.</returns>
        public override BBox3 Bounds()
        {
            if (this.Start < this.End)
            {
                return new BBox3(this.Start, this.End);
            }
            else
            {
                return new BBox3(this.End, this.Start);
            }
        }

        /// <summary>
        /// Calculate the length of the line segment.
        /// </summary>
        public override double Length()
        {
            return this.Start.DistanceTo(this.End);
        }

        public override Vector3 PointAt(double u)
        {
            return this.BasisCurve.PointAt(u);
        }

        public override Transform TransformAt(double u)
        {
            return this.BasisCurve.TransformAt(u);
        }

        public override Curve Transformed(Transform transform)
        {
            throw new System.NotImplementedException();
        }

        internal override IList<Vector3> RenderVertices()
        {
            throw new System.NotImplementedException();
        }
    }
}