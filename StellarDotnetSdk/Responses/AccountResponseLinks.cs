using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.Effects;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses;

/// <summary>
///     Links to related resources for an account.
/// </summary>
public sealed class AccountResponseLinks
{
    /// <summary>
    ///     Link to effects involving this account.
    /// </summary>
    [JsonPropertyName("effects")]
    public required Link<Page<EffectResponse>> Effects { get; init; }

    /// <summary>
    ///     Link to offers created by this account.
    /// </summary>
    [JsonPropertyName("offers")]
    public required Link<Page<OfferResponse>> Offers { get; init; }

    /// <summary>
    ///     Link to operations involving this account.
    /// </summary>
    [JsonPropertyName("operations")]
    public required Link<Page<OperationResponse>> Operations { get; init; }

    /// <summary>
    ///     Link to payments involving this account.
    /// </summary>
    [JsonPropertyName("payments")]
    public required Link<Page<PaymentOperationResponse>> Payments { get; init; }

    /// <summary>
    ///     Link to trades involving this account.
    /// </summary>
    [JsonPropertyName("trades")]
    public required Link<Page<TradeResponse>> Trades { get; init; }

    /// <summary>
    ///     Link to this account resource.
    /// </summary>
    [JsonPropertyName("self")]
    public required Link<AccountResponse> Self { get; init; }

    /// <summary>
    ///     Link to transactions involving this account.
    /// </summary>
    [JsonPropertyName("transactions")]
    public required Link<Page<TransactionResponse>> Transactions { get; init; }

    /// <summary>
    ///     Link to data involving this account.
    /// </summary>
    // [JsonPropertyName("data")]
    // TODO: Add when more info available
    // public required object Data { get; init; }
}