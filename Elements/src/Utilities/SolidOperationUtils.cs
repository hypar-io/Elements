using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Interfaces;

namespace Elements.Utilities
{
    /// <summary>
    /// The helpful methods for work with SolidOperations.
    /// </summary>
    internal static class SolidOperationUtils
    {
        /// <summary>
        /// Get the computed csg solid.
        /// </summary>
        /// <param name="solidOperation">The solid operation.</param>
        /// <param name="element">The element that contains this solid operation.</param>
        /// <param name="addTransform">The additional transform to apply.</param>
        /// <param name="transformed">Indicates if the element transformation must be applied to the solid.</param>
        public static Csg.Solid TransformedSolidOperation(SolidOperation solidOperation, GeometricElement element,
            Transform addTransform = null, bool transformed = false)
        {
            // TODO: call this code from ElementRepresentation? or Element itself
            if (element.Transform == null)
            {
                return solidOperation._solid.ToCsg();
            }

            var transform = transformed ? element.Transform : new Transform();

            // Transform the solid operatioon by the the local transform AND the
            // element's transform, or just by the element's transform.
            var transformedOp = solidOperation.LocalTransform != null
                        ? solidOperation._solid.ToCsg().Transform(transform.Concatenated(solidOperation.LocalTransform).ToMatrix4x4())
                        : solidOperation._solid.ToCsg().Transform(transform.ToMatrix4x4());
            if (addTransform == null)
            {
                return transformedOp;
            }

            // If an addition transform was proovided, don't forget
            // to apply that as well.
            return transformedOp.Transform(addTransform.ToMatrix4x4());
        }

        /// <summary>
        /// Get the computed csg solid.
        /// The csg is centered on the origin by default.
        /// </summary>
        /// <param name="solidOperations">The list of solid operations.</param>
        /// <param name="element">The element that contains this solid operation.</param>
        /// <param name="transformed">Indicates if the element transformation must be applied to the solid.</param>
        public static Csg.Solid GetFinalCsgFromSolids(IList<SolidOperation> solidOperations,
            GeometricElement element, bool transformed = false)
        {
            if (solidOperations.Count == 0)
            {
                return null;
            }

            // To properly compute csgs, all solid operation csgs need
            // to be transformed into their final position. Then the csgs
            // can be computed and by default the final csg will have the inverse of the
            // geometric element's transform applied to "reset" it.
            // The transforms applied to each node in the glTF will then
            // ensure that the elements are correctly transformed.
            Csg.Solid csg = new Csg.Solid();

            var solids = new List<Csg.Solid>();
            var voids = new List<Csg.Solid>();

            foreach (var op in solidOperations)
            {
                if (op.IsVoid)
                {
                    voids.Add(TransformedSolidOperation(op, element));
                }
                else
                {
                    solids.Add(TransformedSolidOperation(op, element));
                }
            }

            if (element is IHasOpenings openingContainer)
            {
                foreach (var opening in openingContainer.Openings)
                {
                    foreach (var op in opening.Representation.SolidOperations)
                    {
                        if (op.IsVoid)
                        {
                            voids.Add(TransformedSolidOperation(op, element, opening.Transform));
                        }
                    }
                }
            }

            var solidItems = solids.ToArray();
            var voidItems = voids.ToArray();

            // Don't try CSG booleans if we only have one one solid.
            if (solids.Count() == 1)
            {
                csg = solids.First();
            }
            else if (solids.Count() > 0)
            {
                csg = csg.Union(solidItems);
            }


            if (voids.Count() > 0)
            {
                csg = csg.Subtract(voidItems);
            }

            if (element.Transform == null || !transformed)
            {
                return csg;
            }
            else
            {
                csg = csg.Transform(element.Transform.ToMatrix4x4());
                return csg;
            }
        }
    }
}