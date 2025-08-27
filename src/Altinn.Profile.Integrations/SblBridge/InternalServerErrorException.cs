namespace Altinn.Profile.Integrations.SblBridge
{
    /// <summary>
    /// Represents an exception that occurs when an internal server error is encountered.
    /// </summary>
    public class InternalServerErrorException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InternalServerErrorException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InternalServerErrorException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalServerErrorException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public InternalServerErrorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
