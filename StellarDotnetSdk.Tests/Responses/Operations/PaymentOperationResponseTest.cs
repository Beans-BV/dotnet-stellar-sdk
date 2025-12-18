using System;
using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Responses.Operations;

namespace StellarDotnetSdk.Tests.Responses.Operations;

/// <summary>
/// Unit tests for <see cref="PaymentOperationResponse"/> class.
/// </summary>
[TestClass]
public class PaymentOperationResponseTest
{
    /// <summary>
    /// Verifies that PaymentOperationResponse can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPaymentOperationJson_ReturnsDeserializedOperation()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("payment.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<PaymentOperationResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertPaymentOperationTestData(instance);
    }

    /// <summary>
    /// Verifies that PaymentOperationResponse can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithPaymentOperation_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("payment.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<PaymentOperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<PaymentOperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertPaymentOperationTestData(back);
    }

    public static void AssertPaymentOperationTestData(PaymentOperationResponse operation)
    {
        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.SourceAccount);
        Assert.IsNull(operation.SourceAccountMuxed);
        Assert.IsNull(operation.SourceAccountMuxedId);
        Assert.AreEqual(DateTime.Parse("2024-10-23T02:12:22Z").ToUniversalTime(), operation.CreatedAt);

        Assert.AreEqual(3940808587743233L, operation.Id);

        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.From);
        Assert.IsNull(operation.FromMuxed);
        Assert.IsNull(operation.FromMuxedId);
        Assert.AreEqual("GDWNY2POLGK65VVKIH5KQSH7VWLKRTQ5M6ADLJAYC2UEHEBEARCZJWWI", operation.To);
        Assert.IsNull(operation.ToMuxed);
        Assert.IsNull(operation.ToMuxedId);
        Assert.AreEqual("100.0", operation.Amount);
        Assert.AreEqual(new AssetTypeNative(), operation.Asset);
        Assert.AreEqual("native", operation.AssetType);
        Assert.IsNull(operation.AssetCode);
        Assert.IsNull(operation.AssetIssuer);

        Assert.AreEqual("5c2e4dad596941ef944d72741c8f8f1a4282f8f2f141e81d827f44bf365d626b", operation.TransactionHash);

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

    /// <summary>
    /// Verifies that PaymentOperationResponse with non-native asset can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPaymentOperationNonNativeJson_ReturnsDeserializedOperation()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("paymentNonNative.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertNonNativePaymentData(instance);
    }

    /// <summary>
    /// Verifies that PaymentOperationResponse with non-native asset can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithPaymentOperationNonNative_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("paymentNonNative.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertNonNativePaymentData(back);
    }

    private static void AssertNonNativePaymentData(OperationResponse instance)
    {
        Assert.IsTrue(instance is PaymentOperationResponse);
        var operation = (PaymentOperationResponse)instance;

        Assert.AreEqual("GAZN3PPIDQCSP5JD4ETQQQ2IU2RMFYQTAL4NNQZUGLLO2XJJJ3RDSDGA", operation.From);
        Assert.AreEqual("GBHUSIZZ7FS2OMLZVZ4HLWJMXQ336NFSXHYERD7GG54NRITDTEWWBBI6", operation.To);
        Assert.AreEqual("1000000000.0", operation.Amount);
        Assert.AreEqual(Asset.CreateNonNativeAsset("EUR", "GAZN3PPIDQCSP5JD4ETQQQ2IU2RMFYQTAL4NNQZUGLLO2XJJJ3RDSDGA"),
            operation.Asset);
    }

    /// <summary>
    /// Verifies that PaymentOperationResponse with muxed account can be deserialized from JSON correctly.
    /// </summary>
    [TestMethod]
    public void Deserialize_WithPaymentOperationMuxedJson_ReturnsDeserializedOperation()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("paymentMuxed.json");
        var json = File.ReadAllText(jsonPath);

        // Act
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(instance);
        AssertPaymentOperationTestDataMuxed(instance);
    }

    /// <summary>
    /// Verifies that PaymentOperationResponse with muxed account can be serialized and deserialized correctly (round-trip).
    /// </summary>
    [TestMethod]
    public void SerializeDeserialize_WithPaymentOperationMuxed_RoundTripsCorrectly()
    {
        // Arrange
        var jsonPath = Utils.GetTestDataPath("paymentMuxed.json");
        var json = File.ReadAllText(jsonPath);
        var instance = JsonSerializer.Deserialize<OperationResponse>(json, JsonOptions.DefaultOptions);

        // Act
        var serialized = JsonSerializer.Serialize(instance);
        var back = JsonSerializer.Deserialize<OperationResponse>(serialized, JsonOptions.DefaultOptions);

        // Assert
        Assert.IsNotNull(back);
        AssertPaymentOperationTestDataMuxed(back);
    }

    public static void AssertPaymentOperationTestDataMuxed(OperationResponse instance)
    {
        Assert.IsTrue(instance is PaymentOperationResponse);
        var operation = (PaymentOperationResponse)instance;

        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.SourceAccount);
        Assert.AreEqual("MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24",
            operation.SourceAccountMuxed);
        Assert.AreEqual(5123456789UL, operation.SourceAccountMuxedId);

        Assert.AreEqual(3940808587743233L, operation.Id);

        Assert.AreEqual("GCKICEQ2SA3KWH3UMQFJE4BFXCBFHW46BCVJBRCLK76ZY5RO6TY5D7Q2", operation.From);
        Assert.AreEqual("MAAAAAABGFQ36FMUQEJBVEBWVMPXIZAKSJYCLOECKPNZ4CFKSDCEWV75TR3C55HR2FJ24", operation.FromMuxed);
        Assert.AreEqual(5123456789UL, operation.FromMuxedId);
        Assert.AreEqual("GDWNY2POLGK65VVKIH5KQSH7VWLKRTQ5M6ADLJAYC2UEHEBEARCZJWWI", operation.To);
        Assert.AreEqual("100.0", operation.Amount);
        Assert.AreEqual(new AssetTypeNative(), operation.Asset);

        Assert.IsFalse(operation.TransactionSuccessful);
    }
}