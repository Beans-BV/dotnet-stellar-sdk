using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

/// <summary>
///     Unit tests for <see cref="ClaimClaimableBalanceOperationResponse" /> class.
/// </summary>
[TestClass]
public class ClaimClaimableBalanceOperationResponseTest
{
    /// <summary>
    ///     Verifies that ClaimClaimableBalanceOperationResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithClaimClaimableBalanceOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("claimClaimableBalance.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertClaimClaimableBalanceData(back);
    }

    private static void AssertClaimClaimableBalanceData(OperationResponse instance)
    {
        Assert.IsTrue(instance is ClaimClaimableBalanceOperationResponse);
        var operation = (ClaimClaimableBalanceOperationResponse)instance;

        Assert.AreEqual(214525026504705, operation.Id);
        Assert.AreEqual("00000000526674017c3cf392614b3f2f500230affd58c7c364625c350c61058fbeacbdf7",
            operation.BalanceId);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.Claimant);
        Assert.IsNull(operation.ClaimantMuxed);
        Assert.IsNull(operation.ClaimantMuxedId);
    }

    /// <summary>
    ///     Verifies that ClaimClaimableBalanceOperationResponse with muxed account can be serialized and deserialized
    ///     correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithClaimClaimableBalanceOperationMuxed_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("claimClaimableBalanceMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertClaimClaimableBalanceDataMuxed(back);
    }

    private static void AssertClaimClaimableBalanceDataMuxed(OperationResponse instance)
    {
        Assert.IsTrue(instance is ClaimClaimableBalanceOperationResponse);
        var operation = (ClaimClaimableBalanceOperationResponse)instance;

        Assert.AreEqual(214525026504705, operation.Id);
        Assert.AreEqual("00000000526674017c3cf392614b3f2f500230affd58c7c364625c350c61058fbeacbdf7",
            operation.BalanceId);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.Claimant);
        Assert.AreEqual("MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24",
            operation.ClaimantMuxed);
        Assert.AreEqual(5123456789UL, operation.ClaimantMuxedId);
    }
}