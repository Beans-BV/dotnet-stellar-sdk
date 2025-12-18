using System;
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
using StellarDotnetSdk.Responses.Effects;
using StellarDotnetSdk.Responses.Operations;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Tests;

/// <summary>
/// Unit tests for memo requirement checking functionality in <see cref="Server"/> class.
/// </summary>
[TestClass]
public class ServerCheckMemoRequiredTest
{
    private const string AccountId = "GAYHAAKPAQLMGIJYMIWPDWCGUCQ5LAWY4Q7Q3IKSP57O7GUPD3NEOSEA";

    /// <summary>
    /// Verifies that CheckMemoRequired throws AccountRequiresMemoException when account requires memo and transaction has no memo.
    /// </summary>
    [TestMethod]
    public async Task CheckMemoRequired_WhenMemoIsRequired_ThrowsAccountRequiresMemoException()
    {
        // Arrange
        var data = new Dictionary<string, string>
        {
            { "config.memo_required", "MQ==" },
        };
        var json = BuildAccountResponse(AccountId, data);

        using var server = Utils.CreateTestServerWithContent(json);

        var tx = BuildTransaction(AccountId);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<AccountRequiresMemoException>(() => server.CheckMemoRequired(tx));
    }

    /// <summary>
    /// Verifies that CheckMemoRequired does not throw when account does not exist.
    /// </summary>
    [TestMethod]
    public async Task CheckMemoRequired_WhenAccountDoesNotExist_DoesNotThrow()
    {
        // Arrange
        var json = BuildAccountResponse(AccountId);
        using var server = Utils.CreateTestServerWithContent(json, HttpStatusCode.NotFound);

        var tx = BuildTransaction(AccountId);

        // Act
        await server.CheckMemoRequired(tx);
    }

    /// <summary>
    /// Verifies that CheckMemoRequired does not throw when account does not have data field.
    /// </summary>
    [TestMethod]
    public async Task CheckMemoRequired_WhenAccountDoesNotHaveDataField_DoesNotThrow()
    {
        // Arrange
        var json = BuildAccountResponse(AccountId);
        using var server = Utils.CreateTestServerWithContent(json);

        var tx = BuildTransaction(AccountId);

        // Act
        await server.CheckMemoRequired(tx);
    }

    /// <summary>
    /// Verifies that CheckMemoRequired rethrows HttpResponseException when server returns bad request.
    /// </summary>
    [TestMethod]
    public async Task CheckMemoRequired_WhenServerReturnsBadRequest_ThrowsHttpResponseException()
    {
        // Arrange
        var json = BuildAccountResponse(AccountId);
        using var server = Utils.CreateTestServerWithContent(json, HttpStatusCode.BadRequest);

        var tx = BuildTransaction(AccountId);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<HttpResponseException>(() => server.CheckMemoRequired(tx));
    }

    /// <summary>
    /// Verifies that CheckMemoRequired does not check the same destination account more than once.
    /// </summary>
    [TestMethod]
    public async Task CheckMemoRequired_WithDuplicateDestination_DoesNotCheckDestinationMoreThanOnce()
    {
        // Arrange
        var json = BuildAccountResponse(AccountId);
        using var server = Utils.CreateTestServerWithContent(json);

        var payment = new PaymentOperation(KeyPair.FromAccountId(AccountId), new AssetTypeNative(), "100.500");

        var tx = BuildTransaction(AccountId, new Operation[] { payment });

        // Act
        await server.CheckMemoRequired(tx);
    }

    /// <summary>
    /// Verifies that CheckMemoRequired checks memo requirement for various operation types with different destinations.
    /// </summary>
    [TestMethod]
    public async Task CheckMemoRequired_WithVariousOperationTypes_ChecksAllDestinations()
    {
        // Arrange
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

        // Act
        await server.CheckMemoRequired(tx);
    }

    /// <summary>
    /// Verifies that CheckMemoRequired skips memo check when transaction already has a memo.
    /// </summary>
    [TestMethod]
    public async Task CheckMemoRequired_WhenTransactionHasMemo_SkipsCheck()
    {
        // Arrange
        using var server = Utils.CreateTestServerWithContent(null);
        var tx = BuildTransaction(AccountId, [], Memo.Text("foobar"));

        // Act
        await server.CheckMemoRequired(tx);
    }

    /// <summary>
    /// Verifies that CheckMemoRequired checks memo requirement for fee bump transactions.
    /// </summary>
    [TestMethod]
    public async Task CheckMemoRequired_WithFeeBumpTransaction_ChecksMemoRequirement()
    {
        // Arrange
        using var server = Utils.CreateTestServerWithContent("");
        var innerTx = BuildTransaction(AccountId, [], Memo.Text("foobar"));
        var feeSource = KeyPair.FromAccountId("GD7HCWFO77E76G6BKJLRHRFRLE6I7BMPJQZQKGNYTT3SPE6BA4DHJAQY");
        var tx = TransactionBuilder.BuildFeeBumpTransaction(feeSource, innerTx, 200);

        // Act
        await server.CheckMemoRequired(tx);
    }

    /// <summary>
    /// Verifies that CheckMemoRequired skips memo check when destination is a muxed account.
    /// </summary>
    [TestMethod]
    public async Task CheckMemoRequired_WhenDestinationIsMuxedAccount_SkipsCheck()
    {
        // Arrange
        var muxed = MuxedAccountMed25519.FromMuxedAccountId(
            "MAAAAAAAAAAAJURAAB2X52XFQP6FBXLGT6LWOOWMEXWHEWBDVRZ7V5WH34Y22MPFBHUHY");

        var payment = new PaymentOperation(muxed, new AssetTypeNative(), "100.500");

        var tx = BuildTransaction(AccountId, [payment], Memo.None(), true);
        using var server = Utils.CreateTestServerWithContent("");

        // Act
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
            Links = new AccountResponseLinks
            {
                Self = Link<AccountResponse>
                    .Create("https://horizon-testnet.stellar.org/accounts/1", false),
                Transactions = Link<Page<TransactionResponse>>
                    .Create("https://horizon-testnet.stellar.org/accounts/1/transactions{?cursor,limit,order}", true),
                Offers = Link<Page<OfferResponse>>
                    .Create("https://horizon-testnet.stellar.org/accounts/1/offers{?cursor,limit,order}", true),
                Operations = Link<Page<OperationResponse>>
                    .Create("https://horizon-testnet.stellar.org/accounts/1/operations{?cursor,limit,order}", true),
                Payments = Link<Page<PaymentOperationResponse>>
                    .Create("https://horizon-testnet.stellar.org/accounts/1/payments{?cursor,limit,order}", true),
                Trades = Link<Page<TradeResponse>>
                    .Create("https://horizon-testnet.stellar.org/accounts/1/trades{?cursor,limit,order}", true),
                Effects = Link<Page<EffectResponse>>
                    .Create("https://horizon-testnet.stellar.org/accounts/1/effects{?cursor,limit,order}", true),
            },
            NumberSponsored = 0,
            NumberSponsoring = 0,
            Data = accountData,
            AccountId = accountId,
            SequenceNumber = 3298702387052545,
            LastModifiedLedger = 1,
            LastModifiedTime = new DateTimeOffset(2025, 8, 14, 19, 44, 19, TimeSpan.Zero),
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