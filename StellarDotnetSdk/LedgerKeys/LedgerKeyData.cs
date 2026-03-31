using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

/// <summary>
///     Represents a ledger key for a data entry attached to a Stellar account.
///     Used to look up named key-value data associated with an account.
/// </summary>
public class LedgerKeyData : LedgerKey
{
    /// <summary>
    ///     Constructs a <c>LedgerKeyData</c> from a Stellar account ID string and data name.
    /// </summary>
    /// <param name="accountId">The Stellar account ID (G... public key).</param>
    /// <param name="dataName">The name of the data entry (max 64 characters).</param>
    public LedgerKeyData(string accountId, string dataName) : this(KeyPair.FromAccountId(accountId), dataName)
    {
    }

    /// <summary>
    ///     Constructs a <c>LedgerKeyData</c> from a key pair and data name.
    /// </summary>
    /// <param name="account">The key pair of the account that owns the data entry.</param>
    /// <param name="dataName">The name of the data entry (max 64 characters).</param>
    public LedgerKeyData(KeyPair account, string dataName)
    {
        if (dataName.Length > 64)
        {
            throw new ArgumentException("Data name cannot exceed 64 characters.", nameof(dataName));
        }
        Account = account;
        DataName = dataName;
    }

    /// <summary>
    ///     The key pair of the account that owns the data entry.
    /// </summary>
    public KeyPair Account { get; }

    /// <summary>
    ///     The name of the data entry (max 64 characters).
    /// </summary>
    public string DataName { get; }

    /// <summary>
    ///     Serializes this ledger key to its XDR representation.
    /// </summary>
    public override Xdr.LedgerKey ToXdr()
    {
        return new Xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.DATA },
            Data = new Xdr.LedgerKey.LedgerKeyData
            {
                AccountID = new AccountID(Account.XdrPublicKey),
                DataName = new String64(DataName),
            },
        };
    }

    /// <summary>
    ///     Deserializes a <see cref="LedgerKeyData" /> from its XDR representation.
    /// </summary>
    /// <param name="xdr">The XDR ledger key data object.</param>
    public static LedgerKeyData FromXdr(Xdr.LedgerKey.LedgerKeyData xdr)
    {
        var account = KeyPair.FromXdrPublicKey(xdr.AccountID.InnerValue);
        var dataName = xdr.DataName.InnerValue;
        return new LedgerKeyData(account, dataName);
    }
}