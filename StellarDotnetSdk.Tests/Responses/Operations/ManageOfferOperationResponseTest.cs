using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

/// <summary>
///     Unit tests for <see cref="ManageOfferOperationResponse" /> class.
/// </summary>
[TestClass]
public class ManageOfferOperationResponseTest
{
    /// <summary>
    ///     Verifies that ManageOfferOperationResponse can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithManageOfferOperationJson_ReturnsDeserializedOperation()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("manageOffer.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertManageOfferData(instance);
    }

    /// <summary>
    ///     Verifies that ManageOfferOperationResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithManageOfferOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("manageOffer.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance, JsonOptions.DefaultOptions);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertManageOfferData(back);
    }

    private static void AssertManageOfferData(OperationResponse instance)
    {
        Assert.IsTrue(instance is ManageSellOfferOperationResponse);
        var operation = (ManageSellOfferOperationResponse)instance;

        Assert.AreEqual("96052902", operation.OfferId);
        Assert.AreEqual("243.7500000", operation.Amount);
        Assert.AreEqual("8.0850240", operation.Price);
        Assert.AreEqual(5054660L, operation.PriceRatio.Numerator);
        Assert.AreEqual(625188L, operation.PriceRatio.Denominator);

        Assert.AreEqual(Asset.CreateNonNativeAsset("USD", "GDSRCV5VTM3U7Y3L6DFRP3PEGBNQMGOWSRTGSBWX6Z3H6C7JHRI4XFJP"),
            operation.SellingAsset);
        Assert.AreEqual(new AssetTypeNative(), operation.BuyingAsset);
    }
}