using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// An axis-aligned bounding box.
    /// </summary>
    public partial struct BBox3
    {
        /// <summary>
        /// Construct a bounding box from an array of points.
        /// </summary>
        /// <param name="points">The points which are contained within the bounding box.</param>
        public BBox3(IList<Vector3> points)
        {
            this.Min = new Vector3(double.MaxValue, double.MaxValue, double.MaxValue);
            this.Max = new Vector3(double.MinValue, double.MinValue, double.MinValue);
            for (var i = 0; i < points.Count; i++)
            {
                this.Extend(points[i]);
            }
        }

        private void Extend(Vector3 v)
        {
            var newMin = new Vector3(Min.X, Min.Y, Min.Z);
            if (v.X < this.Min.X) newMin.X = v.X;
            if (v.Y < this.Min.Y) newMin.Y = v.Y;
            if (v.Z < this.Min.Z) newMin.Z = v.Z;
            this.Min = newMin;

            var newMax = new Vector3(Max.X, Max.Y, Max.Z);
            if (v.X > this.Max.X) newMax.X = v.X;
            if (v.Y > this.Max.Y) newMax.Y = v.Y;
            if (v.Z > this.Max.Z) newMax.Z = v.Z;
            this.Max = newMax;
        }

        /// <summary>
        /// Create the BBox3 for a Profile.
        /// </summary>
        /// <param name="profile">The Profile.</param>
        public BBox3(Profile profile)
        {
            this.Min = new Vector3(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            this.Max = new Vector3(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
            for (var i = 0; i < profile.Perimeter.Vertices.Count; i++)
            {
                this.Extend(profile.Perimeter.Vertices[i]);
            }

            for (var i = 0; i < profile.Voids.Count; i++)
            {
                var v = profile.Voids[i];
                for (var j = 0; j < v.Vertices.Count; j++)
                {
                    this.Extend(v.Vertices[j]);
                }
            }
        }

        /// <summary>
        /// Create a bounding box for a collection of polygons.
        /// </summary>
        /// <param name="polygons"></param>
        public BBox3(IList<Polygon> polygons)
        {
            this.Min = new Vector3(double.MaxValue, double.MaxValue, double.MaxValue);
            this.Max = new Vector3(double.MinValue, double.MinValue, double.MinValue);
            foreach (var p in polygons)
            {
                foreach (var v in p.Vertices)
                {
                    this.Extend(v);
                }
            }
        }

        /// <summary>
        /// Get a transformed copy of the bounding box.
        /// </summary>
        /// <param name="bBox">This bounding box.</param>
        /// <param name="transform">The transform to apply.</param>
        public BBox3 Transformed(this BBox3 bBox, Transform transform)
        {
            return new BBox3(transform.OfPoint(bBox.Min), transform.OfPoint(bBox.Max));
        }

        /// <summary>
        /// Get the center of the bounding box.
        /// </summary>
        /// <returns>The center of the bounding box.</returns>
        public Vector3 Center()
        {
            return this.Max.Average(this.Min);
        }
    }
}