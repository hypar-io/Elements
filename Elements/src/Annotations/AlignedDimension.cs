using Elements.Geometry;
using Newtonsoft.Json;

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
        /// <param name="plane">The plane in which the dimension is created.</param>
        /// <param name="start">The start of the dimension.</param>
        /// <param name="end">The end of the dimension.</param>
        /// <param name="offset">The offset of the reference line.</param>
        public AlignedDimension(Vector3 start,
                                Vector3 end,
                                double offset = 0.0,
                                Plane plane = null) : base()
        {
            if (plane == null)
            {
                plane = new Plane(Vector3.Origin, Vector3.ZAxis);
            }

            this.Start = start.Project(plane);
            this.End = end.Project(plane);

            if (this.Start.DistanceTo(this.End).ApproximatelyEquals(0.0))
            {
                // The vector was collapsed onto the plane.
                throw new System.Exception("The start and end points of the dimension are equal when projected to the plane.");
            }

            var vRef = (this.End - this.Start).Unitized();
            var offsetDirection = vRef.Cross(plane.Normal);

            this.ReferencePlane = new Plane(this.Start + offsetDirection * offset, offsetDirection);
        }
    }
}