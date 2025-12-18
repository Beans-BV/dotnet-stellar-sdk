using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Tests.Operations;

/// <summary>
/// Tests for FeeBumpTransaction functionality.
/// </summary>
[TestClass]
public class FeeBumpOperationTest
{
    /// <summary>
    /// Verifies that fee bump transaction with successful inner transaction submits successfully.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithSuccessfulInnerTransaction_ReturnsSuccessResult()
    {
        // Arrange
        Network.UseTestNetwork();
        using var server = new Server("https://horizon-testnet.stellar.org");

        var sourceKeyPair = KeyPair.Random();
        var sponsorKeyPair = KeyPair.Random();

        await Task.WhenAll(
            server.TestNetFriendBot
                .FundAccount(sourceKeyPair.AccountId)
                .Execute(),
            server.TestNetFriendBot
                .FundAccount(sponsorKeyPair.AccountId)
                .Execute()
        );

        var sourceAccount = await server.Accounts.Account(sourceKeyPair.AccountId);
        var transactionBuilder = new TransactionBuilder(sourceAccount);
        var paymentOperation = new PaymentOperation(
            sponsorKeyPair,
            new AssetTypeNative(),
            "10"
        );
        transactionBuilder.AddOperation(paymentOperation);
        var transaction = transactionBuilder.Build();
        transaction.Sign(sourceKeyPair);

        var feeBumpTransaction = TransactionBuilder.BuildFeeBumpTransaction(
            sponsorKeyPair,
            transaction
        );
        feeBumpTransaction.Sign(sponsorKeyPair);

        // Act
        var response = await server.SubmitTransaction(feeBumpTransaction);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.IsFalse(string.IsNullOrEmpty(response.Hash));
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
    /// Verifies that fee bump transaction with failed inner transaction returns failure result.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithFailedInnerTransaction_ReturnsFailureResult()
    {
        // Arrange
        Network.UseTestNetwork();
        using var server = new Server("https://horizon-testnet.stellar.org");

        var sourceKeyPair = KeyPair.Random();
        var sponsorKeyPair = KeyPair.Random();

        await Task.WhenAll(
            server.TestNetFriendBot
                .FundAccount(sourceKeyPair.AccountId)
                .Execute(),
            server.TestNetFriendBot
                .FundAccount(sponsorKeyPair.AccountId)
                .Execute()
        );

        var sourceAccount = await server.Accounts.Account(sourceKeyPair.AccountId);
        var transactionBuilder = new TransactionBuilder(sourceAccount);
        var paymentOperation = new PaymentOperation(
            sponsorKeyPair,
            new AssetTypeNative(),
            "100000"
        );
        transactionBuilder.AddOperation(paymentOperation);
        var transaction = transactionBuilder.Build();
        transaction.Sign(sourceKeyPair);

        var feeBumpTransaction = TransactionBuilder.BuildFeeBumpTransaction(
            sponsorKeyPair,
            transaction
        );
        feeBumpTransaction.Sign(sponsorKeyPair);

        // Act
        var response = await server.SubmitTransaction(feeBumpTransaction);

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
    /// Verifies that BuildFeeBumpTransaction throws Exception when fee is insufficient.
    /// </summary>
    [TestMethod]
    public async Task BuildFeeBumpTransaction_WithInsufficientFee_ThrowsException()
    {
        // Arrange
        Network.UseTestNetwork();
        using var server = new Server("https://horizon-testnet.stellar.org");

        var sourceKeyPair = KeyPair.Random();
        var sponsorKeyPair = KeyPair.Random();

        await Task.WhenAll(
            server.TestNetFriendBot
                .FundAccount(sourceKeyPair.AccountId)
                .Execute(),
            server.TestNetFriendBot
                .FundAccount(sponsorKeyPair.AccountId)
                .Execute()
        );

        var sourceAccount = await server.Accounts.Account(sourceKeyPair.AccountId);
        const uint maxFee = 2000000u;

        var transactionBuilder = new TransactionBuilder(sourceAccount);
        transactionBuilder.SetFee(maxFee);

        var paymentOperation = new PaymentOperation(
            sponsorKeyPair,
            new AssetTypeNative(),
            "1000"
        );
        transactionBuilder.AddOperation(paymentOperation);
        transactionBuilder.AddOperation(paymentOperation);
        var transaction = transactionBuilder.Build();
        transaction.Sign(sourceKeyPair);

        // Act & Assert
        var exception = Assert.ThrowsException<Exception>(() =>
        {
            TransactionBuilder.BuildFeeBumpTransaction(
                sponsorKeyPair,
                transaction,
                maxFee / 2
            );
        });

        Assert.AreEqual($"Invalid fee, it should be at least {maxFee} stroops", exception.Message);
    }

    /// <summary>
    /// Verifies that fee bump transaction with sufficient fee submits successfully.
    /// </summary>
    [TestMethod]
    public async Task BuildFeeBumpTransaction_WithSufficientFee_SubmitsSuccessfully()
    {
        // Arrange
        Network.UseTestNetwork();
        using var server = new Server("https://horizon-testnet.stellar.org");

        var sourceKeyPair = KeyPair.Random();
        var sponsorKeyPair = KeyPair.Random();

        await Task.WhenAll(
            server.TestNetFriendBot
                .FundAccount(sourceKeyPair.AccountId)
                .Execute(),
            server.TestNetFriendBot
                .FundAccount(sponsorKeyPair.AccountId)
                .Execute()
        );

        var sourceAccount = await server.Accounts.Account(sourceKeyPair.AccountId);
        const uint maxFee = 2000000u;

        var transactionBuilder = new TransactionBuilder(sourceAccount);
        transactionBuilder.SetFee(maxFee);

        var paymentOperation = new PaymentOperation(
            sponsorKeyPair,
            new AssetTypeNative(),
            "1000"
        );
        transactionBuilder.AddOperation(paymentOperation);
        transactionBuilder.AddOperation(paymentOperation);
        var transaction = transactionBuilder.Build();
        transaction.Sign(sourceKeyPair);

        var feeBumpTransaction = TransactionBuilder.BuildFeeBumpTransaction(
            sponsorKeyPair,
            transaction,
            maxFee
        );
        feeBumpTransaction.Sign(sponsorKeyPair);

        // Act
        var response = await server.SubmitTransaction(feeBumpTransaction);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.IsFalse(string.IsNullOrEmpty(response.Hash));
        Assert.IsInstanceOfType(response.Result, typeof(FeeBumpTransactionResultSuccess));
    }
}