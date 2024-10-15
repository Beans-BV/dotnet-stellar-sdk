using Newtonsoft.Json;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses;
#nullable disable
public class AssetResponse : Response, IPagingToken
{
    [JsonProperty(PropertyName = "_links")]
    public AssetResponseLinks Links { get; init; }

    [JsonProperty(PropertyName = "asset_type")]
    public string AssetType { get; init; }

    [JsonProperty(PropertyName = "asset_code")]
    public string AssetCode { get; init; }

    [JsonProperty(PropertyName = "asset_issuer")]
    public string AssetIssuer { get; init; }

    [JsonProperty(PropertyName = "accounts")]
    public AssetAccounts Accounts { get; init; }

    [JsonProperty(PropertyName = "balances")]
    public AssetBalances Balances { get; init; }

    [JsonProperty(PropertyName = "claimable_balances_amount")]
    public string ClaimableBalancesAmount { get; init; }

    [JsonProperty(PropertyName = "num_claimable_balances")]
    public int NumClaimableBalances { get; init; }

    [JsonProperty(PropertyName = "flags")] public AssetResponseFlags Flags { get; init; }

    /// <summary>
    ///     The number of liquidity pools trading this asset
    /// </summary>
    [JsonProperty(PropertyName = "num_liquidity_pools")]
    public int NumLiquidityPools { get; init; }

    /// <summary>
    ///     The amount of this asset held in liquidity pools
    /// </summary>
    [JsonProperty(PropertyName = "liquidity_pools_amount")]
    public string LiquidityPoolsAmount { get; init; }

    /// <summary>
    ///     The quantity of contracts that hold this asset
    /// </summary>
    [JsonProperty(PropertyName = "num_contracts")]
    public uint ContractsQuantity { get; init; }

    /// <summary>
    ///     The total units of this asset held by contracts
    /// </summary>
    [JsonProperty(PropertyName = "contracts_amount")]
    public double ContractsTotalAmount { get; init; }

    public Asset Asset => Asset.Create(AssetType, AssetCode, AssetIssuer);

    [JsonProperty(PropertyName = "paging_token")]
    public string PagingToken { get; init; }

    /// <summary>
    ///     Describe asset accounts
    /// </summary>
    public class AssetAccounts
    {
        public AssetAccounts(int authorized, int authorizedToMaintainLiabilities, int unauthorized)
        {
            Authorized = authorized;
            AuthorizedToMaintainLiabilities = authorizedToMaintainLiabilities;
            Unauthorized = unauthorized;
        }

        [JsonProperty(PropertyName = "authorized")]
        public int Authorized { get; init; }

        [JsonProperty(PropertyName = "authorized_to_maintain_liabilities")]
        public int AuthorizedToMaintainLiabilities { get; init; }

        [JsonProperty(PropertyName = "unauthorized")]
        public int Unauthorized { get; init; }
    }

    /// <summary>
    ///     Describe asset balances
    /// </summary>
    public class AssetBalances
    {
        public AssetBalances(string authorized, string authorizedToMaintainLiabilities, string unauthorized)
        {
            Authorized = authorized;
            AuthorizedToMaintainLiabilities = authorizedToMaintainLiabilities;
            Unauthorized = unauthorized;
        }

        [JsonProperty(PropertyName = "authorized")]
        public string Authorized { get; init; }

        [JsonProperty(PropertyName = "authorized_to_maintain_liabilities")]
        public string AuthorizedToMaintainLiabilities { get; init; }

        [JsonProperty(PropertyName = "unauthorized")]
        public string Unauthorized { get; init; }
    }

    public class AssetResponseLinks
    {
        [JsonProperty(PropertyName = "toml")] public Link Toml { get; init; }
    }

    public class AssetResponseFlags
    {
        /// <summary>
        ///     The anchor must approve anyone who wants to hold this asset.
        /// </summary>
        [JsonProperty(PropertyName = "auth_required")]
        public bool AuthRequired { get; init; }

        /// <summary>
        ///     The anchor can freeze the asset.
        /// </summary>
        [JsonProperty(PropertyName = "auth_revocable")]
        public bool AuthRevocable { get; init; }

        /// <summary>
        ///     None of the authorization flags can be init and the issuing account can never be deleted.
        /// </summary>
        [JsonProperty(PropertyName = "auth_immutable")]
        public bool AuthImmutable { get; init; }
    }
}