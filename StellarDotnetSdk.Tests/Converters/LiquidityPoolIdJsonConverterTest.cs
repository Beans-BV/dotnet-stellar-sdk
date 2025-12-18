using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.LiquidityPool;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Tests for LiquidityPoolIdJsonConverter.
///     Focus: string to LiquidityPoolId conversion.
/// </summary>
[TestClass]
public class LiquidityPoolIdJsonConverterTest
{
    private readonly JsonSerializerOptions _options = JsonOptions.DefaultOptions;

    /// <summary>
    ///     Tests round-trip serialization and deserialization of LiquidityPoolId.
    ///     Verifies that serialized LiquidityPoolId can be deserialized back with matching value.
    /// </summary>
    [TestMethod]
    public void RoundTrip_WithLiquidityPoolId_RoundTripsCorrectly()
    {
        // Arrange
        var original = new LiquidityPoolId("4f7f29db33ead1a38c2edf17aa0416c369c207ca081de5c686c050c1ad320385");

        // Act
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<LiquidityPoolId>(json, _options);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.ToString(), deserialized.ToString());
    }

    /// <summary>
    ///     Tests deserialization of valid hex string to LiquidityPoolId.
    ///     Verifies that hex string JSON deserializes to LiquidityPoolId instance.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithValidHexString_ReturnsLiquidityPoolId()
    {
        // Arrange
        var json = @"""4f7f29db33ead1a38c2edf17aa0416c369c207ca081de5c686c050c1ad320385""";

        // Act
        var result = JsonSerializer.Deserialize<LiquidityPoolId>(json, _options);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("4f7f29db33ead1a38c2edf17aa0416c369c207ca081de5c686c050c1ad320385", result.ToString());
    }

    /// <summary>
    ///     Tests that deserialization throws JsonException for invalid token types.
    ///     Verifies proper error handling when JSON is not a string.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_WithInvalidTokenType_ThrowsJsonException()
    {
        // Arrange - Tests that non-string tokens throw (number, object, array all hit same code path)
        var json = "123";

        // Act & Assert
        JsonSerializer.Deserialize<LiquidityPoolId>(json, _options);
    }
}