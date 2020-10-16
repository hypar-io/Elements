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
            CatalogGenerator.FromUri(testPath, "../../../");
        }
    }
}