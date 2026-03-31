using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using BeginSponsoringFutureReservesResult = StellarDotnetSdk.Xdr.BeginSponsoringFutureReservesResult;
using ChangeTrustResult = StellarDotnetSdk.Xdr.ChangeTrustResult;
using CreateClaimableBalanceResult = StellarDotnetSdk.Xdr.CreateClaimableBalanceResult;
using EndSponsoringFutureReservesResult = StellarDotnetSdk.Xdr.EndSponsoringFutureReservesResult;
using Int32 = StellarDotnetSdk.Xdr.Int32;
using XdrTransactionResult = StellarDotnetSdk.Xdr.TransactionResult;
using XdrInnerTransactionResult = StellarDotnetSdk.Xdr.InnerTransactionResult;
using XdrInnerTransactionResultPair = StellarDotnetSdk.Xdr.InnerTransactionResultPair;
using Int64 = StellarDotnetSdk.Xdr.Int64;
using ManageDataResult = StellarDotnetSdk.Xdr.ManageDataResult;
using ManageSellOfferResult = StellarDotnetSdk.Xdr.ManageSellOfferResult;
using OfferEntry = StellarDotnetSdk.Xdr.OfferEntry;
using OperationResult = StellarDotnetSdk.Xdr.OperationResult;
using PaymentResult = StellarDotnetSdk.Xdr.PaymentResult;
using RevokeSponsorshipResult = StellarDotnetSdk.Xdr.RevokeSponsorshipResult;
using TransactionResult = StellarDotnetSdk.Responses.Results.TransactionResult;
using TransactionResultCodeEnum = StellarDotnetSdk.Xdr.TransactionResultCode.TransactionResultCodeEnum;

namespace StellarDotnetSdk.Tests;

public static class Utils
{
    /// <summary>
    ///     Gets the root path of the test project (StellarDotnetSdk.Tests directory).
    ///     This is calculated by going up 4 levels from AppContext.BaseDirectory
    ///     (bin/Debug/net8.0 -> Debug -> bin -> StellarDotnetSdk.Tests).
    /// </summary>
    /// <returns>The absolute path to the test project root directory.</returns>
    private static string GetTestProjectRoot()
    {
        var baseDir = AppContext.BaseDirectory;
        // Go up 4 levels: bin/Debug/net8.0 -> Debug -> bin -> StellarDotnetSdk.Tests
        for (var i = 0; i < 4; i++)
        {
            baseDir = Path.GetDirectoryName(baseDir);
        }

        return baseDir ?? ".";
    }

    /// <summary>
    ///     Gets the absolute path to a test data file.
    /// </summary>
    /// <param name="jsonFilePath">Path of the test data file (relative to TestData directory or just filename).</param>
    /// <param name="testFilePath">Caller file path (automatically provided by compiler).</param>
    /// <returns>The absolute path to the test data file.</returns>
    public static string GetTestDataAbsolutePath(string jsonFilePath, [CallerFilePath] string testFilePath = "")
    {
        var relativePath = GetTestDataPath(jsonFilePath, testFilePath);
        return Path.Combine(GetTestProjectRoot(), relativePath);
    }

