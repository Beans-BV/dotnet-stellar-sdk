using System;
using System.Threading.Tasks;
using stellar_dotnet_sdk;

namespace TestConsole;

public class Program
{
    private const int DefaultFee = 1000000;

    public static async Task Main(string[] args)
    {
        Network.UsePublicNetwork();
        using var server = new Server("https://horizon.stellar.org");

        var paymentsWithoutTransactions = await server.Payments.Execute().ConfigureAwait(false);
        var paymentsWithTransactions = await server.Payments.IncludeTransaction().Execute().ConfigureAwait(false);

        // await CreateAccount(server).ConfigureAwait(false);

        Console.ReadLine();
    }

    private static async Task CreateAccount(Server server)
    {
        var source = KeyPair.FromSecretSeed("{TO_BE_CONFIGURED}");
        Console.WriteLine("Source account: {TO_BE_CONFIGURED}");
        var destination = KeyPair.Random();
        Console.WriteLine("Destination account: " + destination.AccountId);

        var sourceAccount = await server.Accounts.Account(source.AccountId).ConfigureAwait(false);
        var transaction = new TransactionBuilder(sourceAccount)
            .SetFee(DefaultFee)
            .AddOperation(new CreateAccountOperation.Builder(destination, "1").Build())
            .Build();

        transaction.Sign(source);

        var response = await server.SubmitTransaction(transaction).ConfigureAwait(false);
        if (response.IsSuccess())
        {
            Console.WriteLine("Create account response: " + response.Hash);
            await DeleteAccount(server, destination, source).ConfigureAwait(false);
        }
        else
        {
            Console.WriteLine("Create account failed.");
            Console.WriteLine("TransactionResultCode: " +
                response.SubmitTransactionResponseExtras.ExtrasResultCodes.TransactionResultCode ?? "null");
            Console.WriteLine("TransactionResultCodeOperations: " + string.Join(", ",
                response.SubmitTransactionResponseExtras.ExtrasResultCodes.OperationsResultCodes));
        }
    }

    private static async Task DeleteAccount(Server server, KeyPair source, KeyPair destination)
    {
        var accountToDelete = await server.Accounts.Account(source.AccountId).ConfigureAwait(false);
        var transaction = new TransactionBuilder(accountToDelete)
            .SetFee(DefaultFee)
            .AddOperation(new AccountMergeOperation.Builder(destination).Build())
            .Build();

        transaction.Sign(source);

        var feeBumpTransaction = TransactionBuilder.BuildFeeBumpTransaction(destination, transaction, DefaultFee);

        feeBumpTransaction.Sign(destination);

        var response = await server.SubmitTransaction(feeBumpTransaction).ConfigureAwait(false);
        if (response.IsSuccess())
        {
            Console.WriteLine("Delete account response: " + response.Hash);
        }
        else
        {
            Console.WriteLine("Delete account failed.");
            Console.WriteLine("TransactionResultCode: " +
                              response.SubmitTransactionResponseExtras.ExtrasResultCodes.TransactionResultCode);
            Console.WriteLine("TransactionResultCodeOperations: " + string.Join(", ",
                response.SubmitTransactionResponseExtras.ExtrasResultCodes.OperationsResultCodes));
        }
    }
}