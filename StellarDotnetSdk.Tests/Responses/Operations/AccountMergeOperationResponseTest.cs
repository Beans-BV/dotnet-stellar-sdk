using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
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
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertAccountMergeData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountMergeOperation()
    {
        var jsonPath = Utils.GetTestDataPath("accountMerge.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertAccountMergeData(back);
    }

    private static void AssertAccountMergeData(OperationResponse instance)
    {
        Assert.IsTrue(instance is AccountMergeOperationResponse);
        var operation = (AccountMergeOperationResponse)instance;

        Assert.AreEqual("GD6GKRABNDVYDETEZJQEPS7IBQMERCN44R5RCI4LJNX6BMYQM2KPGGZ2", operation.Account);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.Into);
        Assert.IsNull(operation.AccountMuxed);
        Assert.IsNull(operation.AccountMuxedId);
        Assert.IsNull(operation.IntoMuxed);
        Assert.IsNull(operation.IntoMuxedID);
    }

    [TestMethod]
    public void TestDeserializeAccountMergeOperationMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("accountMergeMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertAccountMergeDataMuxed(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeAccountMergeOperationMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("accountMergeMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertAccountMergeDataMuxed(back);
    }

    private static void AssertAccountMergeDataMuxed(OperationResponse instance)
    {
        Assert.IsTrue(instance is AccountMergeOperationResponse);
        var operation = (AccountMergeOperationResponse)instance;

        Assert.AreEqual("GDI53A4VSMMYMVVTLO3X4SMZXWIPWN3ETEKTFVPOYO67A5FPLLK4T3YR", operation.Account);
        Assert.AreEqual("MDI53A4VSMMYMVVTLO3X4SMZXWIPWN3ETEKTFVPOYO67A5FPLLK4SAAAAAAAAAJKERIZA",
            operation.AccountMuxed);
        Assert.AreEqual(76324UL, operation.AccountMuxedId);
        Assert.AreEqual("GBZG3SMBL6FPLYYNQP6DMVZHDHCDIR4J4GYRGKE5BYRVLBYN364RBL5S", operation.Into);
        Assert.AreEqual("MBZG3SMBL6FPLYYNQP6DMVZHDHCDIR4J4GYRGKE5BYRVLBYN364RAAAAAAAAAAICXIM64", operation.IntoMuxed);
        Assert.AreEqual(66234UL, operation.IntoMuxedID);
    }
}