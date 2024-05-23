using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses;

namespace StellarDotnetSdk.Tests.Responses;

[TestClass]
public class RootDeserializerTest
{
    [TestMethod]
    public void TestDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("root.json");
        var json = File.ReadAllText(jsonPath);
        var root = JsonSerializer.Deserialize<RootResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(root);
        AssertTestData(root);
    }

    [TestMethod]
    public void TestSerializeDeserialize()
    {
        var jsonPath = Utils.GetTestDataPath("root.json");
        var json = File.ReadAllText(jsonPath);
        var root = JsonSerializer.Deserialize<RootResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(root);
        var back = JsonSerializer.Deserialize<RootResponse>(serialized);

        Assert.IsNotNull(back);
        AssertTestData(back);
    }


    private static void AssertTestData(RootResponse root)
    {
        Assert.AreEqual(root.HorizonVersion, "snapshot-c5fe0ff");
        Assert.AreEqual(root.StellarCoreVersion, "stellar-core 9.2.0 (7561c1d53366ec79b908de533726269e08474f77)");
        Assert.AreEqual(root.HistoryLatestLedger, 18369116);
        Assert.AreEqual(root.HistoryElderLedger, 1);
        Assert.AreEqual(root.CoreLatestLedger, 18369117);
        Assert.AreEqual(root.NetworkPassphrase, "Public Global Stellar Network ; September 2015");
        Assert.AreEqual(root.CurrentProtocolVersion, 10);
        Assert.AreEqual(root.CoreSupportedProtocolVersion, 11);
    }
}