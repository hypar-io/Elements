using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Elements.Geometry;

namespace Elements.Spatial.CellComplex
{
    /// <summary>
    /// A Face within a cell. Multiple cells can share the same Face.
    /// </summary>
    public class Face : ChildBase<Face, Polygon>, Interfaces.IHasNeighbors<Face, Polygon>
    {
        /// <summary>
        /// Edge Ids.
        /// </summary>
        public List<ulong> EdgeIds;

        /// <summary>
        /// ID of U orientation.
        /// </summary>
        [JsonProperty]
        private ulong? _orientationUId;

        /// <summary>
        /// ID of V orientation.
        /// </summary>
        [JsonProperty]
        private ulong? _orientationVId;

        /// <summary>
        /// Cells that reference this Face.
        /// </summary>
        [JsonIgnore]
        internal HashSet<Cell> Cells = new HashSet<Cell>();

        /// <summary>
        /// Represents a unique Face within a CellComplex.
        /// Is not intended to be created or modified outside of the CellComplex class code.
        /// </summary>
        /// <param name="cellComplex">CellComplex that this Face belongs to.</param>
        /// <param name="id">ID of this Face.</param>
        /// <param name="edges">List of the edges that make up this Face.</param>
        /// <param name="u">Optional but highly recommended intended U direction for the Face.</param>
        /// <param name="v">Optional but highly recommended intended V direction for the Face.</param>
        internal Face(CellComplex cellComplex, ulong id, List<Edge> edges, Orientation u = null, Orientation v = null) : base(id, cellComplex)
        {
            this.EdgeIds = edges.Select(ds => ds.Id).ToList();
            if (u != null)
            {
                this._orientationUId = u.Id;
            }
            if (v != null)
            {
                this._orientationVId = v.Id;
            }
        }

        /// <summary>
        /// Used for deserialization only!
        /// </summary>
        [JsonConstructor]
        internal Face(ulong id, List<ulong> edgeIds, ulong? _orientationUId = null, ulong? _orientationVId = null) : base(id, null)
        {
            this.Id = id;
            this.EdgeIds = edgeIds;
            this._orientationUId = _orientationUId;
            this._orientationVId = _orientationVId;
        }

        /// <summary>
        /// Get the geometry for this face.
        /// </summary>
        /// <returns>A polygon.</returns>
        public override Polygon GetGeometry()
        {
            var p = new Polygon(this.GetVertices().Select(v => v.Value).ToList());
            return p.IsClockWise() ? p.Reversed() : p;
            // return 
        }

        /// <summary>
        ///  Get the shortest distance from a point to the geometry representing this face.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public override double DistanceTo(Vector3 point)
        {
            return point.DistanceTo(this.GetGeometry());
        }

        /// <summary>
        /// Face lookup hash is EdgeIds in ascending order.
        /// </summary>
        /// <param name="edges"></param>
        /// <returns></returns>
        internal static string GetHash(IList<Edge> edges)
        {
            var sortedIds = edges.Select(ds => ds.Id).ToList();
            sortedIds.Sort();
            var hash = String.Join(",", sortedIds);
            return hash;
        }

        /// <summary>
        /// Get the normal vector for this Face.
        /// </summary>
        /// <returns></returns>
        internal Vector3 GetNormal()
        {
            return this.GetGeometry().Normal();
        }

        /// <summary>
        /// Whether this Face is parallel to another Face.
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        private bool IsParallel(Face face)
        {
            var n1 = face.GetNormal();
            var n2 = this.GetNormal();
            var dot = Math.Abs(n1.Dot(n2));
            return dot == 1;
        }

        /// <summary>
        /// Get associated Cells.
        /// </summary>
        /// <returns></returns>
        public List<Cell> GetCells()
        {
            return this.Cells.ToList();
        }

        /// <summary>
        /// Get the centroid of the face.
        /// </summary>
        internal Vector3 GetCentroid()
        {
            var vertices = GetVerticesUnordered().Select(v => v.Value).ToList();
            var x = 0.0;
            var y = 0.0;
            var z = 0.0;
            foreach (var pnt in vertices)
            {
                x += pnt.X;
                y += pnt.Y;
                z += pnt.Z;
            }
            return new Vector3(x / vertices.Count, y / vertices.Count, z / vertices.Count);
        }

        /// <summary>
        /// Get associated vertices.
        /// </summary>
        /// <returns>A collection of vertices without specific ordering.</returns>
        public List<Vertex> GetVerticesUnordered()
        {
            return this.GetEdges().SelectMany(e => new[] { this.CellComplex.GetVertex(e.StartVertexId), this.CellComplex.GetVertex(e.EndVertexId) }).Distinct().ToList();
        }

        /// <summary>
        /// Get associated Vertices.
        /// This method parses adjacent edges into a loop. If winding is not
        /// required, use GetVerticesUnordered.
        /// </summary>
        /// <returns>A collection of vertices wound according to their edges</returns>
        public List<Vertex> GetVertices()
        {
            var edges = this.GetEdges();
            var test = new List<Edge>(edges);

            // Create tip->tail edge loop
            var vertices = new List<Vertex>();

            Vertex current = null;
            while (test.Count > 0)
            {
                var initial = test.Count;
                for (var j = test.Count - 1; j >= 0; j--)
                {
                    var edge = test[j];
                    var a = this.CellComplex.GetVertex(edge.StartVertexId);
                    var b = this.CellComplex.GetVertex(edge.EndVertexId);

                    if (vertices.Count == 0)
                    {
                        vertices.Add(a);
                        vertices.Add(b);
                        current = b;
                        test.Remove(edge);
                        break;
                    }

                    if (a.Value.IsAlmostEqualTo(current.Value))
                    {
                        if (!vertices.Contains(b))
                        {
                            vertices.Add(b);
                        }
                        current = b;
                        test.Remove(edge);
                        break;
                    }
                    else if (b.Value.IsAlmostEqualTo(current.Value))
                    {
                        if (!vertices.Contains(a))
                        {
                            vertices.Add(a);
                        }
                        current = a;
                        test.Remove(edge);
                        break;
                    }
                }
                if (test.Count == initial)
                {
                    break;
                }
            }

            return vertices;
        }

