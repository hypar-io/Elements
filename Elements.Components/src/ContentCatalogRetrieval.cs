using System;
using System.Collections.Generic;
using System.IO;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Components
{
    /// <summary>
    /// Retrieve a content catalog from a `catalog.json` file stored in the build directory.
    /// </summary>
    public static class ContentCatalogRetrieval
    {
        private static ContentCatalog catalog = null;
        
        /// <summary>
        /// Get the ContentCatalog stored in `catalog.json`.
        /// </summary>
        /// <returns></returns>
        public static ContentCatalog GetCatalog()
        {
            if (catalog == null)
            {
                if (!File.Exists("./catalog.json"))
                {
                    throw new Exception("Catalog not found. To use this class, make sure you have a content catalog stored at catalog.json in the build directory.");
                }
                var json = File.ReadAllText("./catalog.json");
                catalog = JsonConvert.DeserializeObject<ContentCatalog>(json);
                // Mutate IDs, otherwise we run into a bug with the export server, collisions with shared catalogs, big messy mess.
                foreach (var element in catalog.Content)
                {
                    element.Id = Guid.NewGuid();
                }
            }
            return catalog;
        }
    }
}