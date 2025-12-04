using System;
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
///     to avoid double-parsing overhead.
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
    public override bool CanConvert(Type typeToConvert)
    {
        // Only handle the base type, not concrete subclasses
        return typeToConvert == typeof(OperationResponse);
    }

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

        // Deserialize from already-parsed JsonElement
        // Because CanConvert only matches exact type, we can safely pass options
        return type switch
        {
            0 => root.Deserialize<CreateAccountOperationResponse>(options),
            1 => root.Deserialize<PaymentOperationResponse>(options),
            2 => root.Deserialize<PathPaymentStrictReceiveOperationResponse>(options),
            3 => root.Deserialize<ManageSellOfferOperationResponse>(options),
            4 => root.Deserialize<CreatePassiveOfferOperationResponse>(options),
            5 => root.Deserialize<SetOptionsOperationResponse>(options),
            6 => root.Deserialize<ChangeTrustOperationResponse>(options),
            7 => root.Deserialize<AllowTrustOperationResponse>(options),
            8 => root.Deserialize<AccountMergeOperationResponse>(options),
            9 => root.Deserialize<InflationOperationResponse>(options),
            10 => root.Deserialize<ManageDataOperationResponse>(options),
            11 => root.Deserialize<BumpSequenceOperationResponse>(options),
            12 => root.Deserialize<ManageBuyOfferOperationResponse>(options),
            13 => root.Deserialize<PathPaymentStrictSendOperationResponse>(options),
            14 => root.Deserialize<CreateClaimableBalanceOperationResponse>(options),
            15 => root.Deserialize<ClaimClaimableBalanceOperationResponse>(options),
            16 => root.Deserialize<BeginSponsoringFutureReservesOperationResponse>(options),
            17 => root.Deserialize<EndSponsoringFutureReservesOperationResponse>(options),
            18 => root.Deserialize<RevokeSponsorshipOperationResponse>(options),
            19 => root.Deserialize<ClawbackOperationResponse>(options),
            20 => root.Deserialize<ClawbackClaimableBalanceOperationResponse>(options),
            21 => root.Deserialize<SetTrustlineFlagsOperationResponse>(options),
            22 => root.Deserialize<LiquidityPoolDepositOperationResponse>(options),
            23 => root.Deserialize<LiquidityPoolWithdrawOperationResponse>(options),
            24 => root.Deserialize<InvokeHostFunctionOperationResponse>(options),
            25 => root.Deserialize<ExtendFootprintOperationResponse>(options),
            26 => root.Deserialize<RestoreFootprintOperationResponse>(options),
            _ => throw new JsonException(
                $"Unknown operation type_i: {type}. " +
                $"Expected value between 0-26. " +
                $"This may indicate an API version mismatch. Check if your SDK version supports this operation type."
            ),
        };
    }

    public override void Write(Utf8JsonWriter writer, OperationResponse value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}