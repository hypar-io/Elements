using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Dimensions
{
    /// <summary>
    /// A linear dimension aligned along the line between the specified start and end.
    /// </summary>
    public class AlignedDimension : LinearDimension
    {
        /// <summary>
        /// Create an aligned dimension from JSON.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="referencePlane"></param>
        /// <param name="plane"></param>
        [JsonConstructor]
        public AlignedDimension(Vector3 start,
                                   Vector3 end,
                                   Plane referencePlane,
                                   Plane plane = null) : base(start,
                                                              end,
                                                              referencePlane,
                                                              plane)
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
                                Plane plane = null,
                                double offset = 0.0) : base()
        {
            this.Plane = plane ?? new Plane(Vector3.Origin, Vector3.ZAxis);
            this.Start = start.Project(this.Plane);
            this.End = end.Project(this.Plane);
            var vRef = (this.End - this.Start).Unitized();
            var offsetDirection = vRef.Cross(this.Plane.Normal);
            this.ReferencePlane = new Plane(this.Start + offsetDirection * offset, offsetDirection);
        }
    }
}