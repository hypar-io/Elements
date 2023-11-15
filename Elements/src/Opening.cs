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
        public double DepthBack { get; set; }

        /// <summary>
        /// The normal direction of the opening.
        /// </summary>
        public Vector3 Normal { get; set; }

        /// <summary>
        /// Create an opening normal to the ZAxis.
        /// </summary>
        public Opening(Polygon perimeter,
                       double depthFront = 1.0,
                       double depthBack = 1.0,
                       Transform transform = null,
                       Representation representation = null,
                       bool isElementDefinition = false,
                       Guid id = default,
                       string name = null) : this(perimeter, Vector3.ZAxis, depthFront, depthBack, transform, representation, isElementDefinition, id, name) { }

        /// <summary>
        /// Create an opening.
        /// </summary>
        [JsonConstructor]
        public Opening(Polygon perimeter,
                       Vector3 normal,
                       double depthFront = 1.0,
                       double depthBack = 1.0,
                       Transform transform = null,
                       Representation representation = null,
                       bool isElementDefinition = false,
                       Guid id = default,
                       string name = null) : base(transform ?? new Transform(),
                                                  BuiltInMaterials.Void,
                                                  representation ?? new Representation(new List<SolidOperation>()),
                                                  isElementDefinition,
                                                  id != default ? id : Guid.NewGuid(),
                                                  name)
        {
            if (normal == default)
            {
                Normal = Vector3.ZAxis; // legacy openings don't have a normal and assume normal is the Z axis
            }
            else
            {
                Normal = normal.Unitized();
            }
            this.Perimeter = perimeter;
            this.DepthBack = depthBack;
            this.DepthFront = depthFront;
        }

        /// <summary>
        /// Update representations
        /// </summary>
        public override void UpdateRepresentations()
        {
            this.Representation.SolidOperations.Clear();
            var depth = this.DepthFront + this.DepthBack;
            var op = new Extrude(this.Perimeter, depth, Normal, true);
            this.Representation.SolidOperations.Add(op);
        }
    }
}