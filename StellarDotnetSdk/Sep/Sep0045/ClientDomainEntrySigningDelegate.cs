using System.Threading.Tasks;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Sep.Sep0045;

/// <summary>
/// Async delegate that signs a single SorobanAuthorizationEntry with the client-domain key
/// (typically used when the client-domain private key lives on a remote signing service).
/// </summary>
/// <param name="entry">The SorobanAuthorizationEntry to sign.</param>
/// <returns>The signed SorobanAuthorizationEntry.</returns>
public delegate Task<SorobanAuthorizationEntry> ClientDomainEntrySigningDelegate(
    SorobanAuthorizationEntry entry);
