using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerEntryTTL : LedgerEntry
{
    public Hash KeyHash { get; set; }
    public uint LiveUntilLedgerSequence { get; set; }

    public static LedgerEntryTTL FromXdrLedgerEntry(xdr.LedgerEntry xdrLedgerEntry)
    {
        if (xdrLedgerEntry.Data.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.TTL)
            throw new ArgumentException("Not a ContractCodeEntry", nameof(xdrLedgerEntry));
        var xdrTtlEntry = xdrLedgerEntry.Data.Ttl;
        var ledgerEntryTtl = new LedgerEntryTTL
        {
            KeyHash = Hash.FromXdr(xdrTtlEntry.KeyHash),
            LiveUntilLedgerSequence = xdrTtlEntry.LiveUntilLedgerSeq.InnerValue
        };
        ExtraFieldsFromXdr(xdrLedgerEntry, ledgerEntryTtl);

        return ledgerEntryTtl;
    }

    public TTLEntry ToXdr()
    {
        return new TTLEntry
        {
            KeyHash = KeyHash.ToXdr(),
            LiveUntilLedgerSeq = new Uint32(LiveUntilLedgerSequence)
        };
    }
}