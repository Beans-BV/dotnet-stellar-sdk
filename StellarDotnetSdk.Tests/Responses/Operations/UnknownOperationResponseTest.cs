using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

/// <summary>
///     Unit tests for unknown operation response handling.
/// </summary>
[TestClass]
public class UnknownOperationResponseTest
{
    /// <summary>
    ///     Verifies that deserializing an unknown operation type throws JsonException.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithUnknownOperationJson_ThrowsJsonException()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("unknownOperation.json");
        var json = File.ReadAllText(jsonPath);

        // Act & Assert
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions));
    }
}