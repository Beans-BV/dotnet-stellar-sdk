using System;

namespace StellarDotnetSdk.Sep.Sep0045.Exceptions;

/// <summary>Base exception for all SEP-45 (Web Authentication for Contract Accounts) errors.</summary>
public abstract class WebAuthContractException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="WebAuthContractException" /> class.</summary>
    /// <param name="message">The error message.</param>
    protected WebAuthContractException(string message) : base(message) { }

    /// <summary>Initializes a new instance of the <see cref="WebAuthContractException" /> class.</summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The underlying cause of the exception.</param>
    protected WebAuthContractException(string message, Exception innerException) : base(message, innerException) { }
}
