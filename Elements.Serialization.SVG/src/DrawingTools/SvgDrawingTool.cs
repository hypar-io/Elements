using Elements.Geometry;
using Svg;
using Svg.Transforms;
using System.Collections.Generic;

namespace Elements.Serialization.SVG
{
    // public class SvgDrawinTool : IDrawingTool<SvgContext>
    // {
    //     private SvgDocument document;

    //     public SvgDrawinTool(SvgDocument document)
    //     {
    //         this.document = document;
    //     }

    //     public void DrawLine(Line line, SvgSection drawingPlan, SvgContext context)
    //     {
    //         document.Children.Add(line.ToSvgLine(drawingPlan.GetSceneBounds().Min, drawingPlan.ViewBoxHeight, context));
    //     }

    //     public void DrawPolygon(Polygon polygon, SvgSection drawingPlan, SvgContext context)
    //     {
    //         document.Children.Add(polygon.ToSvgPolygon(drawingPlan.GetSceneBounds().Min, drawingPlan.ViewBoxHeight, context));
    //     }

    //     public void DrawText(Vector3 location, string content, SvgSection drawingPlan, double fontSize, double angle = 0, double baselineOffset = 0.03)
    //     {
    //         var x = location.X.ToXUserUnit(drawingPlan);
    //         var y = location.Y.ToYUserUnit(drawingPlan);

    //         var svgText = new SvgText(content)
    //         {
    //             X = new SvgUnitCollection() { x },
    //             Y = new SvgUnitCollection() { y },
    //             TextAnchor = SvgTextAnchor.Middle,
    //             Transforms = new Svg.Transforms.SvgTransformCollection() { new SvgRotate((float)angle, x.Value, y.Value), new SvgTranslate(0, (float)baselineOffset) }
    //         };
    //         svgText.CustomAttributes.Add("style", $"font-family: Arial; font-size: {fontSize}; fill:black");

    //         document.Children.Add(svgText);
    //     }
    //     // public SvgUnit ToXUserUnit(double x, SvgSection drawingPlan)
    //     // {
    //     //     return x.ToXUserUnit(drawingPlan.GetSceneBounds().Min);
    //     // }

    //     // public SvgUnit ToYUserUnit(double y, SvgSection drawingPlan)
    //     // {
    //     //     return y.ToYUserUnit(drawingPlan.ViewBoxHeight, drawingPlan.GetSceneBounds().Min);
    //     // }
    // }
}