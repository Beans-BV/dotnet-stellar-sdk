using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class ServerCheckMemoRequiredTest
{
    private const string AccountId = "GAYHAAKPAQLMGIJYMIWPDWCGUCQ5LAWY4Q7Q3IKSP57O7GUPD3NEOSEA";

    [TestMethod]
    public async Task TestFailsIfMemoIsRequired()
    {
        var data = new Dictionary<string, string>
        {
            { "config.memo_required", "MQ==" },
        };
        var json = BuildAccountResponse(AccountId, data);

        using var server = Utils.CreateTestServerWithContent(json);

        var tx = BuildTransaction(AccountId);
        await Assert.ThrowsExceptionAsync<AccountRequiresMemoException>(() => server.CheckMemoRequired(tx));
    }

    [TestMethod]
    public async Task TestItDoesNotThrowIfAccountDoesNotExists()
    {
        var json = BuildAccountResponse(AccountId);
        using var server = Utils.CreateTestServerWithContent(json, HttpStatusCode.NotFound);

        var tx = BuildTransaction(AccountId);
        await server.CheckMemoRequired(tx);
    }

    [TestMethod]
    public async Task TestItDoesNotThrowIfAccountDoesNotHaveDataField()
    {
        var json = BuildAccountResponse(AccountId);
        using var server = Utils.CreateTestServerWithContent(json);

        var tx = BuildTransaction(AccountId);
        await server.CheckMemoRequired(tx);
    }

    [TestMethod]
    public async Task TestRethrowClientException()
    {
        var json = BuildAccountResponse(AccountId);
        using var server = Utils.CreateTestServerWithContent(json, HttpStatusCode.BadRequest);

        var tx = BuildTransaction(AccountId);
        await Assert.ThrowsExceptionAsync<HttpResponseException>(() => server.CheckMemoRequired(tx));
    }

    [TestMethod]
    public async Task TestDoesNotCheckDestinationMoreThanOnce()
    {
        var json = BuildAccountResponse(AccountId);
        using var server = Utils.CreateTestServerWithContent(json);

        var payment = new PaymentOperation(KeyPair.FromAccountId(AccountId), new AssetTypeNative(), "100.500");

        var tx = BuildTransaction(AccountId, new Operation[] { payment });
        await server.CheckMemoRequired(tx);
    }

    [TestMethod]
    public async Task TestCheckOtherOperationTypes()
    {
        var destinations = new[]
        {
            "GASGNGGXDNJE5C2O7LDCATIVYSSTZKB24SHYS6F4RQT4M4IGNYXB4TIV",
            "GBBM6BKZPEHWYO3E3YKREDPQXMS4VK35YLNU7NFBRI26RAN7GI5POFBB",
            "GCEZWKCA5VLDNRLN3RPRJMRZOX3Z6G5CHCGSNFHEYVXM3XOJMDS674JZ",
        };

        var native = new AssetTypeNative();
        var gbp = Asset.CreateNonNativeAsset("GBP", "GBBM6BKZPEHWYO3E3YKREDPQXMS4VK35YLNU7NFBRI26RAN7GI5POFBB");
        var eur = Asset.CreateNonNativeAsset("EUR", "GDTNXRLOJD2YEBPKK7KCMR7J33AAG5VZXHAJTHIG736D6LVEFLLLKPDL");

        var operations = new Operation[]
        {
            new AccountMergeOperation(KeyPair.FromAccountId(destinations[0])),
            new PathPaymentStrictSendOperation(native, "5.00", KeyPair.FromAccountId(destinations[1]),
                native, "5.00", [gbp, eur]),
            new PathPaymentStrictReceiveOperation(native, "5.00", KeyPair.FromAccountId(destinations[2]),
                native, "5.00", [gbp, eur]),
            new ChangeTrustOperation(gbp, "10000"),
        };

        var mockFakeHttpMessageHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        var httpClient = new HttpClient(mockFakeHttpMessageHandler.Object);

        mockFakeHttpMessageHandler.SetupSequence(a => a.Send(It.IsAny<HttpRequestMessage>()))
            .Returns(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(BuildAccountResponse(AccountId)),
            })
            .Returns(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(BuildAccountResponse(destinations[0])),
            })
            .Returns(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(BuildAccountResponse(destinations[1])),
            })
            .Returns(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(BuildAccountResponse(destinations[2])),
            });

        Network.UseTestNetwork();
        using var server = new Server("https://horizon-testnet.stellar.org", httpClient);
        var tx = BuildTransaction(AccountId, operations, Memo.None());
        await server.CheckMemoRequired(tx);
    }

    [TestMethod]
    public async Task TestSkipCheckIfHasMemo()
    {
        using var server = Utils.CreateTestServerWithContent(null);
        var tx = BuildTransaction(AccountId, [], Memo.Text("foobar"));
        await server.CheckMemoRequired(tx);
    }

    [TestMethod]
    public async Task TestCheckFeeBumpTransaction()
    {
        using var server = Utils.CreateTestServerWithContent("");
        var innerTx = BuildTransaction(AccountId, [], Memo.Text("foobar"));
        var feeSource = KeyPair.FromAccountId("GD7HCWFO77E76G6BKJLRHRFRLE6I7BMPJQZQKGNYTT3SPE6BA4DHJAQY");
        var tx = TransactionBuilder.BuildFeeBumpTransaction(feeSource, innerTx, 200);
        await server.CheckMemoRequired(tx);
    }

    [TestMethod]
    public async Task TestSkipCheckIfDestinationIsMuxedAccount()
    {
        var muxed = MuxedAccountMed25519.FromMuxedAccountId(
            "MAAAAAAAAAAAJURAAB2X52XFQP6FBXLGT6LWOOWMEXWHEWBDVRZ7V5WH34Y22MPFBHUHY");

        var payment = new PaymentOperation(muxed, new AssetTypeNative(), "100.500");

        var tx = BuildTransaction(AccountId, [payment], Memo.None(), true);
        using var server = Utils.CreateTestServerWithContent("");
        await server.CheckMemoRequired(tx);
    }

    private string BuildAccountResponse(string accountId, Dictionary<string, string>? data = null)
    {
        var accountData = data ?? new Dictionary<string, string>();
        var response = new AccountResponse
        {
            Id = "1",
            SubentryCount = 1,
            SequenceUpdatedAtLedger = null,
            SequenceUpdatedAtTime = null,
            InflationDestination = null,
            HomeDomain = null,
            Thresholds = new Thresholds
            {
                LowThreshold = 0,
                MedThreshold = 0,
                HighThreshold = 0,
            },
            Flags = new Flags
            {
                AuthRequired = false,
                AuthRevocable = false,
                AuthImmutable = false,
                AuthClawback = false,
            },
            Balances =
            [
                new Balance
                {
                    AssetType = "native",
                    AssetIssuer = "12345.6789",
                    BuyingLiabilities = "0.0",
                    SellingLiabilities = "0.0",
                    IsAuthorized = false,
                    IsAuthorizedToMaintainLiabilities = true,
                    LiquidityPoolId = "1c80ecd9cc567ef5301683af3ca7c2deeba7d519275325549f22514076396469",
                    BalanceString = "123",
                },
            ],
            Signers =
            [
                new AccountResponse.Signer
                {
                    Key = accountId,
                    Type = "ed25519_public_key",
                    Weight = 1,
                },
            ],
            Links = null!,
            NumberSponsored = 0,
            NumberSponsoring = 0,
            Data = accountData,
            AccountId = accountId,
            SequenceNumber = 3298702387052545,
            LastModifiedLedger = 1,
            LastModifiedTime = "2025-08-14T19:44:19Z",
            PagingToken = "123",
        };
        return JsonSerializer.Serialize(response);
    }

    private Transaction BuildTransaction(string destination)
    {
        return BuildTransaction(destination, []);
    }

    private Transaction BuildTransaction(string destinationAccountId, Operation[] operations, Memo? memo = null,
        bool skipDefaultOp = false)
    {
        var keypair = KeyPair.Random();
        var destination = KeyPair.FromAccountId(destinationAccountId);
        var account = new Account(destinationAccountId, 56199647068161);
        var builder = new TransactionBuilder(account);
        if (!skipDefaultOp)
        {
            builder.AddOperation(
                new PaymentOperation(destination, new AssetTypeNative(), "100.50"));
        }

        if (memo != null)
        {
            builder.AddMemo(memo);
        }

        foreach (var operation in operations)
        {
            builder.AddOperation(operation);
        }

        var tx = builder.Build();
        tx.Sign(keypair);
        return tx;
    }
}