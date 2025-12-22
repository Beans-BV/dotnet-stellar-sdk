using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

/// <summary>
///     Unit tests for <see cref="EndSponsoringFutureReservesOperationResponse" /> class.
/// </summary>
[TestClass]
public class EndSponsoringFutureReservesOperationResponseTest
{
    /// <summary>
    ///     Verifies that EndSponsoringFutureReservesOperationResponse can be serialized and deserialized correctly
    ///     (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithEndSponsoringFutureReservesOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("endSponsoringFutureReserves.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertEndSponsoringFutureReservesData(back);
    }

    private static void AssertEndSponsoringFutureReservesData(OperationResponse instance)
    {
        Assert.IsTrue(instance is EndSponsoringFutureReservesOperationResponse);
        var operation = (EndSponsoringFutureReservesOperationResponse)instance;

        Assert.AreEqual(215542933753859, operation.Id);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.BeginSponsor);
        Assert.IsNull(operation.BeginSponsorMuxed);
        Assert.IsNull(operation.BeginSponsorMuxedId);
    }

    /// <summary>
    ///     Verifies that EndSponsoringFutureReservesOperationResponse with muxed account can be serialized and deserialized
    ///     correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithEndSponsoringFutureReservesOperationMuxed_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("endSponsoringFutureReservesMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertEndSponsoringFutureReservesDataMuxed(back);
    }

    private static void AssertEndSponsoringFutureReservesDataMuxed(OperationResponse instance)
    {
        Assert.IsTrue(instance is EndSponsoringFutureReservesOperationResponse);
        var operation = (EndSponsoringFutureReservesOperationResponse)instance;

        Assert.AreEqual(215542933753859, operation.Id);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.BeginSponsor);
        Assert.AreEqual("MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24",
            operation.BeginSponsorMuxed);
        Assert.AreEqual(5123456789UL, operation.BeginSponsorMuxedId);
    }
}