using System;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses.Effects;
#nullable disable

/// <summary>
///     Base class for effect responses.
///     See: <a href="https://developers.stellar.org/network/horizon/resources/list-all-effects">All effects</a>
///     <seealso cref="Requests.EffectsRequestBuilder" />
///     <seealso cref="Server" />
/// </summary>
[JsonConverter(typeof(EffectResponseJsonConverter))]
public abstract class EffectResponse : Response, IPagingToken
{
    [JsonPropertyName("id")] public string Id { get; protected set; }

    [JsonPropertyName("account")]
    public string Account { get; protected set; }

    [JsonPropertyName("account_muxed")]
    public string AccountMuxed { get; protected set; }

    [JsonPropertyName("account_muxed_id")]
    public ulong? AccountMuxedId { get; protected set; }

    [JsonPropertyName("type")] public string Type { get; protected set; }

    [JsonPropertyName("type_i")]
    public virtual int TypeId { get; }

    [JsonPropertyName("_links")]
    public EffectsResponseLinks Links { get; protected set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; protected set; }

    [JsonPropertyName("paging_token")]
    public string PagingToken { get; protected set; }

    /// <summary>
    ///     Represents effect links.
    /// </summary>
    public class EffectsResponseLinks
    {
        public EffectsResponseLinks(Link<OperationResponse> operation, Link<EffectResponse> precedes,
            Link<EffectResponse> succeeds)
        {
            Operation = operation;
            Precedes = precedes;
            Succeeds = succeeds;
        }

        [JsonPropertyName("operation")]
        public Link<OperationResponse> Operation { get; }

        [JsonPropertyName("precedes")]
        public Link<EffectResponse> Precedes { get; }

        [JsonPropertyName("succeeds")]
        public Link<EffectResponse> Succeeds { get; }
    }
}