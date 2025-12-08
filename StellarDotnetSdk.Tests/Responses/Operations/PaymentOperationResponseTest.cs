using System;
using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

[TestClass]
public class PaymentOperationResponseTest
{
//Payment
    [TestMethod]
    public void TestDeserializePaymentOperation()
    {
        var jsonPath = Utils.GetTestDataPath("payment.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<PaymentOperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertPaymentOperationTestData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializePaymentOperation()
    {
        var jsonPath = Utils.GetTestDataPath("payment.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<PaymentOperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<PaymentOperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertPaymentOperationTestData(back);
    }

    public static void AssertPaymentOperationTestData(PaymentOperationResponse operation)
    {
        Assert.AreEqual(operation.SourceAccount, "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2");
        Assert.IsNull(operation.SourceAccountMuxed);
        Assert.IsNull(operation.SourceAccountMuxedId);
        Assert.AreEqual(DateTime.Parse("2024-10-23T02:12:22Z").ToUniversalTime(),
            operation.CreatedAt.ToUniversalTime());

        Assert.AreEqual(operation.Id, 3940808587743233L);

        Assert.AreEqual(operation.From, "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2");
        Assert.IsNull(operation.FromMuxed);
        Assert.IsNull(operation.FromMuxedId);
        Assert.AreEqual(operation.To, "GDWNY2POLGK65VVKIH5KQSH7VWLKRTQ5M6ADLJAYC2UEHEBEARCZJWWI");
        Assert.AreEqual(operation.Amount, "100.0");
        Assert.AreEqual(operation.Asset, new AssetTypeNative());

        Assert.AreEqual(operation.TransactionHash, "5c2e4dad596941ef944d72741c8f8f1a4282f8f2f141e81d827f44bf365d626b");

        var transaction = operation.Transaction;

        Assert.IsNotNull(transaction);
        Assert.AreEqual("5c2e4dad596941ef944d72741c8f8f1a4282f8f2f141e81d827f44bf365d626b", transaction.Hash);
        Assert.AreEqual(915744L, transaction.Ledger);
        Assert.AreEqual(new DateTimeOffset(2015, 11, 20, 17, 1, 28, TimeSpan.Zero), transaction.CreatedAt);
        Assert.AreEqual("3933090531512320", transaction.PagingToken);
        Assert.AreEqual("GCUB7JL4APK7LKJ6MZF7Q2JTLHAGNBIUA7XIXD5SQTG52GQ2DAT6XZMK", transaction.SourceAccount);
        Assert.AreEqual(2373051035426646L, transaction.SourceAccountSequence);
        Assert.AreEqual(1, transaction.OperationCount);
        Assert.AreEqual(
            "AAAAAKgfpXwD1fWpPmZL+GkzWcBmhRQH7ouPsoTN3RoaGCfrAAAAZAAIbkcAAB9WAAAAAAAAAANRBBZE6D1qyGjISUGLY5Ldvp31PwAAAAAAAAAAAAAAAAAAAAEAAAABAAAAAP1qe44j+i4uIT+arbD4QDQBt8ryEeJd7a0jskQ3nwDeAAAAAAAAAADA7RnarSzCwj3OT+M2btCMFpVBdqxJS+Sr00qBjtFv7gAAAABLCs/QAAAAAAAAAAEaGCfrAAAAQG/56Cj2J8W/KCZr+oC4sWND1CTGWfaccHNtuibQH8kZIb+qBSDY94g7hiaAXrlIeg9b7oz/XuP3x9MWYw2jtwM=",
            transaction.EnvelopeXdr);
        Assert.AreEqual("AAAAAAAAAGQAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAA=", transaction.ResultXdr);

        Assert.IsTrue(transaction.Memo is MemoHash);
        var memo = (MemoHash)transaction.Memo;
        Assert.AreEqual("51041644e83d6ac868c849418b6392ddbe9df53f000000000000000000000000", memo.GetHexValue());

        Assert.AreEqual("/accounts/GCUB7JL4APK7LKJ6MZF7Q2JTLHAGNBIUA7XIXD5SQTG52GQ2DAT6XZMK",
            transaction.Links.Account.Href);
        Assert.AreEqual(
            "/transactions/5c2e4dad596941ef944d72741c8f8f1a4282f8f2f141e81d827f44bf365d626b/effects{?cursor,limit,order}",
            transaction.Links.Effects.Href);
        Assert.AreEqual("/ledgers/915744", transaction.Links.Ledger.Href);
        Assert.AreEqual(
            "/transactions/5c2e4dad596941ef944d72741c8f8f1a4282f8f2f141e81d827f44bf365d626b/operations{?cursor,limit,order}",
            transaction.Links.Operations.Href);
        Assert.AreEqual("/transactions?cursor=3933090531512320&order=asc", transaction.Links.Precedes.Href);
        Assert.AreEqual("/transactions/5c2e4dad596941ef944d72741c8f8f1a4282f8f2f141e81d827f44bf365d626b",
            transaction.Links.Self.Href);
        Assert.AreEqual("/transactions?cursor=3933090531512320&order=desc", transaction.Links.Succeeds.Href);

        Assert.IsFalse(operation.TransactionSuccessful);
    }

    //Payment Non Native
    [TestMethod]
    public void TestDeserializePaymentOperationNonNative()
    {
        var jsonPath = Utils.GetTestDataPath("paymentNonNative.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertNonNativePaymentData(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializePaymentOperationNonNative()
    {
        var jsonPath = Utils.GetTestDataPath("paymentNonNative.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertNonNativePaymentData(back);
    }

    private static void AssertNonNativePaymentData(OperationResponse instance)
    {
        Assert.IsTrue(instance is PaymentOperationResponse);
        var operation = (PaymentOperationResponse)instance;

        Assert.AreEqual(operation.From, "GAZN3PPIDQCSP5JD4ETQQQ2IU2RMFYQTAL4NNQZUGLLO2XJJJ3RDSDGA");
        Assert.AreEqual(operation.To, "GBHUSIZZ7FS2OMLZVZ4HLWJMXQ336NFSXHYERD7GG54NRITDTEWWBBI6");
        Assert.AreEqual(operation.Amount, "1000000000.0");
        Assert.AreEqual(operation.Asset,
            Asset.CreateNonNativeAsset("EUR", "GAZN3PPIDQCSP5JD4ETQQQ2IU2RMFYQTAL4NNQZUGLLO2XJJJ3RDSDGA"));
    }

    //Payment (Muxed)
    [TestMethod]
    public void TestDeserializePaymentOperationMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("paymentMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        Assert.IsNotNull(instance);
        AssertPaymentOperationTestDataMuxed(instance);
    }

    [TestMethod]
    public void TestSerializeDeserializePaymentOperationMuxed()
    {
        var jsonPath = Utils.GetTestDataPath("paymentMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);
        Assert.IsNotNull(back);
        AssertPaymentOperationTestDataMuxed(back);
    }

    public static void AssertPaymentOperationTestDataMuxed(OperationResponse instance)
    {
        Assert.IsTrue(instance is PaymentOperationResponse);
        var operation = (PaymentOperationResponse)instance;

        Assert.AreEqual(operation.SourceAccount, "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2");
        Assert.AreEqual(operation.SourceAccountMuxed,
            "MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24");
        Assert.AreEqual(operation.SourceAccountMuxedId, 5123456789UL);

        Assert.AreEqual(operation.Id, 3940808587743233L);

        Assert.AreEqual(operation.From, "GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2");
        Assert.AreEqual(operation.FromMuxed, "MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24");
        Assert.AreEqual(operation.FromMuxedId, 5123456789UL);
        Assert.AreEqual(operation.To, "GDWNY2POLGK65VVKIH5KQSH7VWLKRTQ5M6ADLJAYC2UEHEBEARCZJWWI");
        Assert.AreEqual(operation.Amount, "100.0");
        Assert.AreEqual(operation.Asset, new AssetTypeNative());

        Assert.IsFalse(operation.TransactionSuccessful);
    }
}