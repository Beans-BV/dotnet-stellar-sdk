using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class AssetDeserializerTest
{
    [TestMethod]
    public void TestDeserializeNative()
    {
        var json = File.ReadAllText(Path.Combine("testdata", "assetAssetTypeNative.json"));
        var asset = JsonSingleton.GetInstance<Asset>(json);

        Assert.AreEqual(asset.Type, "native");
    }

    [TestMethod]
    public void TestDeserializeCredit()
    {
        var json = File.ReadAllText(Path.Combine("testdata", "assetAssetTypeCredit.json"));
        var asset = JsonSingleton.GetInstance<Asset>(json);
        Assert.AreEqual(asset.Type, "credit_alphanum4");
        var creditAsset = (AssetTypeCreditAlphaNum)asset;
        Assert.AreEqual(creditAsset.Code, "CNY");
        Assert.AreEqual(creditAsset.Issuer, "GAREELUB43IRHWEASCFBLKHURCGMHE5IF6XSE7EXDLACYHGRHM43RFOX");
    }
}