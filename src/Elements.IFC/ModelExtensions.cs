namespace Elements.IFC
{
    public static class ModelExtensions
    {
        /// <summary>
        /// Create a model from IFC.
        /// </summary>
        /// <param name="path">The path to the IFC STEP file.</param>
        /// <param name="idsToConvert">An optional array of string identifiers 
        /// of IFC entities to convert.</param>
        /// <returns>A model.</returns>
        public static Model FromIFC(string path, string[] idsToConvert = null)
        {
            return IFCExtensions.FromIFC(path, idsToConvert);
        }
    }
}