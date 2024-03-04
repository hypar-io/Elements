using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Interfaces;
using Elements.Search;
using Elements.Spatial;
using Elements.Utilities;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("Hypar.Elements.Serialization.SVG.Tests"),
            InternalsVisibleTo("Hypar.Elements.Serialization.SVG")]

namespace Elements
{
    /// <summary>
    /// An element with a geometric representation.
    /// </summary>
    [JsonConverter(typeof(Serialization.JSON.JsonInheritanceConverter), "discriminator")]
    public class GeometricElement : Element
    {
        private BBox3 _bounds;
        internal Csg.Solid _csg;

        // Used to attach a "selectable: false" flag to the element in the
        // generated GLB. NOTE: currently only considered in the "ModelLines /
        // ModelPoints" pathways — this is not yet supported for mesh elements.
        internal bool _isSelectable = true;

        /// <summary>
        /// The element's bounds.
        /// The bounds are only available when the geometry has been
        /// updated using UpdateBoundsAndComputeSolid(),
        /// </summary>
        [JsonIgnore]
        public BBox3 Bounds => _bounds;

        /// <summary>The element's transform.</summary>
        [JsonProperty("Transform")]
        public Transform Transform { get; set; }

        /// <summary>The element's material.</summary>
        [JsonProperty("Material")]
        public Material Material { get; set; }

        /// <summary>The element's representation.</summary>
        [JsonProperty("Representation")]
        public Representation Representation { get; set; }

        /// <summary>
        ///  The list of element representations. 
        /// </summary>
        [JsonIgnore]
        public List<RepresentationInstance> RepresentationInstances { get; set; } = new List<RepresentationInstance>();

