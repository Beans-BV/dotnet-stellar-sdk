using System.Linq;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Soroban;

/// <summary>
///     Represents the version 0 extension for Soroban resources, containing references to archived Soroban ledger entries.
/// </summary>
public class SorobanResourceExtensionV0
{
    /// <summary>
    ///     Initializes a new <see cref="SorobanResourceExtensionV0" /> with references to archived Soroban ledger entries.
    /// </summary>
    /// <param name="archivedSorobanEntries">The indices of archived Soroban ledger entries.</param>
    public SorobanResourceExtensionV0(uint[] archivedSorobanEntries)
    {
        ArchivedSorobanEntries = archivedSorobanEntries;
    }

    /// <summary>
    ///     The indices of archived Soroban ledger entries referenced by this extension.
    /// </summary>
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

    /// <summary>
    ///     Creates a new <see cref="SorobanResourceExtensionV0" /> from an XDR <see cref="SorobanResourcesExtV0" /> object.
    /// </summary>
    /// <param name="xdr">The XDR resource extension to convert.</param>
    /// <returns>A <see cref="SorobanResourceExtensionV0" /> instance.</returns>
    public static SorobanResourceExtensionV0 FromXdr(SorobanResourcesExtV0 xdr)
    {
        return new SorobanResourceExtensionV0(
            xdr.ArchivedSorobanEntries.Select(x => x.InnerValue).ToArray()
        );
    }
}