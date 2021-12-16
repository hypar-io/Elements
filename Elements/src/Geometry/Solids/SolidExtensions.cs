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

        internal static Csg.Solid ToCsg(this Solid solid)
        {
            var polygons = new List<Csg.Polygon>();

            foreach (var f in solid.Faces.Values)
            {
                if (f.Inner != null && f.Inner.Count() > 0)
                {
                    var tess = new Tess();
                    tess.NoEmptyPolygons = true;

                    tess.AddContour(f.Outer.ToContourVertexArray(f));

                    foreach (var loop in f.Inner)
                    {
                        tess.AddContour(loop.ToContourVertexArray(f));
                    }

                    tess.Tessellate(WindingRule.Positive, LibTessDotNet.Double.ElementType.Polygons, 3);

                    Vector3 e1 = new Vector3();
                    Vector3 e2 = new Vector3();

                    var vertices = new List<Csg.Vertex>(tess.Elements.Count() * 3);
                    for (var i = 0; i < tess.ElementCount; i++)
                    {
                        var a = tess.Vertices[tess.Elements[i * 3]].ToCsgVector3();
                        var b = tess.Vertices[tess.Elements[i * 3 + 1]].ToCsgVector3();
                        var c = tess.Vertices[tess.Elements[i * 3 + 2]].ToCsgVector3();

                        Csg.Vertex av = null;
                        Csg.Vertex bv = null;
                        Csg.Vertex cv = null;

                        if (i == 0)
                        {
                            var n = f.Plane().Normal;
                            e1 = n.Cross(n.IsParallelTo(Vector3.XAxis) ? Vector3.YAxis : Vector3.XAxis).Unitized();
                            e2 = n.Cross(e1).Unitized();
                        }
                        if (av == null)
                        {
                            var avv = new Vector3(a.X, a.Y, a.Z);
                            av = new Csg.Vertex(a, new Csg.Vector2D(e1.Dot(avv), e2.Dot(avv)));
                            vertices.Add(av);
                        }
                        if (bv == null)
                        {
                            var bvv = new Vector3(b.X, b.Y, b.Z);
                            bv = new Csg.Vertex(b, new Csg.Vector2D(e1.Dot(bvv), e2.Dot(bvv)));
                            vertices.Add(bv);
                        }
                        if (cv == null)
                        {
                            var cvv = new Vector3(c.X, c.Y, c.Z);
                            cv = new Csg.Vertex(c, new Csg.Vector2D(e1.Dot(cvv), e2.Dot(cvv)));
                            vertices.Add(cv);
                        }

                        var p = new Csg.Polygon(new List<Csg.Vertex>() { av, bv, cv });
                        polygons.Add(p);
                    }
                }
                else
                {
                    var verts = new List<Csg.Vertex>(f.Outer.Edges.Count);
                    var n = f.Plane().Normal;
                    var e1 = n.Cross(n.IsParallelTo(Vector3.XAxis) ? Vector3.YAxis : Vector3.XAxis).Unitized();
                    var e2 = n.Cross(e1).Unitized();
                    foreach (var e in f.Outer.Edges)
                    {
                        var l = e.Vertex.Point;
                        var v = new Csg.Vertex(l.ToCsgVector3(), new Csg.Vector2D(e1.Dot(l), e2.Dot(l)));
                        verts.Add(v);
                    }
                    var p = new Csg.Polygon(verts, null, new Csg.Plane(n.ToCsgVector3(), 1));
                    polygons.Add(p);
                }
            }
            return Csg.Solid.FromPolygons(polygons);
        }
    }
}