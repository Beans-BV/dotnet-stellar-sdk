using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;

namespace StellarDotnetSdk.Tests.Converters;

/// <summary>
///     Tests for the centralized <see cref="JsonOptions" /> serializer configuration.
/// </summary>
[TestClass]
public class JsonOptionsTest
{
    /// <summary>
    ///     The shared options must disable <c>AllowDuplicateProperties</c> so malformed JSON
    ///     cannot silently overwrite financial fields.
    /// </summary>
    [TestMethod]
    public void DefaultOptions_DisablesAllowDuplicateProperties()
    {
        // Arrange & Act
        var options = JsonOptions.DefaultOptions;

        // Assert
        Assert.IsFalse(options.AllowDuplicateProperties);
    }

    /// <summary>
    ///     Deserializing a JSON object with a duplicate property must throw, preventing the
    ///     last-write-wins behaviour that could silently corrupt financial data.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithDuplicateProperties_ThrowsJsonException()
    {
        // Arrange: the "amount" field appears twice - a malformed or adversarial response
        // could otherwise overwrite the real amount with the second value.
        const string json = """
                            {
                                "destination": "GABCD",
                                "amount": "100.00",
                                "amount": "0.01"
                            }
                            """;

        // Act & Assert
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<Payment>(json, JsonOptions.DefaultOptions));
    }

    /// <summary>
    ///     Well-formed JSON without duplicates must still deserialize successfully so that
    ///     strict validation does not regress normal API responses.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithoutDuplicateProperties_Succeeds()
    {
        // Arrange
        const string json = """
                            {
                                "destination": "GABCD",
                                "amount": "100.00"
                            }
                            """;

        // Act
        var payment = JsonSerializer.Deserialize<Payment>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(payment);
        Assert.AreEqual("GABCD", payment.Destination);
        Assert.AreEqual("100.00", payment.Amount);
    }

    /// <summary>
    ///     Case-insensitive matching is also in effect, so two properties that differ only in
    ///     casing must still be rejected as duplicates.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithCaseInsensitiveDuplicateProperties_ThrowsJsonException()
    {
        // Arrange
        const string json = """
                            {
                                "destination": "GABCD",
                                "Amount": "100.00",
                                "amount": "0.01"
                            }
                            """;

        // Act & Assert
        Assert.ThrowsException<JsonException>(() =>
            JsonSerializer.Deserialize<Payment>(json, JsonOptions.DefaultOptions));
    }

    /// <summary>
    ///     A sample payload shape used to exercise duplicate-property rejection.
    /// </summary>
    private sealed record Payment(
        string Destination,
        string Amount);
}