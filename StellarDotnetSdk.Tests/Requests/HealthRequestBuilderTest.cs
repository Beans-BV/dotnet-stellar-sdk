using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Tests.Responses;

namespace StellarDotnetSdk.Tests.Requests;

/// <summary>
///     Unit tests for <see cref="HealthRequestBuilder" /> class.
/// </summary>
[TestClass]
public class HealthRequestBuilderTest
{
    /// <summary>
    ///     Verifies that HealthRequestBuilder.BuildUri correctly constructs URI for health endpoint.
    /// </summary>
    [TestMethod]
    public void BuildUri_WithDefaultParameters_BuildsCorrectUri()
    {
        // Arrange
        using var server = new Server("https://horizon-testnet.stellar.org");

        // Act
        var uri = server.Health.BuildUri();

        // Assert
        Assert.AreEqual("https://horizon-testnet.stellar.org/health", uri.ToString());
    }

    /// <summary>
    ///     Verifies that HealthRequestBuilder.Execute correctly retrieves and deserializes health data.
    /// </summary>
    [TestMethod]
    public async Task Execute_WithDefaultParameters_ReturnsDeserializedHealth()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/health.json");

        // Act
        var health = await server.Health.Execute();

        // Assert
        HealthDeserializerTest.AssertTestData(health);
    }
}