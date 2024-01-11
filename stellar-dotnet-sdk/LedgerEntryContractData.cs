using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerEntryContractData : LedgerEntry
{
    public SCVal Key { get; set; }

    public SCVal Value { get; set; }

    public SCAddress Contract { get; set; }

    public ContractDataDurability Durability { get; set; }

    public ExtensionPoint ExtensionPoint { get; set; }

    public static LedgerEntryContractData FromXdrLedgerEntry(xdr.LedgerEntry xdrLedgerEntry)
    {
        if (xdrLedgerEntry.Data.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.CONTRACT_DATA)
            throw new ArgumentException("Not a ContractDataEntry", nameof(xdrLedgerEntry));
        var xdrContractDataEntry = xdrLedgerEntry.Data.ContractData;

        var ledgerEntryContractData = new LedgerEntryContractData
        {
            Key = SCVal.FromXdr(xdrContractDataEntry.Key),
            Value = SCVal.FromXdr(xdrContractDataEntry.Val),
            Contract = SCAddress.FromXdr(xdrContractDataEntry.Contract),
            Durability = xdrContractDataEntry.Durability,
            ExtensionPoint = ExtensionPoint.FromXdr(xdrContractDataEntry.Ext)
        };
        ExtraFieldsFromXdr(xdrLedgerEntry, ledgerEntryContractData);

        return ledgerEntryContractData;
    }

    public ContractDataEntry ToXdr()
    {
        return new ContractDataEntry
        {
            Ext = ExtensionPoint.ToXdr(),
            Contract = Contract.ToXdr(),
            Key = Key.ToXdr(),
            Durability = Durability,
            Val = Value.ToXdr()
        };
    }
}