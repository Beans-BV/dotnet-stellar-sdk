using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class AccountMergeOperationResponseTest
{
    [TestMethod]
    public void TestDeserializeAccountMergeOperation()
    {
        var jsonPath = Utils.GetTestDataPath("accountMerge.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        Assert.IsNotNull(instance);
        AssertAccountMergeData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountMergeOperation()
    {
        var jsonPath = Utils.GetTestDataPath("accountMerge.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertAccountMergeData(back);
    }

    private static void AssertAccountMergeData(OperationResponse instance)
    {
        Assert.IsTrue(instance is AccountMergeOperationResponse);
        var operation = (AccountMergeOperationResponse)instance;

        Assert.AreEqual(operation.Account, "GD6GKRABNDVYDETEZJQEPS7IBQMERCN44R5RCI4LJNX6BMYQM2KPGGZ2");
        Assert.AreEqual(operation.Into, "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2");
        Assert.IsNull(operation.IntoMuxed);
        Assert.IsNull(operation.IntoMuxedID);
    }

    //Account Merge (Muxed)
    [TestMethod]
    public void TestDeserializeAccountMergeOperationMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("accountMergeMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        Assert.IsNotNull(instance);
        AssertAccountMergeDataMuxed(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountMergeOperationMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("accountMergeMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSingleton.GetInstance<OperationResponse>(json);
        var serialized = JsonConvert.SerializeObject(instance);
        var back = JsonConvert.DeserializeObject<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertAccountMergeDataMuxed(back);
    }

    private static void AssertAccountMergeDataMuxed(OperationResponse instance)
    {
        Assert.IsTrue(instance is AccountMergeOperationResponse);
        var operation = (AccountMergeOperationResponse)instance;

        Assert.AreEqual(operation.Account, "GD6GKRABNDVYDETEZJQEPS7IBQMERCN44R5RCI4LJNX6BMYQM2KPGGZ2");
        Assert.AreEqual(operation.Into, "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2");
        Assert.AreEqual(operation.IntoMuxed, "MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24");
        Assert.AreEqual(operation.IntoMuxedID, 5123456789UL);
    }
}