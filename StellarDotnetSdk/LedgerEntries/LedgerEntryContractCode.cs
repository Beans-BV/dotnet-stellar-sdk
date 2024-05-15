using System;
using StellarDotnetSdk.Xdr;
using ExtensionPoint = StellarDotnetSdk.Soroban.ExtensionPoint;

namespace StellarDotnetSdk.LedgerEntries;

public class LedgerEntryContractCode : LedgerEntry
{
    private LedgerEntryContractCode(byte[] hash, byte[] code, ExtensionPoint? extensionPoint, ContractCodeCostInputs? costInputs)
    {
        Hash = hash;
        Code = code;
        ExtensionPoint = extensionPoint;
        CostInputs = costInputs;
    }

    public ExtensionPoint? ExtensionPoint { get; }

    /// <summary>
    ///     Unique identifier of the executable file.
    /// </summary>
    public byte[] Hash { get; }

    /// <summary>
    ///     Wasm bytecode of the WebAssembly (Wasm) executable file.
    /// </summary>
    public byte[] Code { get; }

    public ContractCodeCostInputs? CostInputs { get; }
    /// <summary>
    ///     Creates the corresponding LedgerEntryContractCode object from a <see cref="Xdr.LedgerEntry.LedgerEntryData" />
    ///     object.
    /// </summary>
    /// <param name="xdrLedgerEntryData">A <see cref="Xdr.LedgerEntry.LedgerEntryData" /> object.</param>
    /// <returns>A LedgerEntryContractCode object.</returns>
    /// <exception cref="ArgumentException">Throws when the parameter is not a valid ContractCodeEntry.</exception>
    public static LedgerEntryContractCode FromXdrLedgerEntryData(Xdr.LedgerEntry.LedgerEntryData xdrLedgerEntryData)
    {
        if (xdrLedgerEntryData.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_CODE)
            throw new ArgumentException("Not a ContractCodeEntry", nameof(xdrLedgerEntryData));

        return FromXdr(xdrLedgerEntryData.ContractCode);
    }

    private static LedgerEntryContractCode FromXdr(ContractCodeEntry xdrContractDataEntry)
    {
        return new LedgerEntryContractCode(
            xdrContractDataEntry.Hash.InnerValue,
            xdrContractDataEntry.Code,
            xdrContractDataEntry.Ext.Discriminant == 1 ? ExtensionPoint.FromXdr(xdrContractDataEntry.Ext.V1.Ext) : null,
            xdrContractDataEntry.Ext.Discriminant == 1
                ? ContractCodeCostInputs.FromXdr(xdrContractDataEntry.Ext.V1.CostInputs) : null);
    }
}