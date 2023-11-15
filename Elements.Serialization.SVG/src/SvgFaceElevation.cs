using Elements.Geometry;
using Elements.Geometry.Solids;
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
    /// A section of an element serialized to SVG.
    /// </summary>
    public class SvgFaceElevation
    {
        #region Constants

        private const double _offsetToDimensionLabel = 0.3;
        private const double _offsetDimensionLines = 0.07;
        private const double _fontSize = 0.1;
        private const double _visibleGap = 0.03;

        #endregion

        #region Private fields

        private readonly Face _face;
        private readonly Vector3 _up;

        private readonly List<DimensionLine> supportedDimensionLines = new List<DimensionLine>()
        {
            new DimensionLine(Vector3.XAxis, false),
            new DimensionLine(Vector3.YAxis, false),
            new DimensionLine(Vector3.XAxis.Negate(), true),
            new DimensionLine(Vector3.YAxis.Negate(), true)
            };

        private BBox3 _sceneBounds;
        private Transform _transform;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of SvgFaceElevation class.
        /// </summary>
        /// <param name="face">The face to draw.</param>
        /// <param name="up">The normal to the face.</param>
        public SvgFaceElevation(Face face, Vector3 up)
        {
            _face = face;
            _up = up;
            _transform = new Transform();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The svg context which defines settings for element lines.
        /// </summary>
        public SvgContext ElementLinesContext { get; set; } = new SvgContext()
        {
            Stroke = new SvgColourServer(Colors.Black),
            StrokeWidth = new SvgUnit(SvgUnitType.User, 0.01f)
        };

        /// <summary>
        /// The svg context which defines settings for leader lines .
        /// </summary>
        public SvgContext DimensionLinesContext { get; set; } = new SvgContext()
        {
            Stroke = new SvgColourServer(System.Drawing.Color.DarkBlue),
            StrokeWidth = new SvgUnit(SvgUnitType.User, 0.005f)
        };

        /// <summary>
        /// An additional amount to rotate the drawing.
        /// </summary>
        public double PlanRotationDegrees { get; set; } = 0.0;

        internal float ViewBoxWidth { get; private set; }
        internal float ViewBoxHeight { get; private set; }

        #endregion

        #region Public logic

        /// <summary>
        /// Generate a drawing of the element.
        /// </summary>
        public SvgDocument CreateSvgDocument()
        {
            var doc = new SvgDocument
            {
                Fill = SvgPaintServer.None
            };

            _sceneBounds = new BBox3(Vector3.Max, Vector3.Min);
            // collect element edges
            var lines = GetLines(out var innerLines);
            _transform = new Transform(Vector3.Origin, _up);
            var invertedTransform = _transform.Inverted();
            var transformedLines = lines.Select(l => (Line)l.Transformed(invertedTransform)).ToList();
            transformedLines.ForEach(l => _sceneBounds.Extend(l.Start, l.End));

            var transformedInnerLines = new List<List<Line>>();
            innerLines.ForEach(i => transformedInnerLines.Add(new List<Line>(i.Select(l => l.TransformedLine(invertedTransform)))));

            CreateViewBox(doc, PlanRotationDegrees);
            Draw(doc, transformedLines, transformedInnerLines);
            return doc;
        }

        /// <summary>
        /// Create a drawing of the element and save the resulting section to the provided stream.
        /// </summary>
        /// <param name="stream">The stream to write the SVG data</param>
        public void SaveAsSvg(Stream stream)
        {
            var doc = CreateSvgDocument();
            doc.Write(stream);
        }

        /// <summary>
        /// Create a drawing of the element and save the resulting section to the provided path.
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
        /// Create a drawing of the elemet and save the resulting section to the provided path.
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
        /// Create a drawing of the element and save the resulting section to the provided stream.
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
                SaveAsSvg(svgStream);
                svgStream.Position = 0;

                if (svg.Load(svgStream) is { } && svg.Picture != null)
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
                        pdfCanvas.DrawPicture(svg.Picture);
                    }

                    document.EndPage();
                }
            }

            document.Close();
            stream.Seek(0, SeekOrigin.Begin);
        }

        #endregion

        #region  Private logic

        /// <summary>
        /// Gets element edges
        /// </summary>
        /// <param name="innerLines">The set of inner edges (opening lines)</param>
        /// <returns>The set of outer edges</returns>
        private List<Line> GetLines(out List<List<Line>> innerLines)
        {
            var lines = new List<Line>();
            var innerEdges = new List<List<Line>>();

            var faceNormal = _face.Outer.ToPolygon().Normal();

            var angle = faceNormal.AngleTo(_up);

            foreach (var edge in _face.Outer.Edges)
            {
                var a = edge.Edge.Left.Vertex.Point;
                var b = edge.Edge.Right.Vertex.Point;
                lines.Add(new Line(a, b));
            }

            if (_face.Inner != null)
            {

                foreach (var loop in _face.Inner)
                {
                    if (!loop.Edges.Any())
                    {
                        continue;
                    }
                    var loopLines = new List<Line>();
                    innerEdges.Add(loopLines);
                    foreach (var edge in loop.Edges)
                    {
                        var a = edge.Edge.Left.Vertex.Point;
                        var b = edge.Edge.Right.Vertex.Point;
                        loopLines.Add(new Line(a, b));
                    }
                }
            }

            innerLines = new List<List<Line>>(innerEdges);
            return lines;
        }

        private void CreateViewBox(SvgDocument doc, double rotation)
        {
            //TODO: extend scene bounds with new labels
            _sceneBounds.Extend(new Vector3(_sceneBounds.Min.X - _offsetToDimensionLabel * 3, _sceneBounds.Min.Y - _offsetToDimensionLabel * 3));
            _sceneBounds.Extend(new Vector3(_sceneBounds.Max.X + _offsetToDimensionLabel * 3, _sceneBounds.Max.Y + _offsetToDimensionLabel * 3));

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
                var transform = new Transform(Vector3.Origin);
                transform.Rotate(rotation);
                var bounds = new BBox3(_sceneBounds.Corners().Select(v => transform.OfPoint(v)));
                var wOld = ViewBoxWidth;
                var hOld = ViewBoxHeight;
                ViewBoxWidth = (float)(bounds.Max.X - bounds.Min.X);
                ViewBoxHeight = (float)(bounds.Max.Y - bounds.Min.Y);
                float max = Math.Max(ViewBoxWidth, ViewBoxHeight);
                doc.ViewBox = new SvgViewBox((wOld - ViewBoxWidth) / 2.0f, (ViewBoxHeight - hOld) / 2.0f, ViewBoxWidth, ViewBoxHeight);
                doc.CustomAttributes.Add("transform", $"rotate({rotation} {ViewBoxWidth / 2.0} {ViewBoxHeight / 2.0})");
            }
        }

        private void Draw(SvgDocument doc, IEnumerable<Line> lines, List<List<Line>> innerLines)
        {
            var sortedLines = LinkLinesToDimensionLines(lines);
            var elementBoundaryLines = sortedLines.Values.SelectMany(v => v).Where(v => !v.IsOpening).ToList();
            var boundaryDictionary = FindBoundaries(sortedLines);
            var boundaryProjections = new List<VertexAdapter>();
            var openingProjections = new List<VertexAdapter>();
            foreach (var item in sortedLines)
            {
                foreach (var line in item.Value)
                {
                    if (line.IsOpening)
                    {
                        openingProjections.Add(new VertexAdapter(line.Start, item.Key, boundaryDictionary[item.Key], line));
                        openingProjections.Add(new VertexAdapter(line.End, item.Key, boundaryDictionary[item.Key], line));
                    }
                    else
                    {
                        boundaryProjections.Add(new VertexAdapter(line.Start, item.Key, boundaryDictionary[item.Key], line));
                        boundaryProjections.Add(new VertexAdapter(line.End, item.Key, boundaryDictionary[item.Key], line));
                    }
                }
            }

            foreach (var opening in innerLines)
            {
                var faceProjections = new List<VertexAdapter>();

                foreach (var line in opening)
                {
                    var axis = FindDimensionLine(line, true);
                    var lineAdapter = new LineAdapter(line, true);
                    var startAdapter = new VertexAdapter(line.Start, axis, boundaryDictionary[axis], lineAdapter);
                    faceProjections.Add(startAdapter);
                    var endAdapter = new VertexAdapter(line.End, axis, boundaryDictionary[axis], lineAdapter);
                    faceProjections.Add(endAdapter);
                }

                var openingProjectionsGrouped = faceProjections.GroupBy(g => g.DimensionLine).ToList();
                for (int i = 0; i < openingProjectionsGrouped.Count(); i++)
                {
                    var axis = openingProjectionsGrouped[i].Key;
                    var directGroup = openingProjectionsGrouped[i].ToList();
                    var oppositeGroup = openingProjectionsGrouped.Skip(i + 1).FirstOrDefault(g => g.Key.Direction.IsAlmostEqualTo(axis.Direction.Negate()));
                    if (oppositeGroup != null)
                    {
                        var projections1 = directGroup.ToList();
                        var projections2 = oppositeGroup.ToList();
                        Sort(axis, projections1);
                        Sort(axis, projections2);

                        bool equals = false;
                        if (projections1.Count == projections2.Count)
                        {
                            equals = true;
                            for (int j = 0; j < projections1.Count; j++)
                            {
                                if (!projections1[j].GetProjection(0, Vector3.Origin).IsAlmostEqualTo(projections2[j].GetProjection(0, Vector3.Origin)))
                                {
                                    equals = false;
                                    break;
                                }
                            }
                        }

                        if (equals)
                        {
                            double minDistance1 = directGroup.Min(p => p.Point.DistanceTo(boundaryDictionary[axis]));
                            double minDistance2 = oppositeGroup.Min(p => p.Point.DistanceTo(boundaryDictionary[oppositeGroup.Key]));
                            if (minDistance1 < minDistance2)
                            {
                                // TODO: check witch one is closer
                                openingProjectionsGrouped.Remove(oppositeGroup);
                            }
                            else
                            {
                                openingProjectionsGrouped.RemoveAt(i);
                                i--;
                                continue;
                            }
                        }
                    }
                    for (int j = 0; j < directGroup.Count(); j += 2)
                    {
                        var line = directGroup[j].Line;
                        var normalToLine = line.GetDirection().Cross(Vector3.ZAxis).Unitized();
                        openingProjections.Add(directGroup[j]);
                        openingProjections.Add(directGroup[j + 1]);
                        FindIntersections(line.GetLine(), elementBoundaryLines, normalToLine, out var startPointIntersection, out var endPointIntersection);
                        openingProjections.AddRange(FindClosestProjections(boundaryProjections, directGroup[j], directGroup[j + 1], startPointIntersection, endPointIntersection));
                    }
                }
            }


            foreach (var dimensionLine in supportedDimensionLines)
            {
                var projections = openingProjections.Where(p => p.DimensionLine == dimensionLine).ToList();
                Sort(dimensionLine, projections);
                DrawDimensionLines(doc, dimensionLine, projections, _offsetToDimensionLabel);

                projections = boundaryProjections.Where(p => p.DimensionLine == dimensionLine).ToList();
                Sort(dimensionLine, projections);
                DrawDimensionLines(doc, dimensionLine, projections, _offsetToDimensionLabel * 2);
            }

            foreach (var line in lines)
            {
                doc.Children.Add(line.ToSvgLine(_sceneBounds.Min, ViewBoxHeight, ElementLinesContext));
            }
            foreach (var opening in innerLines)
            {
                foreach (var line in opening)
                {
                    doc.Children.Add(line.ToSvgLine(_sceneBounds.Min, ViewBoxHeight, ElementLinesContext));
                }
            }
        }

        private Dictionary<DimensionLine, List<LineAdapter>> LinkLinesToDimensionLines(IEnumerable<Line> lines)
        {
            var sortedBoundaryEdges = new Dictionary<DimensionLine, List<LineAdapter>>();
            supportedDimensionLines.ForEach(axis => sortedBoundaryEdges.Add(axis, new List<LineAdapter>()));

            foreach (var line in lines)
            {
                var dimensionLine = FindDimensionLine(line, false);
                var normalToLine = line.Direction().Cross(Vector3.ZAxis).Unitized();

                // look for openings lines like this:
                // ___________________
                // |                  |
                // |      _____       |
                // |   ->|     |<-    |
                // |     |     |      |
                // ------       -------
                var rayFromStart = new Ray(line.Start, normalToLine);
                var rayFromEnd = new Ray(line.End, normalToLine);
                bool isOpening = false;

                if (lines.Any(l => rayFromStart.Intersects(l, out var result) && !result.IsAlmostEqualTo(l.Start) && !result.IsAlmostEqualTo(l.End)) ||
                        lines.Any(l => rayFromEnd.Intersects(l, out var result) && !result.IsAlmostEqualTo(l.Start) && !result.IsAlmostEqualTo(l.End)))
                {
                    var oppositeAxis = supportedDimensionLines.FirstOrDefault(l => l.Direction.IsAlmostEqualTo(dimensionLine.Direction.Negate()));
                    if (oppositeAxis != null)
                    {
                        dimensionLine = oppositeAxis;
                        isOpening = true;
                    }
                }
                var lineAdapter = new LineAdapter(line, isOpening);
                sortedBoundaryEdges[dimensionLine].Add(lineAdapter);
            }

            return sortedBoundaryEdges;
        }

        private static List<VertexAdapter> FindClosestProjections(List<VertexAdapter> boundaryProjections, VertexAdapter startAdapter, VertexAdapter endAdapter, LineAdapter startPointIntersection, LineAdapter endPointIntersection)
        {
            var result = new List<VertexAdapter>();
            var startResult = default(VertexAdapter);
            var endResult = default(VertexAdapter);
            double distanceToStartMin = double.MaxValue;
            double distanceToEndMin = double.MaxValue;

            if (startPointIntersection != null)
            {
                var interesectionProjections = boundaryProjections.Where(b => b.Line == startPointIntersection);

                foreach (var item in interesectionProjections)
                {
                    var distance = item.Projection.DistanceTo(startAdapter.Projection);
                    if (distanceToStartMin > distance)
                    {
                        startResult = item;
                        distanceToStartMin = distance;
                    }
                }
            }

            if (endPointIntersection != null)
            {
                var interesectionProjections = boundaryProjections.Where(b => b.Line == endPointIntersection);

                foreach (var item in interesectionProjections)
                {
                    var distance = item.Projection.DistanceTo(endAdapter.Projection);
                    if (distanceToEndMin > distance)
                    {
                        endResult = item;
                        distanceToEndMin = distance;
                    }
                }
            }

            if (startResult == null && endResult == null)
            {
                return result;
            }

            if (startPointIntersection == endPointIntersection)
            {
                if (startResult != null && (distanceToStartMin < distanceToEndMin || distanceToEndMin.ApproximatelyEquals(distanceToStartMin)))
                {
                    result.Add(startResult);
                }
                else if (endResult != null)
                {
                    result.Add(endResult);
                }
            }
            else
            {
                if (startResult != null)
                {
                    result.Add(startResult);
                }

                if (endResult != null)
                {
                    result.Add(endResult);
                }
            }

            return result;
        }

        private void FindIntersections(Line line, List<LineAdapter> lines, Vector3 normalToLine, out LineAdapter startPointIntersection, out LineAdapter endPointIntersection)
        {
            var rayFromStart = new Ray(line.Start, normalToLine);
            var rayFromEnd = new Ray(line.End, normalToLine);

            startPointIntersection = lines.FirstOrDefault(l => rayFromStart.Intersects(l.GetLine(), out var result) && !result.IsAlmostEqualTo(l.Start) && !result.IsAlmostEqualTo(l.End));
            endPointIntersection = lines.FirstOrDefault(l => rayFromEnd.Intersects(l.GetLine(), out var result) && !result.IsAlmostEqualTo(l.Start) && !result.IsAlmostEqualTo(l.End));
        }

        private static Dictionary<DimensionLine, Vector3> FindBoundaries(Dictionary<DimensionLine, List<LineAdapter>> axisDictionary)
        {
            var boundaryDictionary = new Dictionary<DimensionLine, Vector3>();
            foreach (var axis in axisDictionary)
            {
                if (!axis.Value.Any())
                {
                    continue;
                }

                var n = axis.Key.Normal;
                var plane = new Plane(Vector3.Origin, axis.Key.Direction);
                var projections = axis.Value.SelectMany(a => new Vector3[] { a.Start.Project(plane), a.End.Project(plane) }).ToList();
                var max = projections.First();

                foreach (var item in projections.Skip(1))
                {
                    if ((item - max).Unitized().IsAlmostEqualTo(n))
                    {
                        max = item;
                    }
                }

                boundaryDictionary.Add(axis.Key, max);
            }

            return boundaryDictionary;
        }

        private DimensionLine FindDimensionLine(Line line, bool isOpening)
        {
            var lineDirection = line.Direction();
            if (isOpening)
            {
                lineDirection = lineDirection.Negate();
            }

            foreach (var axis in supportedDimensionLines)
            {
                var angle = lineDirection.AngleTo(axis.Direction);

                if (angle.ApproximatelyEquals(0))
                {
                    return axis;
                }

                if (angle >= 0 && angle < 90 && lineDirection.Cross(axis.Direction).IsAlmostEqualTo(Vector3.ZAxis))
                {
                    return axis;
                }
            }

            return supportedDimensionLines.First();
        }

        private void DrawDimensionLines(SvgDocument doc, DimensionLine dimensionLine, List<VertexAdapter> points, double offset)
        {
            Vector3? prevProj = null;
            foreach (var projection in points)
            {
                var newProj = projection.GetProjection(offset, null);
                newProj.Z = 0;
                // draw extension line
                doc.Children.Add(new Line(newProj + dimensionLine.Normal * _offsetDimensionLines, projection.Point + dimensionLine.Normal * _visibleGap).ToSvgLine(_sceneBounds.Min, ViewBoxHeight, DimensionLinesContext));

                if (prevProj.HasValue && prevProj.Value.IsAlmostEqualTo(newProj))
                {
                    continue;
                }

                if (prevProj != null)
                {
                    var lineLength = prevProj.Value.DistanceTo(newProj);
                    var textContent = Units.FeetToFeetAndFractionalInches(Units.MetersToFeet(lineLength), precision: 1 / 8.0);

                    double textHeight = _fontSize * 1.8;
                    double textWidth = _fontSize * textContent.Length * 0.6;
                    var textLocation = (prevProj.Value + newProj) / 2.0;
                    Polygon textRectangle = Polygon.Rectangle(textWidth, textHeight);
                    textRectangle = textRectangle.TransformedPolygon(new Transform(textLocation));

                    // draw dimension line
                    var newLine = new Line(prevProj.Value + dimensionLine.Direction * (-_offsetDimensionLines), newProj + dimensionLine.Direction * _offsetDimensionLines);
                    doc.Children.Add(newLine.ToSvgLine(_sceneBounds.Min, ViewBoxHeight, DimensionLinesContext));

                    var textAngle = newLine.Direction().AngleTo(Vector3.XAxis);
                    if (textAngle.ApproximatelyEquals(90))
                    {
                        textAngle = 270;
                    }
                    else if (textAngle > 90 && textAngle < 180)
                    {
                        textAngle = textAngle + 180;
                    }
                    else if (textAngle >= 180 && textAngle < 270)
                    {
                        textAngle = 180 - textAngle;
                    }

                    // check if leader is required
                    if (lineLength <= textWidth)
                    {
                        var leaderLength = Math.Sqrt(2) * textHeight;
                        var leaderAngle = newLine.Direction().AngleTo(Vector3.XAxis);
                        if (leaderAngle.ApproximatelyEquals(180))
                        {
                            leaderAngle = 0;
                        }
                        var leaderTransform = new Transform(Vector3.Origin, leaderAngle);
                        var leaderEnd = textLocation + leaderTransform.OfVector(new Vector3(0.5, 0.5)) * (leaderLength + textHeight / 2.0);
                        var leaderLine = new Line(textLocation, leaderEnd);
                        doc.Children.Add(leaderLine.ToSvgLine(_sceneBounds.Min, ViewBoxHeight, DimensionLinesContext));
                        doc.Children.Add(new Line(leaderEnd, leaderEnd + leaderTransform.OfVector(Vector3.XAxis) * 0.1).ToSvgLine(_sceneBounds.Min, ViewBoxHeight, DimensionLinesContext));
                        textLocation = textLocation + (dimensionLine.IsNegative ? dimensionLine.Normal : dimensionLine.Normal.Negate()) * (textHeight);
                        textLocation = textLocation + leaderTransform.OfVector(Vector3.XAxis) * (0.1 + textHeight + textWidth / 2.0);
                    }
                    else
                    {
                        textLocation = textLocation + (dimensionLine.IsNegative ? dimensionLine.Normal : dimensionLine.Normal.Negate()) * 0.1;
                    }
                    var text = CreateText(textContent, textLocation, textAngle);
                    doc.Children.Add(text);
                }

                var dimLineLength = Math.Sqrt(2) * _offsetDimensionLines;
                var dimLine = new Line(newProj + new Vector3(-0.5, 0.5) * dimLineLength, newProj + new Vector3(0.5, -0.5) * dimLineLength);
                doc.Children.Add(dimLine.ToSvgLine(_sceneBounds.Min, ViewBoxHeight, DimensionLinesContext));
                prevProj = newProj;
            }
        }

        private static void Sort(DimensionLine dimensionLine, List<VertexAdapter> points)
        {
            points.Sort(new VerticesOnVectorComparer(dimensionLine.Direction));
        }

        private SvgText CreateText(string text, Vector3 location, double angle)
        {
            var x = location.X.ToXUserUnit(_sceneBounds.Min);
            var y = location.Y.ToYUserUnit(ViewBoxHeight, _sceneBounds.Min);

            var svgText = new SvgText(text)
            {
                X = new SvgUnitCollection() { x },
                Y = new SvgUnitCollection() { y },
                TextAnchor = SvgTextAnchor.Middle,
                // move text by Y axis approximately one half of its height
                Transforms = new Svg.Transforms.SvgTransformCollection() { new SvgRotate((float)angle, x.Value, y.Value), new SvgTranslate(0, 0.03f) }
            };
            svgText.CustomAttributes.Add("style", $"font-family: Arial; font-size: {_fontSize}; fill:black");

            return svgText;
        }

        #endregion
    }
}