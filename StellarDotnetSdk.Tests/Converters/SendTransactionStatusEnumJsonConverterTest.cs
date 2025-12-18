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
}