using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Interfaces;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// An element with a geometric representation.
    /// </summary>
    [JsonConverter(typeof(Elements.Serialization.JSON.JsonInheritanceConverter), "discriminator")]
    public class GeometricElement : Element
    {
        /// <summary>The element's transform.</summary>
        [JsonProperty("Transform", Required = Required.AllowNull)]
        public Transform Transform { get; set; }

        /// <summary>The element's material.</summary>
        [JsonProperty("Material", Required = Required.AllowNull)]
        public Material Material { get; set; }

        /// <summary>The element's representation.</summary>
        [JsonProperty("Representation", Required = Required.AllowNull)]
        public Representation Representation { get; set; }

        /// <summary>When true, this element will act as the base definition for element instances, and will not appear in visual output.</summary>
        [JsonProperty("IsElementDefinition", Required = Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
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
                                                      .Select(op => TransformedSolidOperation(op, o.Transform))))
                                                      .ToArray();
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
                csg = csg.Subtract(voids);
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

        internal Csg.Solid[] GetCsgSolids(bool transformed = false)
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

        private Csg.Solid TransformedSolidOperation(Geometry.Solids.SolidOperation op, Transform addTransform = null)
        {
            if (Transform == null)
            {
                return op._solid.ToCsg();
            }

            // Transform the solid operatioon by the the local transform AND the
            // element's transform, or just by the element's transform.
            var transformedOp = op.LocalTransform != null
                        ? op._solid.ToCsg().Transform(Transform.Concatenated(op.LocalTransform).ToMatrix4x4())
                        : op._solid.ToCsg().Transform(Transform.ToMatrix4x4());
            if (addTransform == null)
            {
                return transformedOp;
            }

            // If an addition transform was proovided, don't forget
            // to apply that as well.
            return transformedOp.Transform(addTransform.ToMatrix4x4());
        }

        /// <summary>
        /// Get graphics buffers and other metadata required to modify a GLB.
        /// </summary>
        /// <returns>
        /// True if there is graphicsbuffers data applicable to add, false otherwise.
        /// Out variables should be ignored if the return value is false.
        /// </returns>
        public virtual Boolean TryToGraphicsBuffers(out List<GraphicsBuffers> graphicsBuffers, out string id, out glTFLoader.Schema.MeshPrimitive.ModeEnum? mode)
        {
            id = null;
            mode = null;
            graphicsBuffers = new List<GraphicsBuffers>(); // this is intended to be discarded
            return false;
        }
    }
}