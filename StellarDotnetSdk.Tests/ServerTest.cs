using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Federation;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Responses;
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace StellarDotnetSdk.Tests;

/// <summary>
/// Unit tests for <see cref="Server"/> class.
/// </summary>
[TestClass]
public class ServerTest
{
    private const HttpStatusCode HttpBadRequest = HttpStatusCode.BadRequest;
    private const string ServerFailureJsonPath = "Responses/serverFailure.json";
    private const string ServerSuccessJsonPath = "Responses/serverSuccess.json";

    private static Transaction BuildTransaction()
    {
        var source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
        var destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

        var account = new Account(source.AccountId, 2908908335136768L);
        var builder = new TransactionBuilder(account)
            .AddOperation(new CreateAccountOperation(destination, "2000"))
            .AddMemo(Memo.Text("Hello world!"));

        Assert.AreEqual(1, builder.OperationsCount);
        var transaction = builder.Build();
        Assert.AreEqual(2908908335136769L, transaction.SequenceNumber);
        Assert.AreEqual(2908908335136769L, account.SequenceNumber);
        transaction.Sign(source);

        return transaction;
    }

    private static FeeBumpTransaction BuildFeeBumpTransaction()
    {
        var source = KeyPair.FromSecretSeed("SB7ZMPZB3YMMK5CUWENXVLZWBK4KYX4YU5JBXQNZSK2DP2Q7V3LVTO5V");
        var innerTx = BuildTransaction();
        var feeSource = KeyPair.FromAccountId("GD7HCWFO77E76G6BKJLRHRFRLE6I7BMPJQZQKGNYTT3SPE6BA4DHJAQY");

        var tx = TransactionBuilder.BuildFeeBumpTransaction(feeSource, innerTx, 200);
        tx.Sign(source);
        return tx;
    }

    /// <summary>
    /// Verifies that Server.SubmitTransaction returns success response when transaction is valid.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithValidTransaction_ReturnsSuccessResponse()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson("Responses/serverSuccess.json");

