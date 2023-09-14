using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Geometry.Tessellation;
using Elements.Search;
using Elements.Utilities;
using glTFLoader.Schema;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class SolidRepresentation : ElementRepresentation
    {
        /// <summary>A collection of solid operations.</summary>
        [JsonProperty("SolidOperations", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public IList<SolidOperation> SolidOperations { get; set; } = new List<SolidOperation>();

        /// <summary>
        /// Construct a representation.
        /// </summary>
        /// <param name="solidOperations">A collection of solid operations.</param>
        [JsonConstructor]
        public SolidRepresentation(IList<SolidOperation> @solidOperations)
        {
            this.SolidOperations = @solidOperations;
        }

        /// <summary>
        /// Construct a Representation from SolidOperations. This is a convenience constructor
        /// that can be used like this: `new Representation(new Extrude(...))`
        /// </summary>
        /// <param name="solidOperations">The solid operations composing this representation.</param>
        public SolidRepresentation(params SolidOperation[] solidOperations) : this(new List<SolidOperation>(solidOperations))
        {

        }

        /// <summary>
        /// Automatically convert a single solid operation into a representation containing that operation.
        /// </summary>
        /// <param name="solidOperation">The solid operation composing this Representation.</param>
        public static implicit operator SolidRepresentation(SolidOperation solidOperation)
        {
            return new SolidRepresentation(solidOperation);
        }

        /// <summary>
        /// A flag to disable CSG operations on this representation. Instead,
        /// all solids will be meshed, and all voids will be ignored.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public bool SkipCSGUnion { get; set; } = false;

        /// <summary>
        /// A function used to modify vertex attributes of the object's mesh
        /// during tesselation. Each vertex is passed to the modifier
        /// as the object is tessellated.
        /// </summary>
        [JsonIgnore]
        public Func<(Vector3 position, Vector3 normal, UV uv, Color? color), (Vector3 position, Vector3 normal, UV uv, Color? color)> ModifyVertexAttributes { get; set; }

        /// <summary>
        /// Get graphics buffers and other metadata required to modify a GLB.
        /// </summary>
        /// <param name="element">The element with this representation.</param>
        /// <param name="graphicsBuffers">The list of graphc buffers.</param>
        /// <param name="id">The buffer id. It will be used as a primitive name.</param>
        /// <param name="mode">The gltf primitive mode</param>
        /// <returns>
        /// True if there is graphicsbuffers data applicable to add, false otherwise.
        /// Out variables should be ignored if the return value is false.
        /// </returns>
        public override bool TryToGraphicsBuffers(GeometricElement element, out List<GraphicsBuffers> graphicsBuffers,
            out string id, out MeshPrimitive.ModeEnum? mode)
        {
            if (element == null)
            {
                throw new ArgumentNullException("Element is null");
            }

            id = element.Id + "_mesh";
            graphicsBuffers = new List<GraphicsBuffers>();
            mode = MeshPrimitive.ModeEnum.TRIANGLES;

            var csg = SolidOperationUtils.GetFinalCsgFromSolids(SolidOperations, element, false);

            if (csg == null)
            {
                return false;
            }

            GraphicsBuffers buffers = null;
            if (SkipCSGUnion)
            {
                // There's a special flag on Representation that allows you to
                // skip CSG unions. In this case, we tessellate all solids
                // individually, and do no booleaning. Voids are also ignored.

                // We create a collection of SolidTesselationTargetProviders, one for each solid operation.
                // Each SolidTesselationTargetProvider has a GetTessellationTargets method which returns a new SolidFaceTessAdapter.
                // Each SolidFaceTessAdapter is responsible for the tesselation of a single face.
                // The SolidFaceTessAdapter's GetTess method calls face.ToContourVertexArray.
                // ToContourVertexArray attaches a faceId and a solidIdto the Data object we hang on the contour vertex.
                // The faceId and solidId are used during packing to lookup existing shared vertices, to avoid recreating them.
                uint solidId = 0;
                var providers = new List<SolidTesselationTargetProvider>();
                foreach (var so in SolidOperations)
                {
                    providers.Add(new SolidTesselationTargetProvider(so.Solid, solidId, so.LocalTransform));
                    solidId++;
                }
                buffers = Tessellation.Tessellate<GraphicsBuffers>(providers, false, ModifyVertexAttributes);
            }
            else
            {
                buffers = csg.Tessellate(modifyVertexAttributes: ModifyVertexAttributes);
            }

            if (buffers.Vertices.Count == 0)
            {
                return false;
            }

            graphicsBuffers.Add(buffers);
            return true;
        }
    }
}