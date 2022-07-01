using Elements.Geometry;
using Svg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Colors = System.Drawing.Color;

namespace Elements.Serialization.SVG
{
    public static class SvgSection
    {
        /// <summary>
        /// Generate a plan of a model at the provided elevation.
        /// </summary>
        /// <param name="models">A collection of models to include in the plan.</param>
        /// <param name="elevation">The elevation at which the plan will be cut.</param>
        /// <param name="path">The location on disk to write the SVG file.</param>
        /// <param name="showGrid">If gridlines exist, should they be shown in the plan?</param>
        public static void CreatePlanFromModels(IList<Model> models,
                                                double elevation,
                                                SvgContext frontContext,
                                                SvgContext backContext,
                                                string path,
                                                bool showGrid = true,
                                                double gridHeadExtension = 2.0,
                                                double gridHeadRadius = 0.5)
        {
            var doc = new SvgDocument
            {
                Fill = SvgPaintServer.None
            };

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
                 gridHeadRadius);

            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            doc.Write(stream);
        }

        /// <summary>
        /// Generate a plan of a model at the provided elevation.
        /// </summary>
        /// <param name="models">A collection of models to include in the plan.</param>
        /// <param name="elevation">The elevation at which the plan will be cut.</param>
        /// <param name="path">The location on disk to write the SVG file.</param>
        /// <param name="showGrid">If gridlines exist, should they be shown in the plan?</param>
        public static SvgDocument CreatePlanFromModels(IList<Model> models,
                                                double elevation,
                                                SvgContext frontContext,
                                                SvgContext backContext,
                                                bool showGrid = true,
                                                double gridHeadExtension = 2.0,
                                                double gridHeadRadius = 0.5)
        {
            var doc = new SvgDocument
            {
                Fill = SvgPaintServer.None
            };

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
                 gridHeadRadius);

            return doc;
        }

        private static void Draw(IList<Model> models,
                                 BBox3 sceneBounds,
                                 Plane plane,
                                 SvgDocument doc,
                                 SvgContext frontContext,
                                 SvgContext backContext,
                                 bool showGrid,
                                 double gridHeadExtension,
                                 double gridHeadRadius)
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

            foreach (var model in models)
            {
                doc.ViewBox = new SvgViewBox(0, 0, w, h);

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

                    var text = new SvgText(line.Key.ToString())
                    {
                        X = new SvgUnitCollection() { line.Value.Start.X.ToXUserUnit(sceneBounds.Min) },
                        Y = new SvgUnitCollection() { line.Value.Start.Y.ToYUserUnit(h, sceneBounds.Min) },
                        FontStyle = SvgFontStyle.Normal,
                        FontSize = new SvgUnit(SvgUnitType.User, 0.5f),
                        FontFamily = "Arial",
                        Fill = new SvgColourServer(Colors.Black),
                        TextAnchor = SvgTextAnchor.Middle,
                        BaselineShift = (-0.2).ToString()
                    };
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
                        if (geo.Bounds.Max.X > max.X)
                        {
                            max.X = geo.Bounds.Max.X;
                        }
                        if (geo.Bounds.Max.Y > max.Y)
                        {
                            max.Y = geo.Bounds.Max.Y;
                        }
                        if (geo.Bounds.Min.X < min.X)
                        {
                            min.X = geo.Bounds.Min.X;
                        }
                        if (geo.Bounds.Min.Y < min.Y)
                        {
                            min.Y = geo.Bounds.Min.Y;
                        }
                    }
                }
            }

            return new BBox3(min, max);
        }
    }
}
