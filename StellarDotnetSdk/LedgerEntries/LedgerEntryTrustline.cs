using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents a trustline ledger entry, which tracks an account's trust and balance for a non-native asset.
/// </summary>
public class LedgerEntryTrustline : LedgerEntry
{
    private LedgerEntryTrustline(KeyPair account, TrustlineAsset asset, long balance, long limit, uint flags)
    {
        Account = account;
        Asset = asset;
        Balance = balance;
        Limit = limit;
        Flags = flags;
    }

    /// <summary>
    ///     The account that established this trustline.
    /// </summary>
    public KeyPair Account { get; }

    /// <summary>
    ///     The asset this trustline is for (credit asset or liquidity pool share).
    /// </summary>
    public TrustlineAsset Asset { get; }

    /// <summary>
    ///     The current balance of the trusted asset held by the account, in stroops.
    /// </summary>
    public long Balance { get; }

    /// <summary>
    ///     The maximum amount of this asset the account is willing to hold, in stroops.
    /// </summary>
    public long Limit { get; }

    /// <summary>
    ///     Trustline flags (e.g. AUTHORIZED, AUTHORIZED_TO_MAINTAIN_LIABILITIES, TRUSTLINE_CLAWBACK_ENABLED).
    /// </summary>
    public uint Flags { get; }

    /// <summary>
    ///     Version 1 extension fields for this trustline entry, if present.
    /// </summary>
    public TrustlineEntryExtensionV1? TrustlineExtensionV1 { get; private set; }

    /// <summary>
    ///     Creates the corresponding LedgerEntryTrustline object from a <see cref="Xdr.LedgerEntry.LedgerEntryData" /> object.
    /// </summary>
    /// <param name="xdrLedgerEntryData">A <see cref="Xdr.LedgerEntry.LedgerEntryData" /> object.</param>
    /// <returns>A LedgerEntryTrustline object.</returns>
    /// <exception cref="ArgumentException">Throws when the parameter is not a valid TrustLineEntry.</exception>
    public static LedgerEntryTrustline FromXdrLedgerEntryData(Xdr.LedgerEntry.LedgerEntryData xdrLedgerEntryData)
    {
        if (xdrLedgerEntryData.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.TRUSTLINE)
        {
            throw new ArgumentException("Not a TrustLineEntry.", nameof(xdrLedgerEntryData));
        }

        return FromXdr(xdrLedgerEntryData.TrustLine);
    }

    private static LedgerEntryTrustline FromXdr(TrustLineEntry xdrTrustLineEntry)
    {
        var ledgerEntryTrustLine = new LedgerEntryTrustline(
            KeyPair.FromXdrPublicKey(xdrTrustLineEntry.AccountID.InnerValue),
            TrustlineAsset.FromXdr(xdrTrustLineEntry.Asset),
            xdrTrustLineEntry.Balance.InnerValue,
            xdrTrustLineEntry.Limit.InnerValue,
            xdrTrustLineEntry.Flags.InnerValue);

        if (xdrTrustLineEntry.Ext.Discriminant == 1)
        {
            ledgerEntryTrustLine.TrustlineExtensionV1 = TrustlineEntryExtensionV1.FromXdr(xdrTrustLineEntry.Ext.V1);
        }

        return ledgerEntryTrustLine;
    }
}