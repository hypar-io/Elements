using System.Collections.Generic;

namespace Hypar.Elements
{
    /// <summary>
    /// Interface to be implemented by all classes which extract data from the model.
    /// </summary>
    public interface IDataExtractor
    {
        /// <summary>
        /// Extract data from the provided model.
        /// </summary>
        /// <param name="m">A model.</param>
        /// <returns></returns>
        double ExtractData(Model m);
    }
}