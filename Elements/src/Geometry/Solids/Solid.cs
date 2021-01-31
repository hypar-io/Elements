#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Elements.Geometry.Interfaces;
using LibTessDotNet.Double;

[assembly: InternalsVisibleTo("Hypar.Elements.Tests")]

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// A boundary representation of a solid.
    /// </summary>
    public class Solid : ITessellate
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
            var solid = new Solid();
            var loop1 = new Loop();
            var loop2 = new Loop();
            for (var i = 0; i < perimeter.Count; i++)
            {
                var a = solid.AddVertex(perimeter[i]);
                var b = solid.AddVertex(perimeter[i == perimeter.Count - 1 ? 0 : i + 1]);
                var e = solid.AddEdge(a, b);
                loop1.AddEdgeToEnd(e.Left);
                loop2.AddEdgeToStart(e.Right);
            }
            solid.AddFace(loop1);
            solid.AddFace(loop2);
            return solid;
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
        /// <returns>A solid.</returns>
        public static Solid SweepFaceAlongCurve(Polygon perimeter,
                                                IList<Polygon> holes,
                                                ICurve curve,
                                                double startSetback = 0,
                                                double endSetback = 0)
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

            // Calculate the setback parameter as a percentage
            // of the curve length. This will not work for curves
            // without non-uniform parameterization.
            var ssb = startSetback / l;
            var esb = endSetback / l;

            var transforms = curve.Frames(ssb, esb);

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
                var startCap = solid.AddFace((Polygon)perimeter.Transformed(transforms[0]));
                for (var i = 0; i < transforms.Length - 1; i++)
                {
                    var next = transforms[i + 1];
                    solid.SweepPolygonBetweenPlanes(perimeter, transforms[i], next);
                }
                var endCap = solid.AddFace(((Polygon)perimeter.Transformed(transforms[transforms.Length - 1])).Reversed());
            }
            else
            {
                // Add start cap.
                Face cap = null;
                Edge[][] openEdges;

                if (holes != null)
                {
                    cap = solid.AddFace((Polygon)perimeter.Transformed(transforms[0]), transforms[0].OfPolygons(holes));
                    openEdges = new Edge[1 + holes.Count][];
                }
                else
                {
                    cap = solid.AddFace((Polygon)perimeter.Transformed(transforms[0]));
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
                    fStart = solid.AddFace((Polygon)perimeter.Reversed().Transformed(t), t.OfPolygons(holes.Reversed()));
                }
                else
                {
                    fStart = solid.AddFace((Polygon)perimeter.Reversed().Transformed(t));
                }
            }
            else
            {
                if (holes != null)
                {
                    fStart = solid.AddFace(perimeter.Reversed(), holes.Reversed());
                }
                else
                {
                    fStart = solid.AddFace(perimeter.Reversed());
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
        /// <returns>The newly added face.</returns>
        public Face AddFace(Polygon outer, IList<Polygon> inner = null)
        {
            var outerLoop = LoopFromPolygon(outer);
            Loop[] innerLoops = null;

            if (inner != null)
            {
                innerLoops = new Loop[inner.Count];
                for (var i = 0; i < inner.Count; i++)
                {
                    innerLoops[i] = LoopFromPolygon(inner[i]);
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
                if (!TrySplitEdge(p, e, out var v))
                {
                    continue;
                }
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
        /// Triangulate this solid.
        /// </summary>
        /// <param name="mesh">The mesh to which the solid's tessellated data will be added.</param>
        /// <param name="transform">An optional transform used to transform the generated vertex coordinates.</param>
        /// <param name="color">An optional color to apply to the vertex.</param>
        public void Tessellate(ref Mesh mesh, Transform transform = null, Color color = default(Color))
        {
            foreach (var f in this.Faces.Values)
            {
                var tess = new Tess();
                tess.NoEmptyPolygons = true;

                tess.AddContour(f.Outer.ToContourVertexArray(f));

                if (f.Inner != null)
                {
                    foreach (var loop in f.Inner)
                    {
                        tess.AddContour(loop.ToContourVertexArray(f));
                    }
                }

                tess.Tessellate(WindingRule.Positive, LibTessDotNet.Double.ElementType.Polygons, 3);

                var faceMesh = new Mesh();
                for (var i = 0; i < tess.ElementCount; i++)
                {
                    var a = tess.Vertices[tess.Elements[i * 3]].Position.ToVector3();
                    var b = tess.Vertices[tess.Elements[i * 3 + 1]].Position.ToVector3();
                    var c = tess.Vertices[tess.Elements[i * 3 + 2]].Position.ToVector3();

                    if (transform != null)
                    {
                        a = transform.OfPoint(a);
                        b = transform.OfPoint(b);
                        c = transform.OfPoint(c);
                    }

                    var v1 = faceMesh.AddVertex(a, new UV(), color: color);
                    var v2 = faceMesh.AddVertex(b, new UV(), color: color);
                    var v3 = faceMesh.AddVertex(c, new UV(), color: color);
                    faceMesh.AddTriangle(v1, v2, v3);
                }
                mesh.AddMesh(faceMesh);
            }
        }

        /// <summary>
        /// Triangulate this solid and pack the triangulated data into buffers
        /// appropriate for use with gltf.
        /// </summary>
        public void Tessellate(out byte[] vertexBuffer,
            out byte[] indexBuffer, out byte[] normalBuffer, out byte[] colorBuffer, out byte[] uvBuffer,
            out double[] vmax, out double[] vmin, out double[] nmin, out double[] nmax,
            out float[] cmin, out float[] cmax, out ushort imin, out ushort imax, out double[] uvmin, out double[] uvmax)
        {

            var tessellations = new Tess[this.Faces.Count];

            var fi = 0;
            foreach (var f in this.Faces.Values)
            {
                var tess = new Tess();
                tess.NoEmptyPolygons = true;
                tess.AddContour(f.Outer.ToContourVertexArray(f));

                if (f.Inner != null)
                {
                    foreach (var loop in f.Inner)
                    {
                        tess.AddContour(loop.ToContourVertexArray(f));
                    }
                }

                tess.Tessellate(WindingRule.Positive, LibTessDotNet.Double.ElementType.Polygons, 3);

                tessellations[fi] = tess;
                fi++;
            }

            var floatSize = sizeof(float);
            var ushortSize = sizeof(ushort);

            var vertexCount = tessellations.Sum(t => t.VertexCount);
            var indexCount = tessellations.Sum(t => t.Elements.Length);

            vertexBuffer = new byte[vertexCount * floatSize * 3];
            normalBuffer = new byte[vertexCount * floatSize * 3];
            indexBuffer = new byte[indexCount * ushortSize];
            uvBuffer = new byte[vertexCount * floatSize * 2];

            // Vertex colors are not used in this context currently.
            colorBuffer = new byte[0];
            cmin = new float[0];
            cmax = new float[0];

            vmax = new double[3] { double.MinValue, double.MinValue, double.MinValue };
            vmin = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            nmin = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            nmax = new double[3] { double.MinValue, double.MinValue, double.MinValue };

            // TODO: Set this properly when solids get UV coordinates.
            uvmin = new double[2] { 0, 0 };
            uvmax = new double[2] { 0, 0 };

            imax = ushort.MinValue;
            imin = ushort.MaxValue;

            var vi = 0;
            var ii = 0;
            var uvi = 0;

            var iCursor = 0;

            for (var i = 0; i < tessellations.Length; i++)
            {
                var tess = tessellations[i];

                var a = tess.Vertices[tess.Elements[0]].Position.ToVector3();
                var b = tess.Vertices[tess.Elements[1]].Position.ToVector3();
                var c = tess.Vertices[tess.Elements[2]].Position.ToVector3();
                var n = (b - a).Cross(c - a).Unitized();

                for (var j = 0; j < tess.Vertices.Length; j++)
                {
                    var v = tess.Vertices[j];

                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Position.X), 0, vertexBuffer, vi, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Position.Y), 0, vertexBuffer, vi + floatSize, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Position.Z), 0, vertexBuffer, vi + 2 * floatSize, floatSize);

                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)n.X), 0, normalBuffer, vi, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)n.Y), 0, normalBuffer, vi + floatSize, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)n.Z), 0, normalBuffer, vi + 2 * floatSize, floatSize);

                    // TODO: Update Solids to use something other than UV = {0,0}.
                    System.Buffer.BlockCopy(BitConverter.GetBytes(0f), 0, uvBuffer, uvi, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(0f), 0, uvBuffer, uvi + floatSize, floatSize);

                    uvi += 2 * floatSize;
                    vi += 3 * floatSize;

                    vmax[0] = Math.Max(vmax[0], v.Position.X);
                    vmax[1] = Math.Max(vmax[1], v.Position.Y);
                    vmax[2] = Math.Max(vmax[2], v.Position.Z);
                    vmin[0] = Math.Min(vmin[0], v.Position.X);
                    vmin[1] = Math.Min(vmin[1], v.Position.Y);
                    vmin[2] = Math.Min(vmin[2], v.Position.Z);

                    nmax[0] = Math.Max(nmax[0], n.X);
                    nmax[1] = Math.Max(nmax[1], n.Y);
                    nmax[2] = Math.Max(nmax[2], n.Z);
                    nmin[0] = Math.Min(nmin[0], n.X);
                    nmin[1] = Math.Min(nmin[1], n.Y);
                    nmin[2] = Math.Min(nmin[2], n.Z);

                    // uvmax[0] = Math.Max(uvmax[0], 0);
                    // uvmax[1] = Math.Max(uvmax[1], 0);
                    // uvmin[0] = Math.Min(uvmin[0], 0);
                    // uvmin[1] = Math.Min(uvmin[1], 0);
                }

                for (var k = 0; k < tess.Elements.Length; k++)
                {
                    var t = tess.Elements[k];
                    var index = (ushort)(t + iCursor);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(index), 0, indexBuffer, ii, ushortSize);
                    imax = Math.Max(imax, index);
                    imin = Math.Min(imin, index);
                    ii += ushortSize;
                }

                iCursor = imax + 1;
            }
        }

        public bool TryIntersect(Plane p, out List<Polygon> intersects)
        {
            intersects = null;
            var allSegments = new List<Line>();
            foreach (var f in this.Faces.Values)
            {
                if (f.TryIntersect(p, out var segments))
                {
                    allSegments.AddRange(segments);
                }
            }

            if (allSegments.Count == 0)
            {
                return false;
            }

            intersects = allSegments.ToPolylines().Select(pl => pl.Closed()).ToList();

            return true;
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

        protected Loop LoopFromPolygon(Polygon p)
        {
            var loop = new Loop();
            var verts = new Vertex[p.Vertices.Count];
            for (var i = 0; i < p.Vertices.Count; i++)
            {
                verts[i] = AddVertex(p.Vertices[i]);
            }
            for (var i = 0; i < p.Vertices.Count; i++)
            {
                var v1 = verts[i];
                var v2 = i == verts.Length - 1 ? verts[0] : verts[i + 1];
                var edge = AddEdge(v1, v2);
                loop.AddEdgeToEnd(edge.Left);
            }
            return loop;
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
            var mid = (Polygon)p.Transformed(midTrans);
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

        private bool TrySplitEdge(Plane p, Edge e, out Vertex vertex)
        {
            vertex = null;

            var start = e.Left.Vertex;
            var end = e.Right.Vertex;
            if (!new Line(start.Point, end.Point).Intersects(p, out Vector3 result))
            {
                return false;
            }

            // Add vertex at intersection.
            // Create new edge from vertex to end.
            vertex = AddVertex(result);
            var e1 = AddEdge(vertex, end);

            // Adjust end of existing edge to
            // new vertex
            e.Right.Vertex = vertex;
            if (e.Left.Loop != null)
            {
                e.Left.Loop.InsertEdgeAfter(e.Left, e1.Left);
            }
            if (e.Right.Loop != null)
            {
                e.Right.Loop.InsertEdgeBefore(e.Right, e1.Right);
            }

            return true;
        }
    }
}