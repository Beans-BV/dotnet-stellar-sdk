using System;
using System.Linq;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerEntryAccount : LedgerEntry
{
    private LedgerEntryAccount(
        KeyPair account,
        long balance,
        long sequenceNumber,
        uint numberSubEntries,
        KeyPair? inflationDest,
        uint flags,
        Signer[] signers,
        string homeDomain,
        byte[] thresholds)
    {
        if (homeDomain == null)
            throw new ArgumentNullException(nameof(homeDomain), "Home domain cannot be null.");
        if (homeDomain.Length > 32)
            throw new ArgumentException("Home domain cannot exceed 32 characters.", nameof(homeDomain));
        if (thresholds == null)
            throw new ArgumentNullException(nameof(thresholds), "Thresholds cannot be null.");
        if (thresholds.Length > 4)
            throw new ArgumentException("Thresholds cannot exceed 4 bytes.", nameof(thresholds));

        Account = account;
        Balance = balance;
        SequenceNumber = sequenceNumber;
        NumberSubEntries = numberSubEntries;
        InflationDest = inflationDest;
        Flags = flags;
        Signers = signers;
        HomeDomain = homeDomain;
        Thresholds = thresholds;
    }

    public KeyPair Account { get; }
    public long Balance { get; }
    public long SequenceNumber { get; }
    public uint NumberSubEntries { get; }

    /// <summary>
    ///     The generated <see cref="xdr.AccountEntry.InflationDest" /> field may in fact be null.
    /// </summary>
    public KeyPair? InflationDest { get; }

    public uint Flags { get; }
    public AccountEntryExtensionV1? AccountExtensionV1 { get; private set; }

    public string HomeDomain { get; }

    public byte[] Thresholds { get; }

    public Signer[] Signers { get; }

    /// <summary>
    ///     Creates the corresponding LedgerEntryAccount object from a <see cref="xdr.LedgerEntry.LedgerEntryData" /> object.
    /// </summary>
    /// <param name="xdrLedgerEntryData">A <see cref="xdr.LedgerEntry.LedgerEntryData" /> object.</param>
    /// <returns>A LedgerEntryAccount object.</returns>
    /// <exception cref="ArgumentException">Throws when the parameter is not a valid AccountEntry.</exception>
    public static LedgerEntryAccount FromXdrLedgerEntryData(xdr.LedgerEntry.LedgerEntryData xdrLedgerEntryData)
    {
        if (xdrLedgerEntryData.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.ACCOUNT)
            throw new ArgumentException("Not an AccountEntry.", nameof(xdrLedgerEntryData));

        return FromXdr(xdrLedgerEntryData.Account);
    }

    private static LedgerEntryAccount FromXdr(AccountEntry xdrAccountEntry)
    {
        var ledgerEntryAccount = new LedgerEntryAccount(
            KeyPair.FromXdrPublicKey(xdrAccountEntry.AccountID.InnerValue),
            homeDomain: xdrAccountEntry.HomeDomain.InnerValue,
            sequenceNumber: xdrAccountEntry.SeqNum.InnerValue.InnerValue,
            numberSubEntries: xdrAccountEntry.NumSubEntries.InnerValue,
            balance: xdrAccountEntry.Balance.InnerValue,
            flags: xdrAccountEntry.Flags.InnerValue,
            inflationDest: xdrAccountEntry.InflationDest != null
                ? KeyPair.FromXdrPublicKey(xdrAccountEntry.InflationDest.InnerValue)
                : null, thresholds: xdrAccountEntry.Thresholds.InnerValue,
            signers: xdrAccountEntry.Signers.Select(Signer.FromXdr).ToArray());
        if (xdrAccountEntry.Ext.Discriminant == 1)
            ledgerEntryAccount.AccountExtensionV1 = AccountEntryExtensionV1.FromXdr(xdrAccountEntry.Ext.V1);
        return ledgerEntryAccount;
    }
}