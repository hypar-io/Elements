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
            var contour = new ContourVertex[loop.Edges.Count];
            for (var i = 0; i < loop.Edges.Count; i++)
            {
                var edge = loop.Edges[i];
                var cv = new ContourVertex();
                cv.Position = new Vec3 { X = edge.Vertex.Point.X, Y = edge.Vertex.Point.Y, Z = edge.Vertex.Point.Z };
                contour[i] = cv;
            }
            return contour;
        }

        internal static Edge[] GetLinkedEdges(this Loop loop)
        {
            var edges = new Edge[loop.Edges.Count];
            for (var i = 0; i < edges.Length; i++)
            {
                edges[i] = loop.Edges[i].Edge;
            }
            return edges;
        }

        internal static Plane Plane(this Face f)
        {
            var v = f.Outer.Edges.Select(e => e.Vertex.Point).ToList();
            return new Plane(v[0], v.NormalFromPlanarWoundPoints());
        }
    }
}