        /// <summary>
        /// Get associated Edges.
        /// </summary>
        /// <returns></returns>
        public List<Edge> GetEdges()
        {
            return this.EdgeIds.Select(id => this.CellComplex.GetEdge(id)).ToList();
        }

        /// <summary>
        /// Get the associated Vertex that is closest to a point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vertex GetClosestVertex(Vector3 point)
        {
            return Vertex.GetClosest<Vertex>(this.GetVerticesUnordered(), point);
        }

        /// <summary>
        /// Get the associated Edge that is closest to a point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Edge GetClosestEdge(Vector3 point)
        {
            return Edge.GetClosest<Edge>(this.GetEdges(), point);
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
        /// Get the user-set U and V Orientatinos of this face.
        /// </summary>
        /// <returns></returns>
        public (Orientation U, Orientation V) GetOrientation()
        {
            return (U: this.CellComplex.GetOrientation(this._orientationUId), V: this.CellComplex.GetOrientation(this._orientationVId));
        }

        /// <summary>
        /// Get a list of all neighbors of this Face.
        /// A neighbor is defined as a Face which shares any Edge.
        /// </summary>
        /// <returns></returns>
        public List<Face> GetNeighbors()
        {
            return this.GetNeighbors(false, false);
        }

        /// <summary>
        /// Get a list of all neighbors of this Face.
        /// A neighbor is defined as a Face which shares any Edge.
        /// </summary>
        /// <param name="parallel">If true, only returns Faces that are oriented the same way as this Face.</param>
        /// <param name="includeSharedVertices">If true, includes Faces that share a Vertex as well as Faces that share an Edge.</param>
        /// <returns></returns>
        public List<Face> GetNeighbors(bool parallel = false, bool includeSharedVertices = false)
        {
            var groupedFaces = includeSharedVertices ? this.GetVerticesUnordered().Select(v => v.GetFaces()).ToList() : this.GetEdges().Select(s => s.GetFaces()).ToList();
            var faces = groupedFaces.SelectMany(x => x).Distinct().Where(f => f.Id != this.Id).ToList();
            if (parallel)
            {
                return faces.Where(f => this.IsParallel(f)).ToList();
            }
            else
            {
                return faces;
            }
        }

        /// <summary>
        /// Get a list of neighboring Faces that share a specific Edge.
        /// </summary>
        /// <param name="edge">Edge that the neighbor should share.</param>
        /// <param name="parallel">Whether to only return Faces that are parallel to this Face.</param>
        /// <returns></returns>
        public List<Face> GetNeighbors(Edge edge, bool parallel = false)
        {
            if (!parallel)
            {
                return edge.GetFaces().Where(face => face.Id != this.Id).ToList();
            }
            else
            {
                return edge.GetFaces().Where(face => face.Id != this.Id && this.IsParallel(face)).ToList();
            }
        }

        /// <summary>
        /// Get the closest associated Face to a given point.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public Face GetClosestNeighbor(Vector3 target)
        {
            return this.GetClosestNeighbor(target, false, false);
        }

        /// <summary>
        /// Get the closest associated Face to a given point.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parallel">If true, only checks faces that are oriented the same way as this Face.</param>
        /// <param name="includeSharedVertices">If true, checks Faces that share a Vertex as well as Faces that share a Edge.</param>
        /// <returns></returns>
        public Face GetClosestNeighbor(Vector3 target, bool parallel = false, bool includeSharedVertices = false)
        {
            return Face.GetClosest<Face>(this.GetNeighbors(parallel, includeSharedVertices).Where(f =>
            {
                var d1 = this.DistanceTo(target);
                var d2 = f.DistanceTo(target);
                if (f.DistanceTo(target) < this.DistanceTo(target))
                {

                }
                return f.DistanceTo(target) < this.DistanceTo(target);
            }).ToList(), target);
        }

        /// <summary>
        /// Traverse the neighbors of this Face toward the target point.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="completedRadius"></param>
        /// <returns></returns>
        public List<Face> TraverseNeighbors(Vector3 target, double completedRadius = 0)
        {
            return this.TraverseNeighbors(target, false, false, completedRadius);
        }

        /// <summary>
        /// Traverse the neighbors of this Face toward the target point.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parallel">If true, only checks faces that are oriented the same way as this Face.</param>
        /// <param name="includeSharedVertices">If true, checks Faces that share a Vertex as well as Faces that share a Edge.</param>
        /// <param name="completedRadius">If provided, ends the traversal when the neighbor is within this distance to the target point.</param>
        /// <returns>A collection of traversed Faces, including the starting Face.</returns>
        public List<Face> TraverseNeighbors(Vector3 target, bool parallel = false, bool includeSharedVertices = false, double completedRadius = 0)
        {
            var maxCount = this.CellComplex.GetFaces().Count;
            Func<Face, Face> getNextNeighbor = (Face curNeighbor) => (curNeighbor.GetClosestNeighbor(target, parallel, includeSharedVertices));
            return Face.TraverseNeighbors(this, maxCount, target, completedRadius, getNextNeighbor);
        }
    }
}