using System.IO;
using System.Linq;

namespace Elements
{
    public partial class ContentCatalog
    {
        // This is a very strange hack we do serializing to a model and then retrieving from a model.
        // Necessary because ContentElements have to be elements and the JsonInheritanceConverter makes
        // the pure serialization return only an Id.
        public void SaveToFile(string path)
        {
            var tempModel = new Model();
            tempModel.AddElement(this);
            var json = tempModel.ToJson();
            File.WriteAllText(path, json);
        }

        public static ContentCatalog LoadFromJson(string json)
        {
            var tempModel = Model.FromJson(json);
            var catalog = tempModel.AllElementsOfType<ContentCatalog>().FirstOrDefault();
            return catalog;
        }
    }
}