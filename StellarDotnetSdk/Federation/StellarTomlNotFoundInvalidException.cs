using System;

namespace StellarDotnetSdk.Federation;

/// <summary>
///     Thrown when the stellar.toml file could not be found or returned a non-success HTTP status
///     at the well-known URL (<c>https://{domain}/.well-known/stellar.toml</c>).
/// </summary>
public class StellarTomlNotFoundInvalidException : Exception
{
}