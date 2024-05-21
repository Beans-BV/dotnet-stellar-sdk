using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Transactions;
using SysConsole = System.Console;

namespace StellarDotnetSdk.Console;

public static class Program
{
    private const int DefaultFee = 1000000;

    public static async Task Main(string[] args)
    {
        string json = """
                      {
                        "href": "/ledgers/898826/effects{?cursor,limit,order}",
                        "templated": true
                      }
                      """;
        string json2 = """
                       {
                           "href": "https://horizon-testnet.stellar.org/assets?cursor=&limit=200&order=desc"
                       }
                       """;
        // var back = JsonSerializer.Deserialize<Link<AssetResponse>>(json2);
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.Converters.Add(new LinkJsonConverter<AssetResponse>());
        // var back2 = JsonSingleton2.GetInstance<Page<AssetResponse>.PageLinks<AssetResponse>>(json);
        var back2 = JsonSingleton2.GetInstance<Link<Page<AssetResponse>>>(json2);
        var backEffect = JsonSingleton2.GetInstance<Link<Page<LedgerResponse>>>(json);
        var led = await backEffect.Follow();
        // var back3 = JsonSerializer.Deserialize<Link<AssetResponse>>(json2, options);
    }

    private static async Task CreateAccount(Server server)
    {
        var source = KeyPair.FromSecretSeed("SDR4PTKMR5TAQQCL3RI2MLXXSXQDIR7DCAONQNQP6UCDZCD4OVRWXUHI");
        SysConsole.WriteLine("Source account: {TO_BE_CONFIGURED}");
        var destination = KeyPair.Random();
        SysConsole.WriteLine("Destination account: " + destination.AccountId);

        var sourceAccount = await server.Accounts.Account(source.AccountId).ConfigureAwait(false);
        var transaction = new TransactionBuilder(sourceAccount)
            .SetFee(DefaultFee)
            .AddOperation(new CreateAccountOperation(destination, "1"))
            .Build();

        transaction.Sign(source);

        var response = await server.SubmitTransaction(transaction).ConfigureAwait(false);
        if (response.IsSuccess)
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
            .AddOperation(new AccountMergeOperation(destination))
            .Build();

        transaction.Sign(source);

        var feeBumpTransaction = TransactionBuilder.BuildFeeBumpTransaction(destination, transaction, DefaultFee);

        feeBumpTransaction.Sign(destination);

        var response = await server.SubmitTransaction(feeBumpTransaction).ConfigureAwait(false);
        if (response.IsSuccess)
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