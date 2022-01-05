using System;

namespace Elements
{
    /// <summary>
    /// Represents error that occur inside an element during application execution
    /// </summary>
    public class ElementError : BaseError
    {
        /// <summary>
        /// Initializes a new instance of ElementError class
        /// </summary>
        /// <param name="id">The element Id</param>
        /// <param name="exception">The exception that occured during application execution</param>
        public ElementError(Guid id, Exception exception) : base(exception)
        {
            ElementId = id;
        }

        /// <summary>
        /// Gets element Id where the error occured
        /// </summary>
        public Guid ElementId { get; }
    }
}