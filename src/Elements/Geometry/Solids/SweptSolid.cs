using System;
using System.Collections.Generic;
using Elements.Geometry.Interfaces;
using LibTessDotNet.Double;

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// A Solid created by sweeping a Face.
    /// </summary>
    public class SweptSolid : Solid
    {
        /// <summary>
        /// Construct a SweptSolid.
        /// This constructor is only to be used for deserialization.
        /// </summary>
        /// <param name="material">The Solid's Material.</param>
        /// <returns>A SweptSolid.</returns>
        public SweptSolid(Material material = null): base(material){}

        /// <summary>
        /// Construct a SweptSolid
        /// </summary>
        /// <param name="outerLoop">The perimeter of the Face to sweep.</param>
        /// <param name="innerLoops">The holes of the Face to sweep.</param>
        /// <param name="distance">The distance to sweep.</param>
        /// <param name="material">The Solid's material.</param>
        /// <returns>A SweptSolid.</returns>
        public SweptSolid(Polygon outerLoop, Polygon[] innerLoops, double distance, Material material = null) : this(outerLoop, innerLoops, Vector3.ZAxis, distance, material) { }

        /// <summary>
        /// Construct a SweptSolid
        /// </summary>
        /// <param name="outerLoop">The perimeter of the Face to sweep.</param>
        /// <param name="innerLoops">The holes of the Face to sweep.</param>
        /// <param name="direction">The direction in which to sweep.</param>
        /// <param name="distance">The distance to sweep.</param>
        /// <param name="material">The Solid's material.</param>
        /// <returns>A SweptSolid.</returns>
        public SweptSolid(Polygon outerLoop, Polygon[] innerLoops, Vector3 direction, double distance, Material material = null) : base(material)
        {
            Face fStart;
            if(innerLoops != null)
            {
                fStart = AddFace(outerLoop.Reversed(), innerLoops.Reversed());
            }
            else
            {
                fStart = AddFace(outerLoop.Reversed());
            }

            var fEndOuter = SweepLoop(fStart.Outer, direction, distance);

            if(innerLoops != null)
            {
                var fEndInner = new Loop[innerLoops.Length];
                for(var i=0; i<innerLoops.Length; i++)
                {
                    fEndInner[i] = SweepLoop(fStart.Inner[i], direction, distance);
                }
                AddFace(fEndOuter, fEndInner);
            }
            else
            {
                AddFace(fEndOuter);
            }
        }

        /// <summary>
        /// Construct a SweptSolid.
        /// </summary>
        /// <param name="outer">The perimeter of the Face to sweep.</param>
        /// <param name="inner">The holes of the face to sweep.</param>
        /// <param name="curve">The curve along which to sweep.</param>
        /// <param name="material">The Solid's material.</param>
        /// <param name="startSetback">The setback of the sweep from the start of the curve.</param>
        /// <param name="endSetback">The setback of the sweep from the end of the curve.</param>
        /// <returns>A SweptSolid.</returns>
        public SweptSolid(Polygon outer, Polygon[] inner, ICurve curve, Material material = null,  double startSetback = 0, double endSetback = 0) : base(material)
        {
            var l = curve.Length();
            var ssb = startSetback / l;
            var esb = endSetback / l;

            var transforms = curve.Frames(ssb, esb);

            if (curve is Polygon)
            {
                for (var i = 0; i < transforms.Length; i++)
                {
                    var next = i == transforms.Length - 1 ? transforms[0] : transforms[i + 1];
                    SweepPolygonBetweenPlanes(outer, transforms[i].XY, next.XY);
                }
            }
            else
            {
                // Add start cap.
                Face cap = null;
                Edge[][] openEdges;

                if(inner != null)
                {
                    cap = AddFace(transforms[0].OfPolygon(outer), transforms[0].OfPolygons(inner));
                    openEdges = new Edge[1 + inner.Length][];
                }
                else
                {
                    cap = AddFace(transforms[0].OfPolygon(outer));
                    openEdges = new Edge[1][];
                }

                // last outer edge
                var openEdge = cap.Outer.GetLinkedEdges();
                openEdge = SweepEdges(transforms, openEdge);
                openEdges[0] = openEdge;

                if(inner != null)
                {
                    for(var i=0; i<cap.Inner.Length; i++)
                    {
                        openEdge = cap.Inner[i].GetLinkedEdges();

                        // last inner edge for one hole
                        openEdge = SweepEdges(transforms, openEdge);
                        openEdges[i+1] = openEdge;
                    }
                }

                Cap(openEdges, true);
            }
        }

        private Edge[] SweepEdges(Transform[] transforms, Edge[] openEdge)
        {
            for (var i = 0; i < transforms.Length - 1; i++)
            {
                var v = (transforms[i + 1].Origin - transforms[i].Origin).Normalized();
                openEdge = SweepEdgesBetweenPlanes(openEdge, v, transforms[i + 1].XY);
            }
            return openEdge;
        }

        private Loop SweepLoop(Loop loop, Vector3 direction, double distance)
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