using System;
using System.Linq;
using Elements.Geometry;
using Elements.Interfaces;

namespace Elements
{
    public partial class GeometricElement
    {
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
        /// Get the complete final mesh of this geometric element.  This mesh is centered about the origin, and meant to be transformed
        /// into the final location using the Geometric Element's Transform property.
        /// </summary>
        public Mesh ToMesh()
        {
            if (this.Representation == null || this.Representation.SolidOperations.Count == 0)
            {
                this.UpdateRepresentations();
            }
            var mesh = new Mesh();
            var solid = this.GetSolid();
            solid.Tessellate(ref mesh);
            return mesh;
        }

        /// <summary>
        /// Get the computed csg solid centered about the origin.
        /// </summary>
        internal Csg.Solid GetSolid()
        {
            // To properly compute csgs, all solid operation csgs need
            // to be transformed into their final position. Then the csgs
            // can be computed and the final csg can have the inverse of the
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

            if (this is IHasOpenings)
            {
                var openingContainer = (IHasOpenings)this;
                voids = voids.Concat(openingContainer.Openings.SelectMany(o => o.Representation.SolidOperations
                                                      .Where(op => op.IsVoid == true)
                                                      .Select(op => op._csg.Transform(o.Transform.ToMatrix4x4())))).ToArray();
            }

            csg = csg.Union(solids);
            csg = csg.Substract(voids);

            if (Transform == null)
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

        private Csg.Solid TransformedSolidOperation(Geometry.Solids.SolidOperation op)
        {
            if (Transform == null)
            {
                return op._csg;
            }
            return op.LocalTransform != null
                        ? op._csg.Transform(Transform.Concatenated(op.LocalTransform).ToMatrix4x4())
                        : op._csg.Transform(Transform.ToMatrix4x4());
        }
    }
}