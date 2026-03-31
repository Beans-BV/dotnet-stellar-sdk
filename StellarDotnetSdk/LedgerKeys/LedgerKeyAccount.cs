using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

/// <summary>
///     Represents a ledger key for an account entry on the Stellar network.
///     Used to look up account data from the ledger.
/// </summary>
public class LedgerKeyAccount : LedgerKey
{
    /// <summary>
    ///     Constructs a <c>LedgerKeyAccount</c> from a Stellar account ID string.
    /// </summary>
    /// <param name="accountId">The Stellar account ID (G... public key).</param>
    public LedgerKeyAccount(string accountId) : this(KeyPair.FromAccountId(accountId))
    {
    }

    /// <summary>
    ///     Constructs a <c>LedgerKeyAccount</c> from a key pair.
    /// </summary>
    /// <param name="account">The key pair of the account.</param>
    public LedgerKeyAccount(KeyPair account)
    {
        Account = account;
    }

    /// <summary>
    ///     The key pair identifying the account.
    /// </summary>
    public KeyPair Account { get; }

    /// <summary>
    ///     Serializes this ledger key to its XDR representation.
    /// </summary>
    public override Xdr.LedgerKey ToXdr()
    {
        return new Xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.ACCOUNT },
            Account = new Xdr.LedgerKey.LedgerKeyAccount { AccountID = new AccountID(Account.XdrPublicKey) },
        };
    }

    /// <summary>
    ///     Deserializes a <see cref="LedgerKeyAccount" /> from its XDR representation.
    /// </summary>
    /// <param name="xdr">The XDR ledger key account object.</param>
    public static LedgerKeyAccount FromXdr(Xdr.LedgerKey.LedgerKeyAccount xdr)
    {
        var account = KeyPair.FromXdrPublicKey(xdr.AccountID.InnerValue);
        return new LedgerKeyAccount(account);
    }
}