using System;
using StellarDotnetSdk.Xdr;
using SCAddress = StellarDotnetSdk.Soroban.SCAddress;
using SCVal = StellarDotnetSdk.Soroban.SCVal;
using ExtensionPoint = StellarDotnetSdk.Soroban.ExtensionPoint;

namespace StellarDotnetSdk.LedgerEntries;

public class LedgerEntryContractData : LedgerEntry
{
    private LedgerEntryContractData(
        SCVal key,
        SCVal value,
        SCAddress contract,
        ContractDataDurability durability,
        ExtensionPoint extensionPoint)
    {
        Key = key;
        Value = value;
        Contract = contract;
        Durability = durability;
        ExtensionPoint = extensionPoint;
    }

    public SCVal Key { get; }

    public SCVal Value { get; }

    public SCAddress Contract { get; }

    public ContractDataDurability Durability { get; }

    public ExtensionPoint ExtensionPoint { get; }

    /// <summary>
    ///     Creates the corresponding LedgerEntryContractData object from a <see cref="Xdr.LedgerEntry.LedgerEntryData" />
    ///     object.
    /// </summary>
    /// <param name="xdrLedgerEntryData">A <see cref="Xdr.LedgerEntry.LedgerEntryData" /> object.</param>
    /// <returns>A LedgerEntryContractData object.</returns>
    /// <exception cref="ArgumentException">Throws when the parameter is not a valid ContractDataEntry.</exception>
    public static LedgerEntryContractData FromXdrLedgerEntryData(Xdr.LedgerEntry.LedgerEntryData xdrLedgerEntryData)
    {
        if (xdrLedgerEntryData.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_DATA)
        {
            throw new ArgumentException("Not a ContractDataEntry", nameof(xdrLedgerEntryData));
        }

        return FromXdr(xdrLedgerEntryData.ContractData);
    }

    private static LedgerEntryContractData FromXdr(ContractDataEntry xdrContractDataEntry)
    {
        return new LedgerEntryContractData(
            SCVal.FromXdr(xdrContractDataEntry.Key),
            SCVal.FromXdr(xdrContractDataEntry.Val),
            SCAddress.FromXdr(xdrContractDataEntry.Contract),
            xdrContractDataEntry.Durability,
            ExtensionPoint.FromXdr(xdrContractDataEntry.Ext));
    }
}