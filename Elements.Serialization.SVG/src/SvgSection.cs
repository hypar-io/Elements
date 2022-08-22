using Elements.Geometry;
using Svg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Colors = System.Drawing.Color;

namespace Elements.Serialization.SVG
{
    /// <summary>
    /// Orientations for a plan relative to the page.
    /// </summary>
    public enum PlanRotation
    {
        /// <summary>
        /// Align the longest grid along the long axis of the page.
        /// </summary>
        LongestGridHorizontal,
        /// <summary>
        /// Align the longest grid along the short axis of the page.
        /// </summary>
        LongestGridVertical,
        /// <summary>
        /// Do not reorient the drawing on the page.
        /// </summary>
        None,
        /// <summary>
        /// Rotate the drawing by a specific angle.
        /// </summary>
        Angle,
    }

    /// <summary>
    /// 
    /// </summary>
    public static class SvgSection
    {
        /// <summary>
        /// Create a plan of a model at the provided elevation and save the 
        /// resulting section to the provided path.
        /// </summary>
        /// <param name="models">A collection of models to include in the plan.</param>
        /// <param name="elevation">The elevation at which the plan will be cut.</param>
        /// <param name="frontContext">An svg context which defines settings for elements cut by the cut plane.</param>
        /// <param name="backContext">An svg context which defines settings for elements behind the cut plane.</param>
        /// <param name="path">The location on disk to write the SVG file.</param>
        /// <param name="showGrid">If gridlines exist, should they be shown in the plan?</param>
        /// <param name="gridHeadExtension">The extension of the grid head past the bounds of the drawing in the created plan.</param>
        /// <param name="gridHeadRadius">The radius of grid heads in the created plan.</param>
        /// <param name="planRotation">How should the plan be rotated relative to the page?</param>
        /// <param name="planRotationDegrees">An additional amount to rotate the plan.</param>
        public static void CreateAndSavePlanFromModels(IList<Model> models,
                                                double elevation,
                                                SvgContext frontContext,
                                                SvgContext backContext,
                                                string path,
                                                bool showGrid = true,
                                                double gridHeadExtension = 2.0,
                                                double gridHeadRadius = 0.5,
                                                PlanRotation planRotation = PlanRotation.Angle,
                                                double planRotationDegrees = 0.0)
        {
            var doc = CreatePlanFromModels(models, elevation, frontContext, backContext, showGrid, gridHeadExtension, gridHeadRadius, planRotation, planRotationDegrees);
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            doc.Write(stream);
        }

        /// <summary>
        /// Generate a plan of a model at the provided elevation.
        /// </summary>
        /// <param name="models">A collection of models to include in the plan.</param>
        /// <param name="elevation">The elevation at which the plan will be cut.</param>
        /// <param name="frontContext">An svg context which defines settings for elements cut by the cut plane.</param>
        /// <param name="backContext">An svg context which defines settings for elements behind the cut plane.</param>
        /// <param name="showGrid">If gridlines exist, should they be shown in the plan?</param>
        /// <param name="gridHeadExtension">The extension of the grid head past the bounds of the drawing in the created plan.</param>
        /// <param name="gridHeadRadius">The radius of grid heads in the created plan.</param>
        /// <param name="planRotation">How should the plan be rotated relative to the page?</param>
        /// <param name="planRotationDegrees">An additional amount to rotate the plan.</param>
        public static SvgDocument CreatePlanFromModels(IList<Model> models,
                                                double elevation,
                                                SvgContext frontContext,
                                                SvgContext backContext,
                                                bool showGrid = true,
                                                double gridHeadExtension = 2.0,
                                                double gridHeadRadius = 0.5,
                                                PlanRotation planRotation = PlanRotation.Angle,
                                                double planRotationDegrees = 0.0)
        {
            var rotation = GetRotationValueForPlan(models, planRotation, planRotationDegrees);

            var doc = new SvgDocument
            {
                Fill = SvgPaintServer.None
            };
            doc.CustomAttributes.Add("transform", $"rotate({rotation} 0 0)");

            var sceneBounds = ComputeSceneBounds(models);

            var plane = new Plane(new Vector3(0, 0, elevation), Vector3.ZAxis);
            Draw(models,
                 sceneBounds,
                 plane,
                 doc,
                 frontContext,
                 backContext,
                 showGrid,
                 gridHeadExtension,
                 gridHeadRadius,
                 rotation);

            return doc;
        }

        private static double GetRotationValueForPlan(IList<Model> models, PlanRotation rotation, double angle)
        {
            if (rotation == PlanRotation.Angle)
            {
                return angle;
            }

            var grids = models.SelectMany(m => m.AllElementsOfType<GridLine>()).Select(gl => gl.Curve).Where(gl => gl is Line).ToList();
            if (!grids.Any())
            {
                return 0.0;
            }

            var longest = (Line)grids.OrderBy(g => g.Length()).First();

            return rotation switch
            {
                PlanRotation.LongestGridHorizontal => -longest.Direction().PlaneAngleTo(Vector3.YAxis),
                PlanRotation.LongestGridVertical => -longest.Direction().PlaneAngleTo(Vector3.XAxis),
                PlanRotation.Angle => angle,
                PlanRotation.None => 0.0,
                _ => 0.0,
            };
        }

