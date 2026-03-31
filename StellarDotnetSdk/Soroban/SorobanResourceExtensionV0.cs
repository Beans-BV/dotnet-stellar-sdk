using System.Linq;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Soroban;

/// <summary>
///     Represents the version 0 extension for Soroban resources, containing references to archived Soroban ledger entries.
/// </summary>
public class SorobanResourceExtensionV0
{
    public SorobanResourceExtensionV0(uint[] archivedSorobanEntries)
    {
        ArchivedSorobanEntries = archivedSorobanEntries;
    }

    public uint[] ArchivedSorobanEntries { get; }

    /// <summary>
    ///     Converts this instance to its XDR representation.
    /// </summary>
    /// <returns>A <see cref="SorobanResourcesExtV0" /> XDR object.</returns>
    public SorobanResourcesExtV0 ToXdr()
    {
        return new SorobanResourcesExtV0
        {
            ArchivedSorobanEntries = ArchivedSorobanEntries.Select(x => new Uint32(x)).ToArray(),
        };
    }

    public static SorobanResourceExtensionV0 FromXdr(SorobanResourcesExtV0 xdr)
    {
        return new SorobanResourceExtensionV0(
            xdr.ArchivedSorobanEntries.Select(x => x.InnerValue).ToArray()
        );
    }
}