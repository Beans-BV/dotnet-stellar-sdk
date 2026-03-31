using StellarDotnetSdk.Accounts;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents version 1 extensions to a generic ledger entry, containing the sponsoring account ID.
/// </summary>
public class LedgerEntryExtensionV1
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LedgerEntryExtensionV1" /> class.
    /// </summary>
    /// <param name="sponsoringId">The account that is sponsoring this ledger entry.</param>
    public LedgerEntryExtensionV1(KeyPair sponsoringId)
    {
        SponsoringId = sponsoringId;
    }

    /// <summary>
    ///     The account ID of the sponsor that is paying the reserve for this ledger entry.
    /// </summary>
    public KeyPair SponsoringId { get; }

    /// <summary>
    ///     Creates a <see cref="LedgerEntryExtensionV1" /> from an XDR
    ///     <see cref="Xdr.LedgerEntryExtensionV1" /> object.
    /// </summary>
    /// <param name="xdrEntryExtensionV1">The XDR extension object.</param>
    /// <returns>A <see cref="LedgerEntryExtensionV1" /> instance.</returns>
    public static LedgerEntryExtensionV1 FromXdr(Xdr.LedgerEntryExtensionV1 xdrEntryExtensionV1)
    {
        return new LedgerEntryExtensionV1(
            KeyPair.FromXdrPublicKey(xdrEntryExtensionV1.SponsoringID.InnerValue.InnerValue));
    }
}