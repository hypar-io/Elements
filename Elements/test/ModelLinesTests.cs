using System.Collections.Generic;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class ModelLinesTests : ModelTest
    {
        public ModelLinesTests()
        {
            this.GenerateIfc = false;
        }

        [Fact, Trait("Category", "Examples")]
        public void ModelLines()
        {
            this.Name = "Elements_ModelLines";

            // <example>
            var lines = new List<Line>()
            {
                new Line(new Vector3(0, 0), new Vector3(0, 5)),
                new Line(new Vector3(0, 0), new Vector3(5, 0)),
                new Line(new Vector3(0, 5), new Vector3(5, 5)),
                new Line(new Vector3(5, 0), new Vector3(5, 5)),
                new Line(new Vector3(0, 0), new Vector3(5, 5))
            };

            var modelLines = new ModelLines(lines, new Material("Yellow", Colors.Yellow));
            // </example>

            this.Model.AddElement(modelLines);
        }
    }
}
