using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;

namespace StellarDotnetSdk.LedgerKeys;

/// <summary>
///     Represents a ledger key for a trustline entry on the Stellar network.
///     Used to look up an account's trust relationship with a specific asset or liquidity pool.
/// </summary>
public class LedgerKeyTrustline : LedgerKey
{
    /// <summary>
    ///     Constructs a <c>LedgerKeyTrustline</c> from a Stellar account ID string and an asset.
    /// </summary>
    /// <param name="accountId">The Stellar account ID (G... public key).</param>
    /// <param name="asset">The asset being trusted.</param>
    public LedgerKeyTrustline(string accountId, Asset asset) : this(KeyPair.FromAccountId(accountId),
        TrustlineAsset.Create(asset))
    {
    }

    /// <summary>
    ///     Constructs a <c>LedgerKeyTrustline</c> from a key pair and an asset.
    /// </summary>
    /// <param name="account">The key pair of the account that holds the trustline.</param>
    /// <param name="asset">The asset being trusted.</param>
    public LedgerKeyTrustline(KeyPair account, Asset asset) : this(account, TrustlineAsset.Create(asset))
    {
    }

    /// <summary>
    ///     Constructs a <c>LedgerKeyTrustline</c> from a key pair and a trustline asset.
    /// </summary>
    /// <param name="account">The key pair of the account that holds the trustline.</param>
    /// <param name="asset">The trustline asset (can be a standard asset or a liquidity pool share).</param>
    public LedgerKeyTrustline(KeyPair account, TrustlineAsset asset)
    {
        Account = account;
        Asset = asset;
    }

    /// <summary>
    ///     The key pair of the account that holds the trustline.
    /// </summary>
    public new KeyPair Account { get; }

    /// <summary>
    ///     The trustline asset (can be a standard asset or a liquidity pool share).
    /// </summary>
    public TrustlineAsset Asset { get; }

    /// <summary>
    ///     Serializes this ledger key to its XDR representation.
    /// </summary>
    public override Xdr.LedgerKey ToXdr()
    {
        return new Xdr.LedgerKey
        {
            Discriminant =
                new LedgerEntryType { InnerValue = LedgerEntryType.LedgerEntryTypeEnum.TRUSTLINE },
            TrustLine = new Xdr.LedgerKey.LedgerKeyTrustLine
            {
                AccountID = new AccountID(Account.XdrPublicKey),
                Asset = Asset.ToXdr(),
            },
        };
    }

    /// <summary>
    ///     Deserializes a <see cref="LedgerKeyTrustline" /> from its XDR representation.
    /// </summary>
    /// <param name="xdr">The XDR ledger key trustline object.</param>
    public static LedgerKeyTrustline FromXdr(Xdr.LedgerKey.LedgerKeyTrustLine xdr)
    {
        var account = KeyPair.FromXdrPublicKey(xdr.AccountID.InnerValue);
        var asset = TrustlineAsset.FromXdr(xdr.Asset);
        return new LedgerKeyTrustline(account, asset);
    }
}