    /// <summary>
    ///     Gets the path to the JSON data file relative to the calling test.
    ///     By default, the JSON data files should have the same directory structure as their tests, but under /TestData.
    ///     For example, if the test is in Responses\Effects\LiquidityPoolEffectResponseTest, then the JSON file is located in
    ///     TestData\Responses\Effects\LiquidityPoolEffectResponseTest.
    /// </summary>
    /// <param name="jsonFilePath">
    ///     Path of the JSON file. If only the name of the file provided, then the file is located in
    ///     the default folder structure. Otherwise, provide the relative path of the file to the TestData directory.
    /// </param>
    /// <param name="testFilePath">
    ///     Leave it empty if the JSON file is located in the default location. Otherwise, it's should
    ///     be the relative path to the JSON file.
    /// </param>
    /// <returns>The relative path to the JSON data file.</returns>
    public static string GetTestDataPath(string jsonFilePath, [CallerFilePath] string testFilePath = "")
    {
        if (Path.GetDirectoryName(jsonFilePath) != "")
        {
            return Path.Combine("TestData", jsonFilePath);
        }
        // testFilePath would be something like C:\workspace\dotnet-stellar-sdk\StellarDotnetSdk.Tests\Responses\Effects\LiquidityPoolEffectResponseTest.cs on Windows
        // testFileDirectoryPath would be something like C:\workspace\dotnet-stellar-sdk\StellarDotnetSdk.Tests\Responses\Effects\ on Windows
        // AppContext.BaseDirectory would be /home/runner/work/dotnet-stellar-sdk/dotnet-stellar-sdk/StellarDotnetSdk.Tests/bin/Release/net8.0 on Linux
        // and C:\workspace\dotnet-stellar-sdk\StellarDotnetSdk.Tests\bin\Release\net8.0 on Windows
        // rootPath would be something like C:\workspace\dotnet-stellar-sdk\StellarDotnetSdk.Tests\ on Windows
        var rootPath = GetTestProjectRoot();

        var testFileDirectoryPath = Path.GetDirectoryName(testFilePath);
        // Would be responses\Effects\
        var testRelativeDirectoryPath = Path.GetRelativePath(rootPath, testFileDirectoryPath);

        return Path.Combine("TestData", testRelativeDirectoryPath, jsonFilePath);
    }

    public static TransactionResult AssertResultOfType(string xdr, Type resultType, bool isSuccess)
    {
        var result = TransactionResult.FromXdrBase64(xdr);
        Assert.IsInstanceOfType(result, typeof(TransactionResultFailed));
        var failed = (TransactionResultFailed)result;
        Assert.IsFalse(failed.IsSuccess);
        Assert.AreEqual(1, failed.Results.Count);
        Assert.IsInstanceOfType(failed.Results[0], resultType);
        Assert.AreEqual(isSuccess, failed.Results[0].IsSuccess);
        return result;
    }

    public static string CreateTransactionResultXdr(OperationResult.OperationResultTr operationResultTr)
    {
        var transactionResult = new XdrTransactionResult
        {
            Ext = new XdrTransactionResult.TransactionResultExt(),
            FeeCharged = new Int64(100L),
            Result = new XdrTransactionResult.TransactionResultResult
            {
                Discriminant =
                {
                    InnerValue = TransactionResultCodeEnum.txFAILED,
                },
                Results = new OperationResult[1],
            },
        };

        transactionResult.Result.Results[0] = new OperationResult
        {
            Tr = operationResultTr,
        };

        var outputStream = new XdrDataOutputStream();
        XdrTransactionResult.Encode(outputStream, transactionResult);
        return Convert.ToBase64String(outputStream.ToArray());
    }

    public static string CreateTransactionResultXdr(
        OperationResult.OperationResultTr[] operationResults,
        TransactionResultCodeEnum code = TransactionResultCodeEnum.txSUCCESS)
    {
        var transactionResult = new XdrTransactionResult
        {
            Ext = new XdrTransactionResult.TransactionResultExt(),
            FeeCharged = new Int64(100L),
            Result = new XdrTransactionResult.TransactionResultResult
            {
                Discriminant = { InnerValue = code },
                Results = new OperationResult[operationResults.Length],
            },
        };

        for (var i = 0; i < operationResults.Length; i++)
        {
            transactionResult.Result.Results[i] = new OperationResult
            {
                Discriminant =
                {
                    InnerValue = OperationResultCode.OperationResultCodeEnum.opINNER,
                },
                Tr = operationResults[i],
            };
        }

        var outputStream = new XdrDataOutputStream();
        XdrTransactionResult.Encode(outputStream, transactionResult);
        return Convert.ToBase64String(outputStream.ToArray());
    }

