using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

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

    public override xdr.LedgerKey ToXdr()
    {
        return new xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.ACCOUNT },
            Account = new xdr.LedgerKey.LedgerKeyAccount { AccountID = new AccountID(Account.XdrPublicKey) }
        };
    }

    public static LedgerKeyAccount FromXdr(xdr.LedgerKey.LedgerKeyAccount xdr)
    {
        var account = KeyPair.FromXdrPublicKey(xdr.AccountID.InnerValue);
        return new LedgerKeyAccount(account);
    }
}