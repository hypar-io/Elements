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
        [JsonProperty("bottom_elevation")]
        public double BottomElevation{get;}

        /// <summary>
        /// The elevation of the top perimeter.
        /// </summary>
        [JsonProperty("top_elevation")]
        public double TopElevation{get;}

        /// <summary>
        /// Construct a default mass.
        /// </summary>
        public Mass()
        {
            var defaultProfile = Profiles.Rectangular();
            this.Perimeter = defaultProfile;
            this.BottomElevation = 0.0;
            this.TopElevation = 1.0;
            this.Material = BuiltInMaterials.Mass;
        }

        /// <summary>
        /// Construct a mass from perimeters and elevations.
        /// </summary>
        /// <param name="perimeter">The bottom perimeter of the mass.</param>
        /// <param name="bottomElevation">The elevation of the bottom perimeter.</param>
        /// <param name="top">The top perimeter of the mass.</param>
        /// <param name="topElevation">The elevation of the top perimeter.</param>
        /// <param name="material">The mass' material. The default is the built in mass material.</param>
        /// <returns></returns>
        public Mass(Polygon perimeter, double bottomElevation, Polygon top, double topElevation, Material material = null)
        {
            if (perimeter.Vertices.Count != top.Vertices.Count)
            {
                throw new ArgumentException(Messages.PROFILES_UNEQUAL_VERTEX_EXCEPTION);
            }

            if (topElevation <= bottomElevation)
            {
                throw new ArgumentOutOfRangeException(Messages.TOP_BELOW_BOTTOM_EXCEPTION, "topElevation");
            }

            this.Perimeter = perimeter;
            this.BottomElevation = bottomElevation;
            this.TopElevation = topElevation;
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
                var v1n = new Vector3(v1.X, v1.Y, this.BottomElevation);
                var v2n = new Vector3(v2.X, v2.Y, this.BottomElevation);
                var v3n = new Vector3(v3.X, v3.Y, this.TopElevation);
                var v4n = new Vector3(v4.X, v4.Y, this.TopElevation);
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
                if (e >= this.BottomElevation && e <= this.TopElevation)
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

            mesh.AddTesselatedFace(new[] { this.Perimeter }, this.BottomElevation);
            mesh.AddTesselatedFace(new[] { this.Perimeter }, this.TopElevation, true);
            return mesh;
        }
    }
}