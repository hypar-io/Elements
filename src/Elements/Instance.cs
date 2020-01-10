using System;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// An instance of a geometric element.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Elements.Tests/Examples/InstanceExample.cs?name=example)]
    /// </example>
    [UserElement]
    public class Instance : Element
    {
        /// <summary>
        /// The geometric element of which this is an instance.
        /// </summary>
        public GeometricElement Parent { get; set; }

        /// <summary>
        /// The instance transform.
        /// </summary>
        public Transform Transform { get; set; }

        /// <summary>
        /// Construct an instance
        /// </summary>
        /// <param name="parent">The geomtric element from which to create this instance.</param>
        /// <param name="transform">The transform.</param>
        /// <param name="guid">The identifier of this instance.</param>
        /// <param name="name">The name of this instance.</param>
        public Instance(GeometricElement parent,
                        Transform transform = null,
                        Guid guid = default(Guid),
                        string name = null) : base(guid != default(Guid) ? guid : Guid.NewGuid(), name)
        {
            this.Parent = parent;
            this.Transform = transform == null ? new Transform() : transform;
        }
    }
}