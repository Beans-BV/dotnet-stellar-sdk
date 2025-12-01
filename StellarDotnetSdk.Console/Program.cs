using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Transactions;
using SysConsole = System.Console;

namespace StellarDotnetSdk.Console;

public static class Program
{
    private const int DefaultFee = 1000000;

    public static async Task Main(string[] args)
    {
        var json = """
                   {
                     "href": "/ledgers/898826/effects{?cursor,limit,order}",
                     "templated": true
                   }
                   """;
        var json2 = """
                    {
                        "href": "https://horizon-testnet.stellar.org/assets?cursor=&limit=200&order=desc"
                    }
                    """;
        var options = new JsonSerializerOptions();
        options.Converters.Add(new LinkJsonConverter<Response>());
        options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        options.PropertyNameCaseInsensitive = true;
        var workerJson = """
                         {
                             "name": "Cuong",
                             "type": "Worker",
                             "company": "Microsoft",
                             "gender": "Male",
                             "salary": "100000"
                         }
                         """;
        var farmerJson = """
                         {
                             "name": "Cuong",
                             "type": "Farmer",
                             "gender": "Male",
                             "ranch": "Oxen Ranch"
                         }
                         """;

        var worker = JsonSerializer.Deserialize<Person>(workerJson, options);
        var farmer = JsonSerializer.Deserialize<Person>(farmerJson, JsonOptions.DefaultOptions);
        
        // Network.UseTestNetwork();
        // using var server = new Server("https://horizon-testnet.stellar.org");
        // await CreateAccount(server);
    }
    
    private static async Task CreateAccount(Server server)
    {
        var source = KeyPair.FromSecretSeed("SDR4PTKMR5TAQQCL3RI2MLXXSXQDIR7DCAONQNQP6UCDZCD4OVRWXUHI");
        SysConsole.WriteLine($"Source account: {source.AccountId}");
        var destination = KeyPair.Random();
        SysConsole.WriteLine($"Destination account: {destination.AccountId}");

        AccountResponse sourceAccount;
        try
        {
            sourceAccount = await server.Accounts.Account(source.AccountId).ConfigureAwait(false);
        }
        catch (HttpResponseException ex) when (ex.StatusCode == 404)
        {
            SysConsole.WriteLine("Source account not found. Funding with FriendBot...");
            await server.TestNetFriendBot.FundAccount(source.AccountId).Execute().ConfigureAwait(false);
            SysConsole.WriteLine("Account funded successfully!");
            sourceAccount = await server.Accounts.Account(source.AccountId).ConfigureAwait(false);
        }
        var transaction = new TransactionBuilder(sourceAccount)
            .SetFee(DefaultFee)
            .AddOperation(new CreateAccountOperation(destination, "1"))
            .Build();

        transaction.Sign(source);

        var response = await server.SubmitTransaction(transaction).ConfigureAwait(false);
        if (response.IsSuccess && response.Hash != null)
        {
            SysConsole.WriteLine("Create account response: " + response.Hash!);
            await DeleteAccount(server, destination, source).ConfigureAwait(false);
        }
        else
        {
            SysConsole.WriteLine("Create account failed.");
            SysConsole.WriteLine("TransactionResultCode: " +
                (response.SubmitTransactionResponseExtras?.ExtrasResultCodes?.TransactionResultCode ?? "null"));
            SysConsole.WriteLine("TransactionResultCodeOperations: " + string.Join(", ",
                response.SubmitTransactionResponseExtras?.ExtrasResultCodes?.OperationsResultCodes ?? new List<string>()));
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
        if (response.IsSuccess && response.Hash != null)
        {
            SysConsole.WriteLine("Delete account response: " + response.Hash!);
        }
        else
        {
            SysConsole.WriteLine("Delete account failed.");
            SysConsole.WriteLine("TransactionResultCode: " +
                                 (response.SubmitTransactionResponseExtras?.ExtrasResultCodes?.TransactionResultCode ?? "null"));
            SysConsole.WriteLine("TransactionResultCodeOperations: " + string.Join(", ",
                response.SubmitTransactionResponseExtras?.ExtrasResultCodes?.OperationsResultCodes ?? new List<string>()));
        }
    }
}

[JsonConverter(typeof(PersonConverter))]
internal abstract class Person
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("gender")]
    public required string Gender { get; set; }

    [JsonPropertyName("disabled")]
    public bool IsDisabled { get; init; } = true;
}

internal class Worker : Person
{
    [JsonPropertyName("company")]
    public required string Company { get; set; }

    [JsonPropertyName("salary")]
    [JsonInclude]
    public required long Salary { get; set; }
}

internal class Farmer : Person
{
    [JsonPropertyName("ranch")]
    [JsonInclude]
    // [JsonPropertyName("ranch")] 
    private required string _ranch;

    [JsonPropertyName("farm")]
    public required string Farm { get; set; }
}

internal class PersonConverter : JsonConverter<Person>
{
    public override Person Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (var document = JsonDocument.ParseValue(ref reader))
        {
            var root = document.RootElement;
            var type = root.GetProperty("type").GetString();
            switch (type)
            {
                case "Worker":
                    return JsonSerializer.Deserialize<Worker>(root.GetRawText(), options);
                case "Farmer":
                    return JsonSerializer.Deserialize<Farmer>(root.GetRawText(), options);
                default:
                    throw new JsonException("Unknown type.");
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, Person value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}