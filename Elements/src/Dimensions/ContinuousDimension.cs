using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Dimensions
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
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="referencePlane"></param>
        /// <param name="plane"></param>
        [JsonConstructor]
        public ContinuousDimension(Vector3 start,
                                   Vector3 end,
                                   Plane referencePlane,
                                   Plane plane = null) : base(start,
                                                              end,
                                                              referencePlane,
                                                              plane)
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
            this.Plane = plane ?? new Plane(Vector3.Origin, Vector3.ZAxis);
            this.Start = start.Project(this.Plane);
            this.End = end.Project(this.Plane);
            Vector3 vRef;
            if (dimensionLine != null)
            {
                vRef = (dimensionLine.End.Project(this.Plane) - dimensionLine.Start.Project(this.Plane)).Unitized();
            }
            else
            {
                vRef = (this.End - this.Start).Unitized();
            }
            this.ReferencePlane = new Plane(dimensionLine.Start, this.Plane.Normal.Cross(vRef));
        }
    }
}