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

    private static LedgerEntryContractData FromXdr(xdr.ContractDataEntry xdrContractDataEntry)
    {
        return new LedgerEntryContractData
        {
            Key = SCVal.FromXdr(xdrContractDataEntry.Key),
            Value = SCVal.FromXdr(xdrContractDataEntry.Key),
            Contract = SCAddress.FromXdr(xdrContractDataEntry.Contract),
            Durability = xdrContractDataEntry.Durability,
            ExtensionPoint = ExtensionPoint.FromXdr(xdrContractDataEntry.Ext),
        };
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
    
    /// <summary>
    ///     Creates a new <see cref="LedgerEntryContractData"/> object from the given LedgerEntryData XDR base64 string.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns><see cref="LedgerEntryContractData"/> object</returns>
    public static LedgerEntryContractData FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var xdrLedgerEntryData = xdr.LedgerEntry.LedgerEntryData.Decode(reader);
        return FromXdr(xdrLedgerEntryData.ContractData);
    }
}