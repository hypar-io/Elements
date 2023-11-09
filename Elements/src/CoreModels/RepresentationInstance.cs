using System.Collections.Generic;
using Elements.Interfaces;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// Provides information about element representation, including material and representation types
    /// </summary>
    public class RepresentationInstance
    {
        /// <summary>
        /// Initializes a new instance of RepresentationInstance class.
        /// </summary>
        /// <param name="representation">The element representation.</param>
        /// <param name="material">The material applied to the element representation.</param>
        /// <param name="isDefault">Indicates if this representation is default.</param>
        public RepresentationInstance(ElementRepresentation representation, Material material, bool isDefault = true) :
            this(representation, material, isDefault, "")
        {
        }

        /// <summary>
        /// Initializes a new instance of RepresentationInstance class.
        /// </summary>
        /// <param name="representation">The element representation.</param>
        /// <param name="material">The material applied to the element representation.</param>
        /// <param name="isDefault">Indicates if this representation is default.</param>
        /// <param name="representationTypes">The set of representation type names that can be used by view or by other parts of the sysetem to identify
        /// if this element representation is suitable for display.</param>
        public RepresentationInstance(ElementRepresentation representation, Material material,
                 bool isDefault = true, params string[] representationTypes)
        {
            Representation = representation;
            Material = material;
            IsDefault = isDefault;
            foreach (var type in representationTypes)
            {
                RepresentationTypes.Add(type);
            }
        }

        /// <summary>
        /// The representation's material.
        /// </summary>
        [JsonProperty("Material", Required = Required.AllowNull)]
        public Material Material { get; set; }

        /// <summary>
        /// The element representation.
        /// </summary>
        public ElementRepresentation Representation { get; set; }

        /// <summary>
        /// The set of representation type names that can be used by view or by other parts of the sysetem to identify
        /// if this element representation is suitable for display.
        /// </summary>
        public List<string> RepresentationTypes { get; set; } = new List<string>();

        /// <summary>
        /// Indicates if this element representation instance is displayed by default.
        /// Element can have several default representations.
        /// </summary>
        public bool IsDefault { get; set; } = true;

        /// <summary>
        /// Calculates representation instance hash code including material and element opening information.
        /// This hash code can be used to compare two representation types by Representation Id, Material Id and element openings.
        /// </summary>
        /// <param name="element">The element that owns this representation instance.</param>
        public int GetHashCode(GeometricElement element)
        {
            int hash = 17;
            hash = hash * 31 + Representation.Id.GetHashCode();
            hash = hash * 31 + Material.Id.GetHashCode();
            if (element is IHasOpenings openingContainer)
            {
                foreach (var opening in openingContainer.Openings)
                {
                    hash = hash * 31 + opening.Id.GetHashCode();
                }
            }
            return hash;
        }
    }
}