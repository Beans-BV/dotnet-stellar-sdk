using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;
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
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        Assert.IsNotNull(instance);
        AssertManageBuyOfferData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeManageBuyOfferOperation()
    {
        var jsonPath = Utils.GetTestDataPath("manageBuyOffer.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertManageBuyOfferData(back);
    }

    //Manage Buy Offer (Before Horizon 1.0.0)
    [TestMethod]
    public void TestDeserializeManageBuyOfferOperationPre100()
    {
        var jsonPath = Utils.GetTestDataPath("manageBuyOfferPre100.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        Assert.IsNotNull(instance);
        AssertManageBuyOfferData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeManageBuyOfferOperationPre100()
    {
        var jsonPath = Utils.GetTestDataPath("manageBuyOfferPre100.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
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