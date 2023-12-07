using System.Collections.Generic;
using System.Text.Json.Serialization;
using Elements.Geometry;

namespace Elements.Annotations
{
    /// <summary>
    /// A linear dimension.
    /// </summary>
    public abstract class LinearDimension : Annotation, IOverrideLinked
    {
        /// <summary>
        /// The start of the dimension.
        /// </summary>
        public Vector3 Start { get; protected set; }

        /// <summary>
        /// The end of the dimension.
        /// </summary>
        public Vector3 End { get; protected set; }

        /// <summary>
        /// The plane on which the start and end points are projected.
        /// </summary>
        public Plane ReferencePlane { get; protected set; }

        /// <summary>
        /// Information about the element and property that this dimension is linked to, if any.
        /// </summary>
        public LinkedPropertyInfo LinkedProperty { get; set; } = null;

        /// <summary>
        /// Create a linear dimension with a reference plane.
        /// </summary>
        /// <param name="prefix">Text to be displayed before the dimension's value.</param>
        /// <param name="suffix">Text to be displayed after the dimension's value.</param>
        /// <param name="displayValue">Text to be displayed in place of the dimension's value.</param>
        /// <param name="start">The start of the dimension.</param>
        /// <param name="end">The end of the dimension.</param>
        /// <param name="referencePlane">The plane on which the start and end
        /// points will be projected.</param>
        [JsonConstructor]
        public LinearDimension(Vector3 start,
                               Vector3 end,
                               Plane referencePlane,
                               string prefix = null,
                               string suffix = null,
                               string displayValue = null) : base()
        {
            this.Start = start;
            this.End = end;
            this.ReferencePlane = referencePlane;
            this.Prefix = prefix;
            this.Suffix = suffix;
            this.DisplayValue = displayValue;
        }

        /// <summary>
        /// Default constructor for a linear dimension.
        /// </summary>
        internal LinearDimension() : base() { }

        /// <summary>
        /// Draw the dimension.
        /// </summary>
        public List<Element> ToModelArrowsAndText(Color color)
        {
            if (color == default)
            {
                color = Colors.Black;
            }

            var modelArrowData = new List<(Vector3, Vector3, double, Color?)>();
            var textData = new List<(Vector3, Vector3, Vector3, string, Color?)>();
            var modelCurves = new List<ModelCurve>();
            Draw(color, modelArrowData, textData, modelCurves);
            var elements = new List<Element>();
            elements.AddRange(modelCurves);
            elements.Add(new ModelText(textData, FontSize.PT24));
            elements.Add(new ModelArrows(modelArrowData, true, true));
            return elements;
        }

        private void Draw(Color color,
                          List<(Vector3, Vector3, double, Color?)> modelArrowData,
                          List<(Vector3, Vector3, Vector3, string, Color?)> textData,
                          List<ModelCurve> modelCurves)
        {
            var measureLineDirection = (this.End - this.Start).Unitized();
            var up = this.ReferencePlane.Normal.Cross(measureLineDirection);
            var projectDirection = up.Cross(measureLineDirection);
            var dimStart = this.Start.ProjectAlong(projectDirection, this.ReferencePlane);
            var dimEnd = this.End.ProjectAlong(projectDirection, this.ReferencePlane);
            var dimDirection = (dimEnd - dimStart).Unitized();

            modelArrowData.Add((dimStart, dimDirection, dimStart.DistanceTo(dimEnd), color));

            var c = new Material("Dimension", color, unlit: true);

            if (dimStart.DistanceTo(this.Start) > 0)
            {
                modelCurves.Add(new ModelCurve(new Line(this.Start, dimStart), c));
            }
            if (dimEnd.DistanceTo(this.End) > 0)
            {
                modelCurves.Add(new ModelCurve(new Line(this.End, dimEnd), c));
            }

            // Always try to make the direction vector point in positive x, y, and z.
            var lineDirection = dimDirection.Dot(new Vector3(1, 1, 1)) > 0 ? dimDirection : dimDirection.Negate();

            var value = $"{this.Prefix ?? string.Empty}{this.DisplayValue ?? dimStart.DistanceTo(dimEnd).ToString("0.00")}{this.Suffix ?? string.Empty}";
            var normal = this.ReferencePlane.Normal.Cross(dimDirection);
            textData.Add((dimStart.Average(dimEnd), normal, lineDirection, value, color));
        }

        /// <summary>
        /// Draw a set of dimensions.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="dimensions"></param>
        public static List<Element> ToModelArrowsAndTexts(IList<LinearDimension> dimensions, Color color = default)
        {
            var modelArrows = new List<(Vector3, Vector3, double, Color?)>();
            var texts = new List<(Vector3, Vector3, Vector3, string, Color?)>();
            var modelCurves = new List<ModelCurve>();
            foreach (var d in dimensions)
            {
                d.Draw(color, modelArrows, texts, modelCurves);
            }
            var elements = new List<Element>();
            elements.AddRange(modelCurves);
            elements.Add(new ModelText(texts, FontSize.PT24));
            elements.Add(new ModelArrows(modelArrows, true, true));

            return elements;
        }
    }
}