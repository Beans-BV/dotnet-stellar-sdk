using System.Threading.Tasks;

namespace StellarDotnetSdk.Sep.Sep0010;

/// <summary>
///     Delegate for signing challenge transactions with a client domain's signing key.
///     Used when the client domain key is stored securely elsewhere (e.g., HSM, remote signing service).
/// </summary>
/// <param name="transactionXdr">The base64-encoded XDR transaction envelope to sign</param>
/// <returns>The signed transaction envelope as base64-encoded XDR</returns>
public delegate Task<string> ClientDomainSigningDelegate(string transactionXdr);