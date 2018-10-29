namespace Hypar.Functions
{
    /// <summary>
    /// A point parameter.
    /// </summary>
    public class PointParameter: InputOutputBase
    {
        /// <summary>
        /// Construct a point parameter.
        /// </summary>
        /// <param name="description">The description of the point.</param>
        public PointParameter(string description):base(description, HyparParameterType.Point){}
    }
}