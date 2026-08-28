using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.SorobanRpc;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Tests for SendTransactionStatusEnumJsonConverter.
///     Focus: enum string conversion.
/// </summary>
[TestClass]
public class SendTransactionStatusEnumJsonConverterTest
{
    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new SendTransactionStatusEnumJsonConverter() },
    };

    /// <summary>
    ///     Tests round-trip serialization and deserialization for all status enum values.
    ///     Verifies that all status values serialize to correct strings and deserialize back correctly.
    /// </summary>
    [TestMethod]
    [DataRow("PENDING", SendTransactionResponse.SendTransactionStatus.PENDING)]
    [DataRow("TRY_AGAIN_LATER", SendTransactionResponse.SendTransactionStatus.TRY_AGAIN_LATER)]
    [DataRow("DUPLICATE", SendTransactionResponse.SendTransactionStatus.DUPLICATE)]
    [DataRow("ERROR", SendTransactionResponse.SendTransactionStatus.ERROR)]
    public void RoundTrip_WithAllStatuses_RoundTripsCorrectly(string jsonValue,
        SendTransactionResponse.SendTransactionStatus expected)
    {
        // Arrange
        var json = $"\"{jsonValue}\"";

        // Act - Read
        var deserialized = JsonSerializer.Deserialize<SendTransactionResponse.SendTransactionStatus>(json, _options);

        // Act - Write
        var serialized = JsonSerializer.Serialize(expected, _options);

        // Assert
        Assert.AreEqual(expected, deserialized);
        Assert.AreEqual($"\"{jsonValue}\"", serialized);
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException for unknown enum string values.
    ///     Verifies proper error handling when JSON contains unrecognized status string.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithUnknownValue_ThrowsJsonException()
    {
        // Arrange
        var json = "\"UNKNOWN\"";

        // Act & Assert
        JsonSerializer.Deserialize<SendTransactionResponse.SendTransactionStatus>(json, _options);
    }

    /// <summary>
    ///     Verifies that this converter — and not the catch-all
    ///     <see cref="System.Text.Json.Serialization.JsonStringEnumConverter" /> — is the one that governs under
    ///     <see cref="JsonOptions.DefaultOptions" />, which is what the SDK actually deserializes responses with.
    ///     <para>
    ///         The other tests in this class build their own options containing only this converter, so they passed
    ///         while <c>DefaultOptions</c> had the standard converter registered ahead of it and never reached this
    ///         one. These cases are the ones that distinguish the two: the standard converter is case-insensitive
    ///         and accepts bare integers, so it read <c>"pending"</c> and <c>0</c> as <c>PENDING</c>.
    ///     </para>
    /// </summary>
    [DataTestMethod]
    [DataRow("\"pending\"")]
    [DataRow("\"Pending\"")]
    [DataRow("\"try_again_later\"")]
    [DataRow("0")]
    [DataRow("3")]
    [DataRow("99")]
    public void Deserialize_WithDefaultOptions_RejectsWhatOnlyTheStandardConverterWouldAccept(string json)
    {
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SendTransactionResponse.SendTransactionStatus>(json,
                JsonOptions.DefaultOptions));
    }

    /// <summary>
    ///     Verifies that the four literals Stellar RPC actually emits still round-trip under
    ///     <see cref="JsonOptions.DefaultOptions" /> after the reordering.
    /// </summary>
    [DataTestMethod]
    [DataRow("PENDING", SendTransactionResponse.SendTransactionStatus.PENDING)]
    [DataRow("TRY_AGAIN_LATER", SendTransactionResponse.SendTransactionStatus.TRY_AGAIN_LATER)]
    [DataRow("DUPLICATE", SendTransactionResponse.SendTransactionStatus.DUPLICATE)]
    [DataRow("ERROR", SendTransactionResponse.SendTransactionStatus.ERROR)]
    public void RoundTrip_WithDefaultOptions_RoundTripsRpcLiterals(string wireValue,
        SendTransactionResponse.SendTransactionStatus expected)
    {
        Assert.AreEqual(expected,
            JsonSerializer.Deserialize<SendTransactionResponse.SendTransactionStatus>($"\"{wireValue}\"",
                JsonOptions.DefaultOptions));
        Assert.AreEqual($"\"{wireValue}\"", JsonSerializer.Serialize(expected, JsonOptions.DefaultOptions));
    }

    /// <summary>
    ///     Verifies the guard rail that keeps the ordering honest: the catch-all converter matches every enum, so
    ///     it must be the last enum converter in the collection or it shadows every specific one registered after
    ///     it. Asserting the position directly means a future insertion cannot silently undo this fix.
    /// </summary>
    [TestMethod]
    public void DefaultOptions_RegistersTheStandardEnumConverterLast()
    {
        var converters = JsonOptions.DefaultOptions.Converters;
        var standardIndex = -1;
        var specificIndex = -1;
        for (var i = 0; i < converters.Count; i++)
        {
            switch (converters[i])
            {
                case System.Text.Json.Serialization.JsonStringEnumConverter:
                    standardIndex = i;
                    break;
                case SendTransactionStatusEnumJsonConverter:
                    specificIndex = i;
                    break;
            }
        }

        Assert.AreNotEqual(-1, standardIndex, "The standard enum converter is no longer registered.");
        Assert.AreNotEqual(-1, specificIndex, "SendTransactionStatusEnumJsonConverter is no longer registered.");
        Assert.IsTrue(specificIndex < standardIndex,
            "SendTransactionStatusEnumJsonConverter must precede JsonStringEnumConverter, which matches every " +
            "enum and would otherwise shadow it.");
    }

    /// <summary>
    ///     Verifies that an absent <c>status</c> is rejected rather than silently yielding the zero member.
    ///     <c>SendTransactionStatus</c> is an enum, i.e. a value type, so <c>RespectNullableAnnotations</c> never
    ///     applied: a response of <c>{"hash":"ab"}</c> deserialized to
    ///     <see cref="SendTransactionResponse.SendTransactionStatus.PENDING" />, presenting a submission the server
    ///     never accepted as pending — the same outcome the <c>"status": 0</c> fix closes, reached by a simpler
    ///     payload. Stellar RPC tags the field <c>json:"status"</c> with no <c>omitempty</c>, so requiring it
    ///     rejects nothing a conforming server sends.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithoutStatus_ThrowsJsonException()
    {
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<SendTransactionResponse>(
                "{\"hash\":\"ab\"}", JsonOptions.DefaultOptions));
    }

    /// <summary>
    ///     A response carrying both required fields still deserializes, so the two guards above reject only what a
    ///     conforming server never sends.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithStatusAndHash_Succeeds()
    {
        var response = JsonSerializer.Deserialize<SendTransactionResponse>(
            "{\"hash\":\"ab\",\"status\":\"PENDING\"}", JsonOptions.DefaultOptions);

        Assert.IsNotNull(response);
        Assert.AreEqual("ab", response.Hash);
        Assert.AreEqual(SendTransactionResponse.SendTransactionStatus.PENDING, response.Status);
    }

    /// <summary>
    ///     Verifies that an undefined value cannot be written. Without the check <c>Write</c> emitted the bare
    ///     number as a string (<c>"99"</c>) — a value this converter's own <c>Read</c> rejects, so the type did not
    ///     round-trip, and its sibling <c>EventFilterTypeJsonConverter</c> already refused the equivalent input.
    /// </summary>
    [TestMethod]
    public void Serialize_WithUndefinedStatus_ThrowsJsonException()
    {
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Serialize(
                (SendTransactionResponse.SendTransactionStatus)99, JsonOptions.DefaultOptions));
    }
}