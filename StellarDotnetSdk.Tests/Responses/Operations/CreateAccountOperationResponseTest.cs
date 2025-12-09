using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class CreateAccountOperationResponseTest
{
    [TestMethod]
    public void TestDeserializeCreateAccountOperation()
    {
        var jsonPath = Utils.GetTestDataPath("createAccount.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertCreateAccountOperationData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeCreateAccountOperation()
    {
        var jsonPath = Utils.GetTestDataPath("createAccount.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertCreateAccountOperationData(back);
    }

    public static void AssertCreateAccountOperationData(OperationResponse instance)
    {
        Assert.IsTrue(instance is CreateAccountOperationResponse);
        var operation = (CreateAccountOperationResponse)instance;

        Assert.AreEqual("GD6WU64OEP5C4LRBH6NK3MHYIA2ADN6K6II6EXPNVUR3ERBXT4AN4ACD", operation.SourceAccount);
        Assert.AreEqual("3936840037961729", operation.PagingToken);
        Assert.AreEqual(3936840037961729L, operation.Id);

        Assert.AreEqual("GAR4DDXYNSN2CORG3XQFLAPWYKTUMLZYHYWV4Y2YJJ4JO6ZJFXMJD7PT", operation.Account);
        Assert.AreEqual("299454.904954", operation.StartingBalance);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.Funder);
        Assert.IsNull(operation.FunderMuxed);
        Assert.IsNull(operation.FunderMuxedId);

        Assert.IsTrue(operation.TransactionSuccessful);
        Assert.AreEqual("/operations/3936840037961729/effects{?cursor,limit,order}", operation.Links.Effects.Href);
        Assert.AreEqual("/operations?cursor=3936840037961729&order=asc", operation.Links.Precedes.Href);
        Assert.AreEqual("/operations/3936840037961729", operation.Links.Self.Href);
        Assert.AreEqual("/operations?cursor=3936840037961729&order=desc", operation.Links.Succeeds.Href);
        Assert.AreEqual("/transactions/75608563ae63757ffc0650d84d1d13c0f3cd4970a294a2a6b43e3f454e0f9e6d",
            operation.Links.Transaction.Href);
    }

    [TestMethod]
    public void TestDeserializeCreateAccountOperationMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("createAccountMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertCreateAccountOperationDataMuxed(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializeCreateAccountOperationMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("createAccountMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertCreateAccountOperationDataMuxed(back);
    }

    private static void AssertCreateAccountOperationDataMuxed(OperationResponse instance)
    {
        Assert.IsTrue(instance is CreateAccountOperationResponse);
        var operation = (CreateAccountOperationResponse)instance;

        Assert.AreEqual("GD6WU64OEP5C4LRBH6NK3MHYIA2ADN6K6II6EXPNVUR3ERBXT4AN4ACD", operation.SourceAccount);
        Assert.AreEqual("3936840037961729", operation.PagingToken);
        Assert.AreEqual(3936840037961729L, operation.Id);

        Assert.AreEqual("GAR4DDXYNSN2CORG3XQFLAPWYKTUMLZYHYWV4Y2YJJ4JO6ZJFXMJD7PT", operation.Account);
        Assert.AreEqual("299454.904954", operation.StartingBalance);
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.Funder);
        Assert.AreEqual("MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24", operation.FunderMuxed);
        Assert.AreEqual(5123456789UL, operation.FunderMuxedId);

        Assert.IsTrue(operation.TransactionSuccessful);
        Assert.AreEqual("/operations/3936840037961729/effects{?cursor,limit,order}", operation.Links.Effects.Href);
        Assert.AreEqual("/operations?cursor=3936840037961729&order=asc", operation.Links.Precedes.Href);
        Assert.AreEqual("/operations/3936840037961729", operation.Links.Self.Href);
        Assert.AreEqual("/operations?cursor=3936840037961729&order=desc", operation.Links.Succeeds.Href);
        Assert.AreEqual("/transactions/75608563ae63757ffc0650d84d1d13c0f3cd4970a294a2a6b43e3f454e0f9e6d",
            operation.Links.Transaction.Href);
    }
}