using System.Threading.Tasks;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Transactions;
using SysConsole = System.Console;

namespace StellarDotnetSdk.Console;

public static class Program
{
    private const int DefaultFee = 1000000;

    public static async Task Main(string[] args)
    {
        Network.UsePublicNetwork();
        using var server = new Server("https://horizon.stellar.org");

        var paymentsWithoutTransactions = await server.Payments.Execute().ConfigureAwait(false);
        var paymentsWithTransactions = await server.Payments.IncludeTransaction().Execute().ConfigureAwait(false);

        // await CreateAccount(server).ConfigureAwait(false);

        SysConsole.ReadLine();
    }

    private static async Task CreateAccount(Server server)
    {
        var source = KeyPair.FromSecretSeed("{TO_BE_CONFIGURED}");
        SysConsole.WriteLine("Source account: {TO_BE_CONFIGURED}");
        var destination = KeyPair.Random();
        SysConsole.WriteLine("Destination account: " + destination.AccountId);

        var sourceAccount = await server.Accounts.Account(source.AccountId).ConfigureAwait(false);
        var transaction = new TransactionBuilder(sourceAccount)
            .SetFee(DefaultFee)
            .AddOperation(new CreateAccountOperation.Builder(destination, "1").Build())
            .Build();

        transaction.Sign(source);

        var response = await server.SubmitTransaction(transaction).ConfigureAwait(false);
        if (response.IsSuccess())
        {
            SysConsole.WriteLine("Create account response: " + response.Hash);
            await DeleteAccount(server, destination, source).ConfigureAwait(false);
        }
        else
        {
            SysConsole.WriteLine("Create account failed.");
            SysConsole.WriteLine("TransactionResultCode: " +
                response.SubmitTransactionResponseExtras.ExtrasResultCodes.TransactionResultCode ?? "null");
            SysConsole.WriteLine("TransactionResultCodeOperations: " + string.Join(", ",
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
            SysConsole.WriteLine("Delete account response: " + response.Hash);
        }
        else
        {
            SysConsole.WriteLine("Delete account failed.");
            SysConsole.WriteLine("TransactionResultCode: " +
                                 response.SubmitTransactionResponseExtras.ExtrasResultCodes.TransactionResultCode);
            SysConsole.WriteLine("TransactionResultCodeOperations: " + string.Join(", ",
                response.SubmitTransactionResponseExtras.ExtrasResultCodes.OperationsResultCodes));
        }
    }
}