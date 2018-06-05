using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;

namespace Hypar.Elements
{
    public class Mass : Element, IMeshProvider
    {
        private List<Polyline> m_sides = new List<Polyline>();
        private Polyline _bottom;
        private Polyline _top;
        private double _bottomElevation;
        private double _topElevation;

        /// <summary>
        /// The bottom perimeter of the Mass.
        /// </summary>
        /// <returns></returns>
        public Polyline Bottom => _bottom;

        public double BottomElevation => _bottomElevation;

        /// <summary>
        /// The top perimeter of the Mass.
        /// </summary>
        /// <returns></returns>
        public Polyline Top => _top;

        public double TopElevation => _topElevation;

        /// <summary>
        /// The faces of the mass.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Polyline> Faces
        {
            get { return m_sides; }
        }

        public static Mass WithBottomProfile(Polyline profile)
        {
            if(profile.Count() == 0)
            {
                throw new ArgumentException(Messages.EMPTY_POLYLINE_EXCEPTION, "profile");
            }

            var m = new Mass(profile);
            return m;
        }

        internal Mass(Polyline profile)
        {
            this._bottom = profile;
            this._top = profile;
            this._bottomElevation = 0.0;
            this._topElevation = 10.0;
        }

        internal Mass(Polyline bottom, double bottomElevation, Polyline top, double topElevation, Material material, Transform transform = null) : base(material, transform)
        {
            if (bottom.Vertices.Count() != top.Vertices.Count())
            {
                throw new ArgumentException(Messages.PROFILES_UNEQUAL_VERTEX_EXCEPTION);
            }

            if (topElevation <= bottomElevation)
            {
                throw new ArgumentOutOfRangeException(Messages.TOP_BELOW_BOTTOM_EXCEPTION, "topElevation");
            }

            this._top = top;
            this._bottom = bottom;
            this._bottomElevation = bottomElevation;
            this._topElevation = topElevation;

            var b = bottom.Vertices.ToArray();
            var t = top.Vertices.ToArray();

            for (var i = 0; i < b.Length; i++)
            {
                var next = i + 1;
                if (i == b.Length - 1)
                {
                    next = 0;
                }
                var v1 = b[i];
                var v2 = b[next];
                var v3 = t[next];
                var v4 = t[i];
                var v1n = new Vector3(v1.X, v1.Y, bottomElevation);
                var v2n = new Vector3(v2.X, v2.Y, bottomElevation);
                var v3n = new Vector3(v3.X, v3.Y, topElevation);
                var v4n = new Vector3(v4.X, v4.Y, topElevation);
                var side = new Polyline(new[] { v1n, v2n, v3n, v4n });
                m_sides.Add(side);
            }
        }
        
        public Mesh Tessellate()
        {
            var mesh = new Mesh();
            foreach (var s in m_sides)
            {
                mesh.AddQuad(s.ToArray());
            }

            mesh.AddTesselatedFace(new[] { this.Bottom }, this.BottomElevation);
            mesh.AddTesselatedFace(new[] { this.Top }, this.TopElevation, true);
            return mesh;
        }

        public Mass WithTopAt(double elevation)
        {
            if(elevation <= this._bottomElevation)
            {
                throw new ArgumentException(Messages.TOP_BELOW_BOTTOM_EXCEPTION, "elevation");
            }
            this._topElevation = elevation;
            return this;
        }

        public Mass WithBottomAt(double elevation)
        {
            if(elevation >= this._topElevation)
            {
                throw new ArgumentException(Messages.BOTTOM_ABOVE_TOP_EXCEPTION, "elevation");
            }
            this._bottomElevation = elevation;
            return this;
        }
    
        public Mass WithTopProfile(Polyline profile)
        {
            if(profile.Count() == 0)
            {
                throw new ArgumentException(Messages.EMPTY_POLYLINE_EXCEPTION, "profile");
            }
            this._top = profile;
            return this;
        }

    }
}