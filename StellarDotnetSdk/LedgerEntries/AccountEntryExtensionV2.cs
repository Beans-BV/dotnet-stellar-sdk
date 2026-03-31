using System.Linq;
using StellarDotnetSdk.Accounts;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents version 2 extensions to a Stellar account entry, including sponsorship counts and signer sponsoring IDs.
/// </summary>
public class AccountEntryExtensionV2
{
    private AccountEntryExtensionV2(uint numberSponsored, uint numberSponsoring, KeyPair?[] signerSponsoringIDs)
    {
        NumberSponsored = numberSponsored;
        NumberSponsoring = numberSponsoring;
        SignerSponsoringIDs = signerSponsoringIDs;
    }

    /// <summary>
    ///     The number of entries this account is sponsoring (reserves are being paid by this account).
    /// </summary>
    public uint NumberSponsored { get; }

    /// <summary>
    ///     The number of entries sponsoring this account (reserves paid by other accounts).
    /// </summary>
    public uint NumberSponsoring { get; }

    /// <summary>
    ///     The sponsoring account IDs for each of this account's signers. A <c>null</c> entry means no sponsor.
    /// </summary>
    public KeyPair?[] SignerSponsoringIDs { get; }

    /// <summary>
    ///     Version 3 extension fields for this account entry, if present.
    /// </summary>
    public AccountEntryExtensionV3? ExtensionV3 { get; private set; }

    /// <summary>
    ///     Creates an <see cref="AccountEntryExtensionV2" /> from an XDR
    ///     <see cref="Xdr.AccountEntryExtensionV2" /> object.
    /// </summary>
    /// <param name="xdrExtensionV2">The XDR extension object.</param>
    /// <returns>An <see cref="AccountEntryExtensionV2" /> instance.</returns>
    public static AccountEntryExtensionV2 FromXdr(Xdr.AccountEntryExtensionV2 xdrExtensionV2)
    {
        var entryExtensionV2 = new AccountEntryExtensionV2(
            xdrExtensionV2.NumSponsored.InnerValue,
            xdrExtensionV2.NumSponsoring.InnerValue,
            xdrExtensionV2
                .SignerSponsoringIDs
                .Select(x => x.InnerValue != null ? KeyPair.FromXdrPublicKey(x.InnerValue.InnerValue) : null)
                .ToArray());
        if (xdrExtensionV2.Ext.Discriminant == 3)
        {
            entryExtensionV2.ExtensionV3 = AccountEntryExtensionV3.FromXdr(xdrExtensionV2.Ext.V3);
        }

        return entryExtensionV2;
    }
}