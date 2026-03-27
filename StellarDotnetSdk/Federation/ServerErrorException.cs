using System;

namespace StellarDotnetSdk.Federation;

/// <summary>
///     Thrown when a federation server returns a non-404 HTTP error response.
/// </summary>
public class ServerErrorException : Exception
{
}