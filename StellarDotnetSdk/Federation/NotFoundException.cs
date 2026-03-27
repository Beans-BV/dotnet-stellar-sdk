using System;

namespace StellarDotnetSdk.Federation;

/// <summary>
///     Thrown when a federation server returns a 404 response, indicating the requested address was not found.
/// </summary>
public class NotFoundException : Exception
{
}