using System;

namespace StellarDotnetSdk.Sep.Sep0024.Exceptions;

/// <summary>
///     Exception thrown when the requested SEP-24 transaction cannot be found.
///     This exception is thrown when querying the /transaction endpoint with an ID, stellar_transaction_id,
///     or external_transaction_id that doesn't exist or doesn't belong to the authenticated account.
/// </summary>
public class TransactionNotFoundException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionNotFoundException" /> class.
    /// </summary>
    public TransactionNotFoundException()
        : base("The anchor could not find the transaction.")
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionNotFoundException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public TransactionNotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionNotFoundException" /> class with a specified error message
    ///     and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public TransactionNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

