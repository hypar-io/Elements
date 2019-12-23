using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// An axis-aligned bounding box.
    /// </summary>
    public class BBox3
    {
        /// <summary>
        /// The maximum extent of the bounding box.
        /// </summary>
        public Vector3 Max { get; private set; }

        /// <summary>
        /// The minimum extent of the bounding box.
        /// </summary>
        public Vector3 Min { get; private set; }

        /// <summary>
        /// Construct a bounding box from an array of points.
        /// </summary>
        /// <param name="points">The points which are contained within the bounding box.</param>
        public BBox3(IList<Vector3> points)
        {
            this.Min = new Vector3(double.MaxValue, double.MaxValue, double.MaxValue);
            this.Max = new Vector3(double.MinValue, double.MinValue, double.MinValue);
            for(var i=0;i<points.Count; i++)
            {
                this.Extend(points[i]);
            }
        }

        private void Extend(Vector3 v)
        {
            var newMin = new Vector3();
            if (v.X < this.Min.X) newMin.X = v.X;
            if (v.Y < this.Min.Y) newMin.Y = v.Y;
            if (v.Z < this.Min.Z) newMin.Z = v.Z;
            this.Min = newMin;
            
            var newMax = new Vector3();
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
            for(var i=0;i<profile.Perimeter.Vertices.Count; i++)
            {
                this.Extend(profile.Perimeter.Vertices[i]);
            }

            for(var i=0; i<profile.Voids.Count; i++)
            {
                var v = profile.Voids[i];
                for(var j=0;j<v.Vertices.Count; j++)
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
            this.Min = new Vector3(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            this.Max = new Vector3(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);

            foreach (var p in polygons)
            {
                foreach (var v in p.Vertices)
                {
                    if (v < this.Min) this.Min = v;
                    if (v > this.Max) this.Max = v;
                }
            }
        }

        /// <summary>
        /// Create a bounding box specifying minimum and maximum extents.
        /// </summary>
        /// <param name="min">The minimum extent of the bounding box.</param>
        /// <param name="max">The maximum extent of the bounding box.</param>
        public BBox3(Vector3 min, Vector3 max)
        {
            this.Min = min;
            this.Max = max;
        }
    }
}