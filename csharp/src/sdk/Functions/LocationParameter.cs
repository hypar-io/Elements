namespace Hypar.Functions
{
    /// <summary>
    /// A location parameter.
    /// </summary>
    public class LocationParameter: InputOutputBase
    {
        /// <summary>
        /// Construct a location parameter.
        /// </summary>
        /// <param name="description"></param>
        public LocationParameter(string description):base(description, HyparParameterType.Location){}
    }
}