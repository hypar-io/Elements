using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;

namespace Hypar.Elements
{
    /// <summary>
    /// A Mass represents an extruded building Mass.
    /// </summary>
    public class Mass : Element, ITessellate<Mesh>
    {
        private List<Polyline> _sides = new List<Polyline>();
        private readonly Profile _profile;

        /// <summary>
        /// The type of the element.
        /// </summary>
        public override string Type
        {
            get { return "mass"; }
        }

        /// <summary>
        /// The Profile of the Mass.
        /// </summary>
        [JsonProperty("profile")]
        public Profile Profile
        {
            get{return this.Transform != null ? this.Transform.OfProfile(this._profile) : this._profile;}
        }

        /// <summary>
        /// The elevation of the bottom perimeter.
        /// </summary>
        [JsonProperty("elevation")]
        public double Elevation { get; }

        /// <summary>
        /// The height of the Mass.
        /// </summary>
        [JsonProperty("height")]
        public double Height { get; }

        /// <summary>
        /// The volume of the Mass.
        /// </summary>
        [JsonProperty("volume")]
        public double Volume
        {
            get { return this._profile.Area * this.Height; }
        }

        /// <summary>
        /// Construct a Mass.
        /// </summary>
        /// <param name="profile">The Profile of the Mass.</param>
        /// <param name="elevation">The elevation of the perimeter.</param>
        /// <param name="height">The height of the Mass from the bottom elevation.</param>
        /// <param name="material">The Mass' material. The default is the built in Mass material.</param>
        [JsonConstructor]
        public Mass(Profile profile, double elevation = 0.0, double height = 1.0, Material material = null)
        {
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException("The Mass could not be constructed. The height must be greater than zero.");
            }
            this._profile = profile;
            this.Elevation = elevation;
            this.Height = height;
            this.Material = material != null ? material : BuiltInMaterials.Mass;
        }

        /// <summary>
        /// A collection of curves representing the vertical edges of the Mass.
        /// </summary>
        /// <returns></returns>
        public IList<Line> VerticalEdges()
        {
            return Faces().Select(f=>f.Edges[1]).ToList();
        }

        /// <summary>
        /// A collection of curves representing the horizontal edges of the Mass.
        /// </summary>
        /// <returns></returns>
        public IList<Line> HorizontalEdges()
        {
            return Faces().SelectMany(f=>new[]{f.Edges[0], f.Edges[2]}).ToList();
        }

        /// <summary>
        /// A collection of polylines representing the perimeter of each face of the Mass.
        /// </summary>
        /// <returns></returns>
        public IList<Face> Faces()
        {
            return FacesInternal(this.Profile.Perimeter.Vertices);
        }

        private IList<Face> FacesInternal(IList<Vector3> v)
        {
            var faces = new List<Face>();
            for (var i = 0; i < v.Count; i++)
            {
                var next = i + 1;
                if (i == v.Count - 1)
                {
                    next = 0;
                }
                var v1 = v[i];
                var v2 = v[next];
                var v1n = new Vector3(v1.X, v1.Y, this.Elevation);
                var v2n = new Vector3(v2.X, v2.Y, this.Elevation);
                var v3n = new Vector3(v2.X, v2.Y, this.Elevation + this.Height);
                var v4n = new Vector3(v1.X, v1.Y, this.Elevation + this.Height);
                var l1 = new Line(v1n, v2n);
                var l2 = new Line(v2n, v3n);
                var l3 = new Line(v3n, v4n);
                var l4 = new Line(v4n, v1n);
                faces.Add(new Face(new[] { l1, l2, l3, l4 }));
            }
            return faces;
        }

        /// <summary>
        /// Tessellate the Mass.
        /// </summary>
        /// <returns>A mesh representing the tessellated Mass.</returns>
        public Mesh Tessellate()
        {
            // We use the untransformed faces here,
            // as the transform will be applied on the rendering node.

            var mesh = new Mesh();
            foreach (var f in FacesInternal(this._profile.Perimeter.Vertices))
            {
                mesh.AddQuad(f.Vertices.ToArray());
            }

            mesh.AddTesselatedFace(this._profile.Perimeter, this._profile.Voids, new Transform(0.0,0.0,this.Elevation));
            mesh.AddTesselatedFace(this._profile.Perimeter, this._profile.Voids, new Transform(0.0,0.0,this.Elevation + this.Height), true);
            return mesh;
        }
    }
}