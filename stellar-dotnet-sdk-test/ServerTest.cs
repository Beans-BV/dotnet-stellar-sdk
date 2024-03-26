using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Language;
using stellar_dotnet_sdk;
using stellar_dotnet_sdk.federation;
using stellar_dotnet_sdk.responses;

namespace stellar_dotnet_sdk_test;

[TestClass]
public class ServerTest
{
    private const HttpStatusCode HttpOk = HttpStatusCode.OK;
    private const HttpStatusCode HttpBadRequest = HttpStatusCode.BadRequest;

    private Mock<FakeHttpMessageHandler> _fakeHttpMessageHandler;
    private HttpClient _httpClient;
    private Server _server;

    private ISetupSequentialResult<HttpResponseMessage> When()
    {
        return _fakeHttpMessageHandler.SetupSequence(a => a.Send(It.IsAny<HttpRequestMessage>()));
    }

    [TestInitialize]
    public void Setup()
    {
        Network.UseTestNetwork();

        _fakeHttpMessageHandler = new Mock<FakeHttpMessageHandler> { CallBase = true };
        _httpClient = new HttpClient(_fakeHttpMessageHandler.Object);
        _server = new Server("https://horizon.stellar.org", _httpClient);
    }

    [TestCleanup]
    public void Cleanup()
    {
        Network.Use(null);
        _httpClient.Dispose();
        _server.Dispose();
    }

