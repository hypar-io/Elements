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
    public class Mass : Element, IRepresent<Mesh>
    {
        private List<Polyline> _sides = new List<Polyline>();

        /// <summary>
        /// The perimeter of the mass.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("perimeter")]
        public Polygon Perimeter{get;}

        /// <summary>
        /// The elevation of the bottom perimeter.
        /// </summary>
        [JsonProperty("elevation")]
        public double Elevation{get;}

        /// <summary>
        /// The height of the mass.
        /// </summary>
        [JsonProperty("height")]
        public double Height{get;}

        /// <summary>
        /// The volume of the mass.
        /// </summary>
        [JsonProperty("volume")]
        public double Volume
        {
            get{return this.Perimeter.Area * this.Height;}
        }

        /// <summary>
        /// Construct a default mass.
        /// </summary>
        public Mass()
        {
            var defaultProfile = Profiles.Rectangular();
            this.Perimeter = defaultProfile;
            this.Elevation = 0.0;
            this.Height = 1.0;
            this.Material = BuiltInMaterials.Mass;
        }

        /// <summary>
        /// Construct a mass from perimeters and elevations.
        /// </summary>
        /// <param name="perimeter">The bottom perimeter of the mass.</param>
        /// <param name="elevation">The elevation of the perimeter.</param>
        /// <param name="height">The height of the mass from the bottom elevation.</param>
        /// <param name="material">The mass' material. The default is the built in mass material.</param>
        /// <returns></returns>
        public Mass(Polygon perimeter, double elevation, double height, Material material = null)
        {
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException("The mass could not be constructed. The height must be greater than zero.");
            }
            this.Perimeter = perimeter;
            this.Elevation = elevation;
            this.Height = height;
            this.Material = material != null?material: BuiltInMaterials.Mass;
        }

        /// <summary>
        /// A collection of curves representing the vertical edges of the mass.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Line> VerticalEdges()
        {
            foreach(var f in Faces())
            {
                yield return f.ElementAt(1);
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
                yield return f.ElementAt(0);
                yield return f.ElementAt(2);
            }
        }

        /// <summary>
        /// A collection of polylines representing the perimeter of each face of the mass.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Face> Faces()
        {
            var b = this.Perimeter.Vertices;
            var t = this.Perimeter.Vertices;

            for (var i = 0; i < b.Count; i++)
            {
                var next = i + 1;
                if (i == b.Count - 1)
                {
                    next = 0;
                }
                var v1 = b[i];
                var v2 = b[next];
                var v3 = t[next];
                var v4 = t[i];
                var v1n = new Vector3(v1.X, v1.Y, this.Elevation);
                var v2n = new Vector3(v2.X, v2.Y, this.Elevation);
                var v3n = new Vector3(v3.X, v3.Y, this.Elevation + this.Height);
                var v4n = new Vector3(v4.X, v4.Y, this.Elevation + this.Height);
                var l1 = new Line(v1n, v2n);
                var l2 = new Line(v2n, v3n);
                var l3 = new Line(v3n, v4n);
                var l4 = new Line(v4n, v1n);
                yield return new Face(new[]{l1,l2,l3,l4});
            }
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
                if (e >= this.Elevation && e <= this.Elevation + this.Height)
                {
                    var f = new Floor(this.Perimeter, e, thickness, new Polygon[]{}, material);
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
            foreach (var f in Faces())
            {
                mesh.AddQuad(f.Vertices.ToArray());
            }

            mesh.AddTesselatedFace(new[] { this.Perimeter }, this.Elevation);
            mesh.AddTesselatedFace(new[] { this.Perimeter }, this.Elevation + this.Height, true);
            return mesh;
        }
    }
}