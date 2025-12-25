using System;

namespace StellarDotnetSdk.Sep.Sep0010.Exceptions;

/// <summary>
///     Exception thrown when WebAuthentication verification fails.
/// </summary>
public class InvalidWebAuthenticationException : Exception
{
    public InvalidWebAuthenticationException(string message) : base(message)
    {
    }
}