        // Act
        var response = await server.SubmitTransaction(
            BuildTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = true });

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(response.Ledger, 826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    /// <summary>
    /// Verifies that Server includes default client headers in HTTP requests.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithDefaultHeaders_IncludesClientHeaders()
    {
        // Arrange
        Network.UseTestNetwork();
        var messageHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        var httpClient = Server.CreateHttpClient(messageHandler.Object);
        var server = new Server("https://horizon.stellar.org", httpClient);

        var jsonPath = Utils.GetTestDataPath("Responses/serverSuccess.json");

        var json = await File.ReadAllTextAsync(jsonPath);
        var clientName = "";
        var clientVersion = "";

        messageHandler
            .Setup(h => h.Send(It.IsAny<HttpRequestMessage>()))
            .Callback<HttpRequestMessage>(msg =>
            {
                clientName = msg.Headers.GetValues("X-Client-Name").FirstOrDefault();
                clientVersion = msg.Headers.GetValues("X-Client-Version").FirstOrDefault();
            })
            .Returns(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(json) });

        // Act
        var response = await server.SubmitTransaction(
            BuildTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = true });

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual("StellarDotnetSdk", clientName);
        Assert.IsFalse(string.IsNullOrWhiteSpace(clientVersion));
        var result = response.Result;
        Assert.IsInstanceOfType(result, typeof(TransactionResultSuccess));
        Assert.AreEqual("0.00001", result.FeeCharged);
    }

    /// <summary>
    /// Verifies that Server.SubmitTransaction returns failure response when transaction is invalid.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithInvalidTransaction_ReturnsFailureResponse()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson(ServerFailureJsonPath, HttpBadRequest);

        // Act
        var response = await server.SubmitTransaction(
            BuildTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = true });

        // Assert
        Assert.IsNotNull(response);
        Assert.IsFalse(response.IsSuccess);
        Assert.IsNull(response.Ledger);
        Assert.IsNull(response.Hash);
        Assert.AreEqual(response.SubmitTransactionResponseExtras.EnvelopeXdr,
            "AAAAAK4Pg4OEkjGmSN0AN37K/dcKyKPT2DC90xvjjawKp136AAAAZAAKsZQAAAABAAAAAAAAAAEAAAAJSmF2YSBGVFchAAAAAAAAAQAAAAAAAAABAAAAAG9wfBI7rRYoBlX3qRa0KOnI75W5BaPU6NbyKmm2t71MAAAAAAAAAAABMS0AAAAAAAAAAAEKp136AAAAQOWEjL+Sm+WP2puE9dLIxWlOibIEOz8PsXyG77jOCVdHZfQvkgB49Mu5wqKCMWWIsDSLFekwUsLaunvmXrpyBwQ=");
        Assert.AreEqual(response.SubmitTransactionResponseExtras.ResultXdr,
            "AAAAAAAAAGT/////AAAAAQAAAAAAAAAB////+wAAAAA=");
        Assert.IsNotNull(response.SubmitTransactionResponseExtras);
        Assert.AreEqual("tx_failed", response.SubmitTransactionResponseExtras.ExtrasResultCodes.TransactionResultCode);
        Assert.AreEqual("op_no_destination",
            response.SubmitTransactionResponseExtras.ExtrasResultCodes.OperationsResultCodes[0]);

        var result = response.Result;
        Assert.IsInstanceOfType(result, typeof(TransactionResultFailed));
        Assert.AreEqual("0.00001", result.FeeCharged);
        Assert.AreEqual(1, ((TransactionResultFailed)result).Results.Count);
    }

    /// <summary>
    /// Verifies that Server.SubmitTransaction with EnsureSuccess option returns success response when transaction succeeds.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithEnsureSuccessAndValidTransaction_ReturnsSuccessResponse()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson(ServerSuccessJsonPath);

        // Act
        var response = await server.SubmitTransaction(
            BuildTransaction().ToEnvelopeXdrBase64(), new SubmitTransactionOptions { EnsureSuccess = true });

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(response.Ledger, 826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    /// <summary>
    /// Verifies that Server.SubmitTransaction with EnsureSuccess option throws ConnectionErrorException when transaction fails.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithEnsureSuccessAndFailedTransaction_ThrowsConnectionErrorException()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson(ServerFailureJsonPath, HttpBadRequest);

        // Act & Assert
        var ex = await Assert.ThrowsExceptionAsync<ConnectionErrorException>(async () =>
        {
            await server.SubmitTransaction(BuildTransaction(),
                new SubmitTransactionOptions { EnsureSuccess = true });
        });

        Assert.IsTrue(ex.Message.Contains("Status code (BadRequest) is not success."));
    }

    /// <summary>
    /// Verifies that Server.SubmitTransaction with EnsureSuccess option throws ConnectionErrorException when response has empty content.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithEnsureSuccessAndEmptyContent_ThrowsConnectionErrorException()
    {
        // Arrange
        using var server = Utils.CreateTestServerWithContent("", HttpBadRequest);

        // Act & Assert
        var ex = await Assert.ThrowsExceptionAsync<ConnectionErrorException>(async () =>
        {
            await server.SubmitTransaction(BuildTransaction(),
                new SubmitTransactionOptions { EnsureSuccess = true });
        });

        Assert.AreEqual(ex.Message, "Status code (BadRequest) is not success.");
    }

    /// <summary>
    /// Verifies that Server.SubmitTransaction with EnsureSuccess option throws ConnectionErrorException when response has null content.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithEnsureSuccessAndNullContent_ThrowsConnectionErrorException()
    {
        // Arrange
        using var server = Utils.CreateTestServerWithContent(null, HttpBadRequest);

        // Act & Assert
        var ex = await Assert.ThrowsExceptionAsync<ConnectionErrorException>(async () =>
        {
            await server.SubmitTransaction(BuildTransaction(),
                new SubmitTransactionOptions { EnsureSuccess = true });
        });

        Assert.AreEqual(ex.Message, "Status code (BadRequest) is not success.");
    }

    /// <summary>
    /// Verifies that Server.SubmitTransaction with SkipMemoRequiredCheck set to false returns success response.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithSkipMemoRequiredCheckFalse_ReturnsSuccessResponse()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson(ServerSuccessJsonPath);

        // Act
        var response = await server.SubmitTransaction(
            BuildTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = false });

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(response.Ledger, 826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    /// <summary>
    /// Verifies that Server.SubmitTransaction with envelope XDR base64 string returns success response.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithEnvelopeXdrBase64_ReturnsSuccessResponse()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson(ServerSuccessJsonPath);

        // Act
        var response = await server.SubmitTransaction(
            BuildTransaction().ToEnvelopeXdrBase64(), new SubmitTransactionOptions { SkipMemoRequiredCheck = false });

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(response.Ledger, 826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    /// <summary>
    /// Verifies that Server.SubmitTransaction with fee bump transaction envelope XDR base64 returns success response.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithFeeBumpTransactionEnvelopeXdrBase64_ReturnsSuccessResponse()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson(ServerSuccessJsonPath);

        // Act
        var response = await server.SubmitTransaction(
            BuildFeeBumpTransaction().ToEnvelopeXdrBase64(),
            new SubmitTransactionOptions { SkipMemoRequiredCheck = false, FeeBumpTransaction = true });

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(response.Ledger, 826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    /// <summary>
    /// Verifies that Server.SubmitTransaction with fee bump transaction without options returns success response.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithFeeBumpTransactionWithoutOptions_ReturnsSuccessResponse()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson(ServerSuccessJsonPath);

        // Act
        var response = await server.SubmitTransaction(BuildFeeBumpTransaction());

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(response.Ledger, 826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    /// <summary>
    /// Verifies that Server.SubmitTransaction with fee bump transaction and options returns success response.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithFeeBumpTransactionAndOptions_ReturnsSuccessResponse()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson(ServerSuccessJsonPath);

        // Act
        var response = await server.SubmitTransaction(
            BuildFeeBumpTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = false });

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(response.Ledger, 826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    /// <summary>
    /// Verifies that Server.SubmitTransaction without options returns success response.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithoutOptions_ReturnsSuccessResponse()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson(ServerSuccessJsonPath);

        // Act
        var response = await server.SubmitTransaction(BuildTransaction());

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(response.Ledger, 826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    /// <summary>
    /// Verifies that Server.SubmitTransaction with envelope XDR base64 without options returns success response.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithEnvelopeXdrBase64WithoutOptions_ReturnsSuccessResponse()
    {
        // Arrange
        using var server = await Utils.CreateTestServerWithJson(ServerSuccessJsonPath);

        // Act
        var response = await server.SubmitTransaction(
            BuildTransaction().ToEnvelopeXdrBase64());

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(response.Ledger, 826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    /// <summary>
    /// Verifies that Transaction.FromEnvelopeXdr correctly deserializes transaction with InvokeContractOperation.
    /// </summary>
    [TestMethod]
    public void FromEnvelopeXdr_WithInvokeContractOperation_DeserializesCorrectly()
    {
        // Arrange
        const string xdrBase64 =
            "AAAAAgAAAACPMSyy9mKtfCdjSZ5M9UAOGywn8EzskQZ/LFn+t7f31QAAdLYAK1faAAAABwAAAAAAAAAAAAAAAQAAAAAAAAAYAAAAAAAAAAH+ApqHuMOZcvSEr0QsaWWqzWWy/O/iCwZB9QY7+KjlwQAAAAZzdWJtaXQAAAAAAAEAAAAPAAAACmRhbmNpblJhcGgAAAAAAAAAAAABAAAAAAAAAAIAAAAGAAAAAf4Cmoe4w5ly9ISvRCxpZarNZbL87+ILBkH1Bjv4qOXBAAAAFAAAAAEAAAAHvyeT3bL2fs8RgEztJaliJzxp5kxwYfGR7ftoXA7sMUIAAAAAABumWQAABGwAAAAAAAAAAAAAAAMAAAABt7f31QAAAECV4sHbkhrlj5k0y3Peu+WnWUyhv2voubC/LxGkbP/HvVRdWifEYuoWw/MXg7p7kDmo3IBvEffR85YULwKy284D";

        // Act
        var tx = Transaction.FromEnvelopeXdr(xdrBase64);
        var invokeContractOperation = (InvokeContractOperation)tx.Operations[0];
        var hostFunction = invokeContractOperation.HostFunction;
        var sourceAccount = tx.SourceAccount;

        // Assert
        Assert.AreEqual(sourceAccount.AccountId, "GCHTCLFS6ZRK27BHMNEZ4THVIAHBWLBH6BGOZEIGP4WFT7VXW735KJW2");
        Assert.AreEqual(((ScContractId)hostFunction.ContractAddress).InnerValue,
            StrKey.EncodeContractId(
                Convert.FromHexString("FE029A87B8C39972F484AF442C6965AACD65B2FCEFE20B0641F5063BF8A8E5C1")));
        Assert.AreEqual(hostFunction.FunctionName.InnerValue, "submit");
        Assert.AreEqual(((SCSymbol)hostFunction.Args[0]).InnerValue, "dancinRaph");
        Assert.AreEqual(invokeContractOperation.Auth.Length, 0);
    }


    /// <summary>
    /// Verifies that Server.SubmitTransaction throws TooManyRequestsException with RetryAfter when server returns 429 with integer Retry-After header.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithTooManyRequestsAndIntegerRetryAfter_ThrowsTooManyRequestsExceptionWithRetryAfter()
    {
        // Arrange
        var server = Utils.CreateTestServerWithHeaders(
            new Dictionary<string, IEnumerable<string>>
            {
                { "Retry-After", new[] { "10" } },
            },
            HttpStatusCode.TooManyRequests);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<TooManyRequestsException>(() => server.SubmitTransaction(
            BuildTransaction(),
            new SubmitTransactionOptions { SkipMemoRequiredCheck = true }));

        Assert.AreEqual(10, exception.RetryAfter);
    }

    /// <summary>
    /// Verifies that Server.SubmitTransaction throws TooManyRequestsException with RetryAfter when server returns 429 with DateTime Retry-After header.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithTooManyRequestsAndDateTimeRetryAfter_ThrowsTooManyRequestsExceptionWithRetryAfter()
    {
        // Arrange
        var server = Utils.CreateTestServerWithHeaders(
            new Dictionary<string, IEnumerable<string>>
            {
                { "Retry-After", new[] { JsonSerializer.Serialize(DateTime.UtcNow.AddSeconds(10)).Trim('"') } },
            },
            HttpStatusCode.TooManyRequests);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<TooManyRequestsException>(() => server.SubmitTransaction(
            BuildTransaction(),
            new SubmitTransactionOptions { SkipMemoRequiredCheck = true }
        ));

        Assert.IsTrue(exception.RetryAfter is >= 7 and <= 10, "The RetryAfter value is outside the expected range.");
    }

    /// <summary>
    /// Verifies that Server.SubmitTransaction throws ServiceUnavailableException with RetryAfter when server returns 503 with integer Retry-After header.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithServiceUnavailableAndIntegerRetryAfter_ThrowsServiceUnavailableExceptionWithRetryAfter()
    {
        // Arrange
        var server = Utils.CreateTestServerWithHeaders(
            new Dictionary<string, IEnumerable<string>>
            {
                { "Retry-After", new[] { "10" } },
            },
            HttpStatusCode.ServiceUnavailable);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<ServiceUnavailableException>(() => server.SubmitTransaction(
            BuildTransaction(),
            new SubmitTransactionOptions { SkipMemoRequiredCheck = true }));

        Assert.AreEqual(10, exception.RetryAfter);
    }

    /// <summary>
    /// Verifies that Server.SubmitTransaction throws ServiceUnavailableException with RetryAfter when server returns 503 with DateTime Retry-After header.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransaction_WithServiceUnavailableAndDateTimeRetryAfter_ThrowsServiceUnavailableExceptionWithRetryAfter()
    {
        // Arrange
        var server = Utils.CreateTestServerWithHeaders(
            new Dictionary<string, IEnumerable<string>>
            {
                { "Retry-After", new[] { JsonSerializer.Serialize(DateTime.UtcNow.AddSeconds(10)).Trim('"') } },
            },
            HttpStatusCode.ServiceUnavailable);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<ServiceUnavailableException>(() => server.SubmitTransaction(
            BuildTransaction(),
            new SubmitTransactionOptions { SkipMemoRequiredCheck = true }));

        Assert.IsTrue(exception.RetryAfter is >= 7 and <= 10, "The RetryAfter value is outside the expected range.");
    }

    /// <summary>
    /// Verifies that Server.SubmitTransactionAsync returns response with PENDING status when transaction is pending.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransactionAsync_WithPendingTransaction_ReturnsPendingResponse()
    {
        // Arrange
        const string json =
            """
            {
              "tx_status": "PENDING",
              "hash": "7a9c84f5b6e3d2c1a8f7e6d5c4b3a2918d7c6b5a4f3e2d1c9b8a7f6e5d4c3b21"
            }
            """;
        using var server = Utils.CreateTestServerWithContent(json, HttpStatusCode.Created);

        // Act
        var response = await server.SubmitTransactionAsync(
            BuildTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = true });

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(SubmitTransactionAsyncResponse.TransactionStatus.PENDING, response.TxStatus);
        Assert.AreEqual(response.Hash, "7a9c84f5b6e3d2c1a8f7e6d5c4b3a2918d7c6b5a4f3e2d1c9b8a7f6e5d4c3b21");
    }

    /// <summary>
    /// Verifies that Server.SubmitTransactionAsync returns response with DUPLICATE status when transaction is duplicate.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransactionAsync_WithDuplicateTransaction_ReturnsDuplicateResponse()
    {
        // Arrange
        const string json =
            """
            {
              "tx_status": "DUPLICATE",
              "hash": "ded59eecb33c1c36c05e681744b923377e2358af4b6f66b7471753379fb049ac"
            }
            """;
        using var server = Utils.CreateTestServerWithContent(json);

        // Act
        var response = await server.SubmitTransactionAsync(
            BuildTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = true });

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(SubmitTransactionAsyncResponse.TransactionStatus.DUPLICATE, response.TxStatus);
        Assert.AreEqual(response.Hash, "ded59eecb33c1c36c05e681744b923377e2358af4b6f66b7471753379fb049ac");
    }

    /// <summary>
    /// Verifies that Server.SubmitTransactionAsync returns response with TRY_AGAIN_LATER status when server requests retry.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransactionAsync_WithTryAgainLaterStatus_ReturnsTryAgainLaterResponse()
    {
        // Arrange
        const string json =
            """
            {
              "tx_status": "TRY_AGAIN_LATER",
              "hash": "bf170746f990a53e73dc249a75028cbacadf7248e080e3e30b5c9b8a8e397894"
            }
            """;
        using var server = Utils.CreateTestServerWithContent(json);

        // Act
        var response = await server.SubmitTransactionAsync(
            BuildTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = true });

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(SubmitTransactionAsyncResponse.TransactionStatus.TRY_AGAIN_LATER, response.TxStatus);
        Assert.AreEqual(response.Hash, "bf170746f990a53e73dc249a75028cbacadf7248e080e3e30b5c9b8a8e397894");
    }

    /// <summary>
    /// Verifies that Server.SubmitTransactionAsync returns response with ERROR status and error result XDR for protocol prior to 22.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransactionAsync_WithErrorStatusPriorToProtocol22_ReturnsErrorResponseWithErrorResult()
    {
        // Arrange
        const string json =
            """
            {
              "tx_status": "ERROR",
              "hash": "9f8e7d6c5b4a3210fedcba9876543210abcdef0123456789abcdef0123456789",
              "error_result_xdr": "AAAAAAAAAGT////7AAAAAA=="
            }
            """;
        using var server = Utils.CreateTestServerWithContent(json);

        // Act
        var response = await server.SubmitTransactionAsync(
            BuildTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = true });

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(SubmitTransactionAsyncResponse.TransactionStatus.ERROR, response.TxStatus);
        Assert.AreEqual(response.Hash, "9f8e7d6c5b4a3210fedcba9876543210abcdef0123456789abcdef0123456789");
        Assert.IsNotNull(response.ErrorResult);
    }

    /// <summary>
    /// Verifies that Server.SubmitTransactionAsync returns response with ERROR status and error result XDR for protocol 22.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransactionAsync_WithErrorStatusProtocol22_ReturnsErrorResponseWithErrorResult()
    {
        // Arrange
        const string json =
            """
            {
              "tx_status": "ERROR",
              "hash": "9f8e7d6c5b4a3210fedcba9876543210abcdef0123456789abcdef0123456789",
              "errorResultXdr": "AAAAAAAAAGT////7AAAAAA==",
              "error_result_xdr": "AAAAAAAAAGT////7AAAAAA=="
            }
            """;
        using var server = Utils.CreateTestServerWithContent(json);

        // Act
        var response = await server.SubmitTransactionAsync(
            BuildTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = true });

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(SubmitTransactionAsyncResponse.TransactionStatus.ERROR, response.TxStatus);
        Assert.AreEqual(response.Hash, "9f8e7d6c5b4a3210fedcba9876543210abcdef0123456789abcdef0123456789");
        Assert.IsNotNull(response.ErrorResult);
    }

    /// <summary>
    /// Verifies that Server.SubmitTransactionAsync without SkipMemoRequiredCheck returns response with PENDING status.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransactionAsync_WithoutSkipMemoRequiredCheck_ReturnsPendingResponse()
    {
        // Arrange
        const string json =
            """
            {
              "tx_status": "PENDING",
              "hash": "7a9c84f5b6e3d2c1a8f7e6d5c4b3a2918d7c6b5a4f3e2d1c9b8a7f6e5d4c3b22"
            }
            """;
        using var server = Utils.CreateTestServerWithContent(json);

        // Act
        var response = await server.SubmitTransactionAsync(BuildTransaction());

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(SubmitTransactionAsyncResponse.TransactionStatus.PENDING, response.TxStatus);
        Assert.AreEqual(response.Hash, "7a9c84f5b6e3d2c1a8f7e6d5c4b3a2918d7c6b5a4f3e2d1c9b8a7f6e5d4c3b22");
    }

    /// <summary>
    /// Verifies that Server.SubmitTransactionAsync with envelope XDR base64 returns response with PENDING status.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransactionAsync_WithEnvelopeXdrBase64_ReturnsPendingResponse()
    {
        // Arrange
        const string json =
            """
            {
              "tx_status": "PENDING",
              "hash": "7a9c84f5b6e3d2c1a8f7e6d5c4b3a2918d7c6b5a4f3e2d1c9b8a7f6e5d4c3b23"
            }
            """;
        using var server = Utils.CreateTestServerWithContent(json);

        // Act
        var response = await server.SubmitTransactionAsync(
            "AAAAAgAAAADe1PMFZDEm2ZIvr5IO8uM4QU4HZW4USgDlPjIeJqY2QwAAAGQAAGFLAAAABQAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAQAAAAJPSwAAAAAAAQAAAAAAAAABAAAAALtJgdGXASRLp/M5ZpckEa10nJPtYvrgX6M5wTPacDUYAAAAAAAAAAAAmJaAAAAAAAAAAAEmpjZDAAAAQBYm/5Z7kfwwt9HvOamKuF50118xXu3tKl49yUjHBZ5GVKwlvXJf4HajFZH1XaKXlhQl9YDBdIlPKHEa4PQ+RQI=\n");

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(SubmitTransactionAsyncResponse.TransactionStatus.PENDING, response.TxStatus);
        Assert.AreEqual(response.Hash, "7a9c84f5b6e3d2c1a8f7e6d5c4b3a2918d7c6b5a4f3e2d1c9b8a7f6e5d4c3b23");
    }

    /// <summary>
    /// Verifies that Server.SubmitTransactionAsync with fee bump transaction returns response with PENDING status.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransactionAsync_WithFeeBumpTransaction_ReturnsPendingResponse()
    {
        // Arrange
        const string json =
            """
            {
              "tx_status": "PENDING",
              "hash": "7a9c84f5b6e3d2c1a8f7e6d5c4b3a2918d7c6b5a4f3e2d1c9b8a7f6e5d4c3b24"
            }
            """;
        using var server = Utils.CreateTestServerWithContent(json);

        // Act
        var response = await server.SubmitTransactionAsync(BuildFeeBumpTransaction());

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(SubmitTransactionAsyncResponse.TransactionStatus.PENDING, response.TxStatus);
        Assert.AreEqual(response.Hash, "7a9c84f5b6e3d2c1a8f7e6d5c4b3a2918d7c6b5a4f3e2d1c9b8a7f6e5d4c3b24");
    }

    /// <summary>
    /// Verifies that Server.SubmitTransactionAsync with fee bump transaction and EnsureSuccess option returns response with PENDING status.
    /// </summary>
    [TestMethod]
    public async Task SubmitTransactionAsync_WithFeeBumpTransactionAndEnsureSuccess_ReturnsPendingResponse()
    {
        // Arrange
        const string json =
            """
            {
              "tx_status": "PENDING",
              "hash": "7a9c84f5b6e3d2c1a8f7e6d5c4b3a2918d7c6b5a4f3e2d1c9b8a7f6e5d4c3b25"
            }
            """;
        using var server = Utils.CreateTestServerWithContent(json);

        // Act
        var response = await server.SubmitTransactionAsync(
            BuildFeeBumpTransaction(),
            new SubmitTransactionOptions { EnsureSuccess = true }
        );

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(SubmitTransactionAsyncResponse.TransactionStatus.PENDING, response.TxStatus);
        Assert.AreEqual(response.Hash, "7a9c84f5b6e3d2c1a8f7e6d5c4b3a2918d7c6b5a4f3e2d1c9b8a7f6e5d4c3b25");
    }

    [TestMethod]
    public void Constructor_WithHttpResilienceOptions_CreatesServerWithResilience()
    {
        // Arrange
        var resilienceOptions = new HttpResilienceOptions
        {
            MaxRetryCount = 3,
            BaseDelay = TimeSpan.FromMilliseconds(200),
            MaxDelay = TimeSpan.FromSeconds(5),
        };

        // Act
        using var server = new Server("https://horizon-testnet.stellar.org", resilienceOptions, null);

        // Assert - Server should be created successfully
        Assert.IsNotNull(server);
    }

    [TestMethod]
    public void Constructor_WithBearerTokenAndHttpResilienceOptions_CreatesServerWithBoth()
    {
        // Arrange
        var resilienceOptions = new HttpResilienceOptions
        {
            MaxRetryCount = 5,
            BaseDelay = TimeSpan.FromMilliseconds(500),
            MaxDelay = TimeSpan.FromSeconds(15),
        };

        // Act
        using var server = new Server("https://horizon-testnet.stellar.org", resilienceOptions, "test-token");

        // Assert - Server should be created successfully
        Assert.IsNotNull(server);
    }

    [TestMethod]
    public void Constructor_WithNullHttpResilienceOptions_CreatesServerWithDefaultResilience()
    {
        // Act
        using var server = new Server("https://horizon-testnet.stellar.org", null, null);

        // Assert - Server should be created successfully with default resilience (no retries)
        Assert.IsNotNull(server);
    }

    [TestMethod]
    public void Constructor_WithHttpResilienceOptions_UsesResilienceForRequests()
    {
        // Arrange
        var resilienceOptions = new HttpResilienceOptions
        {
            MaxRetryCount = 1,
            BaseDelay = TimeSpan.FromMilliseconds(10),
            UseJitter = false,
        };

        // Act
        using var server = new Server("https://horizon-testnet.stellar.org", resilienceOptions, null);
        // Note: This test verifies the constructor accepts the parameter
        // Actual resilience behavior is tested in DefaultStellarSdkHttpClientTests

        // Assert - Server should be created successfully
        Assert.IsNotNull(server);
    }
}