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
        /// Get a translated copy of the bounding box.
        /// </summary>
        /// <param name="translation">The translation to apply.</param>
        public BBox3 Translated(Vector3 translation)
        {
            return new BBox3(this.Min + translation, this.Max + translation);
        }

        /// <summary>
        /// Get the center of the bounding box.
        /// </summary>
        /// <returns>The center of the bounding box.</returns>
        public Vector3 Center()
        {
            return this.Max.Average(this.Min);
        }

        /// <summary>
        /// Is the provided object a bounding box? If so, is it
        /// equal to this bounding box within Epsilon?
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is BBox3))
            {
                return false;
            }
            var a = (BBox3)obj;
            return this.Min.IsAlmostEqualTo(a.Min) && this.Max.IsAlmostEqualTo(a.Max);
        }

        /// <summary>
        /// Get the hash code for the bounding box.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        /// The string representation of the bounding box.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Min:{this.Min.ToString()}, Max:{this.Max.ToString()}";
        }

        /// <summary>
        /// Are the two bounding boxes equal within Epsilon?
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static bool operator ==(BBox3 a, BBox3 b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Are the two bounding boxes not equal within Epsilon?
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static bool operator !=(BBox3 a, BBox3 b)
        {
            return !a.Equals(b);
        }
    }
}