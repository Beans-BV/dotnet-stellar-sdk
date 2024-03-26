using System;
using StellarDotnetSdk.Xdr;
using Soroban_ExtensionPoint = StellarDotnetSdk.Soroban.ExtensionPoint;

namespace StellarDotnetSdk.LedgerEntries;

public class LedgerEntryContractCode : LedgerEntry
{
    private LedgerEntryContractCode(byte[] hash, byte[] code, Soroban_ExtensionPoint extensionPoint)
    {
        Hash = hash;
        Code = code;
        ExtensionPoint = extensionPoint;
    }

    public Soroban_ExtensionPoint ExtensionPoint { get; }

    /// <summary>
    ///     Unique identifier of the executable file.
    /// </summary>
    public byte[] Hash { get; }

    /// <summary>
    ///     Wasm bytecode of the WebAssembly (Wasm) executable file.
    /// </summary>
    public byte[] Code { get; }

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
            Soroban_ExtensionPoint.FromXdr(xdrContractDataEntry.Ext));
    }
}