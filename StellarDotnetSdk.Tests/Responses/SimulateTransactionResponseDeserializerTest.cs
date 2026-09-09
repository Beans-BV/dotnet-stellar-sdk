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

    /// <summary>
    ///     Verifies that <see cref="SimulateTransactionResponse.SorobanAuthorization" /> reports "no entries" rather
    ///     than throwing when the response carries no <c>results</c> at all. The property indexes <c>Results[0]</c>,
    ///     so without the length half of its guard this dereferences a null array.
    /// </summary>
    [TestMethod]
    public void SorobanAuthorization_WithoutResults_ReturnsNull()
    {
        // Arrange
        const string json = """{"latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNull(response.SorobanAuthorization);
    }

    /// <summary>
    ///     Verifies the same for an empty <c>results</c> array, which is the shape Stellar RPC returns for a
    ///     simulation that produced no results. Without the length half of the guard this throws
    ///     <see cref="System.IndexOutOfRangeException" />.
    /// </summary>
    [TestMethod]
    public void SorobanAuthorization_WithEmptyResults_ReturnsNull()
    {
        // Arrange
        const string json = """{"results":[],"latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNull(response.SorobanAuthorization);
    }

    /// <summary>
    ///     Verifies the element half of the guard, which is not redundant with the length check: <c>[null]</c> has
    ///     length one and would then be dereferenced. A conforming server cannot send it — the Go type is a slice of
    ///     structs, not pointers — but the property exists to behave predictably for responses that are not
    ///     conforming.
    /// </summary>
    [TestMethod]
    public void SorobanAuthorization_WithNullResultElement_ReturnsNull()
    {
        // Arrange
        const string json = """{"results":[null],"latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNull(response.SorobanAuthorization);
    }

    /// <summary>
    ///     Verifies that a result carrying no <c>auth</c> key reports "no entries" rather than throwing. Stellar RPC
    ///     tags the field <c>auth,omitempty</c>, so this is the ordinary shape for a simulation that requires no
    ///     authorization; without the guard the entry loop dereferences a null array.
    /// </summary>
    [TestMethod]
    public void SorobanAuthorization_WithoutAuth_ReturnsNull()
    {
        // Arrange
        const string json = """{"results":[{"xdr":"AAAAAQ=="}],"latestLedger":"1"}""";

        // Act
        var response = JsonSerializer.Deserialize<SimulateTransactionResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNull(response.SorobanAuthorization);
    }
}
