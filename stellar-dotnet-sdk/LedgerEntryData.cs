using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerEntryData : LedgerEntry
{
    private byte[] _dataValue;
    public new KeyPair AccountID { get; set; }

    public String64 DataName { get; set; }

    public byte[] DataValue
    {
        get => _dataValue;
        set
        {
            if (value.Length > 64) throw new ArgumentException("Data value cannot exceed 64 bytes");
            _dataValue = value;
        }
    }

    public DataEntryExtension? DataExtension { get; set; }

    public static LedgerEntryData FromXdrLedgerEntry(xdr.LedgerEntry xdrLedgerEntry)
    {
        if (xdrLedgerEntry.Data.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.DATA)
            throw new ArgumentException("Not a DataEntry", nameof(xdrLedgerEntry));
        var xdrDataEntry = xdrLedgerEntry.Data.Data;
        var ledgerEntryData = new LedgerEntryData
        {
            AccountID = KeyPair.FromXdrPublicKey(xdrDataEntry.AccountID.InnerValue),
            DataName = String64.FromXdr(xdrDataEntry.DataName),
            DataValue = xdrDataEntry.DataValue.InnerValue,
        };
        if (xdrDataEntry.Ext.Discriminant != 0)
            ledgerEntryData.DataExtension = DataEntryExtension.FromXdr(xdrDataEntry.Ext);
        ExtraFieldsFromXdr(xdrLedgerEntry, ledgerEntryData);
        return ledgerEntryData;
    }

    public DataEntry ToXdr()
    {
        return new DataEntry
        {
            AccountID = new AccountID(AccountID.XdrPublicKey),
            DataName = DataName.ToXdr(),
            DataValue = new DataValue(DataValue),
            Ext = DataExtension?.ToXdr() ?? new DataEntry.DataEntryExt()
        };
    }
}