    public static string CreateFeeBumpTransactionResultXdr(
        OperationResult.OperationResultTr[] innerOperationResults,
        TransactionResultCodeEnum innerCode =
            TransactionResultCodeEnum.txSUCCESS)
    {
        var outerCode = innerCode == TransactionResultCodeEnum.txSUCCESS
            ? TransactionResultCodeEnum.txFEE_BUMP_INNER_SUCCESS
            : TransactionResultCodeEnum.txFEE_BUMP_INNER_FAILED;

        var innerResults = new OperationResult[innerOperationResults.Length];
        for (var i = 0; i < innerOperationResults.Length; i++)
        {
            innerResults[i] = new OperationResult
            {
                Discriminant =
                {
                    InnerValue = OperationResultCode.OperationResultCodeEnum.opINNER,
                },
                Tr = innerOperationResults[i],
            };
        }

        var innerResult = new XdrInnerTransactionResult
        {
            Ext = new InnerTransactionResult.InnerTransactionResultExt(),
            FeeCharged = new Int64(100L),
            Result = new InnerTransactionResult.InnerTransactionResultResult
            {
                Discriminant = { InnerValue = innerCode },
                Results = innerResults,
            },
        };

        var innerResultPair = new XdrInnerTransactionResultPair
        {
            TransactionHash = new Hash { InnerValue = new byte[32] },
            Result = innerResult,
        };

        var transactionResult = new XdrTransactionResult
        {
            Ext = new XdrTransactionResult.TransactionResultExt(),
            FeeCharged = new Int64(100L),
            Result = new XdrTransactionResult.TransactionResultResult
            {
                Discriminant = { InnerValue = outerCode },
                InnerResultPair = innerResultPair,
            },
        };

        var outputStream = new XdrDataOutputStream();
        XdrTransactionResult.Encode(outputStream, transactionResult);
        return Convert.ToBase64String(outputStream.ToArray());
    }

    public static string BuildSubmitTransactionResponseJson(string resultXdr)
    {
        return $$"""
                 {
                   "hash": "abc123",
                   "ledger": 100,
                   "envelope_xdr": "AAAA",
                   "result_xdr": "{{resultXdr}}",
                   "result_meta_xdr": "AAAAAAAAAAAAAAAA"
                 }
                 """;
    }

    public static string BuildSubmitFailureResponseJson(
        string resultXdr,
        string txResultCode = "tx_failed",
        string[]? opResultCodes = null)
    {
        var opsJson = opResultCodes != null
            ? "[" + string.Join(",", opResultCodes.Select(c => $"\"{c}\"")) + "]"
            : "[]";

        return $$"""
                 {
                   "type": "https://stellar.org/horizon-errors/transaction_failed",
                   "title": "Transaction Failed",
                   "status": 400,
                   "extras": {
                     "envelope_xdr": "AAAA",
                     "result_codes": {
                       "transaction": "{{txResultCode}}",
                       "operations": {{opsJson}}
                     },
                     "result_xdr": "{{resultXdr}}"
                   }
                 }
                 """;
    }

