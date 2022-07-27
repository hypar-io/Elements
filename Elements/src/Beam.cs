using System;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// A structural framing element defined by a center line curve and a profile.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/StructuralFramingTests.cs?name=example)]
    /// </example>
    public class Beam : StructuralFraming
    {
        /// <summary>
        /// Construct a beam.
        /// </summary>
        /// <param name="curve">The beam's center line.</param>
        /// <param name="profile">The beam's profile.</param>
        /// <param name="transform">The beam's transform.</param>
        /// <param name="material">The beam's material.</param>
        /// <param name="representation">The beam's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the transform.</param>
        /// <param name="name">The name of the transform.</param>
        public Beam(Curve curve,
                    Profile profile,
                    Transform transform = null,
                    Material material = null,
                    Representation representation = null,
                    bool isElementDefinition = false,
                    Guid id = default,
                    string name = null) : base(curve, profile, material, 0, 0, 0, transform, representation, isElementDefinition, id, name)
        { }

        /// <summary>
        /// Construct a beam.
        /// </summary>
        /// <param name="curve">The beam's center line.</param>
        /// <param name="profile">The beam's profile.</param>
        /// <param name="startSetback">The setback of the beam's geometry at the start.</param>
        /// <param name="endSetback">The setback of the beam's geometry at the end.</param>
        /// <param name="rotation">An optional rotation of the beam's cross section around it's axis.</param>
        /// <param name="transform">The beam's transform.</param>
        /// <param name="material">The beam's material.</param>
        /// <param name="representation">The beam's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the transform.</param>
        /// <param name="name">The name of the transform.</param>
        public Beam(Curve curve,
                    Profile profile,
                    double startSetback,
                    double endSetback,
                    double rotation,
                    Transform transform = null,
                    Material material = null,
                    Representation representation = null,
                    bool isElementDefinition = false,
                    Guid id = default,
                    string name = null) : base(curve, profile, material, startSetback, endSetback, rotation, transform, representation, isElementDefinition, id, name)
        { }

        /// <summary>
        /// Construct a beam.
        /// </summary>
        public Beam() { }
    }
}