        /// <summary>When true, this element will act as the base definition for element instances, and will not appear in visual output.</summary>
        [JsonProperty("IsElementDefinition", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool IsElementDefinition { get; set; } = false;

        /// <summary>
        /// A function used to modify vertex attributes of the object's mesh
        /// during tesselation. Each vertex is passed to the modifier
        /// as the object is tessellated.
        /// </summary>
        [JsonIgnore]
        public Func<(Vector3 position, Vector3 normal, UV uv, Color? color), (Vector3 position, Vector3 normal, UV uv, Color? color)> ModifyVertexAttributes { get; set; }

        /// <summary>
        /// Create a geometric element.
        /// </summary>
        /// <param name="transform">The element's transform.</param>
        /// <param name="material">The element's material.</param>
        /// <param name="representation"></param>
        /// <param name="isElementDefinition"></param>
        /// <param name="id"></param>
        /// <param name="name"></param>
        [JsonConstructor]
        public GeometricElement(Transform @transform = null, Material @material = null, Representation @representation = null, bool @isElementDefinition = false, System.Guid @id = default, string @name = null)
            : base(id, name)
        {
            this.Transform = @transform ?? new Geometry.Transform();
            this.Material = @material ?? BuiltInMaterials.Default;
            this.Representation = @representation;
            this.IsElementDefinition = @isElementDefinition;
        }

        /// <summary>
        /// This method provides an opportunity for geometric elements
        /// to adjust their solid operations before tesselation. As an example,
        /// a floor might want to clip its opening profiles out of
        /// the profile of the floor.
        /// </summary>
        public virtual void UpdateRepresentations()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Update the computed solid and the bounding box of the element.
        /// </summary>
        public void UpdateBoundsAndComputeSolid(bool transformed = false)
        {
            if (Transform != null)
            {
                var tScale = Transform.GetScale();
                if (tScale.X == 0.0 || tScale.Y == 0.0 || tScale.Z == 0.0)
                {
                    throw new ArgumentOutOfRangeException($"A solid cannot be created for elements {Id}. One or more components of the element's transform has a scale equal to zero.");
                }
            }
            _csg = GetFinalCsgFromSolids(transformed);
            if (_csg != null)
            {
                _bounds = new BBox3(_csg.Polygons.SelectMany(p => p.Vertices.Select(v => v.Pos.ToVector3())));
            }

            if (RepresentationInstances != null && RepresentationInstances.Any())
            {
                foreach (var instance in RepresentationInstances)
                {
                    // TODO: filter by view or representation types
                    if (!instance.IsDefault)
                    {
                        continue;
                    }

                    if (instance.Representation is SolidRepresentation solidRepresentation)
                    {
                        var representationBounds = solidRepresentation.ComputeBounds(this);
                        if (!representationBounds.Volume.ApproximatelyEquals(0))
                        {
                            if (_bounds.Volume.ApproximatelyEquals(0))
                            {
                                _bounds = representationBounds;
                            }
                            else
                            {
                                _bounds.Extend(new[] { representationBounds.Min, representationBounds.Max });
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create an instance of this element.
        /// Instances will point to the same instance of an element.
        /// </summary>
        /// <param name="transform">The transform for this element instance.</param>
        /// <param name="name">The name of this element instance.</param>
        public ElementInstance CreateInstance(Transform transform, string name)
        {
            if (!this.IsElementDefinition)
            {
                throw new Exception($"An instance cannot be created of the type {this.GetType().Name} because it is not marked as an element definition. Set the IsElementDefinition flag to true.");
            }

            return new ElementInstance(this, transform, name, Guid.NewGuid());
        }

        /// <summary>
        /// Get the mesh representing the this Element's geometry. By default it will be untransformed.
        /// </summary>
        /// <param name="transform">Should the mesh be transformed into its final location?</param>
        public Mesh ToMesh(bool transform = false)
        {
            if (!HasGeometry())
            {
                this.UpdateRepresentations();
                if (!HasGeometry())
                {
                    throw new ArgumentNullException("This geometric element has no geometry, and cannot be turned into a mesh.");
                }
            }
            var mesh = new Mesh();
            var solid = GetFinalCsgFromSolids(transform);
            solid.Tessellate(ref mesh);
            return mesh;
        }

        /// <summary>
        /// Does this geometric element have geometry?
        /// </summary>
        public bool HasGeometry()
        {
            return Representation != null && Representation.SolidOperations != null && Representation.SolidOperations.Count > 0;
        }

        /// <summary>
        /// Does this element intersect the provided plane?
        /// </summary>
        /// <param name="plane">The plane of intersection.</param>
        /// <param name="intersectionPolygons">A collection of polygons representing
        /// the intersections of the plane and the element's solid geometry.</param>
        /// <param name="beyondPolygons">A collection of polygons representing coplanar 
        /// faces beyond the plane of intersection.</param>
        /// <param name="lines">A collection of lines representing intersections
        /// of zero-thickness elements with the plane.</param>
        /// <returns>True if an intersection occurs, otherwise false.</returns>
        public bool Intersects(Plane plane,
                               out Dictionary<Guid, List<Polygon>> intersectionPolygons,
                               out Dictionary<Guid, List<Polygon>> beyondPolygons,
                               out Dictionary<Guid, List<Line>> lines)
        {
            beyondPolygons = new Dictionary<Guid, List<Polygon>>();
            intersectionPolygons = new Dictionary<Guid, List<Polygon>>();
            lines = new Dictionary<Guid, List<Line>>();

            var graphVertices = new List<Vector3>();
            var graphEdges = new List<List<(int from, int to, int? tag)>>();

            var beyondPolygonsList = new List<Polygon>();

            if (Representation != null && _csg != null)
            {
                // TODO: Can we avoid this copy? It seems to be the most straightforward
                // way to get the csg transformed for sectioning.
                var localCsg = _csg.Transform(Transform.ToMatrix4x4());
                foreach (var csgPoly in localCsg.Polygons)
                {
                    var csgNormal = csgPoly.Plane.Normal.ToVector3();

                    if (csgNormal.IsAlmostEqualTo(plane.Normal) && csgPoly.Plane.IsBehind(plane))
                    {
                        // TODO: We can cut out transformation if the element's transform is null.
                        var backPoly = csgPoly.Project(plane);
                        beyondPolygonsList.Add(backPoly);

                        continue;
                    }

                    var edgeResults = new List<Vector3>();
                    for (var i = 0; i < csgPoly.Vertices.Count; i++)
                    {
                        var a = csgPoly.Vertices[i].Pos.ToVector3();
                        var b = i == csgPoly.Vertices.Count - 1 ? csgPoly.Vertices[0].Pos.ToVector3() : csgPoly.Vertices[i + 1].Pos.ToVector3();
                        if (plane.Intersects((a, b), out var xsect))
                        {
                            edgeResults.Add(xsect);
                        }
                    }

                    if (edgeResults.Count < 2)
                    {
                        continue;
                    }

                    var d = csgNormal.Cross(plane.Normal).Unitized();
                    edgeResults.Sort(new DirectionComparer(d));
                    AddToGraph(edgeResults, graphVertices, graphEdges);
                }
            }

            if (RepresentationInstances != null)
            {
                foreach (var instance in RepresentationInstances)
                {
                    // TODO: filter by view or representation types
                    if (!instance.IsDefault)
                    {
                        continue;
                    }

                    if (instance.Representation is SolidRepresentation solidRepresentation)
                    {
                        foreach (var intersection in solidRepresentation.CalculateIntersectionPoints(this, plane,
                            out var beyondPolygonsLocal))
                        {
                            AddToGraph(intersection, graphVertices, graphEdges);
                        }
                    }
                }
            }

            var heg = new HalfEdgeGraph2d()
            {
                Vertices = graphVertices,
                EdgesPerVertex = graphEdges
            };

            beyondPolygons[Id] = beyondPolygonsList;

            try
            {
                // Elements with zero thickness sections.
                if (heg.Vertices.Count == 2)
                {
                    // TODO: We're over-drawing here because we have edges
                    // that are from->to and to->from.
                    foreach (var edges in heg.EdgesPerVertex)
                    {
                        foreach (var (from, to, tag) in edges)
                        {
                            var start = heg.Vertices[from];
                            var end = heg.Vertices[to];
                            var line = new Line(start, end);
                            if (!lines.ContainsKey(Id))
                            {
                                lines[Id] = new List<Geometry.Line>() { line };
                            }
                            else
                            {
                                lines[Id].Add(line);
                            }
                        }
                    }
                    return true;
                }

                var rebuiltPolys = heg.Polygonize();
                if (rebuiltPolys == null || rebuiltPolys.Count == 0)
                {
                    return false;
                }

                if (!intersectionPolygons.ContainsKey(Id))
                {
                    intersectionPolygons[Id] = new List<Polygon>(rebuiltPolys);
                }
                else
                {
                    intersectionPolygons[Id].AddRange(rebuiltPolys);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        private static void AddToGraph(List<Vector3> intersectionPoints, List<Vector3> graphVertices, List<List<(int from, int to, int? tag)>> graphEdges)
        {
            // Draw segments through the results and add to the
            // half edge graph.
            for (var j = 0; j < intersectionPoints.Count - 1; j += 2)
            {
                // Don't create zero-length edges.
                if (intersectionPoints[j].IsAlmostEqualTo(intersectionPoints[j + 1]))
                {
                    continue;
                }

                var a = Solid.FindOrCreateGraphVertex(intersectionPoints[j], graphVertices, graphEdges);
                var b = Solid.FindOrCreateGraphVertex(intersectionPoints[j + 1], graphVertices, graphEdges);
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

        /// <summary>
        /// Get the computed csg solid.
        /// The csg is centered on the origin by default.
        /// </summary>
        /// <param name="transformed">Should the csg be transformed by the element's transform?</param>
        internal Csg.Solid GetFinalCsgFromSolids(bool transformed = false)
        {
            if (Representation == null || Representation.SolidOperations.Count == 0)
            {
                return null;
            }

            return SolidOperationUtils.GetFinalCsgFromSolids(Representation.SolidOperations, this, transformed);
        }

        internal Csg.Solid[] GetCsgSolids(bool transformed = false)
        {
            var solids = Representation.SolidOperations.Where(op => op.IsVoid == false)
                                                       .Select(op => SolidOperationUtils.TransformedSolidOperation(op, this))
                                                       .ToArray();
            if (Transform == null || transformed)
            {
                return solids;
            }
            else
            {
                var inverse = new Transform(Transform);
                inverse.Invert();
                return solids.Select(s => s.Transform(inverse.ToMatrix4x4())).ToArray();
            }
        }

        /// <summary>
        /// Get graphics buffers and other metadata required to modify a GLB.
        /// </summary>
        /// <returns>
        /// True if there is graphicsbuffers data applicable to add, false otherwise.
        /// Out variables should be ignored if the return value is false.
        /// </returns>
        public virtual bool TryToGraphicsBuffers(out List<GraphicsBuffers> graphicsBuffers, out string id, out glTFLoader.Schema.MeshPrimitive.ModeEnum? mode)
        {
            id = null;
            mode = null;
            graphicsBuffers = new List<GraphicsBuffers>(); // this is intended to be discarded
            return false;
        }
    }
}