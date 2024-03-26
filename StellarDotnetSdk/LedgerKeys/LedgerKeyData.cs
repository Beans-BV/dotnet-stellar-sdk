using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.LedgerKeys;

public class LedgerKeyData : LedgerKey
{
    public LedgerKeyData(string accountId, string dataName) : this(KeyPair.FromAccountId(accountId), dataName)
    {
    }

    public LedgerKeyData(KeyPair account, string dataName)
    {
        if (dataName.Length > 64)
            throw new ArgumentException("Data name cannot exceed 64 characters.", nameof(dataName));
        Account = account;
        DataName = dataName;
    }

    public KeyPair Account { get; }
    public string DataName { get; }

    public override Xdr.LedgerKey ToXdr()
    {
        return new Xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.DATA },
            Data = new Xdr.LedgerKey.LedgerKeyData
            {
                AccountID = new AccountID(Account.XdrPublicKey),
                DataName = new String64(DataName)
            }
        };
    }

    public static LedgerKeyData FromXdr(Xdr.LedgerKey.LedgerKeyData xdr)
    {
        var account = KeyPair.FromXdrPublicKey(xdr.AccountID.InnerValue);
        var dataName = xdr.DataName.InnerValue;
        return new LedgerKeyData(account, dataName);
    }
}