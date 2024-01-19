using System;
using stellar_dotnet_sdk.xdr;
using Int64 = stellar_dotnet_sdk.xdr.Int64;

namespace stellar_dotnet_sdk;

public class LedgerEntryTrustline : LedgerEntry
{
    public KeyPair Account { get; set; }

    public TrustlineAsset Asset { get; set; }

    public long Balance { get; set; }

    public long Limit { get; set; }

    public uint Flags { get; set; }

    public TrustlineEntryExtensionV1? TrustlineExtensionV1 { get; set; }

    public static LedgerEntryTrustline FromXdrLedgerEntry(xdr.LedgerEntry xdrLedgerEntry)
    {
        if (xdrLedgerEntry.Data.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.TRUSTLINE)
            throw new ArgumentException("Not a TrustLineEntry", nameof(xdrLedgerEntry));

        var xdrTrustLineEntry = xdrLedgerEntry.Data.TrustLine;
        var ledgerEntryTrustLine = new LedgerEntryTrustline
        {
            Account = KeyPair.FromXdrPublicKey(xdrTrustLineEntry.AccountID.InnerValue),
            Asset = TrustlineAsset.FromXdr(xdrTrustLineEntry.Asset),
            Balance = xdrTrustLineEntry.Balance.InnerValue,
            Limit = xdrTrustLineEntry.Limit.InnerValue,
            Flags = xdrTrustLineEntry.Flags.InnerValue
        };
        if (xdrTrustLineEntry.Ext.Discriminant == 1)
            ledgerEntryTrustLine.TrustlineExtensionV1 = TrustlineEntryExtensionV1.FromXdr(xdrTrustLineEntry.Ext.V1);
        ExtraFieldsFromXdr(xdrLedgerEntry, ledgerEntryTrustLine);
        return ledgerEntryTrustLine;
    }

    public TrustLineEntry ToXdr()
    {
        return new TrustLineEntry
        {
            AccountID = new AccountID(Account.XdrPublicKey),
            Asset = Asset.ToXdr(),
            Balance = new Int64(Balance),
            Limit = new Int64(Limit),
            Flags = new Uint32(Flags),
            Ext = new TrustLineEntry.TrustLineEntryExt
            {
                Discriminant = TrustlineExtensionV1 != null ? 1 : 0,
                V1 = TrustlineExtensionV1?.ToXdr() ?? new TrustLineEntry.TrustLineEntryExt.TrustLineEntryV1()
            }
        };
    }
}