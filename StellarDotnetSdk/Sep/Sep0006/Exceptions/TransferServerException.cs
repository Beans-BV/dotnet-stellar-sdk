using System;

namespace StellarDotnetSdk.Sep.Sep0006.Exceptions;

/// <summary>
///     Base exception for SEP-6 transfer server errors.
///     Thrown when a transfer server operation fails.
/// </summary>
public class TransferServerException : Exception
{
    public TransferServerException(string message)
        : base(message)
    {
    }

    public TransferServerException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

