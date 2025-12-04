using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Base class for effect responses.
///     See: <a href="https://developers.stellar.org/network/horizon/api-reference/resources/effects/types">All effects</a>
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
[JsonConverter(typeof(EffectResponseJsonConverter))]
public abstract class EffectResponse : Response, IPagingToken
{
    [JsonPropertyName("id")]
    public string Id { get; init; }

    [JsonPropertyName("account")]
    public string Account { get; init; }

    [JsonPropertyName("account_muxed")]
    public string AccountMuxed { get; init; }

    [JsonPropertyName("account_muxed_id")]
    public ulong? AccountMuxedId { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; init; }

    [JsonPropertyName("type_i")]
    public virtual int TypeId { get; }

    [JsonPropertyName("_links")]
    public EffectsResponseLinks Links { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    [JsonPropertyName("paging_token")]
    public string PagingToken { get; init; }

    /// <summary>
    ///     Represents effect links.
    /// </summary>
    public class EffectsResponseLinks
    {
        [JsonPropertyName("operation")]
        public Link<OperationResponse> Operation { get; init; }

        [JsonPropertyName("precedes")]
        public Link<EffectResponse> Precedes { get; init; }

        [JsonPropertyName("succeeds")]
        public Link<EffectResponse> Succeeds { get; init; }
    }
}