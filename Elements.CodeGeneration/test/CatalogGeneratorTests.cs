using System.IO;
using Elements.Generate;
using Xunit;

namespace Elements.Test
{
    public class CatalogGeneratorTests
    {
        [Fact]
        public void GenerateCatalogTest()
        {
            var testPath = "../../../TestData/CatalogFromRevit.json";
            CatalogGenerator.FromUri(testPath, "../");
            Assert.True(File.Exists("../SampleCatalog.g.cs"));
            Assert.Contains("public static List<ContentElement> All = new List<ContentElement>", File.ReadAllText("../SampleCatalog.g.cs"));
        }
    }
}