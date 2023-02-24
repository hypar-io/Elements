using Elements.Geometry;
using SkiaSharp;
using Svg;
using Svg.Skia;
using Svg.Transforms;
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
    /// A section of a model serialized to SVG.
    /// </summary>
    public class SvgSection
    {
        #region Private fields

        private readonly List<Model> _models = new List<Model>();
        private Dictionary<string, Line> _gridLines = new Dictionary<string, Line>();
        private readonly double _elevation;
        private BBox3 _sceneBounds;

        #endregion

        #region Events

        /// <summary>
        /// This event occurs before an element is added to the svg scene.
        /// It can be used to customize element creation.
        /// </summary>
        public event EventHandler<ElementSerializationEventArgs>? OnElementDrawing;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of SvgDrawingPlan class.
        /// </summary>
        /// <param name="models">A collection of models to include in the plan.</param>
        /// <param name="elevation">The elevation at which the plan will be cut.</param>
        public SvgSection(IList<Model> models, double elevation)
        {
            this._models.AddRange(models);
            this._elevation = elevation;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The svg context which defines settings for elements cut by the cut plane.
        /// </summary>
        public SvgContext FrontContext { get; set; } = new SvgContext(Colors.Black, System.Drawing.Color.Black, 0.01);

        /// <summary>
        /// The svg context which defines settings for elements behind the cut plane.
        /// </summary>
        public SvgContext BackContext { get; set; } = new SvgContext(System.Drawing.Color.Black, 0.01);

        /// <summary>
        /// The svg context which defines settings for the grid elements.
        /// </summary>
        public SvgContext GridContext { get; set; } = new SvgContext(Colors.Black, 0.01, new double[] { 0.3, 0.025, 0.05, 0.025 });

        /// <summary>
        /// Should grid lines be shown in the section?
        /// </summary>
        public bool ShowGrid { get; set; } = true;

        /// <summary>
        /// The extension of the grid head past the bounds of the drawing in the created plan.
        /// </summary>
        public double GridHeadExtension { get; set; } = 2.0;

        /// <summary>
        /// The radius of grid heads in the created plan.
        /// </summary>
        public double GridHeadRadius { get; set; } = 0.5;

        /// <summary>
        /// How should the plan be rotated relative to the page.
        /// </summary>
        public PlanRotation PlanRotation { get; set; } = PlanRotation.Angle;

        /// <summary>
        /// An additional amount to rotate the plan.
        /// </summary>
        public double PlanRotationDegrees { get; set; } = 0.0;

        internal float ViewBoxWidth { get; private set; }
        internal float ViewBoxHeight { get; private set; }

        #endregion

        #region Public logic

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
            return CreatePlanFromModels(models, elevation, frontContext, backContext, out _, showGrid, gridHeadExtension, gridHeadRadius,
                    planRotation, planRotationDegrees);
        }

        /// <summary>
        /// Generate a plan of a model at the provided elevation.
        /// </summary>
        /// <param name="models">A collection of models to include in the plan.</param>
        /// <param name="elevation">The elevation at which the plan will be cut.</param>
        /// <param name="frontContext">An svg context which defines settings for elements cut by the cut plane.</param>
        /// <param name="backContext">An svg context which defines settings for elements behind the cut plane.</param>
        /// <param name="sceneBounds">The scene bounds. It can be used to add elements to the output document</param>
        /// <param name="showGrid">If gridlines exist, should they be shown in the plan?</param>
        /// <param name="gridHeadExtension">The extension of the grid head past the bounds of the drawing in the created plan.</param>
        /// <param name="gridHeadRadius">The radius of grid heads in the created plan.</param>
        /// <param name="planRotation">How should the plan be rotated relative to the page?</param>
        /// <param name="planRotationDegrees">An additional amount to rotate the plan.</param>
        public static SvgDocument CreatePlanFromModels(IList<Model> models,
                                                double elevation,
                                                SvgContext frontContext,
                                                SvgContext backContext,
                                                out BBox3 sceneBounds,
                                                bool showGrid = true,
                                                double gridHeadExtension = 2.0,
                                                double gridHeadRadius = 0.5,
                                                PlanRotation planRotation = PlanRotation.Angle,
                                                double planRotationDegrees = 0.0)
        {
            var drawingPlan = new SvgSection(models, elevation);
            drawingPlan.BackContext = backContext;
            drawingPlan.FrontContext = frontContext;
            drawingPlan.ShowGrid = showGrid;
            drawingPlan.GridHeadExtension = gridHeadExtension;
            drawingPlan.GridHeadRadius = gridHeadRadius;
            drawingPlan.PlanRotation = planRotation;
            drawingPlan.PlanRotationDegrees = planRotationDegrees;

            var doc = drawingPlan.CreateSvgDocument();
            sceneBounds = drawingPlan.GetSceneBounds();
            return doc;
        }

        /// <summary>
        /// Create a plan of a model and save the resulting section to the provided stream.
        /// </summary>
        /// <param name="stream">The stream to write the SVG data</param>
        public void SaveAsSvg(Stream stream)
        {
            var doc = CreateSvgDocument();
            doc.Write(stream);
        }

        /// <summary>
        /// Create a plan of a model and save the resulting section to the provided path.
        /// </summary>
        /// <param name="path">The location on disk to write the SVG file</param>
        public void SaveAsSvg(string path)
        {
            var svgPath = System.IO.Path.ChangeExtension(path, ".svg");
            using (var stream = new FileStream(svgPath, FileMode.Create, FileAccess.Write))
            {
                SaveAsSvg(stream);
            }
        }

        /// <summary>
        /// Create a plan of a model and save the resulting section to the provided path.
        /// </summary>
        /// <param name="path">The location on disk to write the PDF file</param>
        /// <param name="saveOptions">The pdf save options: page size, mergin</param>
        public void SaveAsPdf(string path, PdfSaveOptions saveOptions)
        {
            var pdfPath = System.IO.Path.ChangeExtension(path, ".pdf");
            using (var stream = new FileStream(pdfPath, FileMode.Create, FileAccess.Write))
            {
                SaveAsPdf(stream, saveOptions);
            }
        }

        /// <summary>
        /// Create a plan of a model and save the resulting section to the provided stream.
        /// </summary>
        /// <param name="stream">The stream to write the PDF data</param>
        /// <param name="saveOptions">The pdf save options: page size, mergin</param>
        public void SaveAsPdf(Stream stream, PdfSaveOptions saveOptions)
        {
            var pageHeight = saveOptions.PageHeight;
            var pageWidth = saveOptions.PageWidth;
            var margin = saveOptions.Margin;

            using var document = SKDocument.CreatePdf(stream);
            var path = System.IO.Path.GetTempFileName();
            path = System.IO.Path.ChangeExtension(path, ".svg");

            using (var svg = new SKSvg())
            {
                var svgStream = new MemoryStream();
                SaveAsSvg(path);
                if (svg.Load(path) is { } && svg.Picture != null)
                {
                    if (svg.Picture.CullRect.Width > svg.Picture.CullRect.Height)
                    {
                        var tmp = pageHeight;
                        pageHeight = pageWidth;
                        pageWidth = tmp;
                    }

                    using (var pdfCanvas = document.BeginPage(pageWidth, pageHeight))
                    {
                        float scale = Math.Min((pageHeight - margin * 2) / svg.Picture.CullRect.Height,
                            (pageWidth - margin * 2) / svg.Picture.CullRect.Width);
                        var matrix = SKMatrix.CreateScale(scale, scale);
                        matrix = SKMatrix.Concat(SKMatrix.CreateTranslation(margin, margin), matrix);
                        pdfCanvas.DrawPicture(svg.Picture, ref matrix);
                    }

                    document.EndPage();
                }
            }

            document.Close();
            stream.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Create SvgText element.
        /// </summary>
        /// <param name="text">The content.</param>
        /// <param name="location">The element location at the model.</param>
        /// <param name="angle">The angle of the content.</param>
        /// <returns></returns>
        public SvgText CreateText(string text, Vector3 location, double angle)
        {
            var x = location.X.ToXUserUnit(this);
            var y = location.Y.ToYUserUnit(this);

            var svgText = new SvgText(text)
            {
                X = new SvgUnitCollection() { x },
                Y = new SvgUnitCollection() { y },
                TextAnchor = SvgTextAnchor.Middle,
                Transforms = new Svg.Transforms.SvgTransformCollection() { new SvgRotate((float)angle, x.Value, y.Value), new SvgTranslate(0, 0.2f) }
            };

            svgText.CustomAttributes.Add("style", "font-family: Arial; font-size: 0.5; fill:black");
            return svgText;
        }

        /// <summary>
        /// Generate a plan of a model.
        /// </summary>
        public SvgDocument CreateSvgDocument()
        {
            _sceneBounds = SvgSection.ComputeSceneBounds(_models);

            var doc = new SvgDocument
            {
                Fill = SvgPaintServer.None
            };

            var rotation = SvgSection.GetRotationValueForPlan(_models, PlanRotation, PlanRotationDegrees);

            _gridLines.Clear();
            if (ShowGrid)
            {
                _gridLines = ExtendSceneWithGridLines();
            }

            CreateViewBox(doc, rotation);
            Draw(doc, rotation);
            return doc;
        }

        /// <summary>
        /// Get the scene bounds.
        /// </summary>
        public BBox3 GetSceneBounds()
        {
            return _sceneBounds;
        }

        #endregion

        #region Private logic

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

        private static BBox3 ComputeSceneBounds(IList<Model> models)
        {
            var bounds = new BBox3(Vector3.Max, Vector3.Min);
            foreach (var model in models)
            {
                foreach (var element in model.Elements)
                {
                    if (element.Value is GeometricElement geo)
                    {
                        geo.UpdateRepresentations();
                        if (geo.Representation == null || geo.Representation.SolidOperations.All(v => v.IsVoid))
                        {
                            continue;
                        }
                        geo.UpdateBoundsAndComputeSolid();

                        var bbMax = geo.Transform.OfPoint(geo._bounds.Max);
                        var bbMin = geo.Transform.OfPoint(geo._bounds.Min);
                        bounds.Extend(new[] { bbMax, bbMin });
                    }
                }
            }

            return bounds;
        }

        private void Draw(SvgDocument doc, double rotation)
        {
            var plane = new Plane(new Vector3(0, 0, _elevation), Vector3.ZAxis);
            var customElementsBeforeGrid = new List<SvgElement>();
            var customElementsAfterGrid = new List<SvgElement>();

            foreach (var model in _models)
            {
                var modelWithoutCustomElements = model;
                if (OnElementDrawing != null)
                {
                    var elements = new Dictionary<Guid, Element>();
                    foreach (var element in model.Elements)
                    {
                        var e = new ElementSerializationEventArgs(this, element.Value);
                        OnElementDrawing.Invoke(this, e);
                        if (e.IsProcessed)
                        {
                            switch (e.CreationSequence)
                            {
                                case ElementSerializationEventArgs.CreationSequences.AfterGridLines:
                                    customElementsAfterGrid.AddRange(e.SvgElements);
                                    break;
                                case ElementSerializationEventArgs.CreationSequences.BeforeGridLines:
                                    customElementsBeforeGrid.AddRange(e.SvgElements);
                                    break;
                                case ElementSerializationEventArgs.CreationSequences.Immediately:
                                    e.SvgElements.ForEach(el => doc.Children.Add(el));
                                    break;
                            }
                        }
                        else
                        {
                            elements.Add(element.Key, element.Value);
                        }
                    }

                    modelWithoutCustomElements = new Model(model.Transform, elements);
                }

                modelWithoutCustomElements.Intersect(plane,
                                out Dictionary<Guid, List<Polygon>> intersecting,
                                out Dictionary<Guid, List<Polygon>> back,
                                out Dictionary<Guid, List<Line>> lines);

                foreach (var intersectingPolygon in intersecting)
                {
                    foreach (var p in intersectingPolygon.Value)
                    {
                        doc.Children.Add(p.ToSvgPolygon(_sceneBounds.Min, ViewBoxHeight, FrontContext));
                    }
                }

                foreach (var backPolygon in back)
                {
                    foreach (var p in backPolygon.Value)
                    {
                        doc.Children.Add(p.ToSvgPolygon(_sceneBounds.Min, ViewBoxHeight, BackContext));
                    }
                }

                foreach (var line in lines)
                {
                    foreach (var l in line.Value)
                    {
                        doc.Children.Add(l.ToSvgLine(_sceneBounds.Min, ViewBoxHeight, FrontContext));
                    }
                }

            }

            customElementsBeforeGrid.ForEach(el => doc.Children.Add(el));
            DrawGridLines(doc, rotation, _gridLines);
            customElementsAfterGrid.ForEach(el => doc.Children.Add(el));
        }

        private void CreateViewBox(SvgDocument doc, double rotation)
        {
            ViewBoxWidth = (float)(_sceneBounds.Max.X - _sceneBounds.Min.X);
            ViewBoxHeight = (float)(_sceneBounds.Max.Y - _sceneBounds.Min.Y);


            if (rotation == 0.0)
            {
                doc.ViewBox = new SvgViewBox(0, 0, ViewBoxWidth, ViewBoxHeight);
            }
            else
            {
                // Compute a new bounding box around the rotated
                // bounding box, to ensure that we our viewbox isn't too small.
                var t = new Transform(Vector3.Origin);
                t.Rotate(rotation);
                var bounds = new BBox3(_sceneBounds.Corners().Select(v => t.OfPoint(v)));
                var wOld = ViewBoxWidth;
                var hOld = ViewBoxHeight;
                ViewBoxWidth = (float)(bounds.Max.X - bounds.Min.X);
                ViewBoxHeight = (float)(bounds.Max.Y - bounds.Min.Y);
                float max = Math.Max(ViewBoxWidth, ViewBoxHeight);
                doc.ViewBox = new SvgViewBox((wOld - ViewBoxWidth) / 2.0f, (ViewBoxHeight - hOld) / 2.0f, ViewBoxWidth, ViewBoxHeight);
                doc.CustomAttributes.Add("transform", $"rotate({rotation} {ViewBoxWidth / 2.0} {ViewBoxHeight / 2.0})");
            }
        }

        private void DrawGridLines(SvgDocument doc, double rotation, Dictionary<string, Line> gridLines)
        {
            foreach (var line in gridLines)
            {
                doc.Children.Add(line.Value.ToSvgLine(_sceneBounds.Min, ViewBoxHeight, GridContext));
                doc.Children.Add(new SvgCircle()
                {
                    CenterX = line.Value.Start.X.ToXUserUnit(_sceneBounds.Min),
                    CenterY = line.Value.Start.Y.ToYUserUnit(ViewBoxHeight, _sceneBounds.Min),
                    Radius = new SvgUnit(SvgUnitType.User, (float)GridHeadRadius),
                    Stroke = new SvgColourServer(Colors.Black),
                    Fill = new SvgColourServer(Colors.White),
                    StrokeWidth = GridContext.StrokeWidth
                });

                var x = line.Value.Start.X.ToXUserUnit(_sceneBounds.Min);
                var y = line.Value.Start.Y.ToYUserUnit(ViewBoxHeight, _sceneBounds.Min);

                var text = new SvgText(line.Key.ToString())
                {
                    X = new SvgUnitCollection() { x },
                    Y = new SvgUnitCollection() { y },
                    FontStyle = SvgFontStyle.Normal,
                    FontSize = new SvgUnit(SvgUnitType.User, 0.5f),
                    FontFamily = "Arial",
                    Fill = new SvgColourServer(Colors.Black),
                    TextAnchor = SvgTextAnchor.Middle,
                };
                text.CustomAttributes.Add("transform", $"rotate({-1 * rotation} {x} {y}), translate(0, 0.2)");
                doc.Children.Add(text);
            }
        }

        /// <summary>
        /// Grow the bounds by the size of the grid and grid heads
        /// </summary>
        private Dictionary<string, Line> ExtendSceneWithGridLines()
        {
            var gridLines = new Dictionary<string, Line>();
            foreach (var g in _models.SelectMany(m => m.AllElementsOfType<GridLine>()).ToList())
            {
                var gl = (Line)g.Curve;
                var d = gl.Direction();
                var start = gl.Start + d.Negate() * GridHeadExtension;
                var end = gl.End + d * GridHeadExtension;
                gridLines.Add(g.Name, new Line(start, end));

                var l = start + new Vector3(-GridHeadRadius, 0);
                var r = start + new Vector3(GridHeadRadius, 0);
                var t = start + new Vector3(0, GridHeadRadius);
                var b = start + new Vector3(0, -GridHeadRadius);

                _sceneBounds.Extend(start, end, l, r, t, b);
            }
            return gridLines;
        }

        #endregion
    }
}
