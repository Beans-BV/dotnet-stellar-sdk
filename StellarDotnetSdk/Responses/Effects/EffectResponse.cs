using System;
using Newtonsoft.Json;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Responses.Effects;

[JsonConverter(typeof(EffectDeserializer))]
public abstract class EffectResponse : Response, IPagingToken
{
    [JsonProperty(PropertyName = "id")] public string Id { get; protected set; }

    [JsonProperty(PropertyName = "account")]
    public string Account { get; protected set; }

    [JsonProperty(PropertyName = "account_muxed")]
    public string AccountMuxed { get; protected set; }

    [JsonProperty(PropertyName = "account_muxed_id")]
    public ulong? AccountMuxedID { get; protected set; }

    [JsonProperty(PropertyName = "type")] public string Type { get; protected set; }

    [JsonProperty(PropertyName = "type_i")]
    public virtual int TypeId { get; }

    [JsonProperty(PropertyName = "_links")]
    public EffectsResponseLinks Links { get; protected set; }

    [JsonProperty(PropertyName = "created_at")]
    public DateTime CreatedAt { get; protected set; }

    [JsonProperty(PropertyName = "paging_token")]
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

        [JsonProperty(PropertyName = "operation")]
        public Link<OperationResponse> Operation { get; }

        [JsonProperty(PropertyName = "precedes")]
        public Link<EffectResponse> Precedes { get; }

        [JsonProperty(PropertyName = "succeeds")]
        public Link<EffectResponse> Succeeds { get; }
    }
}