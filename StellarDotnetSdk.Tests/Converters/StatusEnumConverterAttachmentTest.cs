using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Requests.SorobanRpc;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.SorobanRpc;
using LiquidityPoolTypeEnum = StellarDotnetSdk.Xdr.LiquidityPoolType.LiquidityPoolTypeEnum;

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
    ///     Pins the negative half of the documented split: the CHANGELOG and the
    ///     <see cref="EventFilterTypeJsonConverter" /> write remarks both say <c>LiquidityPoolTypeEnum</c> is the
    ///     one strict-converter enum <em>without</em> a type-level attribute, so it falls through to the built-in
    ///     handling under bare options. If it is ever pinned too, this fails and points at the two documents to
    ///     update, instead of both going stale silently.
    /// </summary>
    [TestMethod]
    public void LiquidityPoolTypeEnum_CarriesNoTypeLevelConverterAttribute()
    {
        Assert.IsNull(
            Attribute.GetCustomAttribute(typeof(LiquidityPoolTypeEnum),
                typeof(JsonConverterAttribute)),
            "LiquidityPoolTypeEnum now carries a type-level [JsonConverter]: update the CHANGELOG and the " +
            "EventFilterTypeJsonConverter.Write remarks, which both describe it as falling through to the catch-all.");
        Assert.AreEqual("0", JsonSerializer.Serialize(
            LiquidityPoolTypeEnum.LIQUIDITY_POOL_CONSTANT_PRODUCT, new JsonSerializerOptions()));
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
    // The two EventFilterType spellings that differ from a single literal: the combined flags (written with a
    // bare comma, not ", ") and None (the empty key). Writing the combination with a space, or dropping the
    // empty-key branch from ReadAsPropertyName, left the suite green until these rows existed.
    [DataRow(typeof(Dictionary<EventFilterType, int>), "system,contract")]
    [DataRow(typeof(Dictionary<EventFilterType, int>), "")]
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

    /// <summary>
    ///     Covers the <em>rejection</em> branch of every <c>ReadAsPropertyName</c> override. The round-trip test
    ///     above only exercises valid keys, so deleting a guard from any of these methods left the whole suite
    ///     green — the point of these overrides is that a bad key fails as a <see cref="JsonException" /> a
    ///     caller can catch, and that half was unasserted.
    /// </summary>
    [DataTestMethod]
    [DataRow(typeof(Dictionary<SubmitTransactionAsyncResponse.TransactionStatus, int>), "pending")]
    [DataRow(typeof(Dictionary<SubmitTransactionAsyncResponse.TransactionStatus, int>), "0")]
    [DataRow(typeof(Dictionary<SendTransactionResponse.SendTransactionStatus, int>), "NOPE")]
    [DataRow(typeof(Dictionary<SendTransactionResponse.SendTransactionStatus, int>), "0")]
    [DataRow(typeof(Dictionary<SendTransactionResponse.SendTransactionStatus, int>), "pending")]
    [DataRow(typeof(Dictionary<TransactionInfo.TransactionStatus, int>), "success")]
    [DataRow(typeof(Dictionary<TransactionInfo.TransactionStatus, int>), "1")]
    [DataRow(typeof(Dictionary<EventFilterType, int>), "diagnostic")]
    [DataRow(typeof(Dictionary<EventFilterType, int>), "system, contract")]
    public void ReadAsPropertyName_WithAnUnacceptableKey_ThrowsJsonException(Type dictionaryType, string key)
    {
        var json = $"{{\"{key}\":1}}";

        foreach (var options in new[] { new JsonSerializerOptions(), JsonOptions.DefaultOptions })
        {
            var exception = Assert.ThrowsException<JsonException>(
                () => JsonSerializer.Deserialize(json, dictionaryType, options),
                $"Key '{key}' was accepted for {dictionaryType.Name}.");
            // Pin the message, not just the type, so a guard that throws an empty JsonException still fails.
            StringAssert.Contains(exception.Message, "cannot be converted");
        }
    }

    /// <summary>
    ///     A rejected key is echoed through the same clamp-and-escape as a rejected value. The key is as
    ///     server-controlled as the value, but the overlong-value test in <c>UntrustedJsonValueTest</c> only
    ///     reaches the value path: reverting a <c>ReadAsPropertyName</c> to raw interpolation left the suite green
    ///     until this test existed. <see cref="TransactionInfo.TransactionStatus" /> has its own copy in
    ///     <c>TransactionStatusJsonConverterTest</c>.
    /// </summary>
    [DataTestMethod]
    [DataRow(typeof(Dictionary<SubmitTransactionAsyncResponse.TransactionStatus, int>))]
    [DataRow(typeof(Dictionary<SendTransactionResponse.SendTransactionStatus, int>))]
    [DataRow(typeof(Dictionary<EventFilterType, int>))]
    public void ReadAsPropertyName_WithAHostileKey_ProducesABoundedEscapedMessage(Type dictionaryType)
    {
        var json = "{\"FORGED\\r\\nINFO confirmed" + new string('A', 500_000) + "\":1}";

        var exception = Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize(json, dictionaryType, JsonOptions.DefaultOptions));

        Assert.IsTrue(exception.Message.Length < 512,
            $"{dictionaryType.Name}: the message grew with the payload to {exception.Message.Length} characters.");
        Assert.IsFalse(exception.Message.Contains('\n'), "A raw newline in the key reached the message.");
        StringAssert.Contains(exception.Message, "'FORGED\\u000d\\u000aINFO confirmed");
    }

    /// <summary>
    ///     Covers the rejection branch of every <c>WriteAsPropertyName</c> override, reached by casting an
    ///     undefined value into the key position. Without the guard the undefined value is written as a bare
    ///     number, producing a document this converter's own reader then rejects — so the type would not
    ///     round-trip, and nothing asserted it.
    /// </summary>
    [TestMethod]
    public void WriteAsPropertyName_WithAnUndefinedValue_ThrowsJsonException()
    {
        foreach (var options in new[] { new JsonSerializerOptions(), JsonOptions.DefaultOptions })
        {
            // Each rejection pins its message, not just the type: a guard that throws an empty JsonException
            // would still pass a type-only assertion, and leave an operator unable to tell which value was bad.
            AssertRejectsUndefined(() => JsonSerializer.Serialize(
                new Dictionary<SubmitTransactionAsyncResponse.TransactionStatus, int>
                    { [(SubmitTransactionAsyncResponse.TransactionStatus)99] = 1 }, options));
            AssertRejectsUndefined(() => JsonSerializer.Serialize(
                new Dictionary<SendTransactionResponse.SendTransactionStatus, int>
                    { [(SendTransactionResponse.SendTransactionStatus)99] = 1 }, options));
            AssertRejectsUndefined(() => JsonSerializer.Serialize(
                new Dictionary<TransactionInfo.TransactionStatus, int>
                    { [(TransactionInfo.TransactionStatus)99] = 1 }, options));
            AssertRejectsUndefined(() => JsonSerializer.Serialize(
                new Dictionary<EventFilterType, int> { [(EventFilterType)99] = 1 }, options));

            // Positive control: defined values in the same key position serialize, so the rejections above are
            // about the undefined value and not about dictionary keys being unsupported outright.
            Assert.AreEqual("{\"ERROR\":1}", JsonSerializer.Serialize(
                new Dictionary<SubmitTransactionAsyncResponse.TransactionStatus, int>
                    { [SubmitTransactionAsyncResponse.TransactionStatus.ERROR] = 1 }, options));
        }
    }

    /// <summary>
    ///     Pins the CHANGELOG's claim that the property-name overloads always write the wire literal. The
    ///     built-in enum key handling applied the caller's <see cref="JsonSerializerOptions.DictionaryKeyPolicy" />
    ///     (a camelCase policy wrote <c>trY_AGAIN_LATER</c>), and a key written that way is one these converters'
    ///     own readers reject — so honouring the policy would make the type stop round-tripping.
    /// </summary>
    [TestMethod]
    public void WriteAsPropertyName_IgnoresTheCallersDictionaryKeyPolicy()
    {
        var camel = new JsonSerializerOptions { DictionaryKeyPolicy = JsonNamingPolicy.CamelCase };

        Assert.AreEqual("{\"TRY_AGAIN_LATER\":1}", JsonSerializer.Serialize(
            new Dictionary<SubmitTransactionAsyncResponse.TransactionStatus, int>
                { [SubmitTransactionAsyncResponse.TransactionStatus.TRY_AGAIN_LATER] = 1 }, camel));
        Assert.AreEqual("{\"TRY_AGAIN_LATER\":1}", JsonSerializer.Serialize(
            new Dictionary<SendTransactionResponse.SendTransactionStatus, int>
                { [SendTransactionResponse.SendTransactionStatus.TRY_AGAIN_LATER] = 1 }, camel));
        Assert.AreEqual("{\"NOT_FOUND\":1}", JsonSerializer.Serialize(
            new Dictionary<TransactionInfo.TransactionStatus, int>
                { [TransactionInfo.TransactionStatus.NOT_FOUND] = 1 }, camel));
        Assert.AreEqual("{\"system,contract\":1}", JsonSerializer.Serialize(
            new Dictionary<EventFilterType, int> { [EventFilterType.System | EventFilterType.Contract] = 1 },
            new JsonSerializerOptions { DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseUpper }));
    }

    /// <summary>
    ///     Pins the CHANGELOG's claim that a source-generated context with <c>UseStringEnumConverter = true</c>
    ///     does not shadow the type attribute the way a runtime <see cref="JsonStringEnumConverter" /> in the
    ///     options' <c>Converters</c> collection does: the ordinal that read as the zero or success member before
    ///     now throws, while the literal still reads.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSourceGeneratedStringEnumContext_StillRejectsOrdinals()
    {
        var context = StatusEnumSourceGenContext.Default;

        // Control: the option is honoured on this leg. An enum with no converter attribute reads and writes as a
        // string through the same context. If a toolchain ever ignored UseStringEnumConverter, the rejections
        // below would pass without the type attribute having been contested at all.
        Assert.AreEqual(SourceGenControlEnum.Second,
            JsonSerializer.Deserialize("\"Second\"", context.SourceGenControlEnum));
        Assert.AreEqual("\"Second\"",
            JsonSerializer.Serialize(SourceGenControlEnum.Second, context.SourceGenControlEnum));
        // ...and it still accepts an ordinal there, so the rejections below come from the type attribute and not
        // from a string-enum handling that happens to refuse integers on its own.
        Assert.AreEqual(SourceGenControlEnum.First, JsonSerializer.Deserialize("0", context.SourceGenControlEnum));

        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize("0", context.SubmitTransactionAsyncResponseTransactionStatus));
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize("0", context.SendTransactionStatus));
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize("1", context.TransactionInfoTransactionStatus));

        // Positive control: the same context reads the literal, so the rejections are about the ordinal form.
        Assert.AreEqual(SendTransactionResponse.SendTransactionStatus.PENDING,
            JsonSerializer.Deserialize("\"PENDING\"", context.SendTransactionStatus));
    }

    private static void AssertRejectsUndefined(Func<string> serialize)
    {
        var exception = Assert.ThrowsException<JsonException>(() => serialize());
        StringAssert.Contains(exception.Message, "Value '99' is not a defined");
    }
}

/// <summary>An enum with no converter attribute: the positive control for the source-generated context.</summary>
public enum SourceGenControlEnum
{
    First,
    Second,
}

[JsonSourceGenerationOptions(UseStringEnumConverter = true)]
[JsonSerializable(typeof(SourceGenControlEnum))]
[JsonSerializable(typeof(SubmitTransactionAsyncResponse.TransactionStatus),
    TypeInfoPropertyName = "SubmitTransactionAsyncResponseTransactionStatus")]
[JsonSerializable(typeof(SendTransactionResponse.SendTransactionStatus))]
[JsonSerializable(typeof(TransactionInfo.TransactionStatus),
    TypeInfoPropertyName = "TransactionInfoTransactionStatus")]
internal partial class StatusEnumSourceGenContext : JsonSerializerContext
{
}
