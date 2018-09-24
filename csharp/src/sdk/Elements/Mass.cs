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
    public class Mass : Element, ITessellate<Mesh>
    {
        private List<Polyline> _sides = new List<Polyline>();
        private readonly Polygon _perimeter;

        /// <summary>
        /// The type of the element.
        /// </summary>
        public override string Type
        {
            get { return "mass"; }
        }

        /// <summary>
        /// The perimeter of the mass.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("perimeter")]
        public Polygon Perimeter
        {
            get{return this.Transform != null ? this.Transform.OfPolygon(this._perimeter) : this._perimeter;}
        }

        /// <summary>
        /// The elevation of the bottom perimeter.
        /// </summary>
        [JsonProperty("elevation")]
        public double Elevation { get; }

        /// <summary>
        /// The height of the mass.
        /// </summary>
        [JsonProperty("height")]
        public double Height { get; }

        /// <summary>
        /// The volume of the mass.
        /// </summary>
        [JsonProperty("volume")]
        public double Volume
        {
            get { return this._perimeter.Area * this.Height; }
        }

        /// <summary>
        /// Construct a mass from perimeters and elevations.
        /// </summary>
        /// <param name="perimeter">The bottom perimeter of the mass.</param>
        /// <param name="elevation">The elevation of the perimeter.</param>
        /// <param name="height">The height of the mass from the bottom elevation.</param>
        /// <param name="material">The mass' material. The default is the built in mass material.</param>
        [JsonConstructor]
        public Mass(Polygon perimeter, double elevation = 0.0, double height = 1.0, Material material = null)
        {
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException("The mass could not be constructed. The height must be greater than zero.");
            }
            this._perimeter = perimeter;
            this.Elevation = elevation;
            this.Height = height;
            this.Material = material != null ? material : BuiltInMaterials.Mass;
        }

        /// <summary>
        /// A collection of curves representing the vertical edges of the mass.
        /// </summary>
        /// <returns></returns>
        public IList<Line> VerticalEdges()
        {
            return Faces().Select(f=>f.Edges[1]).ToList();
        }

        /// <summary>
        /// A collection of curves representing the horizontal edges of the mass.
        /// </summary>
        /// <returns></returns>
        public IList<Line> HorizontalEdges()
        {
            return Faces().SelectMany(f=>new[]{f.Edges[0], f.Edges[2]}).ToList();
        }

        /// <summary>
        /// A collection of polylines representing the perimeter of each face of the mass.
        /// </summary>
        /// <returns></returns>
        public IList<Face> Faces()
        {
            return FacesInternal(this.Perimeter.Vertices);
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
        /// Tessellate the mass.
        /// </summary>
        /// <returns>A mesh representing the tessellated mass.</returns>
        public Mesh Tessellate()
        {
            // We use the untransformed faces here,
            // as the transform will be applied on the rendering node.

            var mesh = new Mesh();
            foreach (var f in FacesInternal(this._perimeter.Vertices))
            {
                mesh.AddQuad(f.Vertices.ToArray());
            }

            mesh.AddTesselatedFace(new[] { this._perimeter }, this.Elevation);
            mesh.AddTesselatedFace(new[] { this._perimeter }, this.Elevation + this.Height, true);
            return mesh;
        }
    }
}