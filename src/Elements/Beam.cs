using System;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// A structural framing element defined by a center line curve and a profile.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Elements.Tests/Examples/BeamExample.cs?name=example)]
    /// </example>
    [UserElement]
    public class Beam : StructuralFraming
    {
        /// <summary>
        /// Construct a beam.
        /// </summary>
        /// <param name="curve">The beam's center line.</param>
        /// <param name="profile">The beam's profile.</param>
        /// <param name="material">The beam's material.</param>
        /// <param name="startSetback">The setback of the beam's geometry at the start.</param>
        /// <param name="endSetback">The setback of the beam's geometry at the end.</param>
        /// <param name="rotation">An optional rotation of the beam's cross section around it's axis.</param>
        /// <param name="transform">The beam's transform.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the transform.</param>
        /// <param name="name">The name of the transform.</param>
        public Beam(Curve curve,
                    Profile profile,
                    Material material = null,
                    double startSetback = 0.0,
                    double endSetback = 0.0,
                    double rotation = 0.0,
                    Transform transform = null,
                    bool isElementDefinition = false,
                    Guid id = default(Guid),
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
                   name) { }
    }
}