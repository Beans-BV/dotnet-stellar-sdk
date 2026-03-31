using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Xdr;
using FeeBumpTransaction = StellarDotnetSdk.Transactions.FeeBumpTransaction;

namespace StellarDotnetSdk.Tests.Operations;

/// <summary>
///     Tests for FeeBumpTransaction result parsing and fee validation.
/// </summary>
[TestClass]
public class FeeBumpOperationTest
{
    private static readonly KeyPair Source =
        KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");

    private static readonly KeyPair Sponsor =
        KeyPair.FromSecretSeed("SB7ZMPZB3YMMK5CUWENXVLZWBK4KYX4YU5JBXQNZSK2DP2Q7V3LVTO5V");

    private static FeeBumpTransaction CreateDummyFeeBumpTransaction(uint fee = 200)
    {
        var account = new Account(Source.AccountId, 0L);

        var tx = new TransactionBuilder(account)
            .AddOperation(new PaymentOperation(Sponsor, new AssetTypeNative(), "10"))
            .Build();
        tx.Sign(Source);

        var feeBumpTx = TransactionBuilder.BuildFeeBumpTransaction(Sponsor, tx, fee);
        feeBumpTx.Sign(Sponsor);
        return feeBumpTx;
    }

    /// <summary>
    ///     Verifies that fee bump transaction with successful inner transaction submits successfully.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithSuccessfulInnerTransaction_ReturnsSuccessResult()
    {
        // Arrange
        var resultXdr = Utils.CreateFeeBumpTransactionResultXdr(
            [Utils.CreatePaymentResult()]
        );
        using var server = Utils.CreateTestServerWithContent(
            Utils.BuildSubmitTransactionResponseJson(resultXdr)
        );

        // Act
        var response = await server.SubmitTransaction(
            CreateDummyFeeBumpTransaction(),
            new SubmitTransactionOptions { SkipMemoRequiredCheck = true }
        );

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.IsFalse(string.IsNullOrEmpty(response.Hash));
        Assert.IsNotNull(response.Result);
        Assert.IsInstanceOfType(response.Result, typeof(FeeBumpTransactionResultSuccess));
        var result = (FeeBumpTransactionResultSuccess)response.Result;
        Assert.IsNotNull(result.FeeCharged);
        Assert.IsInstanceOfType(result.InnerResultPair.Result, typeof(TransactionResultSuccess));
        var innerResult = (TransactionResultSuccess)result.InnerResultPair.Result;
        Assert.IsNotNull(innerResult.FeeCharged);
        Assert.IsTrue(innerResult.IsSuccess);
        Assert.AreEqual(1, innerResult.Results.Count);
        Assert.IsInstanceOfType(innerResult.Results.First(), typeof(PaymentSuccess));
    }

    /// <summary>
    ///     Verifies that fee bump transaction with failed inner transaction returns failure result.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithFailedInnerTransaction_ReturnsFailureResult()
    {
        // Arrange
        var resultXdr = Utils.CreateFeeBumpTransactionResultXdr(
            [
                Utils.CreatePaymentResult(
                    PaymentResultCode.PaymentResultCodeEnum.PAYMENT_UNDERFUNDED),
            ],
            TransactionResultCode.TransactionResultCodeEnum.txFAILED);
        var json = Utils.BuildSubmitFailureResponseJson(
            resultXdr,
            "tx_failed",
            ["op_underfunded"]
        );
        using var server = Utils.CreateTestServerWithContent(json, HttpStatusCode.BadRequest);

        // Act
        var response = await server.SubmitTransaction(
            CreateDummyFeeBumpTransaction(),
            new SubmitTransactionOptions { SkipMemoRequiredCheck = true }
        );

        // Assert
        Assert.IsNotNull(response);
        Assert.IsFalse(response.IsSuccess);

        var result = response.Result as FeeBumpTransactionResultFailed;
        Assert.IsNotNull(result);

        var innerResult = result.InnerResultPair.Result as TransactionResultFailed;
        Assert.IsNotNull(innerResult);
        Assert.IsInstanceOfType(innerResult.Results[0], typeof(PaymentUnderfunded));
    }

    /// <summary>
    ///     Verifies that BuildFeeBumpTransaction throws Exception when fee is insufficient.
    /// </summary>
    [TestMethod]
    public void BuildFeeBumpTransaction_WithInsufficientFee_ThrowsException()
    {
        // Arrange
        var account = new Account(Source.AccountId, 0L);
        const uint maxFee = 2000000u;

        var tx = new TransactionBuilder(account)
            .SetFee(maxFee)
            .AddOperation(new PaymentOperation(Sponsor, new AssetTypeNative(), "1000"))
            .AddOperation(new PaymentOperation(Sponsor, new AssetTypeNative(), "1000"))
            .Build();
        tx.Sign(Source);

        // Act & Assert
        var exception = Assert.ThrowsException<Exception>(() =>
            TransactionBuilder.BuildFeeBumpTransaction(Sponsor, tx, maxFee / 2));

        Assert.AreEqual($"Invalid fee, it should be at least {maxFee} stroops", exception.Message);
    }

    /// <summary>
    ///     Verifies that fee bump transaction with sufficient fee submits successfully.
    /// </summary>
    [TestMethod]
    public async Task BuildFeeBumpTransaction_WithSufficientFee_SubmitsSuccessfully()
    {
        // Arrange
        var resultXdr = Utils.CreateFeeBumpTransactionResultXdr(
        [
            Utils.CreatePaymentResult(),
            Utils.CreatePaymentResult(),
        ]);
        using var server = Utils.CreateTestServerWithContent(
            Utils.BuildSubmitTransactionResponseJson(resultXdr)
        );

        var account = new Account(Source.AccountId, 0L);
        const uint maxFee = 2000000u;

        var tx = new TransactionBuilder(account)
            .SetFee(maxFee)
            .AddOperation(new PaymentOperation(Sponsor, new AssetTypeNative(), "1000"))
            .AddOperation(new PaymentOperation(Sponsor, new AssetTypeNative(), "1000"))
            .Build();
        tx.Sign(Source);

        var feeBumpTx = TransactionBuilder.BuildFeeBumpTransaction(Sponsor, tx, maxFee);
        feeBumpTx.Sign(Sponsor);

        // Act
        var response = await server.SubmitTransaction(feeBumpTx,
            new SubmitTransactionOptions { SkipMemoRequiredCheck = true });

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.IsFalse(string.IsNullOrEmpty(response.Hash));
        Assert.IsNotNull(response.Result);
        Assert.IsInstanceOfType(response.Result, typeof(FeeBumpTransactionResultSuccess));
    }
}