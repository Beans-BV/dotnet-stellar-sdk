using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerEntryContractCode : LedgerEntry
{
    public Hash Hash { get; set; }
    public byte[] Code { get; set; }
    public ExtensionPoint ExtensionPoint { get; set; }

    public static LedgerEntryContractCode FromXdrLedgerEntry(xdr.LedgerEntry xdrLedgerEntry)
    {
        if (xdrLedgerEntry.Data.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_CODE)
            throw new ArgumentException("Not a ContractCodeEntry", nameof(xdrLedgerEntry));
        var xdrContractCodeEntry = xdrLedgerEntry.Data.ContractCode;
        var ledgerEntryContractCode = new LedgerEntryContractCode
        {
            Hash = Hash.FromXdr(xdrContractCodeEntry.Hash),
            Code = xdrContractCodeEntry.Code,
            ExtensionPoint = ExtensionPoint.FromXdr(xdrContractCodeEntry.Ext)
        };
        ExtraFieldsFromXdr(xdrLedgerEntry, ledgerEntryContractCode);

        return ledgerEntryContractCode;
    }

    private static LedgerEntryContractCode FromXdr(xdr.ContractCodeEntry xdrContractDataEntry)
    {
        return new LedgerEntryContractCode
        {
            Hash = Hash.FromXdr(xdrContractDataEntry.Hash),
            Code = xdrContractDataEntry.Code,
            ExtensionPoint = ExtensionPoint.FromXdr(xdrContractDataEntry.Ext),
        };
    }
    
    public ContractCodeEntry ToXdr()
    {
        return new ContractCodeEntry
        {
            Ext = ExtensionPoint.ToXdr(),
            Hash = Hash.ToXdr(),
            Code = Code
        };
    }
    
    /// <summary>
    ///     Creates a new <see cref="LedgerEntryContractCode"/> object from the given LedgerEntryData XDR base64 string.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns><see cref="LedgerEntryContractCode"/> object</returns>
    public static LedgerEntryContractCode FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var xdrLedgerEntryData = xdr.LedgerEntry.LedgerEntryData.Decode(reader);
        return FromXdr(xdrLedgerEntryData.ContractCode);
    }
}