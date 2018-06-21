using System;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;

namespace Hypar.Elements
{
    /// <summary>
    /// A linear structural element with a cross section.
    /// </summary>
    public class Beam : Element, ITessellate<Mesh>
    {
        private Line _centerLine;
        private Polyline _profile;
        private Vector3 _up;

        /// <summary>
        /// The centerline of the beam.
        /// </summary>
        public Line CenterLine => _centerLine;

        /// <summary>
        /// The cross-section profile of the beam.
        /// </summary>
        public Polyline Profile => _profile;
        
        /// <summary>
        /// The up axis of the beam.
        /// </summary>
        public Vector3 UpAxis => _up;

        /// <summary>
        /// Construct a default beam.
        /// </summary>
        /// <returns></returns>
        public Beam() : base(BuiltInMaterials.Default,null)
        {
            this._centerLine = new Line(Vector3.Origin(), Vector3.ByXYZ(1,0,0));
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
            this.Transform = centerLine.GetTransform(up);
        }

        /// <summary>
        /// Construct a beam along a line.
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public static Beam AlongLine(Line l)
        {
            var beam = new Beam();
            beam._centerLine = l;
            beam.Transform = beam._centerLine.GetTransform();
            return beam;
        }

        /// <summary>
        /// Tessellate the beam.
        /// </summary>
        /// <returns>A mesh representing the tessellated beam.</returns>
        public Mesh Tessellate()
        {
            return Mesh.ExtrudeAlongLine(this.CenterLine, new[] { this.Profile });
        }

        /// <summary>
        /// Set the profile of the beam.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public Beam WithProfile(Polyline profile)
        {
            this._profile = profile;
            return this;
        }

        /// <summary>
        /// Set the profile of the beam using a selector function.
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public Beam WithProfile(Func<Beam,Polyline> selector)
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
            this.Transform = this._centerLine.GetTransform(up);
            return this;
        }

        /// <summary>
        /// Set the material of the beam.
        /// </summary>
        /// <param name="material">The beam's material.</param>
        /// <returns></returns>
        public Beam OfMaterial(Material material)
        {
            this.Material = material;
            return this;
        }
    }

    /// <summary>
    /// Extension methods for beams.
    /// </summary>
    public static class BeamCollectionExtensions
    {   
        /// <summary>
        /// Set the profile of a collection of beams.
        /// </summary>
        /// <param name="beams"></param>
        /// <param name="profile"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<Beam> WithProfile(this IEnumerable<Beam> beams, Polyline profile, Func<Beam,Polyline> selector = null)
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
        /// <param name="beams"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        public static IEnumerable<Beam> OfMaterial(this IEnumerable<Beam> beams, Material material)
        {
            foreach(var b in beams)
            {
                b.OfMaterial(material);
            }
            return beams;
        }

        /// <summary>
        /// Set the up axis of a collection of beams.
        /// </summary>
        /// <param name="beams"></param>
        /// <param name="v"></param>
        /// <returns></returns>
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