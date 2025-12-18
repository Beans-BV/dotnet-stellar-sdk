using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

/// <summary>
/// Unit tests for deserializing fee stats responses from JSON.
/// </summary>
[TestClass]
public class FeeStatsDeserializerTest
{
    /// <summary>
    /// Verifies that FeeStatsResponse can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithFeeStatsJson_ReturnsDeserializedFeeStats()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("feeStats.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var stats = JsonSerializer.Deserialize<FeeStatsResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(stats);
        AssertTestData(stats);
    }

    /// <summary>
    /// Verifies that FeeStatsResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithFeeStats_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("feeStats.json");
        var json = File.ReadAllText(jsonPath);
        var stats = JsonSerializer.Deserialize<FeeStatsResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(stats);
        var back = JsonSerializer.Deserialize<FeeStatsResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(stats);
        Assert.IsNotNull(back);
        Assert.AreEqual(stats.LastLedger, back.LastLedger);
        Assert.AreEqual(stats.LastLedgerBaseFee, back.LastLedgerBaseFee);
        Assert.AreEqual(stats.LedgerCapacityUsage, back.LedgerCapacityUsage);
    }

    public static void AssertTestData(FeeStatsResponse stats)
    {
        Assert.AreEqual(0.97D, stats.LedgerCapacityUsage);

        // Assert Fee Charged Data
        Assert.AreEqual(1L, stats.FeeCharged.Min);
        Assert.AreEqual(100L, stats.FeeCharged.Max);
        Assert.AreEqual(100L, stats.FeeCharged.Mode);
        Assert.AreEqual(10L, stats.FeeCharged.P10);
        Assert.AreEqual(20L, stats.FeeCharged.P20);
        Assert.AreEqual(30L, stats.FeeCharged.P30);
        Assert.AreEqual(40L, stats.FeeCharged.P40);
        Assert.AreEqual(50L, stats.FeeCharged.P50);
        Assert.AreEqual(60L, stats.FeeCharged.P60);
        Assert.AreEqual(70L, stats.FeeCharged.P70);
        Assert.AreEqual(80L, stats.FeeCharged.P80);
        Assert.AreEqual(90L, stats.FeeCharged.P90);
        Assert.AreEqual(95L, stats.FeeCharged.P95);
        Assert.AreEqual(99L, stats.FeeCharged.P99);

        //Assert Max Fee Data
        Assert.AreEqual(1L, stats.MaxFee.Min);
        Assert.AreEqual(100L, stats.MaxFee.Mode);
        Assert.AreEqual(10L, stats.MaxFee.P10);
        Assert.AreEqual(20L, stats.MaxFee.P20);
        Assert.AreEqual(30L, stats.MaxFee.P30);
        Assert.AreEqual(40L, stats.MaxFee.P40);
        Assert.AreEqual(50L, stats.MaxFee.P50);
        Assert.AreEqual(60L, stats.MaxFee.P60);
        Assert.AreEqual(70L, stats.MaxFee.P70);
        Assert.AreEqual(80L, stats.MaxFee.P80);
        Assert.AreEqual(90L, stats.MaxFee.P90);
        Assert.AreEqual(95L, stats.MaxFee.P95);
        Assert.AreEqual(99L, stats.MaxFee.P99);
    }
}