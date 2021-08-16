using System;
using System.Collections.Generic;
using System.Linq;
using LibTessDotNet.Double;

namespace Elements.Geometry.Solids
{
    internal static class SolidExtensions
    {
        /// <summary>
        /// Convert Loop to an array of ContourVertex.
        /// </summary>
        /// <param name="loop"></param>
        /// <param name="face"></param>
        internal static ContourVertex[] ToContourVertexArray(this Loop loop, Face face)
        {
            var contour = new List<ContourVertex>();
            foreach (var edge in loop.Edges)
            {
                var cv = new ContourVertex();
                cv.Position = new Vec3 { X = edge.Vertex.Point.X, Y = edge.Vertex.Point.Y, Z = edge.Vertex.Point.Z };
                contour.Add(cv);
            }
            return contour.ToArray();
        }

        internal static Edge[] GetLinkedEdges(this Loop loop)
        {
            var edges = new List<Edge>();
            foreach (var he in loop.Edges)
            {
                edges.Add(he.Edge);
            }
            return edges.ToArray();
        }

        internal static Plane Plane(this Face f)
        {
            var v = f.Outer.Edges.Select(e => e.Vertex.Point).ToList();
            return new Plane(v[0], v.Normal());
        }
    }
}