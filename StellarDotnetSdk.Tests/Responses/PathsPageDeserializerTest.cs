using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

/// <summary>
///     Unit tests for deserializing paths page responses from JSON.
/// </summary>
[TestClass]
public class PathsPageDeserializerTest
{
    /// <summary>
    ///     Verifies that Page&lt;PathResponse&gt; can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPathsPageJson_ReturnsDeserializedPathsPage()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("pathPage.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var pathsPage = JsonSerializer.Deserialize<Page<PathResponse>>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(pathsPage);
        AssertTestData(pathsPage);
    }

    /// <summary>
    ///     Verifies that Page&lt;PathResponse&gt; can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithPathsPage_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("pathPage.json");
        var json = File.ReadAllText(jsonPath);
        var pathsPage = JsonSerializer.Deserialize<Page<PathResponse>>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(pathsPage);
        var back = JsonSerializer.Deserialize<Page<PathResponse>>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertTestData(back);
    }

    public static void AssertTestData(Page<PathResponse> pathsPage)
    {
        Assert.IsNotNull(pathsPage);
        Assert.IsNull(pathsPage.Links);

        var record1 = pathsPage.Records[0];
        Assert.AreEqual("20.0000000", record1.DestinationAmount);
        Assert.AreEqual(Asset.CreateNonNativeAsset("EUR", "GDSBCQO34HWPGUGQSP3QBFEXVTSR2PW46UIGTHVWGWJGQKH3AFNHXHXN"),
            record1.DestinationAsset);
        Assert.AreEqual(0, record1.Path.Count);
        Assert.AreEqual("30.0000000", record1.SourceAmount);
        Assert.AreEqual(Asset.CreateNonNativeAsset("USD", "GDSBCQO34HWPGUGQSP3QBFEXVTSR2PW46UIGTHVWGWJGQKH3AFNHXHXN"),
            record1.SourceAsset);

        var record2 = pathsPage.Records[1];
        Assert.AreEqual("50.0000000", record2.DestinationAmount);
        Assert.AreEqual(Asset.CreateNonNativeAsset("EUR", "GBFMFKDUFYYITWRQXL4775CVUV3A3WGGXNJUAP4KTXNEQ2HG7JRBITGH"),
            record2.DestinationAsset);
        Assert.AreEqual(1, record2.Path.Count);
        Assert.AreEqual(Asset.CreateNonNativeAsset("GBP", "GDSBCQO34HWPGUGQSP3QBFEXVTSR2PW46UIGTHVWGWJGQKH3AFNHXHXN"),
            record2.Path[0]);
        Assert.AreEqual("60.0000000", record2.SourceAmount);
        Assert.AreEqual(Asset.CreateNonNativeAsset("USD", "GBRAOXQDNQZRDIOK64HZI4YRDTBFWNUYH3OIHQLY4VEK5AIGMQHCLGXI"),
            record2.SourceAsset);

        var record3 = pathsPage.Records[2];
        Assert.AreEqual("200.0000000", record3.DestinationAmount);
        Assert.AreEqual(Asset.CreateNonNativeAsset("EUR", "GBRCOBK7C7UE72PB5JCPQU3ZI45ZCEM7HKQ3KYV3YD3XB7EBOPBEDN2G"),
            record3.DestinationAsset);
        Assert.AreEqual(3, record3.Path.Count);
        Assert.AreEqual(Asset.CreateNonNativeAsset("GBP", "GAX7B3ZT3EOZW5POAMV4NGPPKCYUOYW2QQDIAF23JAXF72NMGRYPYOPM"),
            record3.Path[0]);
        Assert.AreEqual(Asset.CreateNonNativeAsset("PLN", "GACWIA2XGDFWWN3WKPX63JTK4S2J5NDPNOIVYMZY6RVTS7LWF2VHZLV3"),
            record3.Path[1]);
        Assert.AreEqual(new AssetTypeNative(), record3.Path[2]);
        Assert.AreEqual("300.0000000", record3.SourceAmount);
        Assert.AreEqual(Asset.CreateNonNativeAsset("USD", "GC7J5IHS3GABSX7AZLRINXWLHFTL3WWXLU4QX2UGSDEAIAQW2Q72U3KH"),
            record3.SourceAsset);
    }
}