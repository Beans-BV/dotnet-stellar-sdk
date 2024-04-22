using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerEntryTTL : LedgerEntry
{
    private LedgerEntryTTL(byte[] keyHash, uint liveUntilLedgerSequence)
    {
        KeyHash = keyHash;
        LiveUntilLedgerSequence = liveUntilLedgerSequence;
    }

    public byte[] KeyHash { get; }
    public uint LiveUntilLedgerSequence { get; }

    /// <summary>
    ///     Creates the corresponding LedgerEntryTTL object from a <see cref="xdr.LedgerEntry.LedgerEntryData" /> object.
    /// </summary>
    /// <param name="xdrLedgerEntryData">A <see cref="xdr.LedgerEntry.LedgerEntryData" /> object.</param>
    /// <returns>A LedgerEntryTTL object.</returns>
    /// <exception cref="ArgumentException">Throws when the parameter is not a valid TTLEntry.</exception>
    public static LedgerEntryTTL FromXdrLedgerEntryData(xdr.LedgerEntry.LedgerEntryData xdrLedgerEntryData)
    {
        if (xdrLedgerEntryData.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.TTL)
            throw new ArgumentException("Not a TTLEntry.", nameof(xdrLedgerEntryData));

        return FromXdr(xdrLedgerEntryData.Ttl);
    }

    private static LedgerEntryTTL FromXdr(TTLEntry xdrTtlEntry)
    {
        return new LedgerEntryTTL(
            xdrTtlEntry.KeyHash.InnerValue,
            xdrTtlEntry.LiveUntilLedgerSeq.InnerValue);
    }
}