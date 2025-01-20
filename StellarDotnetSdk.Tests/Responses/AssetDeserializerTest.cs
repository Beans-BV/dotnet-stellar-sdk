﻿using System.IO;
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
        var jsonPath = Utils.GetTestDataPath("assetAssetTypeNative.json");
        var json = File.ReadAllText(jsonPath);
        var asset = JsonSingleton2.GetInstance<Asset>(json);
        Assert.IsNotNull(asset);
        Assert.AreEqual(asset.Type, "native");
    }

    [TestMethod]
    public void TestDeserializeCredit()
    {
        var jsonPath = Utils.GetTestDataPath("assetAssetTypeCredit.json");
        var json = File.ReadAllText(jsonPath);
        var asset = JsonSingleton2.GetInstance<Asset>(json);
        Assert.IsNotNull(asset);
        Assert.AreEqual(asset.Type, "credit_alphanum4");
        var creditAsset = (AssetTypeCreditAlphaNum)asset;
        Assert.AreEqual(creditAsset.Code, "CNY");
        Assert.AreEqual(creditAsset.Issuer, "GAREELUB43IRHWEASCFBLKHURCGMHE5IF6XSE7EXDLACYHGRHM43RFOX");
    }
}