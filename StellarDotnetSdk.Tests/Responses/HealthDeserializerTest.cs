using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

/// <summary>
///     Unit tests for deserializing health responses from JSON.
/// </summary>
[TestClass]
public class HealthDeserializerTest
{
    /// <summary>
    ///     Verifies that HealthResponse can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithHealthJson_ReturnsDeserializedHealth()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("health.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var health = JsonSerializer.Deserialize<HealthResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(health);
        AssertTestData(health);
    }

    /// <summary>
    ///     Verifies that HealthResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithHealth_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("health.json");
        var json = File.ReadAllText(jsonPath);
        var health = JsonSerializer.Deserialize<HealthResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(health, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<HealthResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(health);
        Assert.IsNotNull(back);
        Assert.AreEqual(health.DatabaseConnected, back.DatabaseConnected);
        Assert.AreEqual(health.CoreUp, back.CoreUp);
        Assert.AreEqual(health.CoreSynced, back.CoreSynced);
    }

    public static void AssertTestData(HealthResponse health)
    {
        Assert.IsTrue(health.DatabaseConnected);
        Assert.IsTrue(health.CoreUp);
        Assert.IsTrue(health.CoreSynced);
    }
}