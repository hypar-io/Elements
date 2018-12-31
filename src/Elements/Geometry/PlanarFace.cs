using Elements.Geometry.Interfaces;
using LibTessDotNet.Double;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// A planar face.
    /// </summary>
    public class PlanarFace : IFace
    {
        private IProfile _profile;

        /// <summary>
        /// A Transform with its origin at the Face's first vertex,
        /// and its normal defined by the first and second edges of the Face.
        /// </summary>
        public Transform Transform { get; }

        /// <summary>
        /// The vertices of Face.
        /// </summary>
        public Vector3[] Vertices
        {
            get
            {
                var vertices = new List<Vector3>();
                vertices.AddRange(this._profile.Perimeter.Vertices);
                if (this._profile.Voids != null)
                {
                    foreach (var v in this._profile.Voids)
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
        public ICurve[] Edges
        {
            get
            {
                var edges = new List<ICurve>();
                edges.AddRange(this._profile.Perimeter.Segments());
                if (this._profile.Voids != null)
                {
                    foreach (var v in this._profile.Voids)
                    {
                        edges.AddRange(v.Segments());
                    }
                }
                return edges.ToArray();
            }
        }

        /// <summary>
        /// Construct a PlanarFace.
        /// </summary>
        /// <param name="profile"></param>
        public PlanarFace(IProfile profile)
        {
            this._profile = profile;
        }

        /// <summary>
        /// Construct a PlanarFace.
        /// </summary>
        /// <param name="vertices"></param>
        public PlanarFace(Vector3[] vertices)
        {
            this._profile = new Profile(new Polygon(vertices));
        }

        /// <summary>
        /// Construct a PlanarFace.
        /// </summary>
        /// <param name="polygon"></param>
        public PlanarFace(Polygon polygon)
        {
            this._profile = new Profile(polygon);
        }

        internal Plane Plane()
        {
            return this._profile.Perimeter.Plane();
        }

        /// <summary>
        /// Compute the Mesh for this Face.
        /// </summary>
        public virtual void Tessellate(Mesh mesh)
        {
            var tess = Mesh.TessFromPolygon(this._profile.Perimeter, this._profile.Voids);
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