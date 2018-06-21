using haxe.root;
using verb;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Geometry
{
    /// <summary>
    /// NurbsCurve represents a non-uniform, rational, b-spline.
    /// </summary>
    public class NurbsCurve : ICurve
    {
        private verb.geom.NurbsCurve _curve;

        /// <summary>
        /// Construct a nurbs curve that goes through the specified points with the specified degree.
        /// </summary>
        /// <param name="points">The points through which the curve will pass.</param>
        /// <param name="degree">The degree of the curve.</param>
        public NurbsCurve(IEnumerable<Vector3> points, int degree)
        {   
            var pts = points.ToArray();
            if(points == null || pts.Length <= 3)
            {   
                throw new Exception("You must supply at least three points.");
            }

            var cps = new haxe.root.Array<object>();
            foreach(var p in pts)
            {
                var pArr = new haxe.root.Array<double>(p.ToArray());
                cps.push(pArr);
            }
            this._curve = verb.geom.NurbsCurve.byPoints(cps, degree);
        }
        
        /// <summary>
        /// Get a transform whose XY plane is perpendicular to the curve, and whose
        /// positive Z axis points along the curve.
        /// </summary>
        /// <param name="up">The vector which will become the Y vector of the transform.</param>
        /// <returns>A transform.</returns>
        public Transform GetTransform(Vector3 up = null)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Get the length of the curve.
        /// </summary>
        /// <returns>The length.</returns>
        public double Length()
        {
            return this._curve.length();
        }

        /// <summary>
        /// Get the point at parameter u on the curve.
        /// </summary>
        /// <param name="u"></param>
        /// <returns>The point at parameter u on the curve.</returns>
        public Vector3 PointAt(double u)
        {
            return this._curve.point(u).ToVector3();
        }

        /// <summary>
        /// Tessellate the curve.
        /// </summary>
        /// <returns>A collection of points sampled along the curve.</returns>
        public IEnumerable<Vector3> Tessellate()
        {
            var tess = this._curve.tessellate(0.00001);
            return tess.ToVector3Collection();
        }
    }
}