        private static void Draw(IList<Model> models,
                                 BBox3 sceneBounds,
                                 Plane plane,
                                 SvgDocument doc,
                                 SvgContext frontContext,
                                 SvgContext backContext,
                                 bool showGrid,
                                 double gridHeadExtension,
                                 double gridHeadRadius,
                                 double rotation)
        {
            var gridContext = new SvgContext()
            {
                Stroke = new SvgColourServer(Colors.Black),
                StrokeWidth = new SvgUnit(SvgUnitType.User, 0.01f),
                StrokeDashArray = new SvgUnitCollection(){
                    new SvgUnit(SvgUnitType.User, 0.3f),
                    new SvgUnit(SvgUnitType.User, 0.025f),
                    new SvgUnit(SvgUnitType.User, 0.05f),
                    new SvgUnit(SvgUnitType.User, 0.025f),
                }
            };

            // Grow the bounds by the size of the grid and grid heads
            var gridLines = new Dictionary<string, Line>();
            if (showGrid)
            {
                foreach (var g in models.SelectMany(m => m.AllElementsOfType<GridLine>()).ToList())
                {
                    var gl = (Line)g.Curve;
                    var d = gl.Direction();
                    var start = gl.Start + d.Negate() * gridHeadExtension;
                    var end = gl.End + d * gridHeadExtension;
                    gridLines.Add(g.Name, new Line(start, end));

                    var l = start + new Vector3(-gridHeadRadius, 0);
                    var r = start + new Vector3(gridHeadRadius, 0);
                    var t = start + new Vector3(0, gridHeadRadius);
                    var b = start + new Vector3(0, -gridHeadRadius);

                    sceneBounds.Extend(start, end, l, r, t, b);
                }
            }

            var w = (float)(sceneBounds.Max.X - sceneBounds.Min.X);
            var h = (float)(sceneBounds.Max.Y - sceneBounds.Min.Y);

            if (rotation == 0.0)
            {
                doc.ViewBox = new SvgViewBox(0, 0, w, h);
            }
            else
            {
                // Compute a new bounding box around the rotated
                // bounding box, to ensure that we our viewbox isn't too small.
                var t = new Transform(Vector3.Origin);
                t.Rotate(rotation);
                var bounds = new BBox3(sceneBounds.Corners().Select(v => t.OfPoint(v)));
                w = (float)(bounds.Max.X - bounds.Min.X);
                h = (float)(bounds.Max.Y - bounds.Min.Y);
                doc.ViewBox = new SvgViewBox(0, 0, w, h);
            }

            foreach (var model in models)
            {
                model.Intersect(plane,
                                out Dictionary<Guid, List<Polygon>> intersecting,
                                out Dictionary<Guid, List<Polygon>> back,
                                out Dictionary<Guid, List<Line>> lines);

                foreach (var intersectingPolygon in intersecting)
                {
                    foreach (var p in intersectingPolygon.Value)
                    {
                        doc.Children.Add(p.ToSvgPolygon(sceneBounds.Min, h, frontContext));
                    }
                }

                foreach (var backPolygon in back)
                {
                    foreach (var p in backPolygon.Value)
                    {
                        doc.Children.Add(p.ToSvgPolygon(sceneBounds.Min, h, backContext));
                    }
                }

                foreach (var line in lines)
                {
                    foreach (var l in line.Value)
                    {
                        doc.Children.Add(l.ToSvgLine(sceneBounds.Min, h, frontContext));
                    }
                }
            }

            if (showGrid)
            {
                foreach (var line in gridLines)
                {
                    doc.Children.Add(line.Value.ToSvgLine(sceneBounds.Min, h, gridContext));
                    doc.Children.Add(new SvgCircle()
                    {
                        CenterX = line.Value.Start.X.ToXUserUnit(sceneBounds.Min),
                        CenterY = line.Value.Start.Y.ToYUserUnit(h, sceneBounds.Min),
                        Radius = new SvgUnit(SvgUnitType.User, (float)gridHeadRadius),
                        Stroke = new SvgColourServer(Colors.Black),
                        Fill = new SvgColourServer(Colors.White),
                        StrokeWidth = gridContext.StrokeWidth
                    });

                    var x = line.Value.Start.X.ToXUserUnit(sceneBounds.Min);
                    var y = line.Value.Start.Y.ToYUserUnit(h, sceneBounds.Min);

                    var text = new SvgText(line.Key.ToString())
                    {
                        X = new SvgUnitCollection() { x },
                        Y = new SvgUnitCollection() { y },
                        FontStyle = SvgFontStyle.Normal,
                        FontSize = new SvgUnit(SvgUnitType.User, 0.5f),
                        FontFamily = "Arial",
                        Fill = new SvgColourServer(Colors.Black),
                        TextAnchor = SvgTextAnchor.Middle,
                        BaselineShift = (-0.2).ToString(), // TODO: Bit of a magic number here to center the text
                    };

                    // TODO: Figure out how to add the transform-origin attribute
                    // text.CustomAttributes.Add("transform-origin", $"{x * -1} {y * -1}");
                    text.CustomAttributes.Add("transform", $"rotate({-1 * rotation} {x} {y})");
                    doc.Children.Add(text);
                }
            }
        }

        private static BBox3 ComputeSceneBounds(IList<Model> models)
        {
            var max = new Vector3(double.MinValue, double.MinValue);
            var min = new Vector3(double.MaxValue, double.MaxValue);

            foreach (var model in models)
            {
                foreach (var element in model.Elements)
                {
                    if (element.Value is GeometricElement geo)
                    {
                        geo.UpdateRepresentations();
                        geo.UpdateBoundsAndComputeSolid();

                        if (geo._bounds.Max.X > max.X)
                        {
                            max.X = geo._bounds.Max.X;
                        }
                        if (geo._bounds.Max.Y > max.Y)
                        {
                            max.Y = geo._bounds.Max.Y;
                        }
                        if (geo._bounds.Min.X < min.X)
                        {
                            min.X = geo._bounds.Min.X;
                        }
                        if (geo._bounds.Min.Y < min.Y)
                        {
                            min.Y = geo._bounds.Min.Y;
                        }
                    }
                }
            }

            return new BBox3(min, max);
        }
    }
}
