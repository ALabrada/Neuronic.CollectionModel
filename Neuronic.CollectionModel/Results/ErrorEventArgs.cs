using System;

namespace Neuronic.CollectionModel.Results
{
    /// <summary>
    /// Parameters for error events.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ErrorEventArgs: EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorEventArgs"/> class.
        /// </summary>
        /// <param name="error">The error.</param>
        public ErrorEventArgs(Exception error)
        {
            Error = error;
        }

        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public Exception Error { get; }
    }
}