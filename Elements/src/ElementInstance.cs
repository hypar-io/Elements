using System;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// An instance of an element in the model.
    /// Instances point to one instance of a type, but have
    /// individual ids and transforms.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/ElementInstanceTests.cs?name=example)]
    /// </example>
    public class ElementInstance : Element
    {
        /// <summary>
        /// The element from which this instance is derived.
        /// </summary>
        public GeometricElement BaseDefinition { get; }

        /// <summary>
        /// The transform of the instance.
        /// </summary>
        public Transform Transform { get; }

        /// <summary>
        /// Construct an element instance.
        /// </summary>
        /// <param name="baseDefinition">The definition from which this instance is derived.</param>
        /// <param name="transform">The transform of the instance.</param>
        /// <param name="name">The name of the instance.</param>
        /// <param name="id">The id of the instance.</param>
        [JsonConstructor]
        internal ElementInstance(GeometricElement baseDefinition,
                               Transform transform,
                               string name = null,
                               Guid id = default(Guid)) : base(id == default(Guid) ? Guid.NewGuid() : id, name)
        {
            this.BaseDefinition = baseDefinition;
            this.Transform = transform;
        }
    }
}