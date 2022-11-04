using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Elements.Geometry;
using Svg;
using Svg.Transforms;
using Colors = System.Drawing.Color;

namespace Elements.Serialization.SVG
{
    /// <summary>
    /// Svg drawing plan
    /// </summary>
    public class SvgDrawingPlan
    {
        #region Events

        /// <summary>
        /// This event occurs before element is added to the svg scene.
        /// It can be used to customize element creation.
        /// </summary>
        public event EventHandler<ElementSerializationEventArgs>? OnElementDrawing;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of SvgDrawingPlan class
        /// </summary>
        /// <param name="models">A collection of models to include in the plan</param>
        /// <param name="elevation">The elevation at which the plan will be cut</param>
        public SvgDrawingPlan(IList<Model> models, double elevation)
        {
            this.models.AddRange(models);
            this.elevation = elevation;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets an svg context which defines settings for elements cut by the cut plane
        /// </summary>
        public SvgContext FrontContext { get; set; } = new SvgContext()
        {
            Fill = new SvgColourServer(System.Drawing.Color.Black),
            StrokeWidth = new SvgUnit(SvgUnitType.User, 0.01f)
        };

        /// <summary>
        /// Gets or sets an svg context which defines settings for elements behind the cut plane
        /// </summary>
        public SvgContext BackContext { get; set; } = new SvgContext()
        {
            Stroke = new SvgColourServer(System.Drawing.Color.Black),
            StrokeWidth = new SvgUnit(SvgUnitType.User, 0.01f)
        };

        /// <summary>
        /// Gets or sets an svg context which defines settings for grid elements
        /// </summary>
        public SvgContext GridContext { get; set; } = new SvgContext()
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

        /// <summary>
        /// Gets or sets if gridlines exist, should they be shown in the plan
        /// </summary>
        public bool ShowGrid { get; set; } = true;

        /// <summary>
        /// Gets or sets the extension of the grid head past the bounds of the drawing in the created plan
        /// </summary>
        public double GridHeadExtension { get; set; } = 2.0;

        /// <summary>
        /// Gets or sets the radius of grid heads in the created plan
        /// </summary>
        public double GridHeadRadius { get; set; } = 0.5;

        /// <summary>
        /// Gets or sets how should the plan be rotated relative to the page
        /// </summary>
        public PlanRotation PlanRotation { get; set; } = PlanRotation.Angle;

        /// <summary>
        /// Gets or sets an additional amount to rotate the plan
        /// </summary>
        public double PlanRotationDegrees { get; set; } = 0.0;

        internal float ViewBoxWidth { get; private set; }
        internal float ViewBoxHeight { get; private set; }

        #endregion

        #region Public logic

        /// <summary>
        /// Create a plan of a model and save the resulting section to the provided stream
        /// </summary>
        /// <param name="stream">The stream to write the SVG data</param>
        public void SaveAsSvg(Stream stream)
        {
            var doc = CreateSvgDocument();
            doc.Write(stream);
        }

        /// <summary>
        /// Create a plan of a model and save the resulting section to the provided path
        /// </summary>
        /// <param name="path">The location on disk to write the SVG file</param>
        public void SaveAsSvg(string path)
        {
            var svgPath = System.IO.Path.ChangeExtension(path, ".svg");
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                SaveAsSvg(stream);
            }
        }

        /// <summary>
        /// Create a plan of a model and save the resulting section to the provided path
        /// </summary>
        /// <param name="path">The location on disk to write the PDF file</param>
        public void SaveAsPdf(string path)
        {
            var pdfPath = System.IO.Path.ChangeExtension(path, ".pdf");

        }

        /// <summary>
        /// Create SvgText element
        /// </summary>
        /// <param name="text">The content</param>
        /// <param name="location">The element location at the model</param>
        /// <param name="angle">The angle of the content</param>
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
            svgText.CustomAttributes.Add("style", "font-family: Areal; font-size: 0.5; fill:black");

            return svgText;
        }


        /// <summary>
        /// Generate a plan of a model
        /// </summary>
        public SvgDocument CreateSvgDocument()
        {
            sceneBounds = SvgSection.ComputeSceneBounds(models);

            var doc = new SvgDocument
            {
                Fill = SvgPaintServer.None
            };

            var rotation = SvgSection.GetRotationValueForPlan(models, PlanRotation, PlanRotationDegrees);

            gridLines.Clear();
            if (ShowGrid)
            {
                gridLines = ExtendSceneWithGridLines();
            }

            CreateViewBox(doc, rotation);
            Draw(doc, rotation);
            return doc;
        }

        /// <summary>
        /// Get the scene bounds
        /// </summary>
        public BBox3 GetSceneBounds()
        {
            return sceneBounds;
        }

