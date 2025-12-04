using System.IO;
using System.Text.Json;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class CreatePassiveOfferOperationResponseTest
{
    [TestMethod]
    public void TestDeserializeCreatePassiveOfferOperation()
    {
        var jsonPath = Utils.GetTestDataPath("createPassiveOffer.json");
        var json = File.ReadAllText(jsonPath);
        var instance =
            JsonSerializer.Deserialize<CreatePassiveOfferOperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertCreatePassiveOfferData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeCreatePassiveOfferOperation()
    {
        var jsonPath = Utils.GetTestDataPath("createPassiveOffer.json");
        var json = File.ReadAllText(jsonPath);
        var instance =
            JsonSerializer.Deserialize<CreatePassiveOfferOperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<CreatePassiveOfferOperationResponse>(serialized,
            JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertCreatePassiveOfferData(back);
    }

    private static void AssertCreatePassiveOfferData(CreatePassiveOfferOperationResponse operation)
    {
        Assert.AreEqual(operation.Amount, "11.27827");
        Assert.AreEqual(operation.BuyingAsset,
            Asset.CreateNonNativeAsset("USD", "GDS5JW5E6DRSSN5XK4LW7E6VUMFKKE2HU5WCOVFTO7P2RP7OXVCBLJ3Y"));
        Assert.AreEqual(operation.SellingAsset, new AssetTypeNative());

        operation.Price
            .Should().Be("1.2");

        operation.PriceRatio.Numerator
            .Should().Be(11);

        operation.PriceRatio.Denominator
            .Should().Be(10);
    }
}