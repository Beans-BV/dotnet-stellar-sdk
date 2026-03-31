using System;

namespace StellarDotnetSdk.Federation;

/// <inheritdoc />
/// <summary>
///     Federation server is invalid (malformed URL, not HTTPS, etc.)
/// </summary>
public class FederationServerInvalidException : Exception
{
    /// <inheritdoc />
    public override string Message => "Federation server is invalid (malformed URL, not HTTPS, etc.";
}