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

    public ContractCodeEntry ToXdr()
    {
        return new ContractCodeEntry
        {
            Ext = ExtensionPoint.ToXdr(),
            Hash = Hash.ToXdr(),
            Code = Code
        };
    }
}