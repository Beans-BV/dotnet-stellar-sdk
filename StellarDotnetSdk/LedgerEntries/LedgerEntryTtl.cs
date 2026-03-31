using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents a TTL (time-to-live) ledger entry that tracks the expiration ledger sequence for a Soroban state entry.
/// </summary>
public class LedgerEntryTtl : LedgerEntry
{
    private LedgerEntryTtl(byte[] keyHash, uint liveUntilLedgerSequence)
    {
        KeyHash = keyHash;
        LiveUntilLedgerSequence = liveUntilLedgerSequence;
    }

    /// <summary>
    ///     The SHA-256 hash of the ledger key for the entry this TTL tracks.
    /// </summary>
    public byte[] KeyHash { get; }

    /// <summary>
    ///     The ledger sequence number until which the associated entry is live.
    /// </summary>
    public uint LiveUntilLedgerSequence { get; }

    /// <summary>
    ///     Creates the corresponding LedgerEntryTTL object from a <see cref="Xdr.LedgerEntry.LedgerEntryData" /> object.
    /// </summary>
    /// <param name="xdrLedgerEntryData">A <see cref="Xdr.LedgerEntry.LedgerEntryData" /> object.</param>
    /// <returns>A LedgerEntryTTL object.</returns>
    /// <exception cref="ArgumentException">Throws when the parameter is not a valid TTLEntry.</exception>
    public static LedgerEntryTtl FromXdrLedgerEntryData(Xdr.LedgerEntry.LedgerEntryData xdrLedgerEntryData)
    {
        if (xdrLedgerEntryData.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.TTL)
        {
            throw new ArgumentException("Not a TTLEntry.", nameof(xdrLedgerEntryData));
        }

        return FromXdr(xdrLedgerEntryData.Ttl);
    }

    private static LedgerEntryTtl FromXdr(TTLEntry xdrTtlEntry)
    {
        return new LedgerEntryTtl(
            xdrTtlEntry.KeyHash.InnerValue,
            xdrTtlEntry.LiveUntilLedgerSeq.InnerValue);
    }
}