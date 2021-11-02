using Elements.Geometry;
using Newtonsoft.Json;
using Elements.Serialization.glTF;
using glTFLoader.Schema;
using System;
using System.Collections.Generic;

namespace Elements
{
    /// <summary>
    /// An instance of an element in the model.
    /// Instances point to one instance of a type, but have
    /// individual ids and transforms.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/ElementInstanceTests.cs?name=example)]
    /// </example>
    public class ElementInstance : Element
    {
        /// <summary>
        /// The element from which this instance is derived.
        /// </summary>
        public GeometricElement BaseDefinition { get; }

        /// <summary>
        /// The transform of the instance.
        /// </summary>
        public Transform Transform { get; }

        /// <summary>
        /// Construct an element instance.
        /// </summary>
        /// <param name="baseDefinition">The definition from which this instance is derived.</param>
        /// <param name="transform">The transform of the instance.</param>
        /// <param name="name">The name of the instance.</param>
        /// <param name="id">The id of the instance.</param>
        [JsonConstructor]
        internal ElementInstance(GeometricElement baseDefinition,
                               Transform transform,
                               string name = null,
                               Guid id = default(Guid)) : base(id == default(Guid) ? Guid.NewGuid() : id, name)
        {
            this.BaseDefinition = baseDefinition;
            this.Transform = transform;
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

            var transform = new Transform();
            if (this.BaseDefinition is ContentElement contentBase)
            {
                // if we have a stored node for this object, we use that when adding it to the gltf.
                if (nodeElementMap.TryGetValue(this.BaseDefinition.Id, out var nodeToCopy))
                {
                    transform.Concatenate(this.Transform);
                    NodeUtilities.AddInstanceAsCopyOfNode(nodes, nodeElementMap[this.BaseDefinition.Id], transform, this.Id);
                }
                else
                {
                    // If there is a transform stored for the content base definition we
                    // should apply it when creating instances.
                    // TODO check if this meshTransformMap ever does anything.
                    if (meshTransformMap.TryGetValue(this.BaseDefinition.Id, out var baseTransform))
                    {
                        transform.Concatenate(baseTransform);
                    }
                    transform.Concatenate(this.Transform);
                    NodeUtilities.AddInstanceNode(nodes, meshElementMap[this.BaseDefinition.Id], transform);
                }
            }
            else
            {
                transform.Concatenate(this.Transform);
                // Lookup the corresponding mesh in the map.
                NodeUtilities.AddInstanceNode(nodes, meshElementMap[this.BaseDefinition.Id], transform);
            }

            if (drawEdges)
            {
                // Get the edges for the solid
                var geom = this.BaseDefinition;
                if (geom.Representation != null)
                {
                    foreach (var solidOp in geom.Representation.SolidOperations)
                    {
                        if (solidOp.Solid != null)
                        {
                            foreach (var edge in solidOp.Solid.Edges.Values)
                            {
                                lines.AddRange(new[] { this.Transform.OfPoint(edge.Left.Vertex.Point), this.Transform.OfPoint(edge.Right.Vertex.Point) });
                            }
                        }
                    }
                }
            }

        }
    }
}