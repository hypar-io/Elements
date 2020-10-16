using Elements.Generate;
using Xunit;

namespace Elements.Test
{
    public class CatalogGeneratorTests
    {
        [Fact]
        public void GenerateCatalogTest()
        {
            // var testPath = "../../../TestData/CatalogFromRevit.json";
            var testPath = "https://hypar-content-catalogs.s3-us-west-2.amazonaws.com/69402ad0-64fd-4325-ab5d-157daab5b428/Chairs.json";
            CatalogGenerator.FromUri(testPath, "../../../");
        }
    }
}