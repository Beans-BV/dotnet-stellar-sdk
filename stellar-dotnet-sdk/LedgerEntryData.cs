using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerEntryData : LedgerEntry
{
    private LedgerEntryData(KeyPair account, string dataName, byte[] dataValue)
    {
        if (dataValue.Length > 64)
            throw new ArgumentException("Data value cannot exceed 64 bytes.", nameof(dataValue));
        if (dataName.Length > 64)
            throw new ArgumentException("Data name cannot exceed 64 characters.", nameof(dataName));

        Account = account;
        DataName = dataName;
        DataValue = dataValue;
    }

    public byte[] DataValue { get; }

    public new KeyPair Account { get; }

    public string DataName { get; }

    public DataEntryExtension? DataExtension { get; private set; }

    /// <summary>
    ///     Creates the corresponding LedgerEntryData object from a <see cref="xdr.LedgerEntry.LedgerEntryData" /> object.
    /// </summary>
    /// <param name="xdrLedgerEntryData">A <see cref="xdr.LedgerEntry.LedgerEntryData" /> object.</param>
    /// <returns>A LedgerEntryData object.</returns>
    /// <exception cref="ArgumentException">Throws when the parameter is not a valid DataEntry.</exception>
    public static LedgerEntryData FromXdrLedgerEntryData(xdr.LedgerEntry.LedgerEntryData xdrLedgerEntryData)
    {
        if (xdrLedgerEntryData.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.DATA)
            throw new ArgumentException("Not a DataEntry", nameof(xdrLedgerEntryData));
        return FromXdr(xdrLedgerEntryData.Data);
    }

    private static LedgerEntryData FromXdr(DataEntry xdrDataEntry)
    {
        var ledgerEntryData = new LedgerEntryData(
            KeyPair.FromXdrPublicKey(xdrDataEntry.AccountID.InnerValue),
            xdrDataEntry.DataName.InnerValue,
            xdrDataEntry.DataValue.InnerValue);

        if (xdrDataEntry.Ext.Discriminant != 0)
            ledgerEntryData.DataExtension = DataEntryExtension.FromXdr(xdrDataEntry.Ext);

        return ledgerEntryData;
    }
}