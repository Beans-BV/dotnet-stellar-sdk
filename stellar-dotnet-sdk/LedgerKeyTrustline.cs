using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerKeyTrustline : LedgerKey
{
    public LedgerKeyTrustline(string accountId, Asset asset) : this(KeyPair.FromAccountId(accountId), TrustlineAsset.Create(asset))
    {
    }
    
    public LedgerKeyTrustline(KeyPair account, TrustlineAsset asset)
    {
        Account = account;
        Asset = asset;
    }

    public new KeyPair Account { get; }
    public TrustlineAsset Asset { get; }

    public override xdr.LedgerKey ToXdr()
    {
        return new xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.TRUSTLINE },
            TrustLine = new xdr.LedgerKey.LedgerKeyTrustLine
            {
                AccountID = new AccountID(Account.XdrPublicKey),
                Asset = Asset.ToXdr()
            }
        };
    }

    public static LedgerKeyTrustline FromXdr(xdr.LedgerKey.LedgerKeyTrustLine xdr)
    {
        var account = KeyPair.FromXdrPublicKey(xdr.AccountID.InnerValue);
        var asset = TrustlineAsset.FromXdr(xdr.Asset);
        return new LedgerKeyTrustline(account, asset);
    }
}