    public static string BuildAccountResponseJson(string accountId, long sequence)
    {
        return $$"""
                 {
                   "_links": {
                     "self": { "href": "https://horizon-testnet.stellar.org/accounts/{{accountId}}" },
                     "transactions": { "href": "https://horizon-testnet.stellar.org/accounts/{{accountId}}/transactions", "templated": true },
                     "operations": { "href": "https://horizon-testnet.stellar.org/accounts/{{accountId}}/operations", "templated": true },
                     "payments": { "href": "https://horizon-testnet.stellar.org/accounts/{{accountId}}/payments", "templated": true },
                     "effects": { "href": "https://horizon-testnet.stellar.org/accounts/{{accountId}}/effects", "templated": true },
                     "offers": { "href": "https://horizon-testnet.stellar.org/accounts/{{accountId}}/offers", "templated": true },
                     "trades": { "href": "https://horizon-testnet.stellar.org/accounts/{{accountId}}/trades", "templated": true },
                     "data": { "href": "https://horizon-testnet.stellar.org/accounts/{{accountId}}/data/{key}", "templated": true }
                   },
                   "id": "{{accountId}}",
                   "account_id": "{{accountId}}",
                   "sequence": "{{sequence}}",
                   "sequence_ledger": 1234,
                   "sequence_time": "1755199978",
                   "subentry_count": 0,
                   "thresholds": { "low_threshold": 0, "med_threshold": 0, "high_threshold": 0 },
                   "last_modified_ledger": 1234,
                   "last_modified_time": "2025-01-01T00:00:00Z",
                   "flags": { "auth_required": false, "auth_revocable": false, "auth_immutable": false, "auth_clawback_enabled": false },
                   "balances": [{ "asset_type": "native", "balance": "10000.0000000", "buying_liabilities": "0.0000000", "selling_liabilities": "0.0000000" }],
                   "signers": [{ "key": "{{accountId}}", "weight": 1, "type": "ed25519_public_key" }],
                   "data": {},
                   "num_sponsoring": 0,
                   "num_sponsored": 0,
                   "paging_token": "{{accountId}}"
                 }
                 """;
    }

    public static OperationResult.OperationResultTr CreatePaymentResult(
        PaymentResultCode.PaymentResultCodeEnum code =
            PaymentResultCode.PaymentResultCodeEnum.PAYMENT_SUCCESS)
    {
        return new OperationResult.OperationResultTr
        {
            Discriminant = { InnerValue = OperationType.OperationTypeEnum.PAYMENT },
            PaymentResult = new PaymentResult
            {
                Discriminant = PaymentResultCode.Create(code),
            },
        };
    }

