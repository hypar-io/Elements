using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Annotations
{
    /// <summary>
    /// A linear dimension where start and end are projected onto the dimension line.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/DimensionTests.cs?name=continuous_dimension_example)]
    /// </example>
    public class ContinuousDimension : LinearDimension
    {
        /// <summary>
        /// Create a continuous dimension from JSON.
        /// </summary>
        /// <param name="start">The start of the dimension.</param>
        /// <param name="end">The end of the dimension.</param>
        /// <param name="referencePlane">The plane on which the dimension will be projected.</param>
        /// <param name="prefix">Text that appears before the dimension's value.</param>
        /// <param name="suffix">Text that appears after the dimension's value.</param>
        /// <param name="displayValue">Text that appears in place of the dimension's value.</param>
        [JsonConstructor]
        public ContinuousDimension(Vector3 start,
                                   Vector3 end,
                                   Plane referencePlane,
                                   string prefix = null,
                                   string suffix = null,
                                   string displayValue = null) : base(start,
                                                              end,
                                                              referencePlane,
                                                              prefix,
                                                              suffix,
                                                              displayValue)
        { }

        /// <summary>
        /// Create a continuous dimension with an optional reference line.
        /// If a reference line is provided, the start and end points will
        /// be projected onto the reference line.
        /// </summary>
        /// <param name="plane">The plane in which the dimension is measured.</param>
        /// <param name="start">The start of the dimension.</param>
        /// <param name="end">The end of the dimension.</param>
        /// <param name="dimensionLine">A line on which the start and end points 
        /// will be projected.</param>
        public ContinuousDimension(Vector3 start,
                                   Vector3 end,
                                   Line dimensionLine,
                                   Plane plane = null) : base()
        {
            if (plane == null)
            {
                plane = new Plane(Vector3.Origin, Vector3.ZAxis);
            }
            this.Start = start.Project(plane);
            this.End = end.Project(plane);
            Vector3 vRef;
            if (dimensionLine != null)
            {
                vRef = (dimensionLine.End.Project(plane) - dimensionLine.Start.Project(plane)).Unitized();
            }
            else
            {
                vRef = (this.End - this.Start).Unitized();
            }
            this.ReferencePlane = new Plane(dimensionLine.Start, plane.Normal.Cross(vRef));
        }
    }
}