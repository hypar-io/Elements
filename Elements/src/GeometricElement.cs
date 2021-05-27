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
        /// Get the mesh representing the this Element's geometry. By default it will be untransformed.
        /// </summary>
        /// <param name="transform">Wether or not the mesh should be transformed into its final location.</param>
        public Mesh ToMesh(bool transform = false)
        {
            if (HasNoRepresentation())
            {
                this.UpdateRepresentations();
                if (HasNoRepresentation())
                {
                    throw new ArgumentNullException("This geometric element has no representation, no geometry, and cannot be turned into a mesh.");
                }
            }
            var mesh = new Mesh();
            var solid = GetFinalCsgFromSolids(transform);
            solid.Tessellate(ref mesh);
            return mesh;
        }

        /// <summary>
        /// Does this geometric element have geometry?
        /// <summary>
        public bool HasNoRepresentation()
        {
            return Representation == null || Representation.SolidOperations == null || Representation.SolidOperations.Count == 0;
        }

        /// <summary>
        /// Get the computed csg solid centered about the origin.
        /// </summary>
        /// <param name="transformed">Wether or not to get the solid transformed to it's final location.</param>
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

            if (this is IHasOpenings)
            {
                var openingContainer = (IHasOpenings)this;
                voids = voids.Concat(openingContainer.Openings.SelectMany(o => o.Representation.SolidOperations
                                                      .Where(op => op.IsVoid == true)
                                                      .Select(op => op._csg.Transform(o.Transform.ToMatrix4x4())))).ToArray();
            }

            csg = csg.Union(solids);
            csg = csg.Substract(voids);

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