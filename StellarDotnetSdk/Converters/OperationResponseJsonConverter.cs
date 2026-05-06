using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Converters;

/// <summary>
///     JSON converter for polymorphic OperationResponse deserialization.
///     Uses the 'type_i' discriminator field to determine the concrete operation type.
/// </summary>
/// <remarks>
///     This converter handles 27 operation types (type_i: 0-26):
///     0 = CreateAccount
///     1 = Payment
///     2 = PathPaymentStrictReceive
///     etc.
///     Performance: Parses JSON once into JsonDocument, then deserializes from JsonElement
///     to avoid double-parsing overhead. Dispatches via a <see cref="FrozenDictionary{TKey,TValue}" />
///     for an O(1), read-optimized lookup over the immutable discriminator set.
/// </remarks>
/// <remarks>
///     <p>
///         Designed for deserializing collections or properties typed as <see cref="OperationResponse" />.
///         Direct deserialization to concrete subclasses bypasses this converter intentionally.
///         <example>
///             Use with base type: <c>JsonSerializer.Deserialize&lt;OperationResponse&gt;(json)</c> - converter is
///             invoked.
///             <br />
///             Not with subclass: <c>JsonSerializer.Deserialize&lt;CreateAccountOperationResponse&gt;(json)</c> -
///             converter is bypassed.
///         </example>
///     </p>
/// </remarks>
public class OperationResponseJsonConverter : JsonConverter<OperationResponse>
{
    /// <summary>
    ///     Frozen lookup table mapping <c>type_i</c> discriminator values to concrete-type deserializers.
    ///     Built once at type initialization for optimal read performance.
    /// </summary>
    private static readonly FrozenDictionary<int, Func<JsonElement, JsonSerializerOptions, OperationResponse?>>
        Deserializers =
            new Dictionary<int, Func<JsonElement, JsonSerializerOptions, OperationResponse?>>
            {
                [0] = static (root, options) => root.Deserialize<CreateAccountOperationResponse>(options),
                [1] = static (root, options) => root.Deserialize<PaymentOperationResponse>(options),
                [2] = static (root, options) => root.Deserialize<PathPaymentStrictReceiveOperationResponse>(options),
                [3] = static (root, options) => root.Deserialize<ManageSellOfferOperationResponse>(options),
                [4] = static (root, options) => root.Deserialize<CreatePassiveOfferOperationResponse>(options),
                [5] = static (root, options) => root.Deserialize<SetOptionsOperationResponse>(options),
                [6] = static (root, options) => root.Deserialize<ChangeTrustOperationResponse>(options),
                [7] = static (root, options) => root.Deserialize<AllowTrustOperationResponse>(options),
                [8] = static (root, options) => root.Deserialize<AccountMergeOperationResponse>(options),
                [9] = static (root, options) => root.Deserialize<InflationOperationResponse>(options),
                [10] = static (root, options) => root.Deserialize<ManageDataOperationResponse>(options),
                [11] = static (root, options) => root.Deserialize<BumpSequenceOperationResponse>(options),
                [12] = static (root, options) => root.Deserialize<ManageBuyOfferOperationResponse>(options),
                [13] = static (root, options) => root.Deserialize<PathPaymentStrictSendOperationResponse>(options),
                [14] = static (root, options) => root.Deserialize<CreateClaimableBalanceOperationResponse>(options),
                [15] = static (root, options) => root.Deserialize<ClaimClaimableBalanceOperationResponse>(options),
                [16] = static (root, options) =>
                    root.Deserialize<BeginSponsoringFutureReservesOperationResponse>(options),
                [17] = static (root, options) =>
                    root.Deserialize<EndSponsoringFutureReservesOperationResponse>(options),
                [18] = static (root, options) => root.Deserialize<RevokeSponsorshipOperationResponse>(options),
                [19] = static (root, options) => root.Deserialize<ClawbackOperationResponse>(options),
                [20] = static (root, options) => root.Deserialize<ClawbackClaimableBalanceOperationResponse>(options),
                [21] = static (root, options) => root.Deserialize<SetTrustlineFlagsOperationResponse>(options),
                [22] = static (root, options) => root.Deserialize<LiquidityPoolDepositOperationResponse>(options),
                [23] = static (root, options) => root.Deserialize<LiquidityPoolWithdrawOperationResponse>(options),
                [24] = static (root, options) => root.Deserialize<InvokeHostFunctionOperationResponse>(options),
                [25] = static (root, options) => root.Deserialize<ExtendFootprintOperationResponse>(options),
                [26] = static (root, options) => root.Deserialize<RestoreFootprintOperationResponse>(options),
            }.ToFrozenDictionary();

    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        // Only handle the base type, not concrete subclasses
        return typeToConvert == typeof(OperationResponse);
    }

    /// <inheritdoc />
    public override OperationResponse? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        // Parse JSON once into document
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        // Extract discriminator from parsed document
        if (!root.TryGetProperty("type_i", out var typeProperty))
        {
            throw new JsonException(
                "Property 'type_i' not found in JSON. " +
                "This property is required for determining the operation type."
            );
        }

        var type = typeProperty.GetInt32();

        // Dispatch via frozen lookup table (O(1) read-optimized lookup over the immutable discriminator set)
        if (!Deserializers.TryGetValue(type, out var deserializer))
        {
            throw new JsonException(
                $"Unknown operation type_i: {type}. " +
                $"Expected value between 0-26. " +
                $"This may indicate an API version mismatch. Check if your SDK version supports this operation type."
            );
        }

        // Deserialize from already-parsed JsonElement
        // Because CanConvert only matches exact type, we can safely pass options
        return deserializer(root, options);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, OperationResponse value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}