using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents a data entry in the Stellar ledger, which stores a named key-value pair attached to an account.
/// </summary>
public class LedgerEntryData : LedgerEntry
{
    private LedgerEntryData(KeyPair account, string dataName, byte[] dataValue)
    {
        if (dataValue.Length > 64)
        {
            throw new ArgumentException("Data value cannot exceed 64 bytes.", nameof(dataValue));
        }
        if (dataName.Length > 64)
        {
            throw new ArgumentException("Data name cannot exceed 64 characters.", nameof(dataName));
        }

        Account = account;
        DataName = dataName;
        DataValue = dataValue;
    }

    /// <summary>
    ///     The data value, up to 64 bytes.
    /// </summary>
    public byte[] DataValue { get; }

    /// <summary>
    ///     The account this data entry belongs to.
    /// </summary>
    public new KeyPair Account { get; }

    /// <summary>
    ///     The name (key) of this data entry, up to 64 characters.
    /// </summary>
    public string DataName { get; }

    /// <summary>
    ///     Extension fields for this data entry, if present.
    /// </summary>
    public DataEntryExtension? DataExtension { get; private set; }

    /// <summary>
    ///     Creates the corresponding LedgerEntryData object from a <see cref="Xdr.LedgerEntry.LedgerEntryData" /> object.
    /// </summary>
    /// <param name="xdrLedgerEntryData">A <see cref="Xdr.LedgerEntry.LedgerEntryData" /> object.</param>
    /// <returns>A LedgerEntryData object.</returns>
    /// <exception cref="ArgumentException">Throws when the parameter is not a valid DataEntry.</exception>
    public static LedgerEntryData FromXdrLedgerEntryData(Xdr.LedgerEntry.LedgerEntryData xdrLedgerEntryData)
    {
        if (xdrLedgerEntryData.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.DATA)
        {
            throw new ArgumentException("Not a DataEntry", nameof(xdrLedgerEntryData));
        }
        return FromXdr(xdrLedgerEntryData.Data);
    }

    private static LedgerEntryData FromXdr(DataEntry xdrDataEntry)
    {
        var ledgerEntryData = new LedgerEntryData(
            KeyPair.FromXdrPublicKey(xdrDataEntry.AccountID.InnerValue),
            xdrDataEntry.DataName.InnerValue,
            xdrDataEntry.DataValue.InnerValue);

        if (xdrDataEntry.Ext.Discriminant != 0)
        {
            ledgerEntryData.DataExtension = DataEntryExtension.FromXdr(xdrDataEntry.Ext);
        }

        return ledgerEntryData;
    }
}