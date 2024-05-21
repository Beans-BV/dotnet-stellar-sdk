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
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertCreateAccountOperationData(back);
    }

    public static void AssertCreateAccountOperationData(OperationResponse instance)
    {
        Assert.IsTrue(instance is CreateAccountOperationResponse);
        var operation = (CreateAccountOperationResponse)instance;

        Assert.AreEqual(operation.SourceAccount, "GD6WU64OEP5C4LRBH6NK3MHYIA2ADN6K6II6EXPNVUR3ERBXT4AN4ACD");
        Assert.AreEqual(operation.PagingToken, "3936840037961729");
        Assert.AreEqual(operation.Id, 3936840037961729L);

        Assert.AreEqual(operation.Account, "GAR4DDXYNSN2CORG3XQFLAPWYKTUMLZYHYWV4Y2YJJ4JO6ZJFXMJD7PT");
        Assert.AreEqual(operation.StartingBalance, "299454.904954");
        Assert.AreEqual(operation.Funder, "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2");
        Assert.IsNull(operation.FunderMuxed);
        Assert.IsNull(operation.FunderMuxedId);

        Assert.IsTrue(operation.TransactionSuccessful);
        Assert.AreEqual(operation.Links.Effects.Href, "/operations/3936840037961729/effects{?cursor,limit,order}");
        Assert.AreEqual(operation.Links.Precedes.Href, "/operations?cursor=3936840037961729&order=asc");
        Assert.AreEqual(operation.Links.Self.Href, "/operations/3936840037961729");
        Assert.AreEqual(operation.Links.Succeeds.Href, "/operations?cursor=3936840037961729&order=desc");
        Assert.AreEqual(operation.Links.Transaction.Href,
            "/transactions/75608563ae63757ffc0650d84d1d13c0f3cd4970a294a2a6b43e3f454e0f9e6d");
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
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized);
        Assert.IsNotNull(back);
        AssertCreateAccountOperationDataMuxed(back);
    }

    private static void AssertCreateAccountOperationDataMuxed(OperationResponse instance)
    {
        Assert.IsTrue(instance is CreateAccountOperationResponse);
        var operation = (CreateAccountOperationResponse)instance;

        Assert.AreEqual(operation.SourceAccount, "GD6WU64OEP5C4LRBH6NK3MHYIA2ADN6K6II6EXPNVUR3ERBXT4AN4ACD");
        Assert.AreEqual(operation.PagingToken, "3936840037961729");
        Assert.AreEqual(operation.Id, 3936840037961729L);

        Assert.AreEqual(operation.Account, "GAR4DDXYNSN2CORG3XQFLAPWYKTUMLZYHYWV4Y2YJJ4JO6ZJFXMJD7PT");
        Assert.AreEqual(operation.StartingBalance, "299454.904954");
        Assert.AreEqual(operation.Funder, "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2");
        Assert.AreEqual(operation.FunderMuxed, "MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24");
        Assert.AreEqual(operation.FunderMuxedId, 5123456789UL);

        Assert.IsTrue(operation.TransactionSuccessful);
        Assert.AreEqual(operation.Links.Effects.Href, "/operations/3936840037961729/effects{?cursor,limit,order}");
        Assert.AreEqual(operation.Links.Precedes.Href, "/operations?cursor=3936840037961729&order=asc");
        Assert.AreEqual(operation.Links.Self.Href, "/operations/3936840037961729");
        Assert.AreEqual(operation.Links.Succeeds.Href, "/operations?cursor=3936840037961729&order=desc");
        Assert.AreEqual(operation.Links.Transaction.Href,
            "/transactions/75608563ae63757ffc0650d84d1d13c0f3cd4970a294a2a6b43e3f454e0f9e6d");
    }
}