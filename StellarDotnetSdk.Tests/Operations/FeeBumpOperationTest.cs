using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Tests.Operations;

[TestClass]
public class FeeBumpOperationTest
{
    [TestMethod]
    public async Task TransactionResultSuccess()
    {
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

        var paymentOperationBuilder = new PaymentOperation.Builder(
            sponsorKeyPair,
            new AssetTypeNative(),
            "10"
        );
        transactionBuilder.AddOperation(
            paymentOperationBuilder.Build()
        );
        var transaction = transactionBuilder.Build();
        transaction.Sign(sourceKeyPair);

        var feeBumpTransaction = TransactionBuilder.BuildFeeBumpTransaction(
            sponsorKeyPair,
            transaction
        );
        feeBumpTransaction.Sign(sponsorKeyPair);

        var response = await server.SubmitTransaction(feeBumpTransaction);
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess());
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

    [TestMethod]
    public async Task TransactionResultFailed()
    {
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

        var paymentOperationBuilder = new PaymentOperation.Builder(
            sponsorKeyPair,
            new AssetTypeNative(),
            "100000"
        );
        transactionBuilder.AddOperation(
            paymentOperationBuilder.Build()
        );
        var transaction = transactionBuilder.Build();
        transaction.Sign(sourceKeyPair);

        var feeBumpTransaction = TransactionBuilder.BuildFeeBumpTransaction(
            sponsorKeyPair,
            transaction
        );
        feeBumpTransaction.Sign(sponsorKeyPair);

        var response = await server.SubmitTransaction(feeBumpTransaction);

        Assert.IsNotNull(response);
        Assert.IsFalse(response.IsSuccess());

        var result = response.Result as FeeBumpTransactionResultFailed;
        Assert.IsNotNull(result);

        var innerResult = result.InnerResultPair.Result as TransactionResultFailed;
        Assert.IsNotNull(innerResult);
        Assert.IsInstanceOfType(innerResult.Results[0], typeof(PaymentUnderfunded));
    }

    [TestMethod]
    public async Task InsufficientFee()
    {
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

        var paymentOperationBuilder = new PaymentOperation.Builder(
            sponsorKeyPair,
            new AssetTypeNative(),
            "1000"
        );
        transactionBuilder.AddOperation(
            paymentOperationBuilder.Build()
        );
        transactionBuilder.AddOperation(
            paymentOperationBuilder.Build()
        );
        var transaction = transactionBuilder.Build();
        transaction.Sign(sourceKeyPair);

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

    [TestMethod]
    public async Task SufficientFee()
    {
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

        var paymentOperationBuilder = new PaymentOperation.Builder(
            sponsorKeyPair,
            new AssetTypeNative(),
            "1000"
        );
        transactionBuilder.AddOperation(
            paymentOperationBuilder.Build()
        );
        transactionBuilder.AddOperation(
            paymentOperationBuilder.Build()
        );
        var transaction = transactionBuilder.Build();
        transaction.Sign(sourceKeyPair);

        var feeBumpTransaction = TransactionBuilder.BuildFeeBumpTransaction(
            sponsorKeyPair,
            transaction,
            maxFee
        );
        feeBumpTransaction.Sign(sponsorKeyPair);

        var response = await server.SubmitTransaction(feeBumpTransaction);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess());
        Assert.IsFalse(string.IsNullOrEmpty(response.Hash));
        Assert.IsInstanceOfType(response.Result, typeof(FeeBumpTransactionResultSuccess));
    }
}