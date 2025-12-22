using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;
using StellarDotnetSdk.Responses.Predicates;

namespace StellarDotnetSdk.Tests.Responses.Operations;

/// <summary>
///     Unit tests for <see cref="CreateClaimableBalanceOperationResponse" /> class.
/// </summary>
[TestClass]
public class CreateClaimableBalanceOperationResponseTest
{
    /// <summary>
    ///     Verifies that CreateClaimableBalanceOperationResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithCreateClaimableBalanceOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("createClaimableBalance.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertCreateClaimableBalanceData(back);
    }

    /// <summary>
    ///     Verifies that CreateClaimableBalanceOperationResponse with absolute time predicate before max int can be
    ///     deserialized correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithCreateClaimableBalanceAbsBeforeMaxIntOperationJson_DeserializesCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("createClaimableBalanceAbsBeforeMaxInt.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsTrue(instance is CreateClaimableBalanceOperationResponse);
        var operation = (CreateClaimableBalanceOperationResponse)instance;

        Assert.IsInstanceOfType(operation.Claimants[0].Predicate, typeof(PredicateBeforeAbsoluteTime));
        var absPredicate = (PredicateBeforeAbsoluteTime)operation.Claimants[0].Predicate;
        Assert.IsNotNull(absPredicate.AbsBefore);
    }

    private static void AssertCreateClaimableBalanceData(OperationResponse instance)
    {
        Assert.IsTrue(instance is CreateClaimableBalanceOperationResponse);
        var operation = (CreateClaimableBalanceOperationResponse)instance;

        Assert.AreEqual(213223651414017, operation.Id);
        Assert.AreEqual("GD2I2F7SWUHBAD7XBIZTF7MBMWQYWJVEFMWTXK76NSYVOY52OJRYNTIY", operation.Sponsor);
        Assert.AreEqual("native", operation.Asset);
        Assert.AreEqual("1.0000000", operation.Amount);
        Assert.AreEqual("GAEJ2UF46PKAPJYED6SQ45CKEHSXV63UQEYHVUZSVJU6PK5Y4ZVA4ELU", operation.Claimants[0].Destination);
    }
}