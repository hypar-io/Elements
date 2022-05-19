using System.Collections.Generic;
using System.Linq;
using Elements.Serialization.JSON;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elements
{
    /// <summary>
    /// A collection of content elements.
    /// </summary>
    [JsonConverter(typeof(Elements.Serialization.JSON.JsonInheritanceConverter), "discriminator")]
    public class ContentCatalog : Element
    {
        /// <summary>The content elements in this catalog.</summary>
        [JsonProperty("Content", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public IList<ContentElement> Content { get; set; } = new List<ContentElement>();

        /// <summary>An example arrangement of the elements contained in this catalog.</summary>
        [JsonProperty("ReferenceConfiguration", Required = Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public IList<Element> ReferenceConfiguration { get; set; }

        /// <summary>
        /// Construct a content catalog.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="referenceConfiguration"></param>
        /// <param name="id"></param>
        /// <param name="name"></param>
        [JsonConstructor]
        public ContentCatalog(IList<ContentElement> @content, IList<Element> @referenceConfiguration, System.Guid @id = default, string @name = null)
            : base(id, name)
        {
            this.Content = @content;
            this.ReferenceConfiguration = @referenceConfiguration;
        }

        /// <summary>
        /// Convert the ContentCatalog into its JSON representation.
        /// </summary>
        public string ToJson()
        {
            JsonInheritanceConverter.ElementwiseSerialization = true;
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            JsonInheritanceConverter.ElementwiseSerialization = false;
            return json;
        }

        /// <summary>
        /// Deserialize the give JSON text into the ContentCatalog
        /// </summary>
        /// <param name="json"></param>
        public static ContentCatalog FromJson(string json)
        {
            var catalogObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(json);
            if (catalogObject.TryGetValue("discriminator", out _))
            {
                return catalogObject.ToObject<ContentCatalog>();
            }
            else if (catalogObject.TryGetValue("Elements", out _) && catalogObject.TryGetValue("Transform", out _)) // catalog is stored in a model
            {
                var model = Model.FromJson(json);
                return model.AllElementsOfType<ContentCatalog>().First();
            }

            return null;
        }

        /// <summary>
        /// Modifies the transforms of the content internal to this catalog to use
        /// the orientation of the reference instances that exist.
        /// </summary>
        public void UseReferenceOrientation()
        {
            if (ReferenceConfiguration == null)
            {
                return;
            }

            foreach (var content in Content)
            {
                var refInstance = ReferenceConfiguration.FirstOrDefault(r => ((ElementInstance)r).BaseDefinition.Id == content.Id) as ElementInstance;
                if (refInstance == null)
                {
                    continue;
                }
                // Use reference instance to set the rotation, but not the position of the original elements.
                var referenceOrientation = refInstance.Transform.Concatenated(new Geometry.Transform(refInstance.Transform.Origin.Negate()));
                content.Transform = referenceOrientation;
            }
        }
    }
}