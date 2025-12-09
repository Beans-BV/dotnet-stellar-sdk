using System.Text.Json.Serialization;
using StellarDotnetSdk.Assets;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Represents asset information from the Horizon API.
///     Contains statistics about the asset including holder counts, balances, and authorization flags.
/// </summary>
public sealed class AssetResponse : Response, IPagingToken
{
    /// <summary>
    ///     Links related to this asset.
    /// </summary>
    [JsonPropertyName("_links")]
    public required AssetResponseLinks Links { get; init; }

    /// <summary>
    ///     The type of asset: "native", "credit_alphanum4", or "credit_alphanum12".
    /// </summary>
    [JsonPropertyName("asset_type")]
    public required string AssetType { get; init; }

    /// <summary>
    ///     The 4 or 12 character asset code. Empty for native XLM.
    /// </summary>
    [JsonPropertyName("asset_code")]
    public string? AssetCode { get; init; }

    /// <summary>
    ///     The account ID of the asset issuer. Empty for native XLM.
    /// </summary>
    [JsonPropertyName("asset_issuer")]
    public string? AssetIssuer { get; init; }

    /// <summary>
    ///     Statistics about accounts holding this asset, categorized by authorization status.
    /// </summary>
    [JsonPropertyName("accounts")]
    public required AssetAccounts Accounts { get; init; }

    /// <summary>
    ///     Total balances of this asset, categorized by authorization status.
    /// </summary>
    [JsonPropertyName("balances")]
    public required AssetBalances Balances { get; init; }

    /// <summary>
    ///     The total amount of this asset in claimable balances.
    ///     Represented as a string to preserve precision.
    /// </summary>
    [JsonPropertyName("claimable_balances_amount")]
    public required string ClaimableBalancesAmount { get; init; }

    /// <summary>
    ///     The number of claimable balances containing this asset.
    /// </summary>
    [JsonPropertyName("num_claimable_balances")]
    public required int NumClaimableBalances { get; init; }

    /// <summary>
    ///     The authorization flags set on this asset's issuing account.
    /// </summary>
    [JsonPropertyName("flags")]
    public required Flags Flags { get; init; }

    /// <summary>
    ///     The number of liquidity pools trading this asset.
    /// </summary>
    [JsonPropertyName("num_liquidity_pools")]
    public required int NumLiquidityPools { get; init; }

    /// <summary>
    ///     The amount of this asset held in liquidity pools.
    ///     Represented as a string to preserve precision.
    /// </summary>
    [JsonPropertyName("liquidity_pools_amount")]
    public required string LiquidityPoolsAmount { get; init; }

    /// <summary>
    ///     The number of Soroban contracts that hold this asset.
    /// </summary>
    [JsonPropertyName("num_contracts")]
    public required uint ContractsQuantity { get; init; }

    /// <summary>
    ///     The number of units for this asset held by all Soroban contracts.
    /// </summary>
    [JsonPropertyName("contracts_amount")]
    public required string ContractsTotalAmount { get; init; }

    /// <summary>
    ///     The asset object representing this response.
    /// </summary>
    public Asset Asset => Asset.Create(AssetType, AssetCode, AssetIssuer);

    /// <inheritdoc />
    [JsonPropertyName("paging_token")]
    public required string PagingToken { get; init; }

    /// <summary>
    ///     Statistics about accounts holding the asset, categorized by authorization status.
    /// </summary>
    public sealed class AssetAccounts
    {
        /// <summary>
        ///     The number of accounts fully authorized to hold and transact with this asset.
        /// </summary>
        [JsonPropertyName("authorized")]
        public required int Authorized { get; init; }

        /// <summary>
        ///     The number of accounts authorized only to maintain liabilities (can hold but not transact).
        /// </summary>
        [JsonPropertyName("authorized_to_maintain_liabilities")]
        public required int AuthorizedToMaintainLiabilities { get; init; }

        /// <summary>
        ///     The number of unauthorized accounts with a trustline to this asset.
        /// </summary>
        [JsonPropertyName("unauthorized")]
        public required int Unauthorized { get; init; }
    }

    /// <summary>
    ///     Total balances of the asset, categorized by authorization status.
    /// </summary>
    public sealed class AssetBalances
    {
        /// <summary>
        ///     The total balance held by fully authorized accounts.
        ///     Represented as a string to preserve precision.
        /// </summary>
        [JsonPropertyName("authorized")]
        public required string Authorized { get; init; }

        /// <summary>
        ///     The total balance held by accounts authorized only to maintain liabilities.
        ///     Represented as a string to preserve precision.
        /// </summary>
        [JsonPropertyName("authorized_to_maintain_liabilities")]
        public required string AuthorizedToMaintainLiabilities { get; init; }

        /// <summary>
        ///     The total balance held by unauthorized accounts.
        ///     Represented as a string to preserve precision.
        /// </summary>
        [JsonPropertyName("unauthorized")]
        public required string Unauthorized { get; init; }
    }

    /// <summary>
    ///     Links associated with the asset response.
    /// </summary>
    public sealed class AssetResponseLinks
    {
        /// <summary>
        ///     Link to the stellar.toml file for this asset's home domain.
        /// </summary>
        [JsonPropertyName("toml")]
        public required Link Toml { get; init; }
    }
}