    public static OperationResult.OperationResultTr CreateBeginSponsoringResult()
    {
        return new OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = OperationType.OperationTypeEnum.BEGIN_SPONSORING_FUTURE_RESERVES,
            },
            BeginSponsoringFutureReservesResult = new BeginSponsoringFutureReservesResult
            {
                Discriminant = BeginSponsoringFutureReservesResultCode.Create(
                    BeginSponsoringFutureReservesResultCode.BeginSponsoringFutureReservesResultCodeEnum
                        .BEGIN_SPONSORING_FUTURE_RESERVES_SUCCESS),
            },
        };
    }

    public static OperationResult.OperationResultTr CreateEndSponsoringResult()
    {
        return new OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = OperationType.OperationTypeEnum.END_SPONSORING_FUTURE_RESERVES,
            },
            EndSponsoringFutureReservesResult = new EndSponsoringFutureReservesResult
            {
                Discriminant = EndSponsoringFutureReservesResultCode.Create(
                    EndSponsoringFutureReservesResultCode.EndSponsoringFutureReservesResultCodeEnum
                        .END_SPONSORING_FUTURE_RESERVES_SUCCESS),
            },
        };
    }

    public static OperationResult.OperationResultTr CreateRevokeSponsorshipResult()
    {
        return new OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = OperationType.OperationTypeEnum.REVOKE_SPONSORSHIP,
            },
            RevokeSponsorshipResult = new RevokeSponsorshipResult
            {
                Discriminant = RevokeSponsorshipResultCode.Create(
                    RevokeSponsorshipResultCode.RevokeSponsorshipResultCodeEnum.REVOKE_SPONSORSHIP_SUCCESS),
            },
        };
    }

    public static OperationResult.OperationResultTr CreateChangeTrustResult()
    {
        return new OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = OperationType.OperationTypeEnum.CHANGE_TRUST,
            },
            ChangeTrustResult = new ChangeTrustResult
            {
                Discriminant = ChangeTrustResultCode.Create(
                    ChangeTrustResultCode.ChangeTrustResultCodeEnum.CHANGE_TRUST_SUCCESS),
            },
        };
    }

    public static OperationResult.OperationResultTr CreateManageDataResult()
    {
        return new OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = OperationType.OperationTypeEnum.MANAGE_DATA,
            },
            ManageDataResult = new ManageDataResult
            {
                Discriminant = ManageDataResultCode.Create(
                    ManageDataResultCode.ManageDataResultCodeEnum.MANAGE_DATA_SUCCESS),
            },
        };
    }

    public static OperationResult.OperationResultTr CreateClaimableBalanceResult(byte[]? balanceIdHash = null)
    {
        return new OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = OperationType.OperationTypeEnum.CREATE_CLAIMABLE_BALANCE,
            },
            CreateClaimableBalanceResult = new CreateClaimableBalanceResult
            {
                Discriminant = CreateClaimableBalanceResultCode.Create(
                    CreateClaimableBalanceResultCode.CreateClaimableBalanceResultCodeEnum
                        .CREATE_CLAIMABLE_BALANCE_SUCCESS),
                BalanceID = new ClaimableBalanceID
                {
                    Discriminant =
                    {
                        InnerValue = ClaimableBalanceIDType.ClaimableBalanceIDTypeEnum
                            .CLAIMABLE_BALANCE_ID_TYPE_V0,
                    },
                    V0 = new Hash { InnerValue = balanceIdHash ?? new byte[32] },
                },
            },
        };
    }

    public static OperationResult.OperationResultTr CreateManageSellOfferCreatedResult(
        long offerId,
        string sellerAccountId)
    {
        var sellerKeyPair = KeyPair.FromAccountId(sellerAccountId);
        return new OperationResult.OperationResultTr
        {
            Discriminant =
            {
                InnerValue = OperationType.OperationTypeEnum.MANAGE_SELL_OFFER,
            },
            ManageSellOfferResult = new ManageSellOfferResult
            {
                Discriminant = ManageSellOfferResultCode.Create(
                    ManageSellOfferResultCode.ManageSellOfferResultCodeEnum.MANAGE_SELL_OFFER_SUCCESS),
                Success = new ManageOfferSuccessResult
                {
                    OffersClaimed = Array.Empty<StellarDotnetSdk.Xdr.ClaimAtom>(),
                    Offer = new ManageOfferSuccessResult.ManageOfferSuccessResultOffer
                    {
                        Discriminant =
                        {
                            InnerValue = ManageOfferEffect.ManageOfferEffectEnum.MANAGE_OFFER_CREATED,
                        },
                        Offer = new OfferEntry
                        {
                            SellerID = new AccountID(sellerKeyPair.XdrPublicKey),
                            OfferID = new Int64(offerId),
                            Selling = new Asset
                            {
                                Discriminant =
                                {
                                    InnerValue = AssetType.AssetTypeEnum.ASSET_TYPE_NATIVE,
                                },
                            },
                            Buying = new Asset
                            {
                                Discriminant =
                                {
                                    InnerValue = AssetType.AssetTypeEnum.ASSET_TYPE_NATIVE,
                                },
                            },
                            Amount = new Int64(10000000L),
                            Price = new StellarDotnetSdk.Xdr.Price
                            {
                                N = new Int32(3),
                                D = new Int32(2),
                            },
                            Flags = new Uint32(0),
                            Ext = new OfferEntry.OfferEntryExt { Discriminant = 0 },
                        },
                    },
                },
            },
        };
    }

    public static HttpResponseMessage BuildHttpResponse(
        string content,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var response = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(content),
        };
        response.Headers.Add("X-Ratelimit-Limit", "-1");
        response.Headers.Add("X-Ratelimit-Remaining", "-1");
        response.Headers.Add("X-Ratelimit-Reset", "-1");
        return response;
    }

    public static Server CreateTestServerWithResponses(params HttpResponseMessage[] responses)
    {
        Network.UseTestNetwork();
        var mockHandler = new Mock<FakeHttpMessageHandler> { CallBase = true };
        var httpClient = new HttpClient(mockHandler.Object);
        var setup = mockHandler.SetupSequence(a => a.Send(It.IsAny<HttpRequestMessage>()));
        foreach (var response in responses)
        {
            setup = setup.Returns(response);
        }

        var server = new Server("https://horizon-testnet.stellar.org", httpClient);
        return server;
    }

    public static async Task CheckAndCreateAccountOnTestnet(string accountId)
    {
        using Server server = new("https://horizon-testnet.stellar.org");
        try
        {
            await server.Accounts.Account(accountId);
        }
        catch (HttpResponseException)
        {
            bool isSuccess;
            do
            {
                var fundResponse = await server.TestNetFriendBot.FundAccount(accountId).Execute();

                // Check if the account already exists (this is considered success for our purpose)
                var alreadyExists = fundResponse.Extras?.ExtrasResultCodes?.OperationsResultCodes
                    ?.Any(code => code == "op_already_exists") == true;

                if (alreadyExists)
                {
                    isSuccess = true;
                }
                else if (fundResponse.ResultXdr != null)
                {
                    var result = TransactionResult.FromXdrBase64(fundResponse.ResultXdr);
                    isSuccess = result.IsSuccess && result is TransactionResultSuccess;
                }
                else
                {
                    // No ResultXdr and not already_exists - retry
                    isSuccess = false;
                }
            } while (!isSuccess);
        }
    }

    public static Server CreateTestServerWithContent(
        string? content,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string uri = "https://horizon-testnet.stellar.org")
    {
        Network.UseTestNetwork();
        var httpClient = CreateFakeHttpClient(content, statusCode);
        return new Server(uri, httpClient);
    }

    public static StellarRpcServer CreateTestStellarRpcServerWithContent(
        string? content,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string uri = "https://soroban-testnet.stellar.org")
    {
        Network.UseTestNetwork();
        var httpClient = CreateFakeHttpClient(content, statusCode);
        return new StellarRpcServer(uri, httpClient);
    }

    public static Server CreateTestServerWithHeaders(
        Dictionary<string, IEnumerable<string>> headers,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string uri = "https://horizon-testnet.stellar.org")
    {
        Network.UseTestNetwork();
        var httpClient = CreateFakeHttpClient("", statusCode, headers);
        return new Server(uri, httpClient);
    }

    public static async Task<Server> CreateTestServerWithJson(
        string pathToJson,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string uri = "https://horizon-testnet.stellar.org")
    {
        var jsonPath = GetTestDataPath(pathToJson);
        var content = await File.ReadAllTextAsync(jsonPath);
        return CreateTestServerWithContent(content, statusCode, uri);
    }

    public static HttpClient CreateFakeHttpClient(
        string? content,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        IDictionary<string, IEnumerable<string>>? headers = null)
    {
        var mockFakeHttpMessageHandler = new Mock<FakeHttpMessageHandler> { CallBase = true };
        var httpClient = new HttpClient(mockFakeHttpMessageHandler.Object);

        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = content != null ? new StringContent(content) : null,
        };

        httpResponseMessage.Headers.Add("X-Ratelimit-Limit", "-1");
        httpResponseMessage.Headers.Add("X-Ratelimit-Remaining", "-1");
        httpResponseMessage.Headers.Add("X-Ratelimit-Reset", "-1");
        if (headers != null)
        {
            foreach (var header in headers)
            {
                httpResponseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        mockFakeHttpMessageHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>())).Returns(httpResponseMessage);

        return httpClient;
    }

    public abstract class FakeHttpMessageHandler : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        public virtual HttpResponseMessage Send(HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            return await Task.FromResult(Send(request));
        }
    }
}