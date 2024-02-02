using System;
using System.Linq;
using stellar_dotnet_sdk.xdr;
using Int64 = stellar_dotnet_sdk.xdr.Int64;

namespace stellar_dotnet_sdk;

public class LedgerEntryAccount : LedgerEntry
{
    private string _homeDomain;
    private byte[] _thresholds;
    public KeyPair Account { get; set; }
    public long Balance { get; set; }
    public long SequenceNumber { get; set; }
    public uint NumberSubEntries { get; set; }
    public KeyPair? InflationDest { get; set; }
    public uint Flags { get; set; }
    public AccountEntryExtensionV1? AccountExtensionV1 { get; set; }

    public string HomeDomain
    {
        get => _homeDomain;
        set
        {
            if (value.Length > 32)
                throw new ArgumentException("Home domain cannot exceed 32 characters", nameof(value));
            _homeDomain = value;
        }
    }

    public byte[] Thresholds
    {
        get => _thresholds;
        set
        {
            if (value.Length > 4) throw new ArgumentException("Thresholds cannot exceed 4 bytes", nameof(value));
            _thresholds = value;
        }
    }

    public Signer[] Signers { get; set; } = Array.Empty<Signer>();

    public static LedgerEntryAccount FromXdrLedgerEntry(xdr.LedgerEntry xdrLedgerEntry)
    {
        if (xdrLedgerEntry.Data.Discriminant.InnerValue != LedgerEntryType.LedgerEntryTypeEnum.ACCOUNT)
            throw new ArgumentException("Not an AccountEntry", nameof(xdrLedgerEntry));

        var ledgerEntryAccount = FromXdr(xdrLedgerEntry.Data.Account);

        ExtraFieldsFromXdr(xdrLedgerEntry, ledgerEntryAccount);

        return ledgerEntryAccount;
    }

    private static LedgerEntryAccount FromXdr(AccountEntry xdrAccountEntry)
    {
        var ledgerEntryAccount = new LedgerEntryAccount
        {
            Account = KeyPair.FromXdrPublicKey(xdrAccountEntry.AccountID.InnerValue),
            HomeDomain = xdrAccountEntry.HomeDomain.InnerValue,
            SequenceNumber = xdrAccountEntry.SeqNum.InnerValue.InnerValue,
            NumberSubEntries = xdrAccountEntry.NumSubEntries.InnerValue,
            Balance = xdrAccountEntry.Balance.InnerValue,
            Flags = xdrAccountEntry.Flags.InnerValue,
            InflationDest = xdrAccountEntry.InflationDest != null
                ? KeyPair.FromXdrPublicKey(xdrAccountEntry.InflationDest.InnerValue)
                : null,
            Thresholds = xdrAccountEntry.Thresholds.InnerValue,
            Signers = xdrAccountEntry.Signers.Select(Signer.FromXdr).ToArray()
        };
        if (xdrAccountEntry.Ext.Discriminant != 0)
            ledgerEntryAccount.AccountExtensionV1 = AccountEntryExtensionV1.FromXdr(xdrAccountEntry.Ext.V1);
        return ledgerEntryAccount;
    }

    public AccountEntry ToXdr()
    {
        return new AccountEntry
        {
            AccountID = new AccountID(Account.XdrPublicKey),
            Balance = new Int64(Balance),
            SeqNum = new SequenceNumber(new Int64(SequenceNumber)),
            NumSubEntries = new Uint32(NumberSubEntries),
            InflationDest = InflationDest != null ? new AccountID(InflationDest.XdrPublicKey) : null,
            Flags = new Uint32(Flags),
            HomeDomain = new String32(HomeDomain),
            Thresholds = new Thresholds(Thresholds),
            Signers = Signers.Select(x => x.ToXdr()).ToArray(),
            Ext = new AccountEntry.AccountEntryExt
            {
                Discriminant = AccountExtensionV1 != null ? 1 : 0,
                V1 = AccountExtensionV1?.ToXdr() ?? new xdr.AccountEntryExtensionV1()
            }
        };
    }

    /// <summary>
    ///     Creates a new <see cref="LedgerEntryAccount" /> object from the given LedgerEntryData XDR base64 string.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns><see cref="LedgerEntryAccount" /> object</returns>
    public static LedgerEntryAccount FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var xdrLedgerEntryData = xdr.LedgerEntry.LedgerEntryData.Decode(reader);
        return FromXdr(xdrLedgerEntryData.Account);
    }
}