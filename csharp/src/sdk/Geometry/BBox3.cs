using System.Collections.Generic;
using System.Linq;

namespace Hypar.Geometry
{
    /// <summary>
    /// An axis-alignment bounding box.
    /// </summary>
    public class BBox3
    {
        /// <summary>
        /// The maximum extent of the bounding box.
        /// </summary>
        public Vector3 Max{get; private set;}

        /// <summary>
        /// The minimum extent of the bounding box.
        /// </summary>
        public Vector3 Min{get; private set;}

        /// <summary>
        /// Construct a bounding box from a collection of points.
        /// </summary>
        /// <param name="points">The points which are contained within the bounding box.</param>
        public BBox3(IList<Vector3> points)
        {
            this.Min = new Vector3(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            this.Max = new Vector3(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
            foreach(var p in points)
            {
                this.Extend(p);
            }
        }

        private void Extend(Vector3 v)
        {
            if(v.X < this.Min.X) this.Min.X = v.X;
            if(v.Y < this.Min.Y) this.Min.Y = v.Y;
            if(v.Z < this.Min.Z) this.Min.Z = v.Z;

            if(v.X > this.Max.X) this.Max.X = v.X;
            if(v.Y > this.Max.Y) this.Max.Y = v.Y;
            if(v.Z > this.Max.Z) this.Max.Z = v.Z;
        }

        /// <summary>
        /// Construct the BBox3 for a Profile.
        /// </summary>
        /// <param name="profile">The Profile.</param>
        public BBox3(Profile profile)
        {
            this.Min = new Vector3(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            this.Max = new Vector3(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
            foreach(var v in profile.Perimeter.Vertices)
            {
                this.Extend(v);
            }

            foreach(var v in profile.Voids.SelectMany(o=>o.Vertices))
            {
                this.Extend(v);
            }
        }

        /// <summary>
        /// Construct the bounding box for a curve.
        /// </summary>
        /// <param name="curve"></param>
        public BBox3(ICurve curve)
        {
            this.Min = new Vector3(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            this.Max = new Vector3(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
            foreach(var p in curve.Vertices)
            {
                if(p < this.Min) this.Min = p;
                if(p > this.Max) this.Max = p;
            }
        }

        /// <summary>
        /// Construct a bounding box for a collection of polygons.
        /// </summary>
        /// <param name="polygons"></param>
        public BBox3(IList<Polygon> polygons)
        {
            var verts = polygons.SelectMany(p=>p.Curves().SelectMany(v=>v));
            this.Min = new Vector3(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            this.Max = new Vector3(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
            foreach(var p in verts)
            {
                if(p < this.Min) this.Min = p;
                if(p > this.Max) this.Max = p;
            }
        }

        /// <summary>
        /// Construct a bounding box specifying minimum and maximum extents.
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