using System;

namespace Elements.Geometry
{
    /// <summary>
    /// Create mesh primitives.
    /// </summary>
    public partial class Mesh
    {
        /// <summary>
        /// A mesh sphere.
        /// </summary>
        /// <param name="radius">The radius of the sphere.</param>
        /// <param name="divisions">The number of tessellations of the sphere.</param>
        /// <returns>A mesh.</returns>
        public static Mesh Sphere(double radius, int divisions = 10)
        {
            if (divisions < 2)
            {
                throw new ArgumentException(nameof(divisions), "The number of divisions must be greater than 2.");
            }

            var arc = new Arc(Vector3.Origin, radius, 0, 180).ToPolyline(divisions);
            var t = new Transform();
            var vertices = new Vertex[divisions + 1, divisions + 1];
            var mesh = new Mesh();
            var div = 360.0 / divisions;

            for (var u = 0; u <= divisions; u++)
            {
                if (u > 0)
                {
                    t.Rotate(Vector3.XAxis, div);
                }

                for (var v = 1; v < divisions; v++)
                {
                    var pt = t.OfPoint(arc.Vertices[v]);

                    var vx = new Vertex(pt)
                    {
                        UV = new UV((double)v / (double)divisions, (double)u / (double)divisions)
                    };
                    vertices[u, v] = vx;
                    mesh.AddVertex(vx);

                    if (u > 0 && v > 1)
                    {
                        var a = vertices[u, v];
                        var b = vertices[u, v - 1];
                        var c = vertices[u - 1, v - 1];
                        var d = vertices[u - 1, v];

                        mesh.AddTriangle(a, b, c);
                        mesh.AddTriangle(a, c, d);
                    }
                }
            }

            var p1 = new Vertex(arc.Start)
            {
                UV = new UV(0, 0)
            };

            var p2 = new Vertex(arc.End)
            {
                UV = new UV(1, 1)
            };
            mesh.AddVertex(p1);
            mesh.AddVertex(p2);

            // Make the end caps separately to manage the singularity 
            // at the poles. Attempting to do this in the algorithm above
            // will result in duplicate vertices for every arc section, which
            // is currently illegal in meshes.
            // TODO: This causes spiraling of the UV coordinates at the poles.
            for (var u = 1; u <= divisions; u++)
            {
                mesh.AddTriangle(p1, vertices[u - 1, 1], vertices[u, 1]);
                mesh.AddTriangle(p2, vertices[u, divisions - 1], vertices[u - 1, divisions - 1]);
            }

            mesh.ComputeNormals();

            return mesh;
        }
    }
}