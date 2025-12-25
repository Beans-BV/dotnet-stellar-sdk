using System;

namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Base exception for all SEP-0010 Web Authentication errors.
/// </summary>
public class WebAuthException : Exception
{
    public WebAuthException(string message)
        : base(message)
    {
    }

    public WebAuthException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}