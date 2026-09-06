using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Requests.SorobanRpc;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.SorobanRpc;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Tests the third tier of converter attachment for the SDK's strict status enums: the
///     <see cref="JsonConverterAttribute" /> on the enum <em>type</em>.
/// </summary>
/// <remarks>
///     System.Text.Json resolves converters property attribute first, then the options' <c>Converters</c>
///     collection, then the type attribute. The first two tiers only protect an enum reached <em>through</em> a
///     response object under <see cref="JsonOptions.DefaultOptions" />. The type attribute is what covers the
///     enum travelling on its own — a caller's own DTO field, a persisted status column, a queue message — under
///     an options instance that registers nothing for it. Without it the built-in enum handling maps a bare
///     integer by ordinal, and the ordinal that falls out is the dangerous one in every case: <c>0</c> is
///     <c>PENDING</c> for both submission enums and <c>1</c> is <c>SUCCESS</c> for
///     <see cref="TransactionInfo.TransactionStatus" />.
/// </remarks>
[TestClass]
public class StatusEnumConverterAttachmentTest
{
    /// <summary>
    ///     Every strict status enum must carry a type-level converter. Asserted structurally as well as
    ///     behaviourally so the reason a rejection happens is pinned, not just the rejection.
    /// </summary>
    [DataTestMethod]
    [DataRow(typeof(SubmitTransactionAsyncResponse.TransactionStatus),
        typeof(SubmitTransactionAsyncStatusJsonConverter))]
    [DataRow(typeof(SendTransactionResponse.SendTransactionStatus),
        typeof(SendTransactionStatusEnumJsonConverter))]
    [DataRow(typeof(TransactionInfo.TransactionStatus), typeof(TransactionStatusJsonConverter))]
    [DataRow(typeof(EventFilterType), typeof(EventFilterTypeJsonConverter))]
    public void StatusEnum_CarriesATypeLevelConverterAttribute(Type enumType, Type expectedConverter)
    {
        var attribute = (JsonConverterAttribute?)Attribute.GetCustomAttribute(
            enumType, typeof(JsonConverterAttribute));

        Assert.IsNotNull(attribute,
            $"{enumType.Name} has no type-level [JsonConverter]. Deserialized on its own under an options " +
            "instance that registers no converter for it, a bare integer maps by ordinal.");
        Assert.AreEqual(expectedConverter, attribute.ConverterType);
    }

    /// <summary>
    ///     The behaviour the attribute buys: a bare <see cref="JsonSerializerOptions" /> — no converters
    ///     registered at all — must still refuse an ordinal rather than resolving it to the zero member.
    /// </summary>
    [TestMethod]
    public void Deserialize_BareEnumWithBareOptions_RejectsOrdinals()
    {
        var bare = new JsonSerializerOptions();

        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SubmitTransactionAsyncResponse.TransactionStatus>("0", bare));
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SendTransactionResponse.SendTransactionStatus>("0", bare));
        // Ordinal 1 is SUCCESS here — an unguarded read presented an unsettled transaction as a confirmed one.
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<TransactionInfo.TransactionStatus>("1", bare));

        // Positive control: the same bare options accept the wire literals, so the rejections above are about
        // the ordinal form specifically and not about the converter being unreachable.
        Assert.AreEqual(SubmitTransactionAsyncResponse.TransactionStatus.PENDING,
            JsonSerializer.Deserialize<SubmitTransactionAsyncResponse.TransactionStatus>("\"PENDING\"", bare));
        Assert.AreEqual(TransactionInfo.TransactionStatus.SUCCESS,
            JsonSerializer.Deserialize<TransactionInfo.TransactionStatus>("\"SUCCESS\"", bare));
    }

    /// <summary>
    ///     Shows the tier the type attribute cannot win, and the tier that covers for it. An entry in the
    ///     caller's own <c>Converters</c> collection outranks the type attribute — so the bare enum falls back to
    ///     ordinal mapping there — but the property-level pin on the response outranks both, which is why the
    ///     response properties carry their own pins instead of relying on the type attribute alone.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithACallersOwnEnumConverter_TypeAttributeLosesButThePropertyPinHolds()
    {
        var hostile = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };

        // The type attribute loses to the options' Converters collection: the bare enum maps by ordinal again.
        Assert.AreEqual(SubmitTransactionAsyncResponse.TransactionStatus.PENDING,
            JsonSerializer.Deserialize<SubmitTransactionAsyncResponse.TransactionStatus>("0", hostile));

        // Reached through the response, the property-level pin outranks that collection and still rejects it.
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SubmitTransactionAsyncResponse>(
                """{"tx_status":0,"hash":"aa"}""", hostile));

        var ok = JsonSerializer.Deserialize<SubmitTransactionAsyncResponse>(
            """{"tx_status":"PENDING","hash":"aa"}""", hostile);
        Assert.AreEqual(SubmitTransactionAsyncResponse.TransactionStatus.PENDING, ok!.TxStatus);
    }

    /// <summary>
    ///     Every declared member must round-trip. The per-status tests elsewhere use hardcoded rows, so a member
    ///     added to one of these enums without a matching entry in its converter's lookup table would become
    ///     unserializable in both directions with no test noticing. Enumerating the enum closes that.
    /// </summary>
    [DataTestMethod]
    [DataRow(typeof(SubmitTransactionAsyncResponse.TransactionStatus))]
    [DataRow(typeof(SendTransactionResponse.SendTransactionStatus))]
    [DataRow(typeof(TransactionInfo.TransactionStatus))]
    public void EveryDeclaredMember_RoundTripsAsItsOwnName(Type enumType)
    {
        foreach (var member in Enum.GetValues(enumType))
        {
            var name = member.ToString()!;
            var json = JsonSerializer.Serialize(member, enumType, JsonOptions.DefaultOptions);

            Assert.AreEqual($"\"{name}\"", json,
                $"{enumType.Name}.{name} does not serialize to its own name — the converter's lookup table and " +
                "the enum have diverged.");
            Assert.AreEqual(member, JsonSerializer.Deserialize(json, enumType, JsonOptions.DefaultOptions));
        }
    }

    /// <summary>
    ///     A type-level converter also has to answer for the enum used as a JSON object <em>key</em>. A
    ///     <see cref="JsonConverter{T}" /> that overrides only <c>Read</c>/<c>Write</c> makes
    ///     <c>Dictionary&lt;TEnum, T&gt;</c> throw <see cref="NotSupportedException" /> — which is not a
    ///     <see cref="JsonException" />, so a caller's <c>catch (JsonException)</c> would sail straight past it.
    /// </summary>
    [DataTestMethod]
    [DataRow(typeof(Dictionary<SubmitTransactionAsyncResponse.TransactionStatus, int>), "PENDING")]
    [DataRow(typeof(Dictionary<SendTransactionResponse.SendTransactionStatus, int>), "PENDING")]
    [DataRow(typeof(Dictionary<TransactionInfo.TransactionStatus, int>), "SUCCESS")]
    [DataRow(typeof(Dictionary<EventFilterType, int>), "contract")]
    public void UsedAsADictionaryKey_RoundTripsUnderBareAndDefaultOptions(Type dictionaryType, string key)
    {
        var json = $"{{\"{key}\":1}}";

        foreach (var options in new[] { new JsonSerializerOptions(), JsonOptions.DefaultOptions })
        {
            var parsed = JsonSerializer.Deserialize(json, dictionaryType, options);
            Assert.IsNotNull(parsed);
            Assert.AreEqual(json, JsonSerializer.Serialize(parsed, dictionaryType, options));
        }
    }
}
