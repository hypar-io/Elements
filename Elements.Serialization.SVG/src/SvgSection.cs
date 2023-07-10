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
    /// A section of a model serialized to SVG.
    /// </summary>
    public class SvgSection : SvgBaseDrawing
    {
        #region Constants

        private const double DEFAULT_SCALE = 10;

        #endregion

        #region Private fields

        private readonly List<Model> _models = new List<Model>();
        private Dictionary<string, Line> _gridLines = new Dictionary<string, Line>();
        private readonly double _elevation;
        private BaseSvgCanvas _canvas = null!;

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
        public SvgContext FrontContext { get; set; } = new SvgContext(Colors.Black, Colors.Black, 0.01);

        /// <summary>
        /// The svg context which defines settings for elements behind the cut plane.
        /// </summary>
        public SvgContext BackContext { get; set; } = new SvgContext(Colors.Black, 0.01);

        /// <summary>
        /// The svg context which defines settings for the grid elements.
        /// </summary>
        public SvgContext GridContext { get; set; } = new SvgContext(Colors.Black, 0.01, new double[] { 0.3 * 5, 0.025 * 5, 0.05 * 5, 0.025 * 5 });

        /// <summary>
        ///  The svg context which defines settings for the grid text elements.
        /// </summary>
        /// <returns></returns>
        public SvgContext GridTextContext { get; set; } = new SvgContext("Arial", 0.7);

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
            var drawingPlan = new SvgSectionOld(models, elevation);
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
        /// <param name="stream">The stream to write the SVG data.</param>
        public void SaveAsSvg(Stream stream)
        {
            var rotation = CreateSceneViewBox();
            _canvas = new SkiaCanvas(stream, ViewBoxHeight, ViewBoxWidth, this);
            _canvas.SetBounds(SceneBounds, rotation);
            Draw(_canvas);
            _canvas.Close();
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
        /// Draws the models on the canvas from the input.
        /// </summary>
        /// <param name="canvas">The drawing tool (adapter for Skia, SVG.Net, etc.)</param>
        /// <param name="pageHeight">The height of the page.</param>
        /// <param name="pageWidth">The width of the page.</param>
        /// <param name="margin">The margin from the left and right of the page.</param>
        /// <returns></returns>
        public double Draw(BaseSvgCanvas canvas, float pageHeight = -1, float pageWidth = -1, float margin = 0)
        {
            _canvas = canvas;
            var rotation = CreateSceneViewBox(pageHeight, pageWidth, margin);
            var plane = new Plane(new Vector3(0, 0, _elevation), Vector3.ZAxis);
            var customElementsBeforeGrid = new List<DrawingAction>();
            var customElementsAfterGrid = new List<DrawingAction>();

            foreach (var model in _models)
            {
                var modelWithoutCustomElements = model;
                if (OnElementDrawing != null)
                {
                    var elements = new Dictionary<Guid, Element>();
                    foreach (var element in model.Elements)
                    {
                        var e = new ElementSerializationEventArgs(this, element.Value, Scale);
                        OnElementDrawing.Invoke(this, e);
                        if (e.IsProcessed)
                        {
                            switch (e.CreationSequence)
                            {
                                case ElementSerializationEventArgs.CreationSequences.AfterGridLines:
                                    customElementsAfterGrid.AddRange(e.Actions);
                                    break;
                                case ElementSerializationEventArgs.CreationSequences.BeforeGridLines:
                                    customElementsBeforeGrid.AddRange(e.Actions);
                                    break;
                                case ElementSerializationEventArgs.CreationSequences.Immediately:
                                    e.Actions.ForEach(a => a.Draw(_canvas));
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
                        _canvas.DrawPolygon(p, FrontContext);
                    }
                }

                foreach (var backPolygon in back)
                {
                    foreach (var p in backPolygon.Value)
                    {
                        _canvas.DrawPolygon(p, BackContext);
                    }
                }

                foreach (var line in lines)
                {
                    foreach (var l in line.Value)
                    {
                        _canvas.DrawLine(l, FrontContext);
                    }
                }

            }

            customElementsBeforeGrid.ForEach(el => el.Draw(_canvas));
            DrawGridLines(rotation, _gridLines);
            customElementsAfterGrid.ForEach(el => el.Draw(_canvas));
            return rotation;
        }

        /// <summary>
        /// Creates scene view box
        /// </summary>
        /// <param name="pageHeight">The page height.</param>
        /// <param name="pageWidth">The pae widthh.</param>
        /// <param name="margin">The margin from the left and right of the page.</param>
        /// <returns>Returns plan rotation in degrees.</returns>
        public double CreateSceneViewBox(float pageHeight = -1, float pageWidth = -1, float margin = 0)
        {
            SceneBounds = SvgBaseDrawing.ComputeSceneBounds(_models);
            var rotation = SvgBaseDrawing.GetRotationValueForPlan(_models, PlanRotation, PlanRotationDegrees);

            _gridLines.Clear();
            if (ShowGrid)
            {
                _gridLines = ExtendSceneWithGridLines();
            }

            ViewBoxWidth = (float)(SceneBounds.Max.X - SceneBounds.Min.X);
            ViewBoxHeight = (float)(SceneBounds.Max.Y - SceneBounds.Min.Y);
            var transform = new Transform(Vector3.Origin);
            transform.Rotate(rotation);
            var bounds = new BBox3(SceneBounds.Corners().Select(v => transform.OfPoint(v)));
            var wOld = ViewBoxWidth;
            var hOld = ViewBoxHeight;
            ViewBoxWidth = (float)(bounds.Max.X - bounds.Min.X);
            ViewBoxHeight = (float)(bounds.Max.Y - bounds.Min.Y);

            if (pageHeight == -1 || pageWidth == -1)
            {
                Scale = DEFAULT_SCALE;
            }
            else
            {
                Scale = Math.Min((pageHeight - margin * 4) / ViewBoxHeight,
                                (pageWidth - margin * 4) / ViewBoxWidth);
            }

            float max = Math.Max(ViewBoxWidth, ViewBoxHeight);

            if (_canvas != null)
            {
                _canvas.SetBounds(SceneBounds, rotation);
            }
            return rotation;
        }

        #endregion

        #region Private logic

        private void DrawGridLines(double rotation, Dictionary<string, Line> gridLines)
        {
            foreach (var line in gridLines)
            {
                _canvas.DrawLine(line.Value, GridContext);
                _canvas.DrawCircle(line.Value.Start, GridHeadRadius, GridContext);
                _canvas.DrawText(line.Key.ToString(), new Transform(line.Value.Start), GridTextContext);
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

                SceneBounds.Extend(start, end, l, r, t, b);
            }
            return gridLines;
        }

        #endregion
    }
}