        #endregion

        #region Private logic

        private void Draw(SvgDocument doc, double rotation)
        {
            var plane = new Plane(new Vector3(0, 0, elevation), Vector3.ZAxis);
            var customElementsBeforeGrid = new List<SvgElement>();
            var customElementsAfterGrid = new List<SvgElement>();

            foreach (var model in models)
            {
                var modelWithoutCusromElements = model;
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

                    modelWithoutCusromElements = new Model(model.Transform, elements);
                }

                modelWithoutCusromElements.Intersect(plane,
                                out Dictionary<Guid, List<Polygon>> intersecting,
                                out Dictionary<Guid, List<Polygon>> back,
                                out Dictionary<Guid, List<Line>> lines);

                foreach (var intersectingPolygon in intersecting)
                {
                    foreach (var p in intersectingPolygon.Value)
                    {
                        doc.Children.Add(p.ToSvgPolygon(sceneBounds.Min, ViewBoxHeight, FrontContext));
                    }
                }

                foreach (var backPolygon in back)
                {
                    foreach (var p in backPolygon.Value)
                    {
                        doc.Children.Add(p.ToSvgPolygon(sceneBounds.Min, ViewBoxHeight, BackContext));
                    }
                }

                foreach (var line in lines)
                {
                    foreach (var l in line.Value)
                    {
                        doc.Children.Add(l.ToSvgLine(sceneBounds.Min, ViewBoxHeight, FrontContext));
                    }
                }

            }

            customElementsBeforeGrid.ForEach(el => doc.Children.Add(el));
            DrawGridLines(doc, rotation, gridLines);
            customElementsAfterGrid.ForEach(el => doc.Children.Add(el));
        }

        private void CreateViewBox(SvgDocument doc, double rotation)
        {
            ViewBoxWidth = (float)(sceneBounds.Max.X - sceneBounds.Min.X);
            ViewBoxHeight = (float)(sceneBounds.Max.Y - sceneBounds.Min.Y);


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
                var bounds = new BBox3(sceneBounds.Corners().Select(v => t.OfPoint(v)));
                var wOld = ViewBoxWidth;
                var hOld = ViewBoxHeight;
                ViewBoxWidth = (float)(bounds.Max.X - bounds.Min.X);
                ViewBoxHeight = (float)(bounds.Max.Y - bounds.Min.Y);
                float max = Math.Max(ViewBoxWidth, ViewBoxHeight);
                doc.ViewBox = new SvgViewBox((wOld - ViewBoxWidth) / 2.0f, (ViewBoxHeight - hOld) / 2.0f, ViewBoxWidth, ViewBoxHeight);
                doc.CustomAttributes.Add("transform", $"rotate({rotation} {ViewBoxWidth / 2.0} {ViewBoxHeight / 2.0})");// translate({(w - wOld) / 2.0} {(hOld - h) / 2.0})");
            }
        }

        private void DrawGridLines(SvgDocument doc, double rotation, Dictionary<string, Line> gridLines)
        {
            foreach (var line in gridLines)
            {
                doc.Children.Add(line.Value.ToSvgLine(sceneBounds.Min, ViewBoxHeight, GridContext));
                doc.Children.Add(new SvgCircle()
                {
                    CenterX = line.Value.Start.X.ToXUserUnit(sceneBounds.Min),
                    CenterY = line.Value.Start.Y.ToYUserUnit(ViewBoxHeight, sceneBounds.Min),
                    Radius = new SvgUnit(SvgUnitType.User, (float)GridHeadRadius),
                    Stroke = new SvgColourServer(Colors.Black),
                    Fill = new SvgColourServer(Colors.White),
                    StrokeWidth = GridContext.StrokeWidth
                });

                var x = line.Value.Start.X.ToXUserUnit(sceneBounds.Min);
                var y = line.Value.Start.Y.ToYUserUnit(ViewBoxHeight, sceneBounds.Min);

                var text = new SvgText(line.Key.ToString())
                {
                    X = new SvgUnitCollection() { x },
                    Y = new SvgUnitCollection() { y },
                    FontStyle = SvgFontStyle.Normal,
                    FontSize = new SvgUnit(SvgUnitType.User, 0.5f),
                    FontFamily = "Areal",
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
            foreach (var g in models.SelectMany(m => m.AllElementsOfType<GridLine>()).ToList())
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

                sceneBounds.Extend(start, end, l, r, t, b);
            }
            return gridLines;
        }

        #endregion

        #region Private fields

        private readonly List<Model> models = new List<Model>();
        private Dictionary<string, Line> gridLines = new Dictionary<string, Line>();
        private readonly double elevation;
        private BBox3 sceneBounds;

        #endregion
    }
}