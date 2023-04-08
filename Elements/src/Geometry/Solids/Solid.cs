#pragma warning disable 1591

using Elements.Geometry.Tessellation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Elements.Geometry.Interfaces;
using Elements.Search;
using Elements.Spatial;
using LibTessDotNet.Double;

[assembly: InternalsVisibleTo("Hypar.Elements.Tests")]

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// A boundary representation of a solid.
    /// </summary>
    public partial class Solid : ITessellate
    {
        private long _faceId;
        private long _edgeId = 10000;
        private long _vertexId = 100000;

        /// <summary>
        /// The Faces of the Solid.
        /// </summary>
        public Dictionary<long, Face> Faces { get; }

        /// <summary>
        /// The edges of the solid.
        /// </summary>
        public Dictionary<long, Edge> Edges { get; }

        /// <summary>
        /// The vertices of the solid.
        /// </summary>
        public Dictionary<long, Vertex> Vertices { get; }

        /// <summary>
        /// Construct a solid.
        /// </summary>
        public Solid()
        {
            this.Faces = new Dictionary<long, Face>();
            this.Edges = new Dictionary<long, Edge>();
            this.Vertices = new Dictionary<long, Vertex>();
        }

        /// <summary>
        /// Construct a lamina solid.
        /// </summary>
        /// <param name="perimeter">The perimeter of the lamina's faces.</param>
        public static Solid CreateLamina(IList<Vector3> perimeter)
        {
            return CreateLamina(new Polygon(perimeter));
        }

        public static Solid CreateLamina(Polygon perimeter, IList<Polygon> voids = null)
        {
            var solid = new Solid();
            if (voids != null && voids.Count > 0)
            {
                solid.AddFace(perimeter, voids);
                solid.AddFace(perimeter, voids, true, reverse: true);
            }
            else
            {
                solid.AddFace(perimeter);
                solid.AddFace(perimeter, null, true, reverse: true);
            }

            return solid;
        }

        public static Solid CreateLamina(Profile profile)
        {
            return CreateLamina(profile.Perimeter, profile.Voids);
        }

        /// <summary>
        /// Construct a solid by sweeping a face.
        /// </summary>
        /// <param name="perimeter">The perimeter of the face to sweep.</param>
        /// <param name="holes">The holes of the face to sweep.</param>
        /// <param name="distance">The distance to sweep.</param>
        /// <param name="bothSides">Should the sweep start offset by direction distance/2? </param>
        /// <param name="rotation">An optional rotation in degrees of the perimeter around the z axis.</param>
        /// <returns>A solid.</returns>
        public static Solid SweepFace(Polygon perimeter,
                                      IList<Polygon> holes,
                                      double distance,
                                      bool bothSides = false,
                                      double rotation = 0.0)
        {
            return Solid.SweepFace(perimeter, holes, Vector3.ZAxis, distance, bothSides, rotation);
        }

        /// <summary>
        /// Construct a solid by sweeping a face along a curve.
        /// </summary>
        /// <param name="perimeter">The perimeter of the face to sweep.</param>
        /// <param name="holes">The holes of the face to sweep.</param>
        /// <param name="curve">The curve along which to sweep.</param>
        /// <param name="startSetback">The setback distance of the sweep from the start of the curve.</param>
        /// <param name="endSetback">The setback distance of the sweep from the end of the curve.</param>
        /// <param name="profileRotation">The rotation of the profile.</param>
        /// <returns>A solid.</returns>
        public static Solid SweepFaceAlongCurve(Polygon perimeter,
                                                IList<Polygon> holes,
                                                IBoundedCurve curve,
                                                double startSetback = 0,
                                                double endSetback = 0,
                                                double profileRotation = 0)
        {
            var solid = new Solid();

            var l = curve.Length();

            // The start and end setbacks can't be more than
            // the length of the beam together.
            if ((startSetback + endSetback) >= l)
            {
                startSetback = 0;
                endSetback = 0;
            }

            var transforms = curve.Frames(startSetback, endSetback, profileRotation);

            if (curve is Polygon)
            {
                for (var i = 0; i < transforms.Length; i++)
                {
                    var next = i == transforms.Length - 1 ? transforms[0] : transforms[i + 1];
                    solid.SweepPolygonBetweenPlanes(perimeter, transforms[i], next);
                }
            }
            else if (curve is Bezier)
            {
                var startCap = solid.AddFace(perimeter, transform: transforms[0]);
                for (var i = 0; i < transforms.Length - 1; i++)
                {
                    var next = transforms[i + 1];
                    solid.SweepPolygonBetweenPlanes(perimeter, transforms[i], next);
                }
                var endCap = solid.AddFace(perimeter, transform: transforms[transforms.Length - 1], reverse: true);
            }
            else
            {
                // Add start cap.
                Face cap = null;
                Edge[][] openEdges;

                if (holes != null)
                {
                    cap = solid.AddFace(perimeter, holes, transform: transforms[0]);
                    openEdges = new Edge[1 + holes.Count][];
                }
                else
                {
                    cap = solid.AddFace(perimeter, transform: transforms[0]);
                    openEdges = new Edge[1][];
                }

                // last outer edge
                var openEdge = cap.Outer.GetLinkedEdges();
                openEdge = solid.SweepEdges(transforms, openEdge);
                openEdges[0] = openEdge;

                if (holes != null)
                {
                    for (var i = 0; i < cap.Inner.Length; i++)
                    {
                        openEdge = cap.Inner[i].GetLinkedEdges();

                        // last inner edge for one hole
                        openEdge = solid.SweepEdges(transforms, openEdge);
                        openEdges[i + 1] = openEdge;
                    }
                }

                solid.Cap(openEdges, true);
            }

            return solid;
        }

        /// <summary>
        /// Construct a solid by sweeping a face in a direction.
        /// </summary>
        /// <param name="perimeter">The perimeter of the face to sweep.</param>
        /// <param name="holes">The holes of the face to sweep.</param>
        /// <param name="direction">The direction in which to sweep.</param>
        /// <param name="distance">The distance to sweep.</param>
        /// <param name="bothSides">Should the sweep start offset by direction distance/2? </param>
        /// <param name="rotation">An optional rotation in degrees of the perimeter around the direction vector.</param>
        /// <returns>A solid.</returns>
        public static Solid SweepFace(Polygon perimeter,
                                      IList<Polygon> holes,
                                      Vector3 direction,
                                      double distance,
                                      bool bothSides = false,
                                      double rotation = 0.0)
        {
            // We do a difference of the polygons
            // to get the clipped shape. This will fail in interesting
            // ways if the clip creates two islands.
            // if(holes != null)
            // {
            //     var newPerimeter = perimeter.Difference(holes);
            //     perimeter = newPerimeter[0];
            //     holes = newPerimeter.Skip(1).Take(newPerimeter.Count - 1).ToArray();
            // }

            var solid = new Solid();
            Face fStart = null;
            if (bothSides)
            {
                var t = new Transform(direction.Negate() * (distance / 2), rotation);
                if (holes != null)
                {
                    fStart = solid.AddFace(perimeter, holes, transform: t, reverse: true);
                }
                else
                {
                    fStart = solid.AddFace(perimeter, transform: t, reverse: true);
                }
            }
            else
            {
                if (holes != null)
                {
                    fStart = solid.AddFace(perimeter, holes, reverse: true);
                }
                else
                {
                    fStart = solid.AddFace(perimeter, reverse: true);
                }
            }

            var fEndOuter = solid.SweepLoop(fStart.Outer, direction, distance);

            if (holes != null)
            {
                var fEndInner = new Loop[holes.Count];
                for (var i = 0; i < holes.Count; i++)
                {
                    fEndInner[i] = solid.SweepLoop(fStart.Inner[i], direction, distance);
                }
                solid.AddFace(fEndOuter, fEndInner);
            }
            else
            {
                solid.AddFace(fEndOuter);
            }

            return solid;
        }

        /// <summary>
        /// Add a Vertex to the Solid.
        /// </summary>
        /// <param name="position"></param>
        /// <returns>The newly added vertex.</returns>
        public Vertex AddVertex(Vector3 position)
        {
            var v = new Vertex(_vertexId, position);
            this.Vertices.Add(_vertexId, v);
            _vertexId++;
            return v;
        }

        /// <summary>
        /// Add a Face to the Solid.
        /// </summary>
        /// <param name="outer">A polygon representing the perimeter of the face.</param>
        /// <param name="inner">An array of polygons representing the holes in the face.</param>
        /// <param name="mergeVerticesAndEdges">Should existing vertices / edges in the solid be used for the added face?</param>
        /// <param name="transform">An optional transform which is applied to the polygon.</param>
        /// <param name="reverse">Should the loop be reversed?</param>
        /// <returns>The newly added face.</returns>
        public Face AddFace(Polygon outer,
                            IList<Polygon> inner = null,
                            bool mergeVerticesAndEdges = false,
                            Transform transform = null,
                            bool reverse = false)
        {
            var outerLoop = LoopFromPolygon(outer, mergeVerticesAndEdges, transform, reverse);
            Loop[] innerLoops = null;

            if (inner != null)
            {
                innerLoops = new Loop[inner.Count];
                for (var i = 0; i < inner.Count; i++)
                {
                    innerLoops[i] = LoopFromPolygon(inner[i], mergeVerticesAndEdges, transform, reverse);
                }
            }

            var face = this.AddFace(outerLoop, innerLoops);
            return face;
        }

        /// <summary>
        /// Add an edge to the solid.
        /// </summary>
        /// <param name="from">The start vertex.</param>
        /// <param name="to">The end vertex.</param>
        /// <returns>The newly added edge.</returns>
        public Edge AddEdge(Vertex from, Vertex to)
        {
            var e = new Edge(_edgeId, from, to);
            this.Edges.Add(_edgeId, e);
            _edgeId++;
            return e;
        }

        private Edge AddEdge(Vertex from, Vertex to, bool useExistingEdges, out string edgeType)
        {
            if (useExistingEdges)
            {
                var matchingLeftEdge = Edges.Values.FirstOrDefault(e => e.Left.Vertex == from && e.Right.Vertex == to);
                if (matchingLeftEdge != null)
                {
                    edgeType = "left";
                    return matchingLeftEdge;
                }
                var matchingRightEdge = Edges.Values.FirstOrDefault(e => e.Right.Vertex == from && e.Left.Vertex == to);
                if (matchingRightEdge != null)
                {
                    edgeType = "right";
                    return matchingRightEdge;
                }
            }
            edgeType = "left";
            return AddEdge(from, to);
        }

        /// <summary>
        /// Add a face to the solid.
        /// Provided edges are expected to be wound CCW for outer,
        /// and CW for inner. The face will be linked to the edges.
        /// </summary>
        /// <param name="outer">The outer Loop of the Face.</param>
        /// <param name="inner">The inner Loops of the Face.</param>
        /// <returns>The newly added Face.</returns>
        public Face AddFace(Loop outer, Loop[] inner = null)
        {
            var f = new Face(_faceId, outer, inner);
            this.Faces.Add(_faceId, f);
            _faceId++;
            return f;
        }

        /// <summary>
        /// Creates a series of edges from a polygon.
        /// </summary>
        /// <param name="p"></param>
        public Edge[] AddEdges(Polygon p)
        {
            var loop = new Edge[p.Vertices.Count];
            var vertices = new Vertex[p.Vertices.Count];
            for (var i = 0; i < p.Vertices.Count; i++)
            {
                vertices[i] = AddVertex(p.Vertices[i]);
            }
            for (var i = 0; i < p.Vertices.Count; i++)
            {
                loop[i] = AddEdge(vertices[i], i == p.Vertices.Count - 1 ? vertices[0] : vertices[i + 1]);
            }
            return loop;
        }

        /// <summary>
        /// Slice a solid with the provided plane.
        /// </summary>
        /// <param name="p">The plane to be used to slice this solid.</param>
        internal void Slice(Plane p)
        {
            var keys = new List<long>(this.Edges.Keys);
            foreach (var key in keys)
            {
                var e = this.Edges[key];
                SplitEdge(p, e);
            }
        }

        /// <summary>
        /// Get the string representation of the solid.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Faces: {this.Faces.Count}, Edges: {this.Edges.Count}, Vertices: {this.Vertices.Count}");
            foreach (var e in Edges)
            {
                sb.AppendLine($"Edge: {e.ToString()}");
            }
            foreach (var f in Faces.Values)
            {
                sb.AppendLine($"Face: {f.ToString()}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get the Mesh of this Solid.
        /// </summary>
        public Mesh ToMesh()
        {
            var mesh = new Mesh();
            this.Tessellate(ref mesh);
            return mesh;
        }

        /// <summary>
        /// Triangulate this solid.
        /// </summary>
        /// <param name="mesh">The mesh to which the solid's tessellated data will be added.</param>
        /// <param name="transform">An optional transform used to transform the generated vertex coordinates.</param>
        /// <param name="color">An optional color to apply to the vertex.</param>
        public void Tessellate(ref Mesh mesh, Transform transform = null, Color color = default)
        {
            var tessProvider = new SolidTesselationTargetProvider(this);
            foreach (var target in tessProvider.GetTessellationTargets())
            {
                var tess = target.GetTess();
                var faceMesh = tess.ToMesh(transform, color);
                mesh.AddMesh(faceMesh);
            }
        }

        /// <summary>
        /// Intersect this solid with the provided plane.
        /// </summary>
        /// <param name="p">The plane of intersection.</param>
        /// <param name="result">A collection of polygons resulting
        /// from the intersection or null if there was no intersection.</param>
        /// <returns>True if an intersection occurred, otherwise false.</returns>
        public bool Intersects(Plane p, out List<Polygon> result)
        {
            var graphVertices = new List<Vector3>();
            var graphEdges = new List<List<(int from, int to, int? tag)>>();

            foreach (var f in this.Faces)
            {
                var facePlane = f.Value.Plane();

                // If the face is coplanar, skip it. The edges of
                // the face also belong to adjacent faces and will
                // be included in their results.
                if (facePlane.IsCoplanar(p))
                {
                    continue;
                }

                var edgeResults = new List<Vector3>();

                edgeResults.AddRange(IntersectLoop(f.Value.Outer, p));

                if (f.Value.Inner != null)
                {
                    foreach (var inner in f.Value.Inner)
                    {
                        edgeResults.AddRange(IntersectLoop(inner, p));
                    }
                }

                var d = facePlane.Normal.Cross(p.Normal).Unitized();
                edgeResults.Sort(new DirectionComparer(d));

                // Draw segments through the results and add to the 
                // half edge graph.
                for (var j = 0; j < edgeResults.Count - 1; j += 2)
                {
                    // Don't create zero-length edges.
                    if (edgeResults[j].IsAlmostEqualTo(edgeResults[j + 1]))
                    {
                        continue;
                    }

                    var a = FindOrCreateGraphVertex(edgeResults[j], graphVertices, graphEdges);
                    var b = FindOrCreateGraphVertex(edgeResults[j + 1], graphVertices, graphEdges);
                    var e1 = (a, b, 0);
                    var e2 = (b, a, 0);
                    if (graphEdges[a].Contains(e1) || graphEdges[b].Contains(e2))
                    {
                        continue;
                    }
                    else
                    {
                        graphEdges[a].Add(e1);
                    }
                }
            }

            var heg = new HalfEdgeGraph2d()
            {
                Vertices = graphVertices,
                EdgesPerVertex = graphEdges
            };

            try
            {
                var polys = heg.Polygonize();
                if (polys == null || polys.Count == 0)
                {
                    result = null;
                    return false;
                }
                result = polys;
                return true;
            }
            catch (Exception ex)
            {
                // TODO: We could test for known failure modes, but the
                // iteration over the edge graph before attempting to
                // graph to identify these modes, is as expensive as 
                // the graph attempt.
                // Known cases there the half edge graph will throw an
                // exception:
                // - Co-linear edges.
                // - Disconnected graphs.
                // - Graphs with one vertex.
                Console.WriteLine(ex.Message);

                result = null;
                return false;
            }
        }

        internal static int FindOrCreateGraphVertex(Vector3 v, List<Vector3> vertices, List<List<(int from, int to, int? tag)>> edges)
        {
            var a = vertices.IndexOf(v);
            if (a == -1)
            {
                vertices.Add(v);
                a = vertices.Count - 1;
                edges.Add(new List<(int from, int to, int? tag)>());
            }
            return a;
        }

        private List<Vector3> IntersectLoop(Loop loop, Plane p)
        {
            var edgeResults = new List<Vector3>();
            var v = loop.Edges.Select(e => e.Vertex).ToList();
            for (var i = 0; i < v.Count(); i++)
            {
                var a = v[i];
                var b = i == v.Count - 1 ? v[0] : v[i + 1];
                var start = a.Point;
                var end = b.Point;

                // If this edge lies on the plane, add it and continue;
                if (start.DistanceTo(p).ApproximatelyEquals(0) && end.DistanceTo(p).ApproximatelyEquals(0))
                {
                    if (!edgeResults.Contains(start))
                    {
                        edgeResults.Add(start);
                    }
                    if (!edgeResults.Contains(end))
                    {
                        edgeResults.Add(end);
                    }
                    continue;
                }

                if (Line.Intersects(p, start, end, out Vector3 xsect))
                {
                    if (!edgeResults.Contains(xsect))
                    {
                        edgeResults.Add(xsect);
                    }
                }
            }

            return edgeResults;
        }

        /// <summary>
        /// Create a face from edges.
        /// The first edge array is treated as the outer edge.
        /// Additional edge arrays are treated as holes.
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="reverse"></param>
        protected void Cap(Edge[][] edges, bool reverse = true)
        {
            var loop = new Loop();
            for (var i = 0; i < edges[0].Length; i++)
            {
                if (reverse)
                {
                    loop.AddEdgeToStart(edges[0][i].Right);
                }
                else
                {
                    loop.AddEdgeToEnd(edges[0][i].Left);
                }
            }

            var inner = new Loop[edges.Length - 1];
            for (var i = 1; i < edges.Length; i++)
            {
                inner[i - 1] = new Loop();
                for (var j = 0; j < edges[i].Length; j++)
                {
                    if (reverse)
                    {
                        inner[i - 1].AddEdgeToStart(edges[i][j].Right);
                    }
                    else
                    {
                        inner[i - 1].AddEdgeToEnd(edges[i][j].Left);
                    }
                }
            }
            AddFace(loop, inner);
        }

        protected Loop LoopFromPolygon(Polygon p,
                                       bool mergeVerticesAndEdges = false,
                                       Transform transform = null,
                                       bool reverse = false)
        {
            var loop = new Loop();
            var verts = new Vertex[p.Vertices.Count];

            if (reverse)
            {
                for (var i = p.Vertices.Count - 1; i >= 0; i--)
                {
                    var pt = p.Vertices[i];
                    FindOrCreateVertex(pt, p.Vertices.Count - 1 - i, transform, mergeVerticesAndEdges, verts);
                }
            }
            else
            {
                for (var i = 0; i < p.Vertices.Count; i++)
                {
                    var pt = p.Vertices[i];
                    FindOrCreateVertex(pt, i, transform, mergeVerticesAndEdges, verts);
                }
            }

            for (var i = 0; i < p.Vertices.Count; i++)
            {
                var v1 = verts[i];
                var v2 = i == verts.Length - 1 ? verts[0] : verts[i + 1];
                var edge = AddEdge(v1, v2, mergeVerticesAndEdges, out var edgeType);
                loop.AddEdgeToEnd(edgeType == "left" ? edge.Left : edge.Right);
            }

            return loop;
        }

        // TODO: This method should not be required if we have adequate lookup
        // operations based on vertex locations and incident edges. This is 
        // currently used in the case where we want to add a face from a polygon
        // but in most cases where we want to do that we already know something
        // about the shape of the existing solid and should be able to lookup 
        // existing vertices without doing an O(n) search. Implement a better
        // search strategy!
        private void FindOrCreateVertex(Vector3 pt, int i, Transform transform, bool mergeVerticesAndEdges, Vertex[] verts)
        {
            if (transform != null)
            {
                pt = transform.OfPoint(pt);
            }

            if (mergeVerticesAndEdges)
            {
                var existingVertex = Vertices.Select(v => v.Value).FirstOrDefault(v => v.Point.IsAlmostEqualTo(pt));
                verts[i] = existingVertex ?? AddVertex(pt);
            }
            else
            {
                verts[i] = AddVertex(pt);
            }
        }

        internal Face AddFace(long id, Loop outer, Loop[] inner = null)
        {
            var f = new Face(id, outer, inner);
            this.Faces.Add(id, f);
            return f;
        }

        internal Vertex AddVertex(long id, Vector3 position)
        {
            var v = new Vertex(id, position);
            this.Vertices.Add(id, v);
            return v;
        }

        internal Edge AddEdge(long id)
        {
            var e = new Edge(id);
            this.Edges.Add(id, e);
            return e;
        }

        internal Edge[] SweepEdges(Transform[] transforms, Edge[] openEdge)
        {
            for (var i = 0; i < transforms.Length - 1; i++)
            {
                var v = (transforms[i + 1].Origin - transforms[i].Origin).Unitized();
                openEdge = SweepEdgesBetweenPlanes(openEdge, v, transforms[i + 1].XY());
            }
            return openEdge;
        }

        internal Loop SweepLoop(Loop loop, Vector3 direction, double distance)
        {
            var sweepEdges = new Edge[loop.Edges.Count];
            var i = 0;
            foreach (var e in loop.Edges)
            {
                var v1 = e.Vertex;
                var v2 = AddVertex(v1.Point + direction * distance);
                sweepEdges[i] = AddEdge(v1, v2);
                i++;
            }

            var openLoop = new Loop();
            var j = 0;
            foreach (var e in loop.Edges)
            {
                var a = e.Edge;
                var b = sweepEdges[j];
                var d = sweepEdges[j == loop.Edges.Count - 1 ? 0 : j + 1];
                var c = AddEdge(b.Right.Vertex, d.Right.Vertex);
                var faceLoop = new Loop(new[] { a.Right, b.Left, c.Left, d.Right });
                AddFace(faceLoop);
                openLoop.AddEdgeToStart(c.Right);
                j++;
            }
            return openLoop;
        }

        private Edge[] ProjectEdgeAlong(Edge[] loop, Vector3 v, Plane p)
        {
            var edges = new Edge[loop.Length];
            for (var i = 0; i < edges.Length; i++)
            {
                var e = loop[i];
                var a = AddVertex(e.Left.Vertex.Point.ProjectAlong(v, p));
                var b = AddVertex(e.Right.Vertex.Point.ProjectAlong(v, p));
                edges[i] = AddEdge(a, b);
            }
            return edges;
        }

        private Edge[] SweepEdgesBetweenPlanes(Edge[] loop1, Vector3 v, Plane end)
        {
            // Project the starting loops to the end plane along v.
            var loop2 = ProjectEdgeAlong(loop1, v, end);

            var sweepEdges = new Edge[loop1.Length];
            for (var i = 0; i < loop1.Length; i++)
            {
                var v1 = loop1[i].Left.Vertex;
                var v2 = loop2[i].Left.Vertex;
                sweepEdges[i] = AddEdge(v1, v2);
            }

            var openEdge = new Edge[sweepEdges.Length];
            for (var i = 0; i < sweepEdges.Length; i++)
            {
                var a = loop1[i];
                var b = sweepEdges[i];
                var c = loop2[i];
                var d = sweepEdges[i == loop1.Length - 1 ? 0 : i + 1];

                var loop = new Loop(new[] { a.Right, b.Left, c.Left, d.Right });
                AddFace(loop);
                openEdge[i] = c;
            }
            return openEdge;
        }

        private Loop SweepPolygonBetweenPlanes(Polygon p, Transform start, Transform end, double rotation = 0.0)
        {
            // Transform the polygon to the mid plane between two transforms
            // then project onto the end transforms. We do this so that we
            // do not introduce shear into the transform.
            var v = (start.Origin - end.Origin).Unitized();
            var midTrans = new Transform(end.Origin.Average(start.Origin), start.YAxis.Cross(v), v);
            var mid = p.TransformedPolygon(midTrans);
            var startP = mid.ProjectAlong(v, start.XY());
            var endP = mid.ProjectAlong(v, end.XY());

            var loop1 = AddEdges(startP);
            var loop2 = AddEdges(endP);

            var sweepEdges = new Edge[loop1.Length];
            for (var i = 0; i < loop1.Length; i++)
            {
                var v1 = loop1[i].Left.Vertex;
                var v2 = loop2[i].Left.Vertex;
                sweepEdges[i] = AddEdge(v1, v2);
            }

            var openEdge = new Loop();
            for (var i = 0; i < sweepEdges.Length; i++)
            {
                var a = loop1[i];
                var b = sweepEdges[i];
                var c = loop2[i];
                var d = sweepEdges[i == loop1.Length - 1 ? 0 : i + 1];

                var loop = new Loop(new[] { a.Right, b.Left, c.Left, d.Right });
                AddFace(loop);
                openEdge.AddEdgeToEnd(c.Right);
            }
            return openEdge;
        }

        private void SplitEdge(Plane p, Edge e)
        {
            var start = e.Left.Vertex;
            var end = e.Right.Vertex;
            if (!new Line(start.Point, end.Point).Intersects(p, out Vector3 result))
            {
                return;
            }

            // Add vertex at intersection.
            // Create new edge from vertex to end.
            var mid = AddVertex(result);
            var e1 = AddEdge(mid, end);

            // Adjust end of existing edge to
            // new vertex
            e.Right.Vertex = mid;
            if (e.Left.Loop != null)
            {
                e.Left.Loop.InsertEdgeAfter(e.Left, e1.Left);
            }
            if (e.Right.Loop != null)
            {
                e.Right.Loop.InsertEdgeBefore(e.Right, e1.Right);
            }
        }
        internal Csg.Solid ToCsg()
        {
            var polygons = new List<Csg.Polygon>(Faces.Values.Count);

            ushort faceId = 0;
            foreach (var f in Faces.Values)
            {
                var tess = new Tess
                {
                    NoEmptyPolygons = true,
                };

                var a = f.Outer.Edges[0].Vertex.Point;
                var b = f.Outer.Edges[1].Vertex.Point;
                var c = f.Outer.Edges[2].Vertex.Point;
                (Vector3 U, Vector3 V) = Tessellation.Tessellation.ComputeBasisAndNormalForTriangle(a, b, c, out var normal);

                // Create the csg vertices first then use them to create
                // the tessellation vertices. This way we can pass the
                // texture coordinates AND the vertex tag through tessellation
                // to be used for lookup on the other side.

                var outerVerts = f.Outer.ToCsgVertexArray(U, V);
                var outerContourArray = outerVerts.ToContourVertexArray(faceId);
                tess.AddContour(outerContourArray);

                // Map csg vertices by tag.
                var csgVertices = new Dictionary<int, Csg.Vertex>();

                foreach (var v in outerVerts)
                {
                    csgVertices.Add(v.Tag, v);
                }

                if (f.Inner != null)
                {
                    foreach (var loop in f.Inner)
                    {
                        var innerVerts = loop.ToCsgVertexArray(U, V);
                        foreach (var v in innerVerts)
                        {
                            csgVertices.Add(v.Tag, v);
                        }

                        var innerContourArray = innerVerts.ToContourVertexArray(faceId);
                        tess.AddContour(innerContourArray);
                    }
                }

                // Always tessellate to 3 sided polygons because CSGs
                // require convex polys.
                tess.Tessellate(WindingRule.Positive, ElementType.Polygons, 3);

                for (var i = 0; i < tess.Elements.Count(); i += 3)
                {
                    var v1 = tess.Vertices[tess.Elements[i]];
                    var v2 = tess.Vertices[tess.Elements[i + 1]];
                    var v3 = tess.Vertices[tess.Elements[i + 2]];

                    Csg.Vertex av = null;
                    Csg.Vertex bv = null;
                    Csg.Vertex cv = null;

                    if (v1.Data == null)
                    {
                        // It's a new vertex created during tessellation.
                        var avv = v1.Position.ToCsgVector3();
                        av = new Csg.Vertex(new Csg.Vector3D(v1.Position.X, v1.Position.Y, v1.Position.Z), new Csg.Vector2D(U.Dot(avv.X, avv.Y, avv.Z), V.Dot(avv.X, avv.Y, avv.Z)));
                    }
                    else
                    {
                        var vData1 = ((UV uv, int tag, int faceId))v1.Data;
                        av = csgVertices[vData1.tag];
                    }

                    if (v2.Data == null)
                    {
                        var bvv = v2.Position.ToCsgVector3();
                        bv = new Csg.Vertex(bvv, new Csg.Vector2D(U.Dot(bvv.X, bvv.Y, bvv.Z), V.Dot(bvv.X, bvv.Y, bvv.Z)));
                    }
                    else
                    {
                        var vData2 = ((UV uv, int tag, int faceId))v2.Data;
                        bv = csgVertices[vData2.tag];
                    }

                    if (v3.Data == null)
                    {
                        var cvv = v3.Position.ToCsgVector3();
                        cv = new Csg.Vertex(cvv, new Csg.Vector2D(U.Dot(cvv.X, cvv.Y, cvv.Z), V.Dot(cvv.X, cvv.Y, cvv.Z)));
                    }
                    else
                    {
                        var vData3 = ((UV uv, int tag, int faceId))v3.Data;
                        cv = csgVertices[vData3.tag];
                    }

                    // Don't allow us to create a csg that has zero
                    // area triangles.
                    var ab = bv.Pos.ToVector3() - av.Pos.ToVector3();
                    var ac = cv.Pos.ToVector3() - av.Pos.ToVector3();
                    var area = ab.Cross(ac).Length() / 2;
                    if (area == 0.0)
                    {
                        continue;
                    }

                    var p = new Csg.Polygon(new List<Csg.Vertex>() { av, bv, cv });
                    polygons.Add(p);
                }
                faceId++;
            }
            return Csg.Solid.FromPolygons(polygons);
        }
    }
}