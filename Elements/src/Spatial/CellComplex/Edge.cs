using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Elements.Geometry;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A unique edge in a cell complex, regardless of directionality when it comes to face traversal.
    /// Directional edges for this purpose are in associated DirectedEdges.
    /// There is a maximum of two DirectedEdges per Edge.
    /// </summary>
    public class Edge : EdgeBase<Edge>, Interfaces.IHasNeighbors<Edge, Line>
    {
        /// <summary>
        /// DirectedEdges that reference this Edge.
        /// </summary>
        [JsonIgnore]
        internal HashSet<DirectedEdge> DirectedEdges = new HashSet<DirectedEdge>();

        /// <summary>
        /// Represents a unique Edge within a CellComplex.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="cellComplex">CellComplex that this belongs to.</param>
        /// <param name="id">Edge ID.</param>
        /// <param name="vertexId1">One of the vertex IDs for this edge.</param>
        /// <param name="vertexId2">The other vertex ID for this edge.</param>
        internal Edge(CellComplex cellComplex, ulong id, ulong vertexId1, ulong vertexId2) : base(id, cellComplex)
        {
            this.SetVerticesFromIds(vertexId1, vertexId2);
        }

        /// <summary>
        /// Used for deserialization only!
        /// Simply omits the associated CellComplex, which we manually add in the CellComplex deserializer.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="startVertexId"></param>
        /// <param name="endVertexId"></param>
        /// <returns></returns>
        [JsonConstructor]
        internal Edge(ulong id, ulong startVertexId, ulong endVertexId) : base(id, null)
        {
            this.SetVerticesFromIds(startVertexId, endVertexId);
        }

        /// <summary>
        /// Sets the StartVertexId and EndVertexId so that start vertex always has a smaller ID than end vertex.
        /// </summary>
        /// <param name="id1">One of the two applicable vertex IDs.</param>
        /// <param name="id2">The other applicable vertex IDs.</param>
        private void SetVerticesFromIds(ulong id1, ulong id2)
        {
            if (id1 < id2)
            {
                this.StartVertexId = id1;
                this.EndVertexId = id2;
            }
            else
            {
                this.EndVertexId = id1;
                this.StartVertexId = id2;
            }
        }

        /// <summary>
        /// Get the unique hash for an Edge with list (of length 2) of its unordered vertex IDs.
        /// </summary>
        /// <param name="vertexIds"></param>
        /// <returns></returns>
        internal static string GetHash(List<ulong> vertexIds)
        {
            var sortedIds = vertexIds.ToList();
            sortedIds.Sort();
            var hash = String.Join(",", sortedIds);
            return hash;
        }

        /// <summary>
        /// Get associated Vertices.
        /// </summary>
        /// <returns></returns>
        public List<Vertex> GetVertices()
        {
            return new List<Vertex>() { this.CellComplex.GetVertex(this.StartVertexId), this.CellComplex.GetVertex(this.EndVertexId) };
        }

        /// <summary>
        /// Get associated Faces.
        /// </summary>
        /// <returns></returns>
        public List<Face> GetFaces()
        {
            return this.GetDirectedEdges().Select(ds => ds.GetFaces()).SelectMany(x => x).Distinct().ToList();
        }

        /// <summary>
        /// Get associated Cells
        /// </summary>
        /// <returns></returns>
        public List<Cell> GetCells()
        {
            return this.GetFaces().Select(face => face.GetCells()).SelectMany(x => x).Distinct().ToList();
        }

        /// <summary>
        /// Get the associated Vertex that is closest to a point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vertex GetClosestVertex(Vector3 point)
        {
            return Vertex.GetClosest<Vertex>(this.GetVertices(), point);
        }

        /// <summary>
        /// Get the associated Face that is closest to a point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Face GetClosestFace(Vector3 point)
        {
            return Face.GetClosest<Face>(this.GetFaces(), point);
        }

        /// <summary>
        /// Get the associated Cell that is closest to a point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Cell GetClosestCell(Vector3 point)
        {
            return Cell.GetClosest<Cell>(this.GetCells(), point);
        }

        /// <summary>
        /// Get a list of all neighboring Edges that share a Vertex.
        /// </summary>
        /// <returns></returns>
        public List<Edge> GetNeighbors()
        {
            return this.GetVertices().Select(vertex => vertex.GetEdges()).SelectMany(x => x).Where(e => e.Id != this.Id).Distinct().ToList();
        }

        /// <summary>
        /// Get the closest neighboring Edge to a point.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public Edge GetClosestNeighbor(Vector3 target)
        {
            return Edge.GetClosest<Edge>(this.GetNeighbors().Where(e => e.DistanceTo(target) < this.DistanceTo(target)).ToList(), target);
        }

        /// <summary>
        /// Get associated DirectedEdges.
        /// </summary>
        /// <returns></returns>
        internal List<DirectedEdge> GetDirectedEdges()
        {
            return this.DirectedEdges.ToList();
        }

        /// <summary>
        /// Traverse the neighbors of this Edge toward the target point.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="completedRadius">If provided, ends the traversal when the neighbor is within this distance to the target point.</param>
        /// <returns>A collection of traversed Edges, including the starting Edge.</returns>
        public List<Edge> TraverseNeighbors(Vector3 target, double completedRadius = 0)
        {
            var maxCount = this.CellComplex.GetEdges().Count;
            Func<Edge, Edge> getNextNeighbor = (Edge curNeighbor) => (curNeighbor.GetClosestNeighbor(target));
            return Edge.TraverseNeighbors(this, maxCount, target, completedRadius, getNextNeighbor);
        }

        /// <summary>
        /// Split the edge at the specified point.
        /// </summary>
        /// <param name="point">The point at which to split the edge.</param>
        /// <param name="edges">A collection of edges resulting from the split.</param>
        /// <returns>True if the split was successful, or false if the specified point
        /// does not split the edge or the edge could not be split.</returns>
        public bool TrySplit(Vector3 point, out List<Edge> edges)
        {
            edges = new List<Edge>();

            var a = this.CellComplex.GetVertex(this.StartVertexId);
            var b = this.CellComplex.GetVertex(this.EndVertexId);

            if (point.IsAlmostEqualTo(a.Value) || point.IsAlmostEqualTo(b.Value))
            {
                return false;
            }

            var l = new Line(a.Value, b.Value);
            if (point.DistanceTo(l, out Vector3 closestPoint) > Vector3.EPSILON)
            {
                return false;
            }

            // |                              |
            // |            face              |
            // |                              |
            // -------------->----------------> <- Create new directed edges.
            // a-------------x----------------b <- Add a new vertex on edge at x.
            // <-------------<----------------    Create a new edge xb
            // |                              |
            // |            face              |
            // |                              |

            // Add a new vertex to the cell complex
            var x = this.CellComplex.AddVertexOrOrientation<Vertex>(point);
            var id1 = this.Id;
            var id2 = x.Id;

            // Remove this edge from the b collection
            b.Edges.Remove(this);

            // Create a new edge from the new vertex
            // to the existing end vertex.
            this.CellComplex.AddEdge(new List<ulong> { x.Id, this.EndVertexId }, this.CellComplex._edgeId, out Edge xb);
            this.CellComplex._edgeId++;

            // Adjust this edge to point to the new vertex.
            this.EndVertexId = x.Id;

            // Add the existing edge to the new vertex's collection.
            x.Edges.Add(this);

            // Add this edge to the b collection
            b.Edges.Add(xb);

            // Repoint the existing directed edges.
            // Create two new directed edges and insert them into the adjacent 
            // face collections. 
            foreach (var de in this.DirectedEdges)
            {
                DirectedEdge newDirectedEdge = null;
                if (de.StartVertexId == a.Id)
                {
                    de.EndVertexId = x.Id;

                    this.CellComplex.AddDirectedEdge(xb, false, this.CellComplex._directedEdgeId, out newDirectedEdge);

                    foreach (var f in de.GetFaces())
                    {
                        var idx = f.DirectedEdgeIds.IndexOf(de.Id);
                        // Insert in loop after existing
                        var nextIndex = idx == f.DirectedEdgeIds.Count - 1 ? 0 : idx + 1;
                        f.DirectedEdgeIds.Insert(nextIndex, newDirectedEdge.Id);
                        newDirectedEdge.Faces.Add(f);
                    }
                }
                else if (de.EndVertexId == a.Id)
                {
                    de.StartVertexId = x.Id;

                    this.CellComplex.AddDirectedEdge(xb, true, this.CellComplex._directedEdgeId, out newDirectedEdge);

                    foreach (var f in de.GetFaces())
                    {
                        var idx = f.DirectedEdgeIds.IndexOf(de.Id);
                        // Insert in loop before existing
                        var prevIndex = idx == 0 ? f.DirectedEdgeIds.Count - 1 : idx - 1;
                        f.DirectedEdgeIds.Insert(prevIndex, newDirectedEdge.Id);
                        newDirectedEdge.Faces.Add(f);
                    }
                }
                xb.DirectedEdges.Add(newDirectedEdge);
            }

            return true;
        }
    }
}