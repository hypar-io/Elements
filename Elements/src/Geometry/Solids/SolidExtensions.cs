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
        /// <param name="faceId"></param>
        /// <param name="solidId"></param>
        /// <param name="transform">An optional transform to apply to the contour.</param>
        internal static ContourVertex[] ToContourVertexArray(this Loop loop, long faceId, uint solidId, Transform transform = null)
        {
            var contour = new ContourVertex[loop.Edges.Count];
            for (var i = 0; i < loop.Edges.Count; i++)
            {
                var edge = loop.Edges[i];
                var p = transform == null ? edge.Vertex.Point : transform.OfPoint(edge.Vertex.Point);
                var cv = new ContourVertex
                {
                    Position = new Vec3 { X = p.X, Y = p.Y, Z = p.Z },
                    Data = (default(UV), edge.Vertex.Id, faceId, solidId)
                };
                contour[i] = cv;
            }
            return contour;
        }
        internal static Csg.Vertex[] ToCsgVertexArray(this Loop loop, Vector3 e1, Vector3 e2)
        {
            var vertices = new Csg.Vertex[loop.Edges.Count];
            for (var i = 0; i < loop.Edges.Count; i++)
            {
                var edge = loop.Edges[i];
                var p = edge.Vertex.Point;
                var avv = new Vector3(p.X, p.Y, p.Z);
                var cv = new Csg.Vertex(p.ToCsgVector3(), new Csg.Vector2D(e1.Dot(avv), e2.Dot(avv)));
                vertices[i] = cv;
            }
            return vertices;
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
            var n = v.NormalFromPlanarWoundPoints();
            if (n.Length() > 0)
            {
                return new Plane(v[0], v.NormalFromPlanarWoundPoints());
            }
            else
            {
                throw new System.Exception("Could not get valid normal from points.");
            }
        }
    }
}