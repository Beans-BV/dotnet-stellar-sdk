using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class AllowTrustOperationResponseTest
{
    //Allow Trust
    [TestMethod]
    public void TestDeserializeAllowTrustOperation()
    {
        var jsonPath = Utils.GetTestDataPath("allowTrust.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertAllowTrustOperationData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAllowTrustOperation()
    {
        var jsonPath = Utils.GetTestDataPath("allowTrust.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertAllowTrustOperationData(back);
    }

    private static void AssertAllowTrustOperationData(OperationResponse instance)
    {
        Assert.IsTrue(instance is AllowTrustOperationResponse);
        var operation = (AllowTrustOperationResponse)instance;

        Assert.AreEqual(operation.Trustor, "GDZ55LVXECRTW4G36EZPTHI4XIYS5JUC33TUS22UOETVFVOQ77JXWY4F");
        Assert.AreEqual(operation.Trustee, "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2");
        Assert.IsNull(operation.TrusteeMuxed);
        Assert.IsNull(operation.TrusteeMuxedID);
        Assert.AreEqual(operation.Authorize, true);
        Assert.AreEqual(operation.Asset,
            Asset.CreateNonNativeAsset("EUR", "GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM"));
    }


    //Allow Trust (Muxed)
    [TestMethod]
    public void TestDeserializeAllowTrustOperationMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("allowTrustMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertAllowTrustOperationMuxed(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAllowTrustOperationMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("allowTrustMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertAllowTrustOperationMuxed(back);
    }

    private static void AssertAllowTrustOperationMuxed(OperationResponse instance)
    {
        Assert.IsTrue(instance is AllowTrustOperationResponse);
        var operation = (AllowTrustOperationResponse)instance;

        Assert.AreEqual(operation.Trustor, "GDZ55LVXECRTW4G36EZPTHI4XIYS5JUC33TUS22UOETVFVOQ77JXWY4F");
        Assert.AreEqual(operation.Trustee, "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2");
        Assert.AreEqual(operation.TrusteeMuxed,
            "MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24");
        Assert.AreEqual(operation.TrusteeMuxedID, 5123456789UL);
        Assert.AreEqual(operation.Authorize, true);
        Assert.AreEqual(operation.Asset,
            Asset.CreateNonNativeAsset("EUR", "GDIROJW2YHMSFZJJ4R5XWWNUVND5I45YEWS5DSFKXCHMADZ5V374U2LM"));
    }
}