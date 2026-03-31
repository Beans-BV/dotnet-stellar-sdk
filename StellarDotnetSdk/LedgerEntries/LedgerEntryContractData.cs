using System;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using SCVal = StellarDotnetSdk.Soroban.SCVal;
using ExtensionPoint = StellarDotnetSdk.Soroban.ExtensionPoint;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents a Soroban smart contract data ledger entry, storing key-value data associated with a contract.
/// </summary>
public class LedgerEntryContractData : LedgerEntry
{
    private LedgerEntryContractData(
        SCVal key,
        SCVal value,
        ScAddress contract,
        ContractDataDurability durability,
        ExtensionPoint extensionPoint)
    {
        Key = key;
        Value = value;
        Contract = contract;
        Durability = durability;
        ExtensionPoint = extensionPoint;
    }

    /// <summary>
    ///     The key of this contract data entry (a Soroban value).
    /// </summary>
    public SCVal Key { get; }

    /// <summary>
    ///     The value stored in this contract data entry (a Soroban value).
    /// </summary>
    public SCVal Value { get; }

    /// <summary>
    ///     The address of the contract that owns this data entry.
    /// </summary>
    public ScAddress Contract { get; }

    /// <summary>
    ///     The durability type of this entry: <c>Persistent</c> or <c>Temporary</c>.
    /// </summary>
    public ContractDataDurability Durability { get; }

    /// <summary>
    ///     Reserved for future use.
    /// </summary>
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
            ScAddress.FromXdr(xdrContractDataEntry.Contract),
            xdrContractDataEntry.Durability,
            ExtensionPoint.FromXdr(xdrContractDataEntry.Ext));
    }
}