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
using StellarDotnetSdk.Responses.Results;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Transactions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace StellarDotnetSdk.Tests;

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

    [TestMethod]
    public async Task TestSubmitTransactionSuccess()
    {
        using var server = await Utils.CreateTestServerWithJson("Responses/serverSuccess.json");
        var response = await server.SubmitTransaction(
            BuildTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = true });
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(response.Ledger, (uint)826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    [TestMethod]
    public async Task TestDefaultClientHeaders()
    {
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

        var response = await server.SubmitTransaction(
            BuildTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = true });
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual("StellarDotnetSdk", clientName);
        Assert.IsFalse(string.IsNullOrWhiteSpace(clientVersion));
        var result = response.Result;
        Assert.IsInstanceOfType(result, typeof(TransactionResultSuccess));
        Assert.AreEqual("0.00001", result.FeeCharged);
    }

    [TestMethod]
    public async Task TestSubmitTransactionFail()
    {
        using var server = await Utils.CreateTestServerWithJson(ServerFailureJsonPath, HttpBadRequest);

        var response = await server.SubmitTransaction(
            BuildTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = true });
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

    [TestMethod]
    public async Task TestSubmitTransactionEnsureSuccess()
    {
        using var server = await Utils.CreateTestServerWithJson(ServerSuccessJsonPath);

        var response = await server.SubmitTransaction(
            BuildTransaction().ToEnvelopeXdrBase64(), new SubmitTransactionOptions { EnsureSuccess = true });
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(response.Ledger, (uint)826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    [TestMethod]
    public async Task TestSubmitTransactionEnsureSuccessWithContent()
    {
        using var server = await Utils.CreateTestServerWithJson(ServerFailureJsonPath, HttpBadRequest);

        var ex = await Assert.ThrowsExceptionAsync<ConnectionErrorException>(async () =>
        {
            await server.SubmitTransaction(BuildTransaction(),
                new SubmitTransactionOptions { EnsureSuccess = true });
        });

        Assert.IsTrue(ex.Message.Contains("Status code (BadRequest) is not success."));
    }

    [TestMethod]
    public async Task TestSubmitTransactionEnsureSuccessWithEmptyContent()
    {
        using var server = Utils.CreateTestServerWithContent("", HttpBadRequest);
        var ex = await Assert.ThrowsExceptionAsync<ConnectionErrorException>(async () =>
        {
            await server.SubmitTransaction(BuildTransaction(),
                new SubmitTransactionOptions { EnsureSuccess = true });
        });

        Assert.AreEqual(ex.Message, "Status code (BadRequest) is not success.");
    }

    [TestMethod]
    public async Task TestSubmitTransactionEnsureSuccessWithNullContent()
    {
        using var server = Utils.CreateTestServerWithContent(null, HttpBadRequest);

        var ex = await Assert.ThrowsExceptionAsync<ConnectionErrorException>(async () =>
        {
            await server.SubmitTransaction(BuildTransaction(),
                new SubmitTransactionOptions { EnsureSuccess = true });
        });

        Assert.AreEqual(ex.Message, "Status code (BadRequest) is not success.");
    }

    [TestMethod]
    public async Task TestNoSkipMemoRequiredCheck()
    {
        using var server = await Utils.CreateTestServerWithJson(ServerSuccessJsonPath);

        var response = await server.SubmitTransaction(
            BuildTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = false });
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(response.Ledger, (uint)826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    [TestMethod]
    public async Task TestSubmitTransactionEnvelopeBase64()
    {
        using var server = await Utils.CreateTestServerWithJson(ServerSuccessJsonPath);

        var response = await server.SubmitTransaction(
            BuildTransaction().ToEnvelopeXdrBase64(), new SubmitTransactionOptions { SkipMemoRequiredCheck = false });
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(response.Ledger, (uint)826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    [TestMethod]
    public async Task TestSubmitFeeBumpTransactionEnvelopeBase64()
    {
        using var server = await Utils.CreateTestServerWithJson(ServerSuccessJsonPath);

        var response = await server.SubmitTransaction(
            BuildFeeBumpTransaction().ToEnvelopeXdrBase64(),
            new SubmitTransactionOptions { SkipMemoRequiredCheck = false, FeeBumpTransaction = true });
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(response.Ledger, (uint)826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    [TestMethod]
    public async Task TestSubmitFeeBumpTransactionWithoutOptions()
    {
        using var server = await Utils.CreateTestServerWithJson(ServerSuccessJsonPath);

        var response = await server.SubmitTransaction(BuildFeeBumpTransaction());
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(response.Ledger, (uint)826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    [TestMethod]
    public async Task TestSubmitFeeBumpTransactionWithOptions()
    {
        using var server = await Utils.CreateTestServerWithJson(ServerSuccessJsonPath);

        var response = await server.SubmitTransaction(
            BuildFeeBumpTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = false });
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(response.Ledger, (uint)826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    [TestMethod]
    public async Task TestSubmitTransactionWithoutOptions()
    {
        using var server = await Utils.CreateTestServerWithJson(ServerSuccessJsonPath);

        var response = await server.SubmitTransaction(BuildTransaction());
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(response.Ledger, (uint)826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    [TestMethod]
    public async Task TestSubmitTransactionEnvelopeBase64WithoutOptions()
    {
        using var server = await Utils.CreateTestServerWithJson(ServerSuccessJsonPath);

        var response = await server.SubmitTransaction(
            BuildTransaction().ToEnvelopeXdrBase64());
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(response.Ledger, (uint)826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    [TestMethod]
    public void TestSubmitInvokeContractTransaction()
    {
        const string xdrBase64 =
            "AAAAAgAAAACPMSyy9mKtfCdjSZ5M9UAOGywn8EzskQZ/LFn+t7f31QAAdLYAK1faAAAABwAAAAAAAAAAAAAAAQAAAAAAAAAYAAAAAAAAAAH+ApqHuMOZcvSEr0QsaWWqzWWy/O/iCwZB9QY7+KjlwQAAAAZzdWJtaXQAAAAAAAEAAAAPAAAACmRhbmNpblJhcGgAAAAAAAAAAAABAAAAAAAAAAIAAAAGAAAAAf4Cmoe4w5ly9ISvRCxpZarNZbL87+ILBkH1Bjv4qOXBAAAAFAAAAAEAAAAHvyeT3bL2fs8RgEztJaliJzxp5kxwYfGR7ftoXA7sMUIAAAAAABumWQAABGwAAAAAAAAAAAAAAAMAAAABt7f31QAAAECV4sHbkhrlj5k0y3Peu+WnWUyhv2voubC/LxGkbP/HvVRdWifEYuoWw/MXg7p7kDmo3IBvEffR85YULwKy284D";
        var tx = Transaction.FromEnvelopeXdr(xdrBase64);
        var invokeContractOperation = (InvokeContractOperation)tx.Operations[0];
        var hostFunction = invokeContractOperation.HostFunction;
        var sourceAccount = tx.SourceAccount;
        Assert.AreEqual(sourceAccount.AccountId, "GCHTCLFS6ZRK27BHMNEZ4THVIAHBWLBH6BGOZEIGP4WFT7VXW735KJW2");
        Assert.AreEqual(((SCContractId)hostFunction.ContractAddress).InnerValue,
            StrKey.EncodeContractId(
                Convert.FromHexString("FE029A87B8C39972F484AF442C6965AACD65B2FCEFE20B0641F5063BF8A8E5C1")));
        Assert.AreEqual(hostFunction.FunctionName.InnerValue, "submit");
        Assert.AreEqual(((SCSymbol)hostFunction.Args[0]).InnerValue, "dancinRaph");
        Assert.AreEqual(invokeContractOperation.Auth.Length, 0);
    }


    [TestMethod]
    public async Task TestSubmitTransactionTooManyRequestsWithRetryAfterInt()
    {
        var server = Utils.CreateTestServerWithHeaders(
            new Dictionary<string, IEnumerable<string>>
            {
                { "Retry-After", new[] { "10" } }
            },
            HttpStatusCode.TooManyRequests);

        var exception = await Assert.ThrowsExceptionAsync<TooManyRequestsException>(
            () => server.SubmitTransaction(
                BuildTransaction(),
                new SubmitTransactionOptions { SkipMemoRequiredCheck = true }));

        Assert.AreEqual(10, exception.RetryAfter);
    }

    [TestMethod]
    public async Task TestSubmitTransactionTooManyRequestsWithRetryAfterDateTime()
    {
        var server = Utils.CreateTestServerWithHeaders(
            new Dictionary<string, IEnumerable<string>>
            {
                { "Retry-After", new[] { JsonSerializer.Serialize(DateTime.UtcNow.AddSeconds(10)).Trim('"') } }
            },
            HttpStatusCode.TooManyRequests);

        var exception = await Assert.ThrowsExceptionAsync<TooManyRequestsException>(
            () => server.SubmitTransaction(
                BuildTransaction(),
                new SubmitTransactionOptions { SkipMemoRequiredCheck = true }
            ));

        Assert.IsTrue(exception.RetryAfter is >= 7 and <= 10, "The RetryAfter value is outside the expected range.");
    }

    [TestMethod]
    public async Task TestSubmitTransactionServiceUnavailableWithRetryAfterInt()
    {
        var server = Utils.CreateTestServerWithHeaders(
            new Dictionary<string, IEnumerable<string>>
            {
                { "Retry-After", new[] { "10" } }
            },
            HttpStatusCode.ServiceUnavailable);

        var exception = await Assert.ThrowsExceptionAsync<ServiceUnavailableException>(
            () => server.SubmitTransaction(
                BuildTransaction(),
                new SubmitTransactionOptions { SkipMemoRequiredCheck = true }));

        Assert.AreEqual(10, exception.RetryAfter);
    }

    [TestMethod]
    public async Task TestSubmitTransactionServiceUnavailableWithRetryAfterDateTime()
    {
        var server = Utils.CreateTestServerWithHeaders(
            new Dictionary<string, IEnumerable<string>>
            {
                { "Retry-After", new[] { JsonSerializer.Serialize(DateTime.UtcNow.AddSeconds(10)).Trim('"') } }
            },
            HttpStatusCode.ServiceUnavailable);
     
        var exception = await Assert.ThrowsExceptionAsync<ServiceUnavailableException>(
            () => server.SubmitTransaction(
                BuildTransaction(),
                new SubmitTransactionOptions { SkipMemoRequiredCheck = true }));

        Assert.IsTrue(exception.RetryAfter is >= 7 and <= 10, "The RetryAfter value is outside the expected range.");
    }
}