using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
/// Unit tests for <see cref="FeeStatsRequestBuilder"/> class.
/// </summary>
[TestClass]
public class FeeStatsRequestBuilderTest
{
    /// <summary>
    /// Verifies that FeeStatsRequestBuilder.BuildUri correctly constructs URI for fee stats endpoint.
    /// </summary>
    [TestMethod]
    public void BuildUri_WithDefaultParameters_BuildsCorrectUri()
    {
        // Arrange
        var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.FeeStats.BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/fee_stats", uri.ToString());
    }

    /// <summary>
    /// Verifies that FeeStatsRequestBuilder.Execute correctly retrieves and deserializes fee stats data.
    /// </summary>
    [TestMethod]
    public async Task Execute_WithDefaultParameters_ReturnsDeserializedFeeStats()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/feeStats.json");

        // Act
        var fees = await server.FeeStats.Execute();

        // Assert
        FeeStatsDeserializerTest.AssertTestData(fees);
    }
}