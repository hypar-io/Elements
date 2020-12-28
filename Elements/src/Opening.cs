using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A polygonal opening.
    /// An opening's placement is defined by the x and y coordinates.
    /// The direction of the opening corresponds to the +Z axis of the transform.
    /// </summary>
    [UserElement]
    public class Opening : GeometricElement
    {
        /// <summary>
        /// The profile of the opening.
        /// </summary>
        [Obsolete("Use perimeter instead.")]
        public Profile Profile { get; set; }

        /// <summary>
        /// The perimeter of the opening.
        /// </summary>
        public Polygon Perimeter { get; set; }

        /// <summary>
        /// The depth of the opening along the opening's +Z axis.
        /// </summary>
        public double DepthFront { get; set; }

        /// <summary>
        /// The depth of the opening along the opening's -Z axis.
        /// </summary>
        [Obsolete("Describe the full depth of the opening using DepthFront.")]
        public double DepthBack { get; set; }

        /// <summary>
        /// Create an opening.
        /// </summary>
        [JsonConstructor]
        public Opening(Polygon perimeter,
                       double depthFront = 1.0,
                       Transform transform = null,
                       IList<Representation> representations = null,
                       bool isElementDefinition = false,
                       Guid id = default(Guid),
                       string name = null) : base(transform != null ? transform : new Transform(),
                                                  representations != null ? representations : new[] { new SolidRepresentation(BuiltInMaterials.Void) },
                                                  isElementDefinition,
                                                  id != default(Guid) ? id : Guid.NewGuid(),
                                                  name)
        {
            this.Perimeter = perimeter;
            this.DepthFront = depthFront;

            var rep = this.FirstRepresentationOfType<SolidRepresentation>();
            rep.SolidOperations.Add(new Extrude(this.Perimeter, this.DepthFront, Vector3.ZAxis, true));
        }

        /// <summary>
        /// Update representations
        /// </summary>
        public override void UpdateRepresentations()
        {
            var rep = this.FirstRepresentationOfType<SolidRepresentation>();
            var extrude = (Extrude)rep.SolidOperations[0];
            extrude.Profile = this.Perimeter;
            extrude.Height = this.DepthFront;
        }
    }
}