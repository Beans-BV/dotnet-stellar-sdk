using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Examples.Horizon;

/// <summary>
///     Examples demonstrating fee bump transaction functionality on the Stellar test network.
/// </summary>
public static class FeeBumpExamples
{
    private const string TestNetUrl = "https://horizon-testnet.stellar.org";

    public static async Task Run()
    {
        Console.WriteLine("Creating source and sponsor accounts for fee bump examples...");
        var sourceKeyPair = HorizonExamples.CreateKeyPair();
        var sponsorKeyPair = HorizonExamples.CreateKeyPair();

        await Task.WhenAll(
            HorizonExamples.FundAccountUsingFriendBot(sourceKeyPair.AccountId),
            HorizonExamples.FundAccountUsingFriendBot(sponsorKeyPair.AccountId)
        );

        await FeeBumpWithSuccessfulInnerTransaction(sourceKeyPair, sponsorKeyPair);
        await FeeBumpWithFailedInnerTransaction(sourceKeyPair, sponsorKeyPair);
        FeeBumpWithInsufficientFee(sourceKeyPair, sponsorKeyPair);
        await FeeBumpWithExplicitMaxFee(sourceKeyPair, sponsorKeyPair);
    }

    /// <summary>
    ///     Demonstrates a fee bump transaction wrapping a successful inner payment transaction.
    /// </summary>
    public static async Task FeeBumpWithSuccessfulInnerTransaction(
        KeyPair sourceKeyPair,
        KeyPair sponsorKeyPair)
    {
        Console.WriteLine("\n--- Fee Bump With Successful Inner Transaction ---");
        var server = new Server(TestNetUrl);

        var sourceAccount = await server.Accounts.Account(sourceKeyPair.AccountId);
        var paymentOperation = new PaymentOperation(sponsorKeyPair, new AssetTypeNative(), "10");

        var transaction = new TransactionBuilder(sourceAccount)
            .AddOperation(paymentOperation)
            .Build();
        transaction.Sign(sourceKeyPair);

        var feeBumpTransaction = TransactionBuilder.BuildFeeBumpTransaction(sponsorKeyPair, transaction);
        feeBumpTransaction.Sign(sponsorKeyPair);

        var response = await server.SubmitTransaction(feeBumpTransaction);
        ArgumentNullException.ThrowIfNull(response);

        if (!response.IsSuccess)
        {
            throw new Exception("Fee bump transaction with successful inner transaction failed.");
        }

        Console.WriteLine($"  Transaction hash: {response.Hash}");
        Console.WriteLine($"  Result type: {response.Result?.GetType().Name}");

        var result = (FeeBumpTransactionResultSuccess)response.Result!;
        Console.WriteLine($"  Fee charged: {result.FeeCharged}");

        var innerResult = (TransactionResultSuccess)result.InnerResultPair.Result;
        Console.WriteLine($"  Inner result fee charged: {innerResult.FeeCharged}");
        Console.WriteLine($"  Inner operation count: {innerResult.Results.Count}");
        Console.WriteLine($"  Inner result type: {innerResult.Results[0].GetType().Name}");
    }

    /// <summary>
    ///     Demonstrates a fee bump transaction wrapping a failed inner payment transaction (underfunded).
    /// </summary>
    public static async Task FeeBumpWithFailedInnerTransaction(
        KeyPair sourceKeyPair,
        KeyPair sponsorKeyPair)
    {
        Console.WriteLine("\n--- Fee Bump With Failed Inner Transaction ---");
        var server = new Server(TestNetUrl);

        var sourceAccount = await server.Accounts.Account(sourceKeyPair.AccountId);
        // Payment amount exceeds account balance, causing PAYMENT_UNDERFUNDED
        var paymentOperation = new PaymentOperation(sponsorKeyPair, new AssetTypeNative(), "100000");

        var transaction = new TransactionBuilder(sourceAccount)
            .AddOperation(paymentOperation)
            .Build();
        transaction.Sign(sourceKeyPair);

        var feeBumpTransaction = TransactionBuilder.BuildFeeBumpTransaction(sponsorKeyPair, transaction);
        feeBumpTransaction.Sign(sponsorKeyPair);

        var response = await server.SubmitTransaction(feeBumpTransaction);
        ArgumentNullException.ThrowIfNull(response);

        Console.WriteLine($"  Transaction succeeded: {response.IsSuccess}");

        var result = response.Result as FeeBumpTransactionResultFailed;
        ArgumentNullException.ThrowIfNull(result);
        Console.WriteLine($"  Fee bump result type: {result.GetType().Name}");

        var innerResult = result.InnerResultPair.Result as TransactionResultFailed;
        ArgumentNullException.ThrowIfNull(innerResult);
        Console.WriteLine($"  Inner failure result type: {innerResult.Results[0].GetType().Name}");
    }

    /// <summary>
    ///     Demonstrates that building a fee bump transaction with an insufficient fee throws an exception.
    ///     This is purely local validation — no network call is made.
    /// </summary>
    public static void FeeBumpWithInsufficientFee(
        KeyPair sourceKeyPair,
        KeyPair sponsorKeyPair)
    {
        Console.WriteLine("\n--- Fee Bump With Insufficient Fee ---");

        var sourceAccount = new Account(sourceKeyPair.AccountId, 100);
        const uint maxFee = 2000000u;

        var paymentOperation = new PaymentOperation(sponsorKeyPair, new AssetTypeNative(), "1000");

        var transaction = new TransactionBuilder(sourceAccount)
            .SetFee(maxFee)
            .AddOperation(paymentOperation)
            .AddOperation(paymentOperation)
            .Build();
        transaction.Sign(sourceKeyPair);

        try
        {
            TransactionBuilder.BuildFeeBumpTransaction(sponsorKeyPair, transaction, maxFee / 2);
            Console.WriteLine("  ERROR: Expected exception was not thrown");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Caught expected exception: {ex.Message}");
        }
    }

    /// <summary>
    ///     Demonstrates a fee bump transaction with an explicit max fee that is sufficient.
    /// </summary>
    public static async Task FeeBumpWithExplicitMaxFee(
        KeyPair sourceKeyPair,
        KeyPair sponsorKeyPair)
    {
        Console.WriteLine("\n--- Fee Bump With Explicit Max Fee ---");
        var server = new Server(TestNetUrl);

        var sourceAccount = await server.Accounts.Account(sourceKeyPair.AccountId);
        const uint maxFee = 2000000u;

        var paymentOperation = new PaymentOperation(sponsorKeyPair, new AssetTypeNative(), "1000");

        var transaction = new TransactionBuilder(sourceAccount)
            .SetFee(maxFee)
            .AddOperation(paymentOperation)
            .AddOperation(paymentOperation)
            .Build();
        transaction.Sign(sourceKeyPair);

        var feeBumpTransaction = TransactionBuilder.BuildFeeBumpTransaction(
            sponsorKeyPair,
            transaction,
            maxFee
        );
        feeBumpTransaction.Sign(sponsorKeyPair);

        var response = await server.SubmitTransaction(feeBumpTransaction);
        ArgumentNullException.ThrowIfNull(response);

        if (!response.IsSuccess)
        {
            throw new Exception("Fee bump transaction with explicit max fee failed.");
        }

        Console.WriteLine($"  Transaction hash: {response.Hash}");
        Console.WriteLine($"  Result type: {response.Result?.GetType().Name}");
    }
}
