using Elements.Geometry.Interfaces;
using LibTessDotNet.Double;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// A planar face.
    /// </summary>
    public class PlanarFace : IFace
    {
        /// <summary>
        /// The vertices of Face.
        /// </summary>
        [JsonIgnore]
        public Vector3[] Vertices
        {
            get
            {
                var vertices = new List<Vector3>();
                vertices.AddRange(this.Profile.Perimeter.Vertices);
                if (this.Profile.Voids != null)
                {
                    foreach (var v in this.Profile.Voids)
                    {
                        vertices.AddRange(v.Vertices);
                    }
                }
                return vertices.ToArray();
            }
        }

        /// <summary>
        /// The edges of the Face.
        /// </summary>
        [JsonIgnore]
        public ICurve[] Edges
        {
            get
            {
                var edges = new List<ICurve>();
                edges.AddRange(this.Profile.Perimeter.Segments());
                if (this.Profile.Voids != null)
                {
                    foreach (var v in this.Profile.Voids)
                    {
                        edges.AddRange(v.Segments());
                    }
                }
                return edges.ToArray();
            }
        }

        /// <summary>
        /// The face's Profile.
        /// </summary>
        [JsonProperty("profile")]
        public IProfile Profile { get; }

        /// <summary>
        /// Construct a PlanarFace.
        /// </summary>
        /// <param name="profile"></param>
        [JsonConstructor]
        public PlanarFace(IProfile profile)
        {
            this.Profile = profile;
        }

        /// <summary>
        /// Construct a PlanarFace.
        /// </summary>
        /// <param name="vertices"></param>
        public PlanarFace(Vector3[] vertices)
        {
            this.Profile = new Profile(new Polygon(vertices));
        }

        /// <summary>
        /// Construct a PlanarFace.
        /// </summary>
        /// <param name="polygon"></param>
        public PlanarFace(Polygon polygon)
        {
            this.Profile = new Profile(polygon);
        }

        internal Plane Plane()
        {
            return this.Profile.Perimeter.Plane();
        }

        /// <summary>
        /// Compute the Mesh for this Face.
        /// </summary>
        public virtual void Tessellate(Mesh mesh)
        {
            var tess = Mesh.TessFromPolygon(this.Profile.Perimeter, this.Profile.Voids);
            tess.Tessellate(WindingRule.Positive, LibTessDotNet.Double.ElementType.Polygons, 3);

            for (var i = 0; i < tess.ElementCount; i++)
            {
                var a = tess.Vertices[tess.Elements[i * 3]].Position.ToVector3();
                var b = tess.Vertices[tess.Elements[i * 3 + 1]].Position.ToVector3();
                var c = tess.Vertices[tess.Elements[i * 3 + 2]].Position.ToVector3();

                mesh.AddTriangle(a, b, c);
            }
        }
    }
}