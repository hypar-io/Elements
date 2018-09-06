using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;

namespace Hypar.Elements
{
    /// <summary>
    /// A mass represents an extruded building mass.
    /// </summary>
    public class Mass : Element, ILocateable<Polyline>, ITessellate<Mesh>, IMaterialize
    {
        private List<Polyline> _sides = new List<Polyline>();

        /// <summary>
        /// The bottom perimeter of the mass.
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
        /// The elevation of the top perimeter.
        /// </summary>
        [JsonProperty("top_elevation")]
        public double TopElevation{get;}

        /// <summary>
        /// The material of the Mass.
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public Material Material{get;set;}

        /// <summary>
        /// Construct a default mass.
        /// </summary>
        public Mass()
        {
            var defaultProfile = Profiles.Rectangular();
            this.Location = defaultProfile;
            this.BottomElevation = 0.0;
            this.TopElevation = 1.0;
            this.Material = BuiltInMaterials.Mass;
        }

        /// <summary>
        /// Construct a mass from perimeters and elevations.
        /// </summary>
        /// <param name="bottom">The bottom perimeter of the mass.</param>
        /// <param name="bottomElevation">The elevation of the bottom perimeter.</param>
        /// <param name="top">The top perimeter of the mass.</param>
        /// <param name="topElevation">The elevation of the top perimeter.</param>
        /// <returns></returns>
        public Mass(Polyline bottom, double bottomElevation, Polyline top, double topElevation)
        {
            if (bottom.Vertices.Count() != top.Vertices.Count())
            {
                throw new ArgumentException(Messages.PROFILES_UNEQUAL_VERTEX_EXCEPTION);
            }

            if (topElevation <= bottomElevation)
            {
                throw new ArgumentOutOfRangeException(Messages.TOP_BELOW_BOTTOM_EXCEPTION, "topElevation");
            }

            this.Location = bottom;
            this.BottomElevation = bottomElevation;
            this.TopElevation = topElevation;
            this.Material = BuiltInMaterials.Mass;
        }

        /// <summary>
        /// A collection of curves representing the vertical edges of the mass.
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
        /// A collection of curves representing the horizontal edges of the mass.
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
        /// A collection of polylines representing the perimeter of each face of the mass.
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
                var v3n = new Vector3(v3.X, v3.Y, this.TopElevation);
                var v4n = new Vector3(v4.X, v4.Y, this.TopElevation);
                yield return new Polyline(new[] { v1n, v2n, v3n, v4n });
            }
        }

        /// <summary>
        /// For each face of the mass apply a creator function.
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
        /// Create floors at the specified elevations within a mass.
        /// </summary>
        /// <param name="elevations"></param>
        /// <param name="thickness"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        public IEnumerable<Floor> CreateFloors(IEnumerable<double> elevations, double thickness, Material material)
        {
            var floors = new List<Floor>();
            foreach(var e in elevations)
            {
                if (e >= this.BottomElevation && e <= this.TopElevation)
                {
                    var f = new Floor(this.Location, new Polyline[]{}, e, thickness, material);
                    floors.Add(f);
                }
            }
            return floors;
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
            mesh.AddTesselatedFace(new[] { this.Location }, this.TopElevation, true);
            return mesh;
        }
    }
}