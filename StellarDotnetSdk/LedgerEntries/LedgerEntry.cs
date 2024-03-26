using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Abstract class for ledger entries.
///     See https://developers.stellar.org/docs/fundamentals-and-concepts/stellar-data-structures/ledgers
/// </summary>
public abstract class LedgerEntry
{
    /// <summary>
    ///     The ledger number of the last time this entry was updated.
    /// </summary>
    public uint LastModifiedLedgerSeq { get; set; }

    /// <summary>
    ///     The ledger sequence number after which the ledger entry would expire.
    ///     This field exists only for ContractCodeEntry and ContractDataEntry ledger entries (optional).
    /// </summary>
    public uint? LiveUntilLedger { get; set; }

    /// <summary>
    ///     Extension.
    /// </summary>
    public LedgerEntryExtensionV1? LedgerExtensionV1 { get; private set; }

    /// <summary>
    ///     Creates the corresponding <c>LedgerEntry</c> object from an <c>xdr.LedgerEntryData</c> object.
    /// </summary>
    /// <param name="xdrLedgerEntry">An <c>xdr.LedgerEntryData</c> object to be converted.</param>
    /// <returns>A <c>LedgerEntry</c> object.</returns>
    private static LedgerEntry FromXdr(Xdr.LedgerEntry.LedgerEntryData xdrLedgerEntryData)
    {
        return xdrLedgerEntryData.Discriminant.InnerValue switch
        {
            LedgerEntryType.LedgerEntryTypeEnum.ACCOUNT =>
                LedgerEntryAccount.FromXdrLedgerEntryData(xdrLedgerEntryData),
            LedgerEntryType.LedgerEntryTypeEnum.TRUSTLINE =>
                LedgerEntryTrustline.FromXdrLedgerEntryData(xdrLedgerEntryData),
            LedgerEntryType.LedgerEntryTypeEnum.OFFER =>
                LedgerEntryOffer.FromXdrLedgerEntryData(xdrLedgerEntryData),
            LedgerEntryType.LedgerEntryTypeEnum.DATA =>
                LedgerEntryData.FromXdrLedgerEntryData(xdrLedgerEntryData),
            LedgerEntryType.LedgerEntryTypeEnum.CLAIMABLE_BALANCE =>
                LedgerEntryClaimableBalance.FromXdrLedgerEntryData(xdrLedgerEntryData),
            LedgerEntryType.LedgerEntryTypeEnum.LIQUIDITY_POOL =>
                LedgerEntryLiquidityPool.FromXdrLedgerEntryData(xdrLedgerEntryData),
            LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_DATA =>
                LedgerEntryContractData.FromXdrLedgerEntryData(xdrLedgerEntryData),
            LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_CODE =>
                LedgerEntryContractCode.FromXdrLedgerEntryData(xdrLedgerEntryData),
            LedgerEntryType.LedgerEntryTypeEnum.CONFIG_SETTING =>
                LedgerEntryConfigSetting.FromXdrLedgerEntryData(xdrLedgerEntryData),
            LedgerEntryType.LedgerEntryTypeEnum.TTL =>
                LedgerEntryTTL.FromXdrLedgerEntryData(xdrLedgerEntryData),
            _ => throw new InvalidOperationException("Unknown LedgerEntry type")
        };
    }

    /// <summary>
    ///     Creates the corresponding LedgerEntry object from an <see cref="Xdr.LedgerEntry">xdr.LedgerEntry</see> object.
    /// </summary>
    /// <param name="xdrLedgerEntry">An <see cref="Xdr.LedgerEntry">xdr.LedgerEntry</see> object to be converted.</param>
    /// <returns>A <c>LedgerEntry</c> object.</returns>
    public static LedgerEntry FromXdr(Xdr.LedgerEntry xdrLedgerEntry)
    {
        var ledgerEntry = FromXdr(xdrLedgerEntry.Data);
        ledgerEntry.LastModifiedLedgerSeq = xdrLedgerEntry.LastModifiedLedgerSeq.InnerValue;
        if (xdrLedgerEntry.Ext.Discriminant == 1)
            ledgerEntry.LedgerExtensionV1 = LedgerEntryExtensionV1.FromXdr(xdrLedgerEntry.Ext.V1);
        return ledgerEntry;
    }

    /// <summary>
    ///     Creates a <c>LedgerEntry</c> object from a base-64 encoded XDR string of an
    ///     <see cref="Xdr.LedgerEntry.LedgerEntryData" />.
    /// </summary>
    /// <param name="xdrBase64">
    ///     A base-64 encoded XDR string of an
    ///     <see cref="Xdr.LedgerEntry.LedgerEntryData">xdr.LedgerEntryData</see> object.
    /// </param>
    /// <returns>A <c>LedgerEntry</c> object decoded and deserialized from the provided string.</returns>
    public static LedgerEntry FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = Xdr.LedgerEntry.LedgerEntryData.Decode(reader);
        return FromXdr(thisXdr);
    }
}