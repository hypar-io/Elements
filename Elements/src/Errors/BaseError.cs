using System;

namespace Elements
{
    /// <summary>
    /// Represents error that occur during application execution
    /// </summary>
    public class BaseError
    {
        /// <summary>
        /// Initializes a new instance of BaseError class base on System.Exception info
        /// </summary>
        /// <param name="exception">s</param>
        public BaseError(Exception exception) : this(exception.Message, exception.StackTrace) { }

        /// <summary>
        /// Initializes a new instance of BaseError class
        /// </summary>
        /// <param name="message">The error message</param>
        public BaseError(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of BaseError class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="stackTrace">A string representation of the immediate frames on the call stack</param>
        public BaseError(string message, string stackTrace) : this(message)
        {
            StackTrace = stackTrace;
        }

        /// <summary>
        /// Gets a message that describes the current error
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets a string representation of the immediate frames on the call stack
        /// </summary>
        public string StackTrace { get; }
    }
}