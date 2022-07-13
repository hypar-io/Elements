using System;
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
        /// The start and end points will be projected onto the dimension line.
        /// </summary>
        /// <param name="start">The start of the dimension.</param>
        /// <param name="end">The end of the dimension.</param>
        /// <param name="dimensionLine">A line on which the start and end points 
        /// will be projected.</param>
        public ContinuousDimension(Vector3 start,
                                   Vector3 end,
                                   Line dimensionLine) : base()
        {
            var refDirection = (dimensionLine.End - dimensionLine.Start).Unitized();

            var temp = Math.Abs(refDirection.Dot(Vector3.ZAxis)).ApproximatelyEquals(1.0) ? Vector3.XAxis : Vector3.ZAxis;
            var referenceNormal = refDirection.Cross(temp).Negate();

            this.Start = start;
            this.End = end;

            this.ReferencePlane = new Plane(dimensionLine.Start, referenceNormal);
        }
    }
}