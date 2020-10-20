using System.IO;
using System.Linq;
using Elements.Serialization.JSON;

namespace Elements
{
    public partial class ContentCatalog
    {
        /// <summary>
        /// Convert the ContentCatalog into it's JSON representation.
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
        /// <returns></returns>
        public static ContentCatalog FromJson(string json)
        {
            var catalog = Newtonsoft.Json.JsonConvert.DeserializeObject<ContentCatalog>(json);
            return catalog;
        }
    }
}