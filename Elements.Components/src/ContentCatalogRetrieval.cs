using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Elements.Geometry;
using System.Text.Json.Serialization;

namespace Elements.Components
{
    /// <summary>
    /// Retrieve a content catalog from a `catalog.json` file stored in the build directory.
    /// </summary>
    public static class ContentCatalogRetrieval
    {
        private static ContentCatalog catalog = null;

        private static string catalogFilePath = null;

        /// <summary>
        /// If you're using a different location for the catalog file, you can 
        /// set it here.
        /// </summary>
        /// <param name="path"></param>
        public static void SetCatalogFilePath(string path)
        {
            catalogFilePath = path;
        }

        /// <summary>
        /// Get the ContentCatalog stored in `catalog.json`.
        /// </summary>
        /// <returns></returns>
        public static ContentCatalog GetCatalog()
        {
            if (catalog == null)
            {
                var catalogPath = catalogFilePath ?? Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "catalog.json");
                if (!File.Exists(catalogPath))
                {
                    throw new Exception("Catalog not found. To use this class, make sure you have a content catalog stored at catalog.json in the build directory.");
                }
                var json = File.ReadAllText(catalogPath);
                catalog = ContentCatalog.FromJson(json);
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