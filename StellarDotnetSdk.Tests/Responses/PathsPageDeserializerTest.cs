﻿using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class PathsPageDeserializerTest
{
    [TestMethod]
    public void TestSerializeDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("pathPage.json");
        var json = File.ReadAllText(jsonPath);
        var pathsPage = JsonSingleton.GetInstance<Page<PathResponse>>(json);
        Assert.IsNotNull(pathsPage);
        AssertTestData(pathsPage);
        var serialized = JsonConvert.SerializeObject(pathsPage);
        var back = JsonConvert.DeserializeObject<Page<PathResponse>>(serialized);
        Assert.IsNotNull(back);
        AssertTestData(back);
    }

    public static void AssertTestData(Page<PathResponse> pathsPage)
    {
        Assert.IsNull(pathsPage.NextPage());
        Assert.IsNull(pathsPage.PreviousPage());

        Assert.AreEqual(pathsPage.Records[0].DestinationAmount, "20.0000000");
        Assert.AreEqual(pathsPage.Records[0].DestinationAsset,
            Asset.CreateNonNativeAsset("EUR", "GDSBCQO34HWPGUGQSP3QBFEXVTSR2PW46UIGTHVWGWJGQKH3AFNHXHXN"));
        Assert.AreEqual(pathsPage.Records[0].Path.Count, 0);
        Assert.AreEqual(pathsPage.Records[0].SourceAmount, "30.0000000");
        Assert.AreEqual(pathsPage.Records[0].SourceAsset,
            Asset.CreateNonNativeAsset("USD", "GDSBCQO34HWPGUGQSP3QBFEXVTSR2PW46UIGTHVWGWJGQKH3AFNHXHXN"));

        Assert.AreEqual(pathsPage.Records[1].DestinationAmount, "50.0000000");
        Assert.AreEqual(pathsPage.Records[1].DestinationAsset,
            Asset.CreateNonNativeAsset("EUR", "GBFMFKDUFYYITWRQXL4775CVUV3A3WGGXNJUAP4KTXNEQ2HG7JRBITGH"));
        Assert.AreEqual(pathsPage.Records[1].Path.Count, 1);
        Assert.AreEqual(pathsPage.Records[1].Path[0],
            Asset.CreateNonNativeAsset("GBP", "GDSBCQO34HWPGUGQSP3QBFEXVTSR2PW46UIGTHVWGWJGQKH3AFNHXHXN"));
        Assert.AreEqual(pathsPage.Records[1].SourceAmount, "60.0000000");
        Assert.AreEqual(pathsPage.Records[1].SourceAsset,
            Asset.CreateNonNativeAsset("USD", "GBRAOXQDNQZRDIOK64HZI4YRDTBFWNUYH3OIHQLY4VEK5AIGMQHCLGXI"));

        Assert.AreEqual(pathsPage.Records[2].DestinationAmount, "200.0000000");
        Assert.AreEqual(pathsPage.Records[2].DestinationAsset,
            Asset.CreateNonNativeAsset("EUR", "GBRCOBK7C7UE72PB5JCPQU3ZI45ZCEM7HKQ3KYV3YD3XB7EBOPBEDN2G"));
        Assert.AreEqual(pathsPage.Records[2].Path.Count, 3);
        Assert.AreEqual(pathsPage.Records[2].Path[0],
            Asset.CreateNonNativeAsset("GBP", "GAX7B3ZT3EOZW5POAMV4NGPPKCYUOYW2QQDIAF23JAXF72NMGRYPYOPM"));
        Assert.AreEqual(pathsPage.Records[2].Path[1],
            Asset.CreateNonNativeAsset("PLN", "GACWIA2XGDFWWN3WKPX63JTK4S2J5NDPNOIVYMZY6RVTS7LWF2VHZLV3"));
        Assert.AreEqual(pathsPage.Records[2].Path[2], new AssetTypeNative());
        Assert.AreEqual(pathsPage.Records[2].SourceAmount, "300.0000000");
        Assert.AreEqual(pathsPage.Records[2].SourceAsset,
            Asset.CreateNonNativeAsset("USD", "GC7J5IHS3GABSX7AZLRINXWLHFTL3WWXLU4QX2UGSDEAIAQW2Q72U3KH"));
    }
}