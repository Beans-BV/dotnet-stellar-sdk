using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class ManageSellOfferOperationResponseTest
{
    [TestMethod]
    public void TestDeserializeManageSellOfferOperation()
    {
        var jsonPath = Utils.GetTestDataPath("manageSellOffer.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertManageSellOfferData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeManageSellOfferOperation()
    {
        var jsonPath = Utils.GetTestDataPath("manageSellOffer.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertManageSellOfferData(back);
    }

    private static void AssertManageSellOfferData(OperationResponse instance)
    {
        Assert.IsTrue(instance is ManageSellOfferOperationResponse);
        var operation = (ManageSellOfferOperationResponse)instance;

        Assert.AreEqual("MB7BTYMGED4DATO5U2BMPWKYABQQ3QBOQZK5T46N5CSCVPI2G3PVUAAAAAAAAABRN3NWG",
            operation.SourceAccountMuxed);
        Assert.AreEqual(12654UL, operation.SourceAccountMuxedId);
        Assert.AreEqual("0", operation.OfferId);
        Assert.AreEqual("12.1621000", operation.Amount);
        Assert.AreEqual("0.1400000", operation.Price);
        Assert.AreEqual(7L, operation.PriceRatio.Numerator);
        Assert.AreEqual(50L, operation.PriceRatio.Denominator);
        Assert.AreEqual(Asset.CreateNonNativeAsset("TEST", "GB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST"),
            operation.BuyingAsset);
        Assert.AreEqual("credit_alphanum4", operation.BuyingAssetType);
        Assert.AreEqual("TEST", operation.BuyingAssetCode);
        Assert.AreEqual("GB7DCP4SQBU3XZIJTJ55WEEVRBLSGT3ILJD2VUDMCTSZ4JVS2AUHTEST", operation.BuyingAssetIssuer);
        Assert.AreEqual(new AssetTypeNative(), operation.SellingAsset);
        Assert.AreEqual("native", operation.SellingAssetType);
        Assert.IsNull(operation.SellingAssetCode);
        Assert.IsNull(operation.SellingAssetIssuer);
    }
}