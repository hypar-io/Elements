using System.Collections.Generic;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests.Examples
{
    public class GridLinesTests : ModelTest
    {
        public GridLinesTests()
        {
            this.GenerateIfc = false;
        }

        [Fact, Trait("Category", "Examples")]
        public void Example()
        {
            Name = "Elements_GridLine";

            // <example>
            var gridData = new List<(string name, Vector3 origin)>() {
                ("A", new Vector3()),
                ("B", new Vector3(10, 0, 0)),
                ("C", new Vector3(20, 0, 0)),
                ("D", new Vector3(30, 0, 0)),
            };

            var texts = new List<(Vector3 location, Vector3 facingDirection, Vector3 lineDirection, string text, Color? color)>();
            var radius = 1;
            var material = new Material("Red", Colors.Red);

            foreach (var (name, origin) in gridData)
            {
                var gridline = new GridLine
                {
                    Name = name,
                    Curve = new Line(origin, origin + new Vector3(25, 25, 0)),
                    Material = material,
                    Radius = radius
                };
                gridline.AddTextToCollection(texts, Colors.Black);
                this.Model.AddElement(gridline);
            }
            // </example>

            this.Model.AddElement(new ModelText(texts, FontSize.PT72, 50));
        }

        [Fact]
        public void CurvedGridline()
        {
            Name = nameof(CurvedGridline);

            var size = 10;
            var texts = new List<(Vector3 location, Vector3 facingDirection, Vector3 lineDirection, string text, Color? color)>();

            // Semicircle
            var gridline = new GridLine
            {
                Name = "A",
                Material = new Material("Red", new Color(1, 0, 0, 1)),
                Curve = new Arc(new Vector3(size, 0, 0), size, 0, 180),
                ExtensionBeginning = size / 2,
                ExtensionEnd = size / 2,
            };
            gridline.AddTextToCollection(texts, Colors.White);

            // Bezier
            var bezierGridline = new GridLine
            {
                Name = "B",
                Curve = new Bezier(new List<Vector3>() { new Vector3(0, size * 2, 0), new Vector3(size, size * 3, 0), new Vector3(size * 2, size * 2, 0), new Vector3(size * 3, size * 3, 0) }),
                Material = new Material("Yellow", new Color(1, 1, 0, 1))
            };
            bezierGridline.AddTextToCollection(texts, Colors.White);

            // Polyline
            var polyGridline = new GridLine
            {
                Name = "C",
                Curve = new Polyline(new List<Vector3>() { new Vector3(0, size * 3, 0), new Vector3(size, size * 4, 0), new Vector3(size * 2, size * 3, 0), new Vector3(size * 3, size * 4, 0) }),
                Material = new Material("Green", new Color(0, 1, 0, 1))
            };
            polyGridline.AddTextToCollection(texts, Colors.White);

            // Vertical
            var verticalGridline = new GridLine
            {
                Name = "D",
                Curve = new Line(new Vector3(), new Vector3(0, 0, 50)),
                Material = new Material("Blue", new Color(0, 0, 1, 1))
            };
            verticalGridline.AddTextToCollection(texts, Colors.White);

            this.Model.AddElements(gridline, bezierGridline, polyGridline, verticalGridline, new ModelText(texts, FontSize.PT72, 50));
        }
    }
}