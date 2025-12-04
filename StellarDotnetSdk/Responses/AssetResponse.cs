using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses;
#nullable disable
public class AssetResponse : Response, IPagingToken
{
    [JsonPropertyName("_links")]
    public AssetResponseLinks Links { get; init; }

    [JsonPropertyName("asset_type")]
    public string AssetType { get; init; }

    [JsonPropertyName("asset_code")]
    public string AssetCode { get; init; }

    [JsonPropertyName("asset_issuer")]
    public string AssetIssuer { get; init; }

    [JsonPropertyName("accounts")]
    public AssetAccounts Accounts { get; init; }

    [JsonPropertyName("balances")]
    public AssetBalances Balances { get; init; }

    [JsonPropertyName("claimable_balances_amount")]
    public string ClaimableBalancesAmount { get; init; }

    [JsonPropertyName("num_claimable_balances")]
    public int NumClaimableBalances { get; init; }

    [JsonPropertyName("flags")]
    public AssetResponseFlags Flags { get; init; }

    /// <summary>
    ///     The number of liquidity pools trading this asset
    /// </summary>
    [JsonPropertyName("num_liquidity_pools")]
    public int NumLiquidityPools { get; init; }

    /// <summary>
    ///     The amount of this asset held in liquidity pools
    /// </summary>
    [JsonPropertyName("liquidity_pools_amount")]
    public string LiquidityPoolsAmount { get; init; }

    /// <summary>
    ///     The quantity of contracts that hold this asset
    /// </summary>
    [JsonPropertyName("num_contracts")]
    public uint ContractsQuantity { get; init; }

    /// <summary>
    ///     The total units of this asset held by contracts
    /// </summary>
    [JsonPropertyName("contracts_amount")]
    public double ContractsTotalAmount { get; init; }

    public Asset Asset => Asset.Create(AssetType, AssetCode, AssetIssuer);

    [JsonPropertyName("paging_token")]
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

        [JsonPropertyName("authorized")]
        public int Authorized { get; init; }

        [JsonPropertyName("authorized_to_maintain_liabilities")]
        public int AuthorizedToMaintainLiabilities { get; init; }

        [JsonPropertyName("unauthorized")]
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

        [JsonPropertyName("authorized")]
        public string Authorized { get; init; }

        [JsonPropertyName("authorized_to_maintain_liabilities")]
        public string AuthorizedToMaintainLiabilities { get; init; }

        [JsonPropertyName("unauthorized")]
        public string Unauthorized { get; init; }
    }

    public class AssetResponseLinks
    {
        [JsonPropertyName("toml")]
        public Link Toml { get; init; }
    }

    public class AssetResponseFlags
    {
        /// <summary>
        ///     The anchor must approve anyone who wants to hold this asset.
        /// </summary>
        [JsonPropertyName("auth_required")]
        public bool AuthRequired { get; init; }

        /// <summary>
        ///     The anchor can freeze the asset.
        /// </summary>
        [JsonPropertyName("auth_revocable")]
        public bool AuthRevocable { get; init; }

        /// <summary>
        ///     None of the authorization flags can be init and the issuing account can never be deleted.
        /// </summary>
        [JsonPropertyName("auth_immutable")]
        public bool AuthImmutable { get; init; }
    }
}