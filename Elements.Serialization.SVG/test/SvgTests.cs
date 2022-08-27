using Svg;
using System.IO;
using Xunit;
using Color = System.Drawing.Color;

namespace Elements.Serialization.SVG.Tests
{
    public class SvgTests
    {
        private readonly SvgContext _frontContext = new SvgContext()
        {
            Fill = new SvgColourServer(Color.Black),
            StrokeWidth = new SvgUnit(SvgUnitType.User, 0.01f)
        };

        private readonly SvgContext _backContext = new SvgContext()
        {
            Stroke = new SvgColourServer(Color.Black),
            StrokeWidth = new SvgUnit(SvgUnitType.User, 0.01f)
        };

        [Fact]
        public void Plan()
        {
            var json = File.ReadAllText("../../../models/tower.json");
            var model = Model.FromJson(json);

            model.UpdateRepresentations();
            model.UpdateBoundsAndComputedSolids();

            SvgSection.CreateAndSavePlanFromModels(new[] { model },
                                                   3,
                                                   _frontContext,
                                                   _backContext,
                                                   "ModelPlan.svg",
                                                   planRotation: PlanRotation.LongestGridHorizontal);
        }
    }
}