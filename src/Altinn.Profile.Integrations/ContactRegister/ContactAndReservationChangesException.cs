﻿using System.Diagnostics.CodeAnalysis;

namespace Altinn.Profile.Integrations.ContactRegister;

/// <summary>
/// Represents errors that occur during order processing operations.
/// </summary>
[ExcludeFromCodeCoverage]
public class ContactAndReservationChangesException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContactAndReservationChangesException"/> class.
    /// </summary>
    public ContactAndReservationChangesException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactAndReservationChangesException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ContactAndReservationChangesException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactAndReservationChangesException"/> class
    /// with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public ContactAndReservationChangesException(string message, Exception inner) : base(message, inner)
    {
    }
}
