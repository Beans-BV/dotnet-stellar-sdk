using System;

namespace StellarDotnetSdk.Federation;

/// <summary>
///     Federation server was not found in stellar.toml file.
/// </summary>
internal class NoFederationServerException : Exception
{
}