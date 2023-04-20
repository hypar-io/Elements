using System;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// A Brace is a structural framing element which is often diagonal.
    /// </summary>
    public class Brace : StructuralFraming
    {
        /// <summary>
        /// Construct a Brace.
        /// </summary>
        /// <param name="curve">The brace's center line.</param>
        /// <param name="profile">The brace's profile.</param>
        /// <param name="material">The brace's material.</param>
        /// <param name="startSetback">The setback of the brace's geometry at the start.</param>
        /// <param name="endSetback">The setback of the brace's geometry at the end.</param>
        /// <param name="rotation">An optional rotation of the beam's profile around its axis.</param>
        /// <param name="transform">The brace's transform.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The brace's id.</param>
        /// <param name="name">The brace's name.</param>
        public Brace(BoundedCurve curve,
                     Profile profile,
                     Material material = null,
                     double startSetback = 0.0,
                     double endSetback = 0.0,
                     double rotation = 0.0,
                     Transform transform = null,
                     bool isElementDefinition = false,
                     Guid id = default,
                     string name = null)
            : base(curve,
                   profile,
                   material,
                   startSetback,
                   endSetback,
                   rotation,
                   transform,
                   null,
                   isElementDefinition,
                   id,
                   name)
        { }
    }
}