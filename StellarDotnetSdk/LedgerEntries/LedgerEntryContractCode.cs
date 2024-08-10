using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

public class LedgerEntryContractCode : LedgerEntry
{
    private LedgerEntryContractCode(byte[] hash, byte[] code)
    {
        Hash = hash;
        Code = code;
    }

    public ContractCodeEntryExtensionV1? ContractCodeExtensionV1 { get; private set; }

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
        {
            throw new ArgumentException("Not a ContractCodeEntry", nameof(xdrLedgerEntryData));
        }

        return FromXdr(xdrLedgerEntryData.ContractCode);
    }

    private static LedgerEntryContractCode FromXdr(ContractCodeEntry xdrContractCodeEntry)
    {
        var ledgerEntryContractCode = new LedgerEntryContractCode(
            xdrContractCodeEntry.Hash.InnerValue,
            xdrContractCodeEntry.Code);
        if (xdrContractCodeEntry.Ext.Discriminant == 1)
        {
            ledgerEntryContractCode.ContractCodeExtensionV1 =
                ContractCodeEntryExtensionV1.FromXdr(xdrContractCodeEntry.Ext.V1);
        }
        return ledgerEntryContractCode;
    }
}