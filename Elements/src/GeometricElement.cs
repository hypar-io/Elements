using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Validators;

namespace Elements
{
    public partial class GeometricElement
    {
        /// <summary>The element's material.</summary>
        [Newtonsoft.Json.JsonProperty("Material", Required = Newtonsoft.Json.Required.AllowNull)]
        [Obsolete("The material is now stored on the representation.")]
        public Material Material { get; set; }

        /// <summary>The element's representation.</summary>
        [Newtonsoft.Json.JsonProperty("Representation", Required = Newtonsoft.Json.Required.AllowNull)]
        [Obsolete("The Representations property should now be used to store a list of representations.")]
        public Representation Representation { get; set; }

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
        /// Construct a geometric element with a solid representation.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="material"></param>
        /// <param name="representation"></param>
        /// <param name="isElementDefinition"></param>
        /// <param name="id"></param>
        /// <param name="name"></param>
        [Obsolete("Geometric elements should now be constructed using the constructor which requires a list of representations.")]
        public GeometricElement(Transform @transform, Material @material, Representation @representation, bool @isElementDefinition, System.Guid @id, string @name)
            : base(id, name)
        {
            var validator = Validator.Instance.GetFirstValidatorForType<GeometricElement>();
            if (validator != null)
            {
                validator.PreConstruct(new object[] { @transform, @material, @representation, @isElementDefinition, @id, @name });
            }

            this.Transform = @transform;
            this.Material = @material;
            this.Representation = @representation;
            this.IsElementDefinition = @isElementDefinition;

            if (validator != null)
            {
                validator.PostConstruct(this);
            }
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