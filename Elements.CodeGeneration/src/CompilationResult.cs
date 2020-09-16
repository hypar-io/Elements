using System.Reflection;

namespace Elements.Generate
{
    /// <summary>
    /// The result of a compilation.
    /// </summary>
    public struct CompilationResult
    {
        /// <summary>
        /// True if the compilation succeeded.
        /// </summary>
        public bool Success { get; internal set; }
        /// <summary>
        /// The Assembly loaded from the compilation, if successful.
        /// </summary>
        public Assembly Assembly { get; internal set; }
        /// <summary>
        /// Any messages or errors that arose during compilation.
        /// </summary>
        public string[] DiagnosticResults { get; internal set; }
    }
}