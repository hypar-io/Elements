#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using glTFLoader.Schema;
using LibTessDotNet.Double;
using Newtonsoft.Json;

[assembly:InternalsVisibleTo("Hypar.Elements.Tests")]

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// The base class for all Solids.
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
        /// The Edges of the Solid.
        /// </summary>
        public Dictionary<long, Edge> Edges { get; }

        /// <summary>
        /// The Vertices of the Solid.
        /// </summary>
        public Dictionary<long, Vertex> Vertices { get; }

        /// <summary>
        /// The Material of the Solid.
        /// </summary>
        public Material Material{get;}

        /// <summary>
        /// Construct a Solid.
        /// </summary>
        public Solid(Material material = null)
        {
            this.Faces = new Dictionary<long, Face>();
            this.Edges = new Dictionary<long, Edge>();
            this.Vertices = new Dictionary<long, Vertex>();
            this.Material = material != null ? material : BuiltInMaterials.Default;
        }

        /// <summary>
        /// Construct a LaminaSolid.
        /// </summary>
        /// <param name="perimeter">The perimeter of the lamina's faces.</param>
        /// <param name="material">The LaminaSolid's Material.</param>
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
        /// Add a Vertex to the Solid.
        /// </summary>
        /// <param name="position"></param>
        /// <returns>The newly added Vertex.</returns>
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
        /// <param name="outer">A Polygon representing the perimeter of the Face.</param>
        /// <param name="inner">An array of Polygons representing the holes in the Face.</param>
        /// <returns>The newly added Face.</returns>
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
        /// Add an Edge to the Solid.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>The newly added Edge.</returns>
        public Edge AddEdge(Vertex from, Vertex to)
        {
            var e = new Edge(_edgeId, from, to);
            this.Edges.Add(_edgeId, e);
            _edgeId++;
            return e;
        }

        /// <summary>
        /// Add a Face to the solid.
        /// Provided edges are expected to be wound CCW for outer,
        /// and CW for inner. The Face will be linked to the Edges.
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
        /// Creates a series of Edges from a Polygon.
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
        /// Get the string representation of the Solid.
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
        /// The first Edge array is treated as the outer edge.
        /// Additional Edge arrays are treated as holes.
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
}