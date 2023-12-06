using System;
using System.Text.Json.Serialization;
using Elements.Geometry;

namespace Elements.Annotations
{
    /// <summary>
    /// A linear dimension aligned along the line between the specified start and end.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/DimensionTests.cs?name=aligned_dimension_example)]
    /// </example>
    public class AlignedDimension : LinearDimension
    {
        /// <summary>
        /// Create an aligned dimension from JSON.
        /// </summary>
        /// <param name="start">The start of the dimension.</param>
        /// <param name="end">The end of the dimension.</param>
        /// <param name="referencePlane">The plane on which the dimension is projected.</param>
        /// <param name="prefix">Text that appears before the dimension's value.</param>
        /// <param name="suffix">Text that appears after the dimension's value.</param>
        /// <param name="displayValue">Text that appears in place of the dimension's value.</param>
        [JsonConstructor]
        public AlignedDimension(Vector3 start,
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
        /// Create a linear dimension where the reference line is created
        /// by offsetting from the line created between start and end 
        /// by the provided value.
        /// </summary>
        /// <param name="start">The start of the dimension.</param>
        /// <param name="end">The end of the dimension.</param>
        /// <param name="offset">The offset of the reference line.</param>
        /// <param name="offsetDirection">The direction in which the annotation will
        /// be offset from the dimension line.</param>
        public AlignedDimension(Vector3 start,
                                Vector3 end,
                                double offset = 0.0,
                                Vector3 offsetDirection = default) : base()
        {
            this.Start = start;
            this.End = end;

            var dimDirection = (end - start).Unitized();
            if (offsetDirection == default)
            {
                var temp = Math.Abs(dimDirection.Dot(Vector3.ZAxis)).ApproximatelyEquals(1.0) ? Vector3.XAxis : Vector3.ZAxis;
                offsetDirection = dimDirection.Cross(temp);
            }

            this.ReferencePlane = new Plane(this.Start + offsetDirection * offset, offsetDirection);
        }
    }
}