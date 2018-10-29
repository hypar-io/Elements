namespace Hypar.Functions
{
    /// <summary>
    /// A numeric parameter.
    /// </summary>
    public class NumberParameter: InputOutputBase
    {
        /// <summary>
        ///  Construct a NumberParameter.
        /// </summary>
        /// <param name="description"></param>
        public NumberParameter(string description) : base(description, HyparParameterType.Number){}
    }
}