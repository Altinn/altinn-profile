namespace Altinn.Profile.Integrations.Register
{
    /// <summary>
    /// Represents an exception that is thrown when a party cannot be found in the register.
    /// </summary>
    public class PartyNotFoundException : RegisterException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PartyNotFoundException"/> class.
        /// </summary>
        public PartyNotFoundException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyNotFoundException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public PartyNotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyNotFoundException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception.</param>
        public PartyNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
