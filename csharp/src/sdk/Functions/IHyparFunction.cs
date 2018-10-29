using Hypar.Elements;
using System.Collections.Generic;

namespace Hypar.Functions
{
    /// <summary>
    /// An interface which describes a Hypar function.
    /// </summary>
    public interface IHyparFunction
    {
        /// <summary>
        /// Execute the function.
        /// </summary>
        /// <param name="model">The Model to be used as an input to the function.</param>
        /// <param name="parameters">A map of parameters to the function. The keys of this map will correspond to the names of the Parameters in the HyparConfig.</param>
        /// <param name="returns">A map of return values from the function. The keys of this map will correspond to the names of the Returns in the HyparConfig.</param>
        /// <returns>A Model. This can be the same Model as was supplied or a new Model.</returns>
        Model Execute(Model model, Dictionary<string,object> parameters, Dictionary<string,object> returns);
    }
}