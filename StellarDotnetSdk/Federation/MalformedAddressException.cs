using System;

namespace StellarDotnetSdk.Federation;

/// <summary>
///     Thrown when a Stellar federation address is not in the expected format (e.g., "name*domain").
/// </summary>
public class MalformedAddressException : Exception
{
}