using System;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;

namespace Hypar.Elements
{
    public class Beam : Element, IMeshProvider, IDataProvider
    {
        private Line _centerLine;
        private Polyline _profile;
        private Vector3 _up;

        private double _verticalOffset = 0.0;
        public double VerticalOffset => _verticalOffset;

        private double _horizontalOffset = 0.0;
        public double HorizontalOffset => _horizontalOffset;

        public Line CenterLine => _centerLine;

        public Polyline Profile => _profile;
        
        public Vector3 UpAxis => _up;

        internal Beam() : base(BuiltIntMaterials.Default,null)
        {
            this._profile = Profiles.WideFlangeProfile();
        }

        internal Beam(Line centerLine, Polyline profile, Material material, Vector3 up = null, Transform transform = null) : base(material, transform)
        {
            this._profile = profile;
            this._centerLine = centerLine;
            if(up != null)
            {
                this._up = up;
            }
            this._transform = centerLine.GetTransform(up);
        }
        
        /// <summary>
        /// Construct a beam along a line.
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public static Beam AlongLine(Line l)
        {
            var b = new Beam();
            b._centerLine = l;
            b._transform = b._centerLine.GetTransform();
            return b;
        }

        /// <summary>
        /// Construct many beams along a collection of lines.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static IEnumerable<Beam> AlongLines(IEnumerable<Line> lines)
        {
            var beams = new List<Beam>();
            foreach(var l in lines)
            {
                var b = Beam.AlongLine(l);
                beams.Add(b);
            }
            return beams;
        }

        /// <summary>
        /// Construct many beams along a collection of lines.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static IEnumerable<Beam> AlongLines(params Line[] lines)
        {
            var beams = new List<Beam>();
            foreach(var l in lines)
            {
                var b = Beam.AlongLine(l);
                beams.Add(b);
            }
            return beams;
        }

        public Mesh Tessellate()
        {
            return Mesh.ExtrudeAlongLine(this.CenterLine, new[] { this.Profile });
        }

        public Dictionary<string, double> Data()
        {
            var data = new Dictionary<string, double>();
            data.Add("length", this.CenterLine.Length());
            return data;
        }

        /// <summary>
        /// Set the profile of the beam.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public Beam WithProfile(Polyline profile, double verticalOffset = 0.0, double horizontalOffset = 0.0)
        {
            this._profile = profile;
            this._verticalOffset = verticalOffset;
            this._horizontalOffset = horizontalOffset;
            return this;
        }

        /// <summary>
        /// Set the profile of the beam using a selector function.
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public Beam WithProfile(Func<Beam,Polyline> selector, double verticalOffset = 0.0, double horizontalOffset = 0.0)
        {
            this._profile = selector(this);
            return this; 
        }

        /// <summary>
        /// Set the up axis of the beam.
        /// </summary>
        /// <param name="up"></param>
        /// <returns></returns>
        public Beam WithUpAxis(Vector3 up)
        {
            this._up = up;
            this._transform = this._centerLine.GetTransform(up);
            return this;
        }

        public Beam OfMaterial(Material m)
        {
            this._material = m;
            return this;
        }
    }

    public static class BeamCollectionExtensions
    {   
        /// <summary>
        /// Set the profile of a collection of beams.
        /// </summary>
        /// <param name="beams"></param>
        /// <param name="profile"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerable<Beam> WithProfile(this IEnumerable<Beam> beams, Polyline profile, Action<Beam,Polyline> predicate = null)
        {
            foreach(var b in beams)
            {
                b.WithProfile(profile);
            }
            return beams;
        }

        /// <summary>
        /// Set the profile of a collection of beams using a selector function.
        /// </summary>
        /// <param name="beams"></param>
        /// <param name="profileSelector"></param>
        /// <returns></returns>
        public static IEnumerable<Beam> WithProfile(this IEnumerable<Beam> beams, Func<Beam,Polyline> profileSelector)
        {
            foreach(var b in beams)
            {
                beams.WithProfile(profileSelector(b));
            }
            return beams;
        }

        /// <summary>
        /// Set the material of a collection of beams.
        /// </summary>
        /// <param name="IEnumerable<Beam>beams"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        public static IEnumerable<Beam> OfMaterial(this IEnumerable<Beam>beams, Material material)
        {
            foreach(var b in beams)
            {
                b.OfMaterial(material);
            }
            return beams;
        }

        public static IEnumerable<Beam> WithUpAxis(this IEnumerable<Beam> beams, Vector3 v)
        {
            foreach(var b in beams)
            {
                b.WithUpAxis(v);
            }
            return beams;
        }
    }
}