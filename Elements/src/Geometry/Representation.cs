using System;
using System.Collections.Generic;
using Elements.Geometry.Solids;
using Elements.Validators;

namespace Elements.Geometry
{
    public partial class Representation
    {
        /// <summary>A collection of solid operations.</summary>
        [Newtonsoft.Json.JsonProperty("SolidOperations", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [Obsolete("Use SolidRepresentation instead.")]
        public IList<SolidOperation> SolidOperations { get; set; } = new List<SolidOperation>();

        /// This constructor is here to support deserialization
        /// of representations with a collection of solid operations.
        /// <summary>
        /// Construct a representation.
        /// </summary>
        /// <param name="solidOperations"></param>
        [Obsolete("Use SolidRepresentation instead.")]
        public Representation(IList<SolidOperation> @solidOperations) : base(Guid.NewGuid(), null)
        {
            var validator = Validator.Instance.GetFirstValidatorForType<Representation>();
            if (validator != null)
            {
                validator.PreConstruct(new object[] { @solidOperations });
            }

            this.SolidOperations = @solidOperations;

            if (validator != null)
            {
                validator.PostConstruct(this);
            }
        }

        /// <summary>
        /// Create a representation with a default id.
        /// </summary>
        public Representation(Material material) : base(Guid.NewGuid(), null)
        {
            this.Material = material;
        }
    }
}