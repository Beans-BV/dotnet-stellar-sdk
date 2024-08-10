using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

public class LedgerKeyAccount : LedgerKey
{
    public LedgerKeyAccount(string accountId) : this(KeyPair.FromAccountId(accountId))
    {
    }

    public LedgerKeyAccount(KeyPair account)
    {
        Account = account;
    }

    public KeyPair Account { get; }

    public override Xdr.LedgerKey ToXdr()
    {
        return new Xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.ACCOUNT },
            Account = new Xdr.LedgerKey.LedgerKeyAccount { AccountID = new AccountID(Account.XdrPublicKey) },
        };
    }

    public static LedgerKeyAccount FromXdr(Xdr.LedgerKey.LedgerKeyAccount xdr)
    {
        var account = KeyPair.FromXdrPublicKey(xdr.AccountID.InnerValue);
        return new LedgerKeyAccount(account);
    }
}