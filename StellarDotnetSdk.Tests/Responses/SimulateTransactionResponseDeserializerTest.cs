using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.SorobanRpc;

namespace StellarDotnetSdk.Tests.Responses;

/// <summary>
///     Unit tests for deserializing <see cref="SimulateTransactionResponse" /> from Stellar RPC payloads.
/// </summary>
[TestClass]
public class SimulateTransactionResponseDeserializerTest
{
    /// <summary>
    ///     Verifies that a <c>minResourceFee</c> within the 32-bit range is deserialized unchanged.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithSmallMinResourceFee_ReturnsValue()
    {
        // Arrange
        const string json = """{"minResourceFee":"100","latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(100L, response.MinResourceFee);
    }

    /// <summary>
    ///     Verifies that a <c>minResourceFee</c> above <see cref="uint.MaxValue" /> deserializes at full width.
    ///     Stellar RPC declares the field as an <c>int64</c>, and values beyond 4 294 967 295 stroops (~429 XLM) are
    ///     reachable on large uploads and restores.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithMinResourceFeeAboveUintMaxValue_ReturnsFullValue()
    {
        // Arrange
        const string json = """{"minResourceFee":"5000000000","latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(5_000_000_000L, response.MinResourceFee);
    }

    /// <summary>
    ///     Verifies that the largest value the <c>int64</c> wire type can carry deserializes without loss.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithMaximumInt64MinResourceFee_ReturnsFullValue()
    {
        // Arrange
        const string json = """{"minResourceFee":"9223372036854775807","latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(long.MaxValue, response.MinResourceFee);
    }

    /// <summary>
    ///     Verifies that an omitted <c>minResourceFee</c> — the shape Stellar RPC returns for a failed simulation —
    ///     stays null.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithoutMinResourceFee_ReturnsNull()
    {
        // Arrange
        const string json = """{"error":"host invocation failed","latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNull(response.MinResourceFee);
    }
}
