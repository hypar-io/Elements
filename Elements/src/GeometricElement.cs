using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;

namespace Elements
{
    public partial class GeometricElement
    {
        /// <summary>
        /// The geometric element's representation.
        /// </summary>
        [Obsolete("Geometric elements can now store multiple representations. Use Representations instead.")]
        public SolidRepresentation Representation
        {
            // Geometric elements previously only stored one type of representation,
            // a type akin to a SolidRepresentation. We serialize a solid representation,
            // if one exists, in order to make JSON created from this version
            // compatible with previous versions. If the incoming JSON contains
            // a representation we do similarly, setting it to the first representation.
            get
            {
                return this.FirstRepresentationOfType<SolidRepresentation>();
            }
        }

        /// <summary>
        /// Create a geometric element with a transform and a representations.
        /// </summary>
        /// <param name="transform">The element's transform.</param>
        /// <param name="representation">The element's representation.</param>
        /// <returns></returns>
        public GeometricElement(Representation representation, Transform transform = null) : base(Guid.NewGuid(), null)
        {
            this.Transform = transform != null ? transform : new Transform();
            this.Representations.Add(representation);
        }

        /// <summary>
        /// This method provides an opportunity for geometric elements
        /// to update their representations.
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
        /// Get the first representation of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the representation.</typeparam>
        /// <returns>A representation or null if no representations of the specified type exist.</returns>
        public T FirstRepresentationOfType<T>()
        {
            if (this.Representations == null)
            {
                return default(T);
            }

            var reps = this.Representations.OfType<T>();
            return reps.Count() > 0 ? (T)reps.First() : default(T);
        }

        /// <summary>
        /// Get all representations of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the representation.</typeparam>
        /// <returns>A collection of representations</returns>
        public List<T> AllRepresentationsOfType<T>()
        {
            if (this.Representations == null)
            {
                return null;
            }
            return this.Representations.OfType<T>().ToList();
        }
    }
}