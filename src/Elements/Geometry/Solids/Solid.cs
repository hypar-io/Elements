#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Elements.Geometry.Interfaces;
using glTFLoader.Schema;
using LibTessDotNet.Double;
using Newtonsoft.Json;

[assembly:InternalsVisibleTo("Hypar.Elements.Tests")]

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// A boundary representation of a solid.
    /// </summary>
    public class Solid
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
        /// The material of the solid.
        /// </summary>
        public Material Material{get;}

        /// <summary>
        /// Construct a solid.
        /// </summary>
        public Solid(Material material = null)
        {
            this.Faces = new Dictionary<long, Face>();
            this.Edges = new Dictionary<long, Edge>();
            this.Vertices = new Dictionary<long, Vertex>();
            this.Material = material != null ? material : BuiltInMaterials.Default;
        }

        /// <summary>
        /// Construct a lamina solid.
        /// </summary>
        /// <param name="perimeter">The perimeter of the lamina's faces.</param>
        /// <param name="material">The solid's material.</param>
        public static Solid CreateLamina(Vector3[] perimeter, Material material)
        {   
            var solid = new Solid(material);
            var loop1 = new Loop();
            var loop2 = new Loop();
            for (var i = 0; i < perimeter.Length; i++)
            {
                var a = solid.AddVertex(perimeter[i]);
                var b = solid.AddVertex(perimeter[i == perimeter.Length - 1 ? 0 : i + 1]);
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
        /// <param name="outerLoop">The perimeter of the face to sweep.</param>
        /// <param name="innerLoops">The holes of the face to sweep.</param>
        /// <param name="distance">The distance to sweep.</param>
        /// <param name="material">The solid's material.</param>
        /// <returns>A solid.</returns>
        public static Solid SweepFace(Polygon outerLoop, Polygon[] innerLoops, double distance, Material material = null) 
        {
            return Solid.SweepFace(outerLoop, innerLoops, Vector3.ZAxis, distance, material);
        }

        /// <summary>
        /// Construct a solid by sweeping a face along a curve.
        /// </summary>
        /// <param name="outer">The perimeter of the face to sweep.</param>
        /// <param name="inner">The holes of the face to sweep.</param>
        /// <param name="curve">The curve along which to sweep.</param>
        /// <param name="material">The solid's material.</param>
        /// <param name="startSetback">The setback of the sweep from the start of the curve.</param>
        /// <param name="endSetback">The setback of the sweep from the end of the curve.</param>
        /// <returns>A solid.</returns>
        public static Solid SweepFaceAlongCurve(Polygon outer, Polygon[] inner, ICurve curve, Material material = null,  double startSetback = 0, double endSetback = 0)
        {
            var solid = new Solid(material);

            var l = curve.Length();
            var ssb = startSetback / l;
            var esb = endSetback / l;

            var transforms = curve.Frames(ssb, esb);

            if (curve is Polygon)
            {
                for (var i = 0; i < transforms.Length; i++)
                {
                    var next = i == transforms.Length - 1 ? transforms[0] : transforms[i + 1];
                    solid.SweepPolygonBetweenPlanes(outer, transforms[i].XY, next.XY);
                }
            }
            else
            {
                // Add start cap.
                Face cap = null;
                Edge[][] openEdges;

                if(inner != null)
                {
                    cap = solid.AddFace(transforms[0].OfPolygon(outer), transforms[0].OfPolygons(inner));
                    openEdges = new Edge[1 + inner.Length][];
                }
                else
                {
                    cap = solid.AddFace(transforms[0].OfPolygon(outer));
                    openEdges = new Edge[1][];
                }

                // last outer edge
                var openEdge = cap.Outer.GetLinkedEdges();
                openEdge = solid.SweepEdges(transforms, openEdge);
                openEdges[0] = openEdge;

                if(inner != null)
                {
                    for(var i=0; i<cap.Inner.Length; i++)
                    {
                        openEdge = cap.Inner[i].GetLinkedEdges();

                        // last inner edge for one hole
                        openEdge = solid.SweepEdges(transforms, openEdge);
                        openEdges[i+1] = openEdge;
                    }
                }

                solid.Cap(openEdges, true);
            }

            return solid;
        }

        /// <summary>
        /// Construct a solid by sweeping a face in a direction.
        /// </summary>
        /// <param name="outerLoop">The perimeter of the face to sweep.</param>
        /// <param name="innerLoops">The holes of the face to sweep.</param>
        /// <param name="direction">The direction in which to sweep.</param>
        /// <param name="distance">The distance to sweep.</param>
        /// <param name="material">The solid's material.</param>
        /// <returns>A solid.</returns>
        public static Solid SweepFace(Polygon outerLoop, Polygon[] innerLoops, Vector3 direction, double distance, Material material = null)
        {
            var solid = new Solid(material);
            Face fStart;
            if(innerLoops != null)
            {
                fStart = solid.AddFace(outerLoop.Reversed(), innerLoops.Reversed());
            }
            else
            {
                fStart = solid.AddFace(outerLoop.Reversed());
            }

            var fEndOuter = solid.SweepLoop(fStart.Outer, direction, distance);

            if(innerLoops != null)
            {
                var fEndInner = new Loop[innerLoops.Length];
                for(var i=0; i<innerLoops.Length; i++)
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
        public Face AddFace(Polygon outer, Polygon[] inner = null)
        {
            var outerLoop = LoopFromPolygon(outer);
            Loop[] innerLoops = null;

            if(inner != null)
            {
                innerLoops = new Loop[inner.Length];
                for(var i=0; i<inner.Length; i++)
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
        /// <param name="from"></param>
        /// <param name="to"></param>
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
            var loop = new Edge[p.Vertices.Length];
            var vertices = new Vertex[p.Vertices.Length];
            for (var i = 0; i < p.Vertices.Length; i++)
            {
                vertices[i] = AddVertex(p.Vertices[i]);
            }
            for(var i=0; i< p.Vertices.Length; i++)
            {
                loop[i] = AddEdge(vertices[i], i == p.Vertices.Length - 1 ? vertices[0] : vertices[i+1]);
            }
            return loop;
        }

        /// <summary>
        /// Get the string representation of the solid.
        /// </summary>
        public override string ToString() 
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Faces: {this.Faces.Count}, Edges: {this.Edges.Count}, Vertices: {this.Vertices.Count}");
            foreach(var e in Edges)
            {
                sb.AppendLine($"Edge: {e.ToString()}");
            }
            return sb.ToString();
        }
    
        /// <summary>
        /// Create a face from edges.
        /// The first edge array is treated as the outer edge.
        /// Additional edge arrays are treated as holes.
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="reverse"></param>
        protected void Cap(Edge[][] edges,  bool reverse = true)
        {
            var loop = new Loop();
            for(var i=0; i<edges[0].Length; i++)
            {
                if(reverse)
                {
                    loop.AddEdgeToStart(edges[0][i].Right);
                }
                else
                {
                    loop.AddEdgeToEnd(edges[0][i].Left);
                }
            }

            var inner = new Loop[edges.Length - 1];
            for(var i=1; i<edges.Length; i++)
            {
                inner[i-1] = new Loop();
                for(var j=0; j<edges[i].Length; j++)
                {
                    if(reverse)
                    {
                        inner[i-1].AddEdgeToStart(edges[i][j].Right);
                    }
                    else
                    {
                        inner[i-1].AddEdgeToEnd(edges[i][j].Left);
                    }
                }
            }
            AddFace(loop, inner);
        }
    
        protected Loop LoopFromPolygon(Polygon p)
        {
            var loop = new Loop();
            var verts = new Vertex[p.Vertices.Length];
            for (var i = 0; i < p.Vertices.Length; i++)
            {
                verts[i] = AddVertex(p.Vertices[i]);
            }
            for (var i=0; i<p.Vertices.Length; i++)
            {
                var v1 = verts[i];
                var v2 = i == verts.Length - 1 ? verts[0] : verts[i+1];
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

        internal virtual void Tessellate(ref Mesh mesh)
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

                // try
                // {
                    tess.Tessellate(WindingRule.Positive, LibTessDotNet.Double.ElementType.Polygons, 3);
                // }
                // catch
                // {
                //     continue;
                // }

                for (var i = 0; i < tess.ElementCount; i++)
                {
                    var a = tess.Vertices[tess.Elements[i * 3]].Position.ToVector3();
                    var b = tess.Vertices[tess.Elements[i * 3 + 1]].Position.ToVector3();
                    var c = tess.Vertices[tess.Elements[i * 3 + 2]].Position.ToVector3();

                    mesh.AddTriangle(a, b, c);
                }
            }
        }
    
        internal Edge[] SweepEdges(Transform[] transforms, Edge[] openEdge)
        {
            for (var i = 0; i < transforms.Length - 1; i++)
            {
                var v = (transforms[i + 1].Origin - transforms[i].Origin).Normalized();
                openEdge = SweepEdgesBetweenPlanes(openEdge, v, transforms[i + 1].XY);
            }
            return openEdge;
        }

        internal Loop SweepLoop(Loop loop, Vector3 direction, double distance)
        {
            var sweepEdges = new Edge[loop.Edges.Count];
            var i=0;
            foreach(var e in loop.Edges)
            {
                var v1 = e.Vertex;
                var v2 = AddVertex(v1.Point + direction * distance);
                sweepEdges[i] = AddEdge(v1, v2);
                i++;
            }

            var openLoop = new Loop();
            var j=0;
            foreach(var e in loop.Edges)
            {
                var a = e.Edge;
                var b = sweepEdges[j];
                var d = sweepEdges[j == loop.Edges.Count - 1 ? 0 : j+1];
                var c = AddEdge(b.Right.Vertex, d.Right.Vertex);
                var faceLoop = new Loop(new[]{a.Right, b.Left, c.Left, d.Right});
                AddFace(faceLoop);
                openLoop.AddEdgeToStart(c.Right);
                j++;
            }
            return openLoop;
        }

        private Edge[] ProjectEdgeAlong(Edge[] loop, Vector3 v, Plane p)
        {
            var edges = new Edge[loop.Length];
            for(var i=0; i<edges.Length; i++)
            {
                var e = loop[i];
                var a = AddVertex(e.Left.Vertex.Point.ProjectAlong(v, p));
                var b = AddVertex(e.Right.Vertex.Point.ProjectAlong(v, p));
                edges[i] = AddEdge(a,b);
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
            for(var i=0; i<sweepEdges.Length; i++)
            {
                var a = loop1[i];
                var b = sweepEdges[i];
                var c = loop2[i];
                var d = sweepEdges[i == loop1.Length - 1 ? 0 : i+1];
                
                var loop = new Loop(new[]{a.Right, b.Left, c.Left, d.Right});
                AddFace(loop);
                openEdge[i] = c;
            }
            return openEdge;
        }

        private Loop SweepPolygonBetweenPlanes(Polygon p, Plane start, Plane end)
        {
            // Transform the polygon to the mid plane between two transforms.
            var mid = new Line(start.Origin, end.Origin).TransformAt(0.5).OfPolygon(p);
            var v = (end.Origin - start.Origin).Normalized();
            var startP = mid.ProjectAlong(v, start);
            var endP = mid.ProjectAlong(v, end);

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
            for(var i=0; i<sweepEdges.Length; i++)
            {
                var a = loop1[i];
                var b = sweepEdges[i];
                var c = loop2[i];
                var d = sweepEdges[i == loop1.Length - 1 ? 0 : i+1];
                
                var loop = new Loop(new[]{a.Right, b.Left, c.Left, d.Right});
                AddFace(loop);
                openEdge.AddEdgeToEnd(c.Right);
            }
            return openEdge;
        }
    }
}