using System.Collections.Generic;
using System.Linq;
using Elements.Serialization.JSON;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Elements
{
    /// <summary>
    /// A collection of content elements.
    /// </summary>
    public class ContentCatalog : Element
    {
        /// <summary>The content elements in this catalog.</summary>
        [System.ComponentModel.DataAnnotations.Required]
        public IList<ContentElement> Content { get; set; } = new List<ContentElement>();

        /// <summary>An example arrangement of the elements contained in this catalog.</summary>
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
            var serializerOptions = new JsonSerializerOptions();
            serializerOptions.Converters.Add(new ElementConverterFactory(true));
            return JsonSerializer.Serialize(this, serializerOptions);
        }

        /// <summary>
        /// Deserialize the give JSON text into the ContentCatalog
        /// </summary>
        /// <param name="json"></param>
        public static ContentCatalog FromJson(string json)
        {
            using (var doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;
                if (root.TryGetProperty("discriminator", out _))
                {
                    var options = new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true,
                    };
                    var typeCache = AppDomainTypeCache.BuildAppDomainTypeCache(out _);
                    var refHandler = new ElementReferenceHandler(typeCache, root);
                    options.ReferenceHandler = refHandler;
                    return root.Deserialize<ContentCatalog>(options);
                }
                else if (root.TryGetProperty("Elements", out _) && root.TryGetProperty("Transform", out _))
                {
                    var model = Model.FromJson(json);
                    return model.AllElementsOfType<ContentCatalog>().First();
                }
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