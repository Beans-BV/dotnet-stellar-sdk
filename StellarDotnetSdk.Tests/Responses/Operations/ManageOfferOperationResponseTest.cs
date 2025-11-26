using System.IO;
using System.Text.Json;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class ManageOfferOperationResponseTest
{
    [TestMethod]
    public void TestDeserializeManageOfferOperation()
    {
        var jsonPath = Utils.GetTestDataPath("manageOffer.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertManageOfferData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeManageOfferOperation()
    {
        var jsonPath = Utils.GetTestDataPath("manageOffer.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertManageOfferData(back);
    }

    private static void AssertManageOfferData(OperationResponse instance)
    {
        Assert.IsTrue(instance is ManageSellOfferOperationResponse);
        var operation = (ManageSellOfferOperationResponse)instance;

        Assert.AreEqual(operation.OfferId, "96052902");
        Assert.AreEqual(operation.Amount, "243.7500000");

        operation.Price
            .Should().Be("8.0850240");

        operation.PriceRatio.Numerator
            .Should().Be(5054660);

        operation.PriceRatio.Denominator
            .Should().Be(625188);

        Assert.AreEqual(operation.SellingAsset,
            Asset.CreateNonNativeAsset("USD", "GDSRCV5VTM3U7Y3L6DFRP3PEGBNQMGOWSRTGSBWX6Z3H6C7JHRI4XFJP"));
        Assert.AreEqual(operation.BuyingAsset, new AssetTypeNative());
    }
}