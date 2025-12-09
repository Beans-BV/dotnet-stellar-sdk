using System.IO;
using System.Text.Json;
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
        Assert.AreEqual("1.0000000", operation.Amount);
        Assert.AreEqual("credit_alphanum4", operation.BuyingAssetType);
        Assert.AreEqual("TEST", operation.BuyingAssetCode);
        Assert.AreEqual("GC6ZBHGJGGTPVLYALOKQNQSQUXHJUYDZ7VLMAPU2MERVTYMKVL2GTEST", operation.BuyingAssetIssuer);
        Assert.AreEqual("native", operation.SellingAssetType);
        Assert.IsNull(operation.SellingAssetCode);
        Assert.IsNull(operation.SellingAssetIssuer);
        Assert.AreEqual(Asset.CreateNonNativeAsset("TEST", "GC6ZBHGJGGTPVLYALOKQNQSQUXHJUYDZ7VLMAPU2MERVTYMKVL2GTEST"),
            operation.BuyingAsset);
        Assert.AreEqual(new AssetTypeNative(), operation.SellingAsset);
        Assert.AreEqual("10.1000000", operation.Price);
        Assert.AreEqual(101L, operation.PriceRatio.Numerator);
        Assert.AreEqual(10L, operation.PriceRatio.Denominator);
    }
}