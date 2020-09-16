namespace Elements.Generate
{
    /// <summary>
    /// The result of code generation.
    /// </summary>
    public struct GenerationResult
    {
        /// <summary>
        /// True if the code was generated successfully.
        /// </summary>
        public bool Success { get; internal set; }
        /// <summary>
        /// The file path to the generated code.
        /// </summary>
        public string FilePath { get; internal set; }
        /// <summary>
        /// Any messages or errors that arose during code generation.
        /// </summary>
        public string[] DiagnosticResults { get; internal set; }
    }
}