using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

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

    public KeyPair Account { get; }

    public TrustlineAsset Asset { get; }

    public long Balance { get; }

    public long Limit { get; }

    public uint Flags { get; }

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