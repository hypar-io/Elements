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
        /// The type of the element.
        /// Used during deserialization to disambiguate derived types.
        /// </summary>
        [JsonProperty("type", Order = -100)]
        public string Type
        {
            get { return this.GetType().FullName.ToLower(); }
        }

        /// <summary>
        /// The vertices of Face.
        /// </summary>
        [JsonIgnore]
        public Vector3[] Vertices
        {
            get
            {
                var vertices = new List<Vector3>();
                foreach(var p in this.Bounds)
                {
                    vertices.AddRange(p.Vertices);
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
                foreach(var b in this.Bounds)
                {
                    edges.AddRange(b.Segments());
                }
                return edges.ToArray();
            }
        }

        /// <summary>
        /// The face's bounds.
        /// </summary>
        [JsonProperty("bounds")]
        public Polygon[] Bounds { get; }

        /// <summary>
        /// Construct a PlanarFace.
        /// </summary>
        /// <param name="bounds">An array of Polygons which define the bounds of the face.</param>
        [JsonConstructor]
        public PlanarFace(Polygon[] bounds)
        {
            this.Bounds = bounds;
        }

        /// <summary>
        /// Construct a PlanarFace.
        /// </summary>
        /// <param name="vertices"></param>
        public PlanarFace(Vector3[] vertices)
        {
            this.Bounds = new[] { new Polygon(vertices) };
        }

        /// <summary>
        /// Construct a PlanarFace.
        /// </summary>
        /// <param name="polygon"></param>
        public PlanarFace(Polygon polygon)
        {
            this.Bounds = new[] { polygon };
        }

        internal Plane Plane()
        {
            return this.Bounds[0].Plane();
        }

        /// <summary>
        /// Compute the Mesh for this Face.
        /// </summary>
        public virtual void Tessellate(Mesh mesh)
        {
            var tess = Mesh.TessFromPolygons(this.Bounds);
            tess.Tessellate(WindingRule.Positive, LibTessDotNet.Double.ElementType.Polygons, 3);
            
            for (var i = 0; i < tess.ElementCount; i++)
            {
                var a = tess.Vertices[tess.Elements[i * 3]].Position.ToVector3();
                var b = tess.Vertices[tess.Elements[i * 3 + 1]].Position.ToVector3();
                var c = tess.Vertices[tess.Elements[i * 3 + 2]].Position.ToVector3();

                mesh.AddTriangle(a, b, c);  //, tess.Normal.ToVector3().Normalized());
            }
        }

        public ICurve Intersect(Plane p)
        {
            throw new System.NotImplementedException();
        }
    }
}