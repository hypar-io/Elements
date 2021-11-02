using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Interfaces;
using Elements.Serialization.glTF;
using glTFLoader.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Elements.Collections.Generics;
// using SixLabors.ImageSharp;

namespace Elements
{
    /// <summary>
    /// An element with a geometric representation.
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Elements.Serialization.JSON.JsonInheritanceConverter), "discriminator")]
    public class GeometricElement : Element
    {
        /// <summary>The element's transform.</summary>
        [Newtonsoft.Json.JsonProperty("Transform", Required = Newtonsoft.Json.Required.AllowNull)]
        public Transform Transform { get; set; }

        /// <summary>The element's material.</summary>
        [Newtonsoft.Json.JsonProperty("Material", Required = Newtonsoft.Json.Required.AllowNull)]
        public Material Material { get; set; }

        /// <summary>The element's representation.</summary>
        [Newtonsoft.Json.JsonProperty("Representation", Required = Newtonsoft.Json.Required.AllowNull)]
        public Representation Representation { get; set; }

        /// <summary>When true, this element will act as the base definition for element instances, and will not appear in visual output.</summary>
        [Newtonsoft.Json.JsonProperty("IsElementDefinition", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool IsElementDefinition { get; set; } = false;

        /// <summary>
        /// A function used to modify vertex attributes of the object's mesh
        /// during tesselation. Each vertex is passed to the modifier
        /// as the object is tessellated.
        /// </summary>
        [JsonIgnore]
        public Func<(Vector3 position, Vector3 normal, UV uv, Color color), (Vector3 position, Vector3 normal, UV uv, Color color)> ModifyVertexAttributes { get; set; }

        /// <summary>
        /// Create a geometric element.
        /// </summary>
        /// <param name="transform">The element's transform.</param>
        /// <param name="material">The element's material.</param>
        /// <param name="representation"></param>
        /// <param name="isElementDefinition"></param>
        /// <param name="id"></param>
        /// <param name="name"></param>
        [Newtonsoft.Json.JsonConstructor]
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
            // Override in derived classes.
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
        public Elements.Geometry.Mesh ToMesh(bool transform = false)
        {
            if (!HasGeometry())
            {
                this.UpdateRepresentations();
                if (!HasGeometry())
                {
                    throw new ArgumentNullException("This geometric element has no geometry, and cannot be turned into a mesh.");
                }
            }
            var mesh = new Elements.Geometry.Mesh();
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

        internal override void UpdateGLTF(Gltf gltf,
                                                    Dictionary<string, int> materialIndexMap,
                                                    List<byte> buffer,
                                                    List<byte[]> allBuffers,
                                                    List<glTFLoader.Schema.Buffer> schemaBuffers,
                                                    List<BufferView> bufferViews,
                                                    List<Accessor> accessors,
                                                    List<glTFLoader.Schema.Material> materials,
                                                    List<Texture> textures,
                                                    List<Image> images,
                                                    List<Sampler> samplers,
                                                    List<glTFLoader.Schema.Mesh> meshes,
                                                    List<glTFLoader.Schema.Node> nodes,
                                                    Dictionary<Guid, List<int>> meshElementMap,
                                                    Dictionary<Guid, ProtoNode> nodeElementMap,
                                                    Dictionary<Guid, Transform> meshTransformMap,
                                                    List<Vector3> lines,
                                                    bool drawEdges,
                                                    bool mergeVertices = false)
        {
            var materialId = BuiltInMaterials.Default.Id.ToString();
            int meshId = -1;

            if (typeof(ContentElement).IsAssignableFrom(this.GetType()))
            {
                var content = this as ContentElement;
                Stream glbStream = GltfExtensions.GetGlbStreamFromPath(content.GltfLocation);
                if (glbStream != System.IO.Stream.Null)
                {
                    var meshIndices = GltfMergingUtils.AddAllMeshesFromFromGlb(glbStream,
                                                            schemaBuffers,
                                                            allBuffers,
                                                            bufferViews,
                                                            accessors,
                                                            meshes,
                                                            materials,
                                                            textures,
                                                            images,
                                                            samplers,
                                                            true,
                                                            this.Id,
                                                            out var parentNode
                                                            );


                    if (!nodeElementMap.ContainsKey(this.Id) && parentNode != null)
                    {
                        nodeElementMap.Add(this.Id, parentNode);
                    }
                    if (!content.IsElementDefinition)
                    {
                        // This element is not used for instancing.
                        // apply scale transform here to bring the content glb into meters
                        var transform = content.Transform.Scaled(content.GltfScaleToMeters);
                        NodeUtilities.CreateNodeForMesh(meshId, nodes, transform);
                    }
                    else
                    {
                        // This element will be used for instancing.  Save the transform of the
                        // content element base that will be needed when instances are placed.
                        // The scaled transform is only necessary because we are using the glb.
                        if (!meshTransformMap.ContainsKey(this.Id))
                        {
                            meshTransformMap[this.Id] = content.Transform.Scaled(content.GltfScaleToMeters);
                        }
                    }
                }
                else
                {
                    meshId = GltfExtensions.ProcessGeometricRepresentation(this,
                                                            ref gltf,
                                                            ref materialIndexMap,
                                                            ref buffer,
                                                            bufferViews,
                                                            accessors,
                                                            meshes,
                                                            nodes,
                                                            meshElementMap,
                                                            lines,
                                                            drawEdges,
                                                            materialId,
                                                            ref meshId,
                                                            content,
                                                            mergeVertices);
                    if (!meshElementMap.ContainsKey(this.Id))
                    {
                        meshElementMap.Add(this.Id, new List<int> { meshId });
                    }
                }
            }
            else
            {
                materialId = this.Material.Id.ToString();

                meshId = GltfExtensions.ProcessGeometricRepresentation(this,
                                                        ref gltf,
                                                        ref materialIndexMap,
                                                        ref buffer,
                                                        bufferViews,
                                                        accessors,
                                                        meshes,
                                                        nodes,
                                                        meshElementMap,
                                                        lines,
                                                        drawEdges,
                                                        materialId,
                                                        ref meshId,
                                                        this,
                                                        mergeVertices);
                if (meshId > -1 && !meshElementMap.ContainsKey(this.Id))
                {
                    meshElementMap.Add(this.Id, new List<int> { meshId });
                }
            }

            if (this is Geometry.Interfaces.ITessellate)
            {
                var geo = (Geometry.Interfaces.ITessellate)this;
                var mesh = new Elements.Geometry.Mesh();
                geo.Tessellate(ref mesh);
                if (mesh == null)
                {
                    return;
                }

                var gbuffers = mesh.GetBuffers();

                // TODO(Ian): Remove this cast to GeometricElement when we
                // consolidate mesh under geometric representations.
                meshId = gltf.AddTriangleMesh(this.Id + "_mesh",
                                     buffer,
                                     bufferViews,
                                     accessors,
                                     materialIndexMap[materialId],
                                     gbuffers,
                                     null,
                                     meshes);

                if (!meshElementMap.ContainsKey(this.Id))
                {
                    meshElementMap.Add(this.Id, new List<int>());
                }
                meshElementMap[this.Id].Add(meshId);

                if (!this.IsElementDefinition)
                {
                    NodeUtilities.CreateNodeForMesh(meshId, nodes, this.Transform);
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
            // To properly compute csgs, all solid operation csgs need
            // to be transformed into their final position. Then the csgs
            // can be computed and by default the final csg will have the inverse of the
            // geometric element's transform applied to "reset" it.
            // The transforms applied to each node in the glTF will then
            // ensure that the elements are correctly transformed.
            Csg.Solid csg = new Csg.Solid();

            var solids = Representation.SolidOperations.Where(op => op.IsVoid == false)
                                                       .Select(op => TransformedSolidOperation(op))
                                                       .ToArray();
            var voids = Representation.SolidOperations.Where(op => op.IsVoid == true)
                                                      .Select(op => TransformedSolidOperation(op))
                                                      .ToArray();

            if (this is IHasOpenings openingContainer)
            {
                openingContainer.Openings.ForEach(o => o.UpdateRepresentations());
                voids = voids.Concat(openingContainer.Openings.SelectMany(o => o.Representation.SolidOperations
                                                      .Where(op => op.IsVoid == true)
                                                      .Select(op => op._solid.ToCsg().Transform(o.Transform.ToMatrix4x4())))).ToArray();
            }
            // Don't try CSG booleans if we only have one one solid and no voids.
            if (solids.Count() == 1 && voids.Count() == 0)
            {
                csg = solids.First();
            }
            else if (solids.Count() > 0)
            {
                csg = csg.Union(solids);
            }
            else
            {
                return csg;
            }
            if (voids.Count() > 0)
            {
                csg = csg.Substract(voids);
            }

            if (Transform == null || transformed)
            {
                return csg;
            }
            else
            {
                var inverse = new Transform(Transform);
                inverse.Invert();

                csg = csg.Transform(inverse.ToMatrix4x4());
                return csg;
            }
        }

        internal Csg.Solid[] GetSolids(bool transformed = false)
        {
            var solids = Representation.SolidOperations.Where(op => op.IsVoid == false)
                                                       .Select(op => TransformedSolidOperation(op))
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

        private Csg.Solid TransformedSolidOperation(Geometry.Solids.SolidOperation op)
        {
            if (Transform == null)
            {
                return op._solid.ToCsg();
            }
            return op.LocalTransform != null
                        ? op._solid.ToCsg().Transform(Transform.Concatenated(op.LocalTransform).ToMatrix4x4())
                        : op._solid.ToCsg().Transform(Transform.ToMatrix4x4());
        }
    }
}