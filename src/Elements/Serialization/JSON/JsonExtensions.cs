using System.IO;
using Elements.Interfaces;
using Newtonsoft.Json;

namespace Elements.Serialization.JSON
{
    /// <summary>
    /// Extensions for JSON serialization.
    /// </summary>
    public static class JsonExtensions
    {
        /// <summary>
        /// Save a model to JSON.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="indented"></param>
        public static string ToJson(this Model model, bool indented = false)
        {
            var result = JsonConvert.SerializeObject(model, Formatting.Indented, new JsonSerializerSettings
            {
                Converters = new[] { new ModelConverter() },
                NullValueHandling = NullValueHandling.Ignore
            });
            if (indented)
            {
                return result.Replace("\n", "").Replace("\r\n", "").Replace("\t", "").Replace("  ", "");
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// Load a model from JSON.
        /// </summary>
        /// <returns>A model.</returns>
        internal static Model FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Model>(json, new JsonSerializerSettings
            {
                Converters = new[] { new ModelConverter() },
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}