using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;

namespace Hypar.Elements
{
    /// <summary>
    /// A space represents an extruded boundary an occupiable region within a building.
    /// </summary>
    public class Space : Element, ILocateable<Polyline>, ITessellate<Mesh>, IMaterialize
    {
        private List<Polyline> _sides = new List<Polyline>();

        /// <summary>
        /// The bottom perimeter of the space.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("location")]
        public Polyline Location{get;}

        /// <summary>
        /// The elevation of the bottom perimeter.
        /// </summary>
        [JsonProperty("bottom_elevation")]
        public double BottomElevation{get;}

        /// <summary>
        /// The height of the space above its bottom elevation.
        /// </summary>
        [JsonProperty("height")]
        public double Height{get;}

        /// <summary>
        /// The material of the Mass.
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public Material Material{get;set;}

        /// <summary>
        /// Construct a default space.
        /// </summary>
        public Space()
        {
            var defaultProfile = Profiles.Rectangular();
            this.Location = defaultProfile;
            this.BottomElevation = 0.0;
            this.Height = 1.0;
            this.Material = BuiltInMaterials.Default;
        }

        /// <summary>
        /// Construct a space from a perimeter and a height.
        /// </summary>
        /// <param name="bottom">The bottom perimeter of the space.</param>
        /// <param name="bottomElevation">The elevation of the bottom perimeter.</param>
        /// <param name="height">The height of the space above the bottom elevation.</param>
        /// <returns></returns>
        public Space(Polyline bottom, double bottomElevation, double height)
        {

            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException(Messages.HEIGHT_EXCEPTION, "height");
            }

            this.Location = bottom;
            this.BottomElevation = bottomElevation;
            this.Material = BuiltInMaterials.Default;
        }

        /// <summary>
        /// A collection of curves representing the vertical edges of the space.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Line> VerticalEdges()
        {
            foreach(var f in Faces())
            {
                yield return f.Segments().ElementAt(1);
            }
            
        }

        /// <summary>
        /// A collection of curves representing the horizontal edges of the space.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Line> HorizontalEdges()
        {
            foreach(var f in Faces())
            {
                yield return f.Segments().ElementAt(0);
                yield return f.Segments().ElementAt(2);
            }
        }

        /// <summary>
        /// A collection of polylines representing the perimeter of each face of the space.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Polyline> Faces()
        {
            var b = this.Location.Vertices.ToArray();
            var t = this.Location.Vertices.ToArray();

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
                var v1n = new Vector3(v1.X, v1.Y, this.BottomElevation);
                var v2n = new Vector3(v2.X, v2.Y, this.BottomElevation);
                var v3n = new Vector3(v3.X, v3.Y, this.BottomElevation + this.Height);
                var v4n = new Vector3(v4.X, v4.Y, this.BottomElevation + this.Height);
                yield return new Polyline(new[] { v1n, v2n, v3n, v4n });
            }
        }

        /// <summary>
        /// For each face of the space apply a creator function.
        /// </summary>
        /// <param name="creator">The function to apply.</param>
        /// <typeparam name="T">The creator function's return type.</typeparam>
        /// <returns></returns>
        public IEnumerable<T> ForEachFaceCreateElementsOfType<T>(Func<Polyline, IEnumerable<T>> creator)
        {
            var results = new List<T>();
            foreach(var p in Faces())
            {
                results.AddRange(creator(p));
            }
            return results;
        }

       
        /// <summary>
        /// Tessellate the mass.
        /// </summary>
        /// <returns>A mesh representing the tessellated mass.</returns>
        public Mesh Tessellate()
        {
            var mesh = new Mesh();
            foreach (var s in Faces())
            {
                mesh.AddQuad(s.ToArray());
            }

            mesh.AddTesselatedFace(new[] { this.Location }, this.BottomElevation);
            mesh.AddTesselatedFace(new[] { this.Location }, this.BottomElevation + this.Height, true);
            return mesh;
        }
    }
}