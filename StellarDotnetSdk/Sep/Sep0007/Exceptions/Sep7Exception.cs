using System;

namespace StellarDotnetSdk.Sep.Sep0007.Exceptions;

/// <summary>Base exception for all SEP-7 (URI Scheme to facilitate delegated signing) errors.</summary>
public abstract class Sep7Exception : Exception
{
    /// <summary>Initializes a new instance of the <see cref="Sep7Exception" /> class.</summary>
    /// <param name="message">The error message.</param>
    protected Sep7Exception(string message) : base(message) { }

    /// <summary>Initializes a new instance of the <see cref="Sep7Exception" /> class.</summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The underlying cause of the exception.</param>
    protected Sep7Exception(string message, Exception innerException) : base(message, innerException) { }
}
