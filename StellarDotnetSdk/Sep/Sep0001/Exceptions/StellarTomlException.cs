using System;

namespace StellarDotnetSdk.Sep.Sep0001.Exceptions;

/// <summary>
///     Exception thrown when stellar.toml parsing or fetching fails.
/// </summary>
public class StellarTomlException : Exception
{
    public StellarTomlException(string message)
        : base(message)
    {
    }

    public StellarTomlException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}