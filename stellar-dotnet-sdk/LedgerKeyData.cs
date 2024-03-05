using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerKeyData : LedgerKey
{
    public LedgerKeyData(KeyPair account, string dataName)
    {
        if (dataName.Length > 64)
            throw new ArgumentException("Data name cannot exceed 64 characters.", nameof(dataName));
        Account = account;
        DataName = dataName;
    }

    public KeyPair Account { get; }
    public string DataName { get; }

    public override xdr.LedgerKey ToXdr()
    {
        return new xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.DATA },
            Data = new xdr.LedgerKey.LedgerKeyData
            {
                AccountID = new AccountID(Account.XdrPublicKey),
                DataName = new String64(DataName)
            }
        };
    }

    public static LedgerKeyData FromXdr(xdr.LedgerKey.LedgerKeyData xdr)
    {
        var account = KeyPair.FromXdrPublicKey(xdr.AccountID.InnerValue);
        var dataName = xdr.DataName.InnerValue;
        return new LedgerKeyData(account, dataName);
    }
}