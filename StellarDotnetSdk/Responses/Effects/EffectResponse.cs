using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses.Effects;

/// <summary>
///     Base class for all effect responses.
///     Effects represent changes that occur in the ledger as a result of operations.
///     See: <a href="https://developers.stellar.org/network/horizon/resources/list-all-effects">All effects</a>
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
[JsonConverter(typeof(EffectResponseJsonConverter))]
public abstract class EffectResponse : Response, IPagingToken
{
    /// <summary>
    ///     The unique identifier of the effect.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    ///     The account that was affected.
    /// </summary>
    [JsonPropertyName("account")]
    public string? Account { get; init; }

    /// <summary>
    ///     The muxed account representation of the affected account, if applicable.
    /// </summary>
    [JsonPropertyName("account_muxed")]
    public string? AccountMuxed { get; init; }

    /// <summary>
    ///     The muxed account ID of the affected account, if applicable.
    /// </summary>
    [JsonPropertyName("account_muxed_id")]
    public ulong? AccountMuxedId { get; init; }

    /// <summary>
    ///     The type name of the effect (e.g., "account_created", "trade").
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>
    ///     The numeric type identifier of the effect.
    /// </summary>
    [JsonPropertyName("type_i")]
    public virtual int TypeId { get; }

    /// <summary>
    ///     Links to related resources.
    /// </summary>
    [JsonPropertyName("_links")]
    public EffectsResponseLinks? Links { get; init; }

    /// <summary>
    ///     The time when the effect was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    ///     A cursor value for use in pagination.
    /// </summary>
    [JsonPropertyName("paging_token")]
    public string PagingToken { get; init; } = string.Empty;

    /// <summary>
    ///     Links to related resources for an effect.
    /// </summary>
    public sealed class EffectsResponseLinks
    {
        /// <summary>
        ///     Link to the operation that created this effect.
        /// </summary>
        [JsonPropertyName("operation")]
        public required Link<OperationResponse> Operation { get; init; }

        /// <summary>
        ///     Link to the preceding effect in the operation.
        /// </summary>
        [JsonPropertyName("precedes")]
        public required Link<EffectResponse> Precedes { get; init; }

        /// <summary>
        ///     Link to the succeeding effect in the operation.
        /// </summary>
        [JsonPropertyName("succeeds")]
        public required Link<EffectResponse> Succeeds { get; init; }
    }
}