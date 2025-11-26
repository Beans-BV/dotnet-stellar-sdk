using System.IO;
using System.Text.Json;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class ManageBuyOfferOperationResponseTest
{
    [TestMethod]
    public void TestDeserializeManageBuyOfferOperation()
    {
        var jsonPath = Utils.GetTestDataPath("manageBuyOffer.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertManageBuyOfferData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeManageBuyOfferOperation()
    {
        var jsonPath = Utils.GetTestDataPath("manageBuyOffer.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertManageBuyOfferData(back);
    }

    private static void AssertManageBuyOfferData(OperationResponse instance)
    {
        Assert.IsTrue(instance is ManageBuyOfferOperationResponse);
        var operation = (ManageBuyOfferOperationResponse)instance;

        Assert.AreEqual(operation.OfferId, "1");
        Assert.AreEqual(operation.Amount, "50000.0000000");

        operation.Price
            .Should().Be("0.0463000");

        operation.PriceRatio.Numerator
            .Should().Be(463);

        operation.PriceRatio.Denominator
            .Should().Be(10000);

        Assert.AreEqual(operation.BuyingAsset,
            Asset.CreateNonNativeAsset("RMT", "GDEGOXPCHXWFYY234D2YZSPEJ24BX42ESJNVHY5H7TWWQSYRN5ZKZE3N"));
        Assert.AreEqual(operation.SellingAsset, new AssetTypeNative());
    }
}