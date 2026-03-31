using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents version 1 extensions to a trustline entry, including liabilities (buying and selling).
/// </summary>
public class TrustlineEntryExtensionV1
{
    private TrustlineEntryExtensionV1(Liabilities liabilities)
    {
        Liabilities = liabilities;
    }

    /// <summary>
    ///     The buying and selling liabilities for this trustline.
    /// </summary>
    public Liabilities Liabilities { get; }

    /// <summary>
    ///     Version 2 extension fields for this trustline entry, if present.
    /// </summary>
    public TrustLineEntryExtensionV2? TrustlineExtensionV2 { get; private set; }

    /// <summary>
    ///     Creates a <see cref="TrustlineEntryExtensionV1" /> from an XDR
    ///     <see cref="TrustLineEntry.TrustLineEntryExt.TrustLineEntryV1" /> object.
    /// </summary>
    /// <param name="xdrExtensionV1">The XDR extension object.</param>
    /// <returns>A <see cref="TrustlineEntryExtensionV1" /> instance.</returns>
    public static TrustlineEntryExtensionV1 FromXdr(TrustLineEntry.TrustLineEntryExt.TrustLineEntryV1 xdrExtensionV1)
    {
        var entryExtensionV1 = new TrustlineEntryExtensionV1(Liabilities.FromXdr(xdrExtensionV1.Liabilities));
        if (xdrExtensionV1.Ext.Discriminant == 2)
        {
            entryExtensionV1.TrustlineExtensionV2 = TrustLineEntryExtensionV2.FromXdr(xdrExtensionV1.Ext.V2);
        }

        return entryExtensionV1;
    }
}