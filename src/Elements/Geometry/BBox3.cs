using Elements.Geometry.Interfaces;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// An axis-alignment bounding box.
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
        public BBox3(List<Vector3> points)
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
            if (v.X < this.Min.X) this.Min.X = v.X;
            if (v.Y < this.Min.Y) this.Min.Y = v.Y;
            if (v.Z < this.Min.Z) this.Min.Z = v.Z;

            if (v.X > this.Max.X) this.Max.X = v.X;
            if (v.Y > this.Max.Y) this.Max.Y = v.Y;
            if (v.Z > this.Max.Z) this.Max.Z = v.Z;
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