    public static HttpResponseMessage ResponseMessage(HttpStatusCode statusCode)
    {
        return new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = null
        };
    }

    public static HttpResponseMessage ResponseMessage(HttpStatusCode statusCode, string content)
    {
        return new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(content)
        };
    }

    private static Transaction BuildTransaction()
    {
        var source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
        var destination = KeyPair.FromAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR");

        var account = new Account(source.AccountId, 2908908335136768L);
        var builder = new TransactionBuilder(account)
            .AddOperation(new CreateAccountOperation.Builder(destination, "2000").Build())
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
        var json = await File.ReadAllTextAsync(Path.Combine("testdata", "serverSuccess.json"));
        When().Returns(ResponseMessage(HttpOk, json));

        var response = await _server.SubmitTransaction(
            BuildTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = true });
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess());
        Assert.AreEqual(response.Ledger, (uint)826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    [TestMethod]
    public async Task TestDefaultClientHeaders()
    {
        var messageHandler = new Mock<FakeHttpMessageHandler> { CallBase = true };
        var httpClient = Server.CreateHttpClient(messageHandler.Object);
        var server = new Server("https://horizon.stellar.org", httpClient);

        var json = File.ReadAllText(Path.Combine("testdata", "serverSuccess.json"));
        var clientName = "";
        var clientVersion = "";

        messageHandler
            .Setup(h => h.Send(It.IsAny<HttpRequestMessage>()))
            .Callback<HttpRequestMessage>(msg =>
            {
                clientName = msg.Headers.GetValues("X-Client-Name").FirstOrDefault();
                clientVersion = msg.Headers.GetValues("X-Client-Version").FirstOrDefault();
            })
            .Returns(ResponseMessage(HttpOk, json));

        var response = await server.SubmitTransaction(
            BuildTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = true });
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess());
        Assert.AreEqual("stellar-dotnet-sdk", clientName);
        Assert.IsFalse(string.IsNullOrWhiteSpace(clientVersion));
        var result = response.Result;
        Assert.IsInstanceOfType(result, typeof(TransactionResultSuccess));
        Assert.AreEqual("0.00001", result.FeeCharged);
    }

    [TestMethod]
    public async Task TestSubmitTransactionFail()
    {
        var json = await File.ReadAllTextAsync(Path.Combine("testdata", "serverFailure.json"));
        When().Returns(ResponseMessage(HttpBadRequest, json));

        var response = await _server.SubmitTransaction(
            BuildTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = true });
        Assert.IsNotNull(response);
        Assert.IsFalse(response.IsSuccess());
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
        var json = await File.ReadAllTextAsync(Path.Combine("testdata", "serverSuccess.json"));
        When().Returns(ResponseMessage(HttpOk, json));

        var response = await _server.SubmitTransaction(
            BuildTransaction().ToEnvelopeXdrBase64(), new SubmitTransactionOptions { EnsureSuccess = true });
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess());
        Assert.AreEqual(response.Ledger, (uint)826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    [TestMethod]
    public async Task TestSubmitTransactionEnsureSuccessWithContent()
    {
        var json = await File.ReadAllTextAsync(Path.Combine("testdata", "serverFailure.json"));
        When().Returns(ResponseMessage(HttpBadRequest, json));

        var ex = await Assert.ThrowsExceptionAsync<ConnectionErrorException>(async () =>
        {
            await _server.SubmitTransaction(BuildTransaction(),
                new SubmitTransactionOptions { EnsureSuccess = true });
        });

        Assert.IsTrue(ex.Message.Contains("Status code (BadRequest) is not success."));
    }

    [TestMethod]
    public async Task TestSubmitTransactionEnsureSuccessWithEmptyContent()
    {
        When().Returns(ResponseMessage(HttpBadRequest, ""));

        var ex = await Assert.ThrowsExceptionAsync<ConnectionErrorException>(async () =>
        {
            await _server.SubmitTransaction(BuildTransaction(),
                new SubmitTransactionOptions { EnsureSuccess = true });
        });

        Assert.AreEqual(ex.Message, "Status code (BadRequest) is not success.");
    }

    [TestMethod]
    public async Task TestSubmitTransactionEnsureSuccessWithNullContent()
    {
        When().Returns(ResponseMessage(HttpBadRequest));

        var ex = await Assert.ThrowsExceptionAsync<ConnectionErrorException>(async () =>
        {
            await _server.SubmitTransaction(BuildTransaction(),
                new SubmitTransactionOptions { EnsureSuccess = true });
        });

        Assert.AreEqual(ex.Message, "Status code (BadRequest) is not success.");
    }

    [TestMethod]
    public async Task TestNoSkipMemoRequiredCheck()
    {
        var json = await File.ReadAllTextAsync(Path.Combine("testdata", "serverSuccess.json"));
        When().Returns(ResponseMessage(HttpOk, json));

        var response = await _server.SubmitTransaction(
            BuildTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = false });
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess());
        Assert.AreEqual(response.Ledger, (uint)826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    [TestMethod]
    public async Task TestSubmitTransactionEnvelopeBase64()
    {
        var json = await File.ReadAllTextAsync(Path.Combine("testdata", "serverSuccess.json"));
        When().Returns(ResponseMessage(HttpOk, json));

        var response = await _server.SubmitTransaction(
            BuildTransaction().ToEnvelopeXdrBase64(), new SubmitTransactionOptions { SkipMemoRequiredCheck = false });
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess());
        Assert.AreEqual(response.Ledger, (uint)826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    [TestMethod]
    public async Task TestSubmitFeeBumpTransactionEnvelopeBase64()
    {
        var json = await File.ReadAllTextAsync(Path.Combine("testdata", "serverSuccess.json"));
        When().Returns(ResponseMessage(HttpOk, json));

        var response = await _server.SubmitTransaction(
            BuildFeeBumpTransaction().ToEnvelopeXdrBase64(),
            new SubmitTransactionOptions { SkipMemoRequiredCheck = false, FeeBumpTransaction = true });
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess());
        Assert.AreEqual(response.Ledger, (uint)826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    [TestMethod]
    public async Task TestSubmitFeeBumpTransactionWithoutOptions()
    {
        var json = await File.ReadAllTextAsync(Path.Combine("testdata", "serverSuccess.json"));
        When().Returns(ResponseMessage(HttpOk, json));

        var response = await _server.SubmitTransaction(BuildFeeBumpTransaction());
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess());
        Assert.AreEqual(response.Ledger, (uint)826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    [TestMethod]
    public async Task TestSubmitFeeBumpTransactionWithOptions()
    {
        var json = await File.ReadAllTextAsync(Path.Combine("testdata", "serverSuccess.json"));
        When().Returns(ResponseMessage(HttpOk, json));

        var response = await _server.SubmitTransaction(
            BuildFeeBumpTransaction(), new SubmitTransactionOptions { SkipMemoRequiredCheck = false });
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess());
        Assert.AreEqual(response.Ledger, (uint)826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    [TestMethod]
    public async Task TestSubmitTransactionWithoutOptions()
    {
        var json = await File.ReadAllTextAsync(Path.Combine("testdata", "serverSuccess.json"));
        When().Returns(ResponseMessage(HttpOk, json));

        var response = await _server.SubmitTransaction(BuildTransaction());
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess());
        Assert.AreEqual(response.Ledger, (uint)826150);
        Assert.AreEqual(response.Hash, "2634d2cf5adcbd3487d1df042166eef53830115844fdde1588828667bf93ff42");
        Assert.IsNull(response.SubmitTransactionResponseExtras);
    }

    [TestMethod]
    public async Task TestSubmitTransactionEnvelopeBase64WithoutOptions()
    {
        var json = await File.ReadAllTextAsync(Path.Combine("testdata", "serverSuccess.json"));
        When().Returns(ResponseMessage(HttpOk, json));

        var response = await _server.SubmitTransaction(
            BuildTransaction().ToEnvelopeXdrBase64());
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess());
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

    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        public Uri RequestUri { get; private set; }

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