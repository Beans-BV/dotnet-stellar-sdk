using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;

namespace StellarDotnetSdk.LedgerKeys;

public class LedgerKeyTrustline : LedgerKey
{
    public LedgerKeyTrustline(string accountId, Asset asset) : this(KeyPair.FromAccountId(accountId),
        TrustlineAsset.Create(asset))
    {
    }

    public LedgerKeyTrustline(KeyPair account, Asset asset) : this(account, TrustlineAsset.Create(asset))
    {
    }

    public LedgerKeyTrustline(KeyPair account, TrustlineAsset asset)
    {
        Account = account;
        Asset = asset;
    }

    public new KeyPair Account { get; }
    public TrustlineAsset Asset { get; }

    public override Xdr.LedgerKey ToXdr()
    {
        return new Xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.TRUSTLINE },
            TrustLine = new Xdr.LedgerKey.LedgerKeyTrustLine
            {
                AccountID = new AccountID(Account.XdrPublicKey),
                Asset = Asset.ToXdr()
            }
        };
    }

    public static LedgerKeyTrustline FromXdr(Xdr.LedgerKey.LedgerKeyTrustLine xdr)
    {
        var account = KeyPair.FromXdrPublicKey(xdr.AccountID.InnerValue);
        var asset = TrustlineAsset.FromXdr(xdr.Asset);
        return new LedgerKeyTrustline(account, asset);
    }
}