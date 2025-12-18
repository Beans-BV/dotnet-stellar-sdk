using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

/// <summary>
///     Unit tests for <see cref="ManageBuyOfferOperationResponse" /> class.
/// </summary>
[TestClass]
public class ManageBuyOfferOperationResponseTest
{
    /// <summary>
    ///     Verifies that ManageBuyOfferOperationResponse can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageBuyOfferOperationJson_ReturnsDeserializedOperation()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("manageBuyOffer.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertManageBuyOfferData(instance);
    }

    /// <summary>
    ///     Verifies that ManageBuyOfferOperationResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithManageBuyOfferOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("manageBuyOffer.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertManageBuyOfferData(back);
    }

    private static void AssertManageBuyOfferData(OperationResponse instance)
    {
        Assert.IsTrue(instance is ManageBuyOfferOperationResponse);
        var operation = (ManageBuyOfferOperationResponse)instance;

        Assert.AreEqual("1", operation.OfferId);
        Assert.AreEqual("50000.0000000", operation.Amount);

        Assert.AreEqual("0.0463000", operation.Price);
        Assert.AreEqual(463L, operation.PriceRatio.Numerator);
        Assert.AreEqual(10000L, operation.PriceRatio.Denominator);

        Assert.AreEqual(Asset.CreateNonNativeAsset("RMT", "GDEGOXPCHXWFYY234D2YZSPEJ24BX42ESJNVHY5H7TWWQSYRN5ZKZE3N"),
            operation.BuyingAsset);
        Assert.AreEqual(new AssetTypeNative(), operation.SellingAsset);
    }
}