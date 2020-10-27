using System.Collections.Generic;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class ImportMeshElementTests : ModelTest
    {
        [Fact, Trait("Category", "Examples")]
        public void Component()
        {
            this.Name = "Elements_ImportMeshElement";
            var model = this.Model;

            // <example>
            var path = "../../../models/STL/Hilti_2008782_Speed lock clevis hanger MH-SLC 2_ EG_2.stl";
            var shiny = new Material("shiny", Colors.Red, 1.0, 0.9);
            var bracket = new ImportMeshElement(path, Units.LengthUnit.Millimeter, shiny);
            model.AddElement(bracket);

            var brackets = new List<ElementInstance>();
            for (var u = 0; u < 360.0; u += 20)
            {
                var t = new Transform(new Vector3(1, 0, 0));
                t.Rotate(Vector3.ZAxis, u);
                var instance = bracket.CreateInstance(t, $"Component_{u}");
                model.AddElement(instance);
            }
            // </example>
        }
    }
}