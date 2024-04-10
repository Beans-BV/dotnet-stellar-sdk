using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;
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
        var instance = JsonSingleton.GetInstance<CreatePassiveOfferOperationResponse>(json);
        Assert.IsNotNull(instance);
        AssertCreatePassiveOfferData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeCreatePassiveOfferOperation()
    {
        var jsonPath = Utils.GetTestDataPath("createPassiveOffer.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<CreatePassiveOfferOperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<CreatePassiveOfferOperationResponse>(serialized);
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