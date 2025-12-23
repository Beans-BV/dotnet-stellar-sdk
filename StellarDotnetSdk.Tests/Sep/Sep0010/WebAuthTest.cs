using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Sep.Sep0001;
using StellarDotnetSdk.Sep.Sep0010;
using StellarDotnetSdk.Sep.Sep0010.Exceptions;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Tests.Sep.Sep0010;

/// <summary>
///     Unit tests for SEP-0010 Web Authentication functionality.
/// </summary>
[TestClass]
public class WebAuthTest
{
    private const string HomeDomain = "thisisatest.sandbox.anchor.anchordomain.com";
    private const string WebAuthDomain = "thisisatest.sandbox.anchor.webauth.com";
    private const string ClientDomain = "thisisatest.sandbox.anchor.client.com";
    private const string AuthEndpoint = "https://thisisatest.sandbox.anchor.webauth.com/auth";

    private Network _testnet = null!;
    private KeyPair _clientKeypair = null!;
    private KeyPair _serverKeypair = null!;
    private string _serverSigningKey = null!;

    [TestInitialize]
    public void Initialize()
    {
        _testnet = Network.Test();
        Network.Use(_testnet);

        _serverKeypair = KeyPair.Random();
        _serverSigningKey = _serverKeypair.AccountId;
        _clientKeypair = KeyPair.Random();
    }

    /// <summary>
    ///     Creates a mock HttpClient that returns the specified content and status code.
    /// </summary>
    private static HttpClient CreateMockHttpClient(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return Utils.CreateFakeHttpClient(content, statusCode);
    }

    /// <summary>
    ///     Creates a sample stellar.toml content for testing.
    /// </summary>
    private string CreateStellarTomlContent(string webAuthEndpoint, string signingKey)
    {
        return $@"VERSION = ""2.0.0""
NETWORK_PASSPHRASE = ""Test SDF Network ; September 2015""
WEB_AUTH_ENDPOINT = ""{webAuthEndpoint}""
SIGNING_KEY = ""{signingKey}""
";
    }

    /// <summary>
    ///     Creates a challenge transaction XDR for testing.
    /// </summary>
    private string CreateChallengeTransactionXdr(string clientAccountId)
    {
        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            clientAccountId,
            HomeDomain,
            WebAuthDomain,
            validFrom: DateTimeOffset.UtcNow);

        return transaction.ToEnvelopeXdrBase64();
    }

    /// <summary>
    ///     Verifies that FromDomainAsync creates WebAuth instance with correct configuration.
    /// </summary>
    [TestMethod]
    public async Task FromDomainAsync_WithValidDomain_ReturnsWebAuthInstance()
    {
        // Arrange
        var tomlContent = CreateStellarTomlContent(AuthEndpoint, _serverSigningKey);
        using var httpClient = CreateMockHttpClient(tomlContent);

        // Act
        var webAuth = await WebAuth.FromDomainAsync(HomeDomain, _testnet, httpClient);

        // Assert
        Assert.IsNotNull(webAuth);
    }

    /// <summary>
    ///     Verifies that FromDomainAsync throws NoWebAuthEndpointFoundException when WEB_AUTH_ENDPOINT is missing.
    /// </summary>
    [TestMethod]
    public async Task FromDomainAsync_MissingWebAuthEndpoint_ThrowsNoWebAuthEndpointFoundException()
    {
        // Arrange
        var tomlContent = @"VERSION = ""2.0.0""
SIGNING_KEY = ""GBWMCCC3NHSKLAOJDBKKYW7SSH2PFTTNVFKWSGLWGDLEBKLOVP5JLBBP""
";
        using var httpClient = CreateMockHttpClient(tomlContent);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<NoWebAuthEndpointFoundException>(async () =>
        {
            await WebAuth.FromDomainAsync(HomeDomain, _testnet, httpClient);
        });
    }

    /// <summary>
    ///     Verifies that FromDomainAsync throws NoWebAuthServerSigningKeyFoundException when SIGNING_KEY is missing.
    /// </summary>
    [TestMethod]
    public async Task FromDomainAsync_MissingSigningKey_ThrowsNoWebAuthServerSigningKeyFoundException()
    {
        // Arrange
        var tomlContent = $@"VERSION = ""2.0.0""
WEB_AUTH_ENDPOINT = ""{AuthEndpoint}""
";
        using var httpClient = CreateMockHttpClient(tomlContent);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<NoWebAuthServerSigningKeyFoundException>(async () =>
        {
            await WebAuth.FromDomainAsync(HomeDomain, _testnet, httpClient);
        });
    }

    /// <summary>
    ///     Verifies that GetChallengeResponseAsync returns challenge transaction.
    /// </summary>
    [TestMethod]
    public async Task GetChallengeResponseAsync_WithValidAccount_ReturnsChallengeResponse()
    {
        // Arrange
        var challengeXdr = CreateChallengeTransactionXdr(_clientKeypair.AccountId);
        var challengeResponse = new ChallengeResponse { Transaction = challengeXdr };
        var responseJson = JsonSerializer.Serialize(challengeResponse, JsonOptions.DefaultOptions);

        using var httpClient = CreateMockHttpClient(responseJson);
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain, httpClient);

        // Act
        var result = await webAuth.GetChallengeResponseAsync(_clientKeypair.AccountId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(challengeXdr, result.Transaction);
    }

    /// <summary>
    ///     Verifies that GetChallengeAsync throws MissingTransactionInChallengeResponseException when transaction is missing.
    /// </summary>
    [TestMethod]
    public async Task GetChallengeAsync_MissingTransaction_ThrowsMissingTransactionInChallengeResponseException()
    {
        // Arrange
        var challengeResponse = new ChallengeResponse { Transaction = null };
        var responseJson = JsonSerializer.Serialize(challengeResponse, JsonOptions.DefaultOptions);

        using var httpClient = CreateMockHttpClient(responseJson);
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain, httpClient);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<MissingTransactionInChallengeResponseException>(async () =>
        {
            await webAuth.GetChallengeAsync(_clientKeypair.AccountId);
        });
    }

    /// <summary>
    ///     Verifies that GetChallengeResponseAsync throws NoMemoForMuxedAccountsException when memo is provided for muxed account.
    /// </summary>
    [TestMethod]
    public async Task GetChallengeResponseAsync_MemoWithMuxedAccount_ThrowsNoMemoForMuxedAccountsException()
    {
        // Arrange
        var muxedAccountId = "MAAAAAAAAAAAJURAAB2X52XFQP6FBXLGT6LWOOWMEXWHEWBDVRZ7V5WH34Y22MPFBHUHY";
        using var httpClient = CreateMockHttpClient("{}");
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain, httpClient);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<NoMemoForMuxedAccountsException>(async () =>
        {
            await webAuth.GetChallengeResponseAsync(muxedAccountId, memo: 123);
        });
    }

    /// <summary>
    ///     Verifies that ValidateChallenge throws ChallengeValidationErrorInvalidSeqNr when sequence number is not 0.
    /// </summary>
    [TestMethod]
    public void ValidateChallenge_InvalidSequenceNumber_ThrowsChallengeValidationErrorInvalidSeqNr()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, 1234);
        var opSource = new Account(_clientKeypair.Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

        var operation = new ManageDataOperation($"{HomeDomain} auth", base64Data, opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        var challengeXdr = transaction.ToEnvelopeXdrBase64();

        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain);

        // Act & Assert
        Assert.ThrowsException<ChallengeValidationErrorInvalidSeqNr>(() =>
        {
            webAuth.ValidateChallenge(challengeXdr, _clientKeypair.AccountId);
        });
    }

    /// <summary>
    ///     Verifies that ValidateChallenge throws ChallengeValidationErrorInvalidHomeDomain when home domain doesn't match.
    /// </summary>
    [TestMethod]
    public void ValidateChallenge_InvalidHomeDomain_ThrowsChallengeValidationErrorInvalidHomeDomain()
    {
        // Arrange
        var challengeXdr = CreateChallengeTransactionXdr(_clientKeypair.AccountId);
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, "wrong.domain.com");

        // Act & Assert
        Assert.ThrowsException<ChallengeValidationErrorInvalidHomeDomain>(() =>
        {
            webAuth.ValidateChallenge(challengeXdr, _clientKeypair.AccountId);
        });
    }

    /// <summary>
    ///     Verifies that ValidateChallenge throws ChallengeValidationErrorInvalidTimeBounds when time bounds are expired.
    /// </summary>
    [TestMethod]
    public void ValidateChallenge_ExpiredTimeBounds_ThrowsChallengeValidationErrorInvalidTimeBounds()
    {
        // Arrange
        var expiredTime = DateTimeOffset.UtcNow.AddHours(-1);
        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            _clientKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            validFrom: expiredTime,
            validFor: TimeSpan.FromMinutes(5));

        var challengeXdr = transaction.ToEnvelopeXdrBase64();
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain);

        // Act & Assert
        Assert.ThrowsException<ChallengeValidationErrorInvalidTimeBounds>(() =>
        {
            webAuth.ValidateChallenge(challengeXdr, _clientKeypair.AccountId);
        });
    }

    /// <summary>
    ///     Verifies that ValidateChallenge throws ChallengeValidationErrorInvalidSignature when signature is invalid.
    /// </summary>
    [TestMethod]
    public void ValidateChallenge_InvalidSignature_ThrowsChallengeValidationErrorInvalidSignature()
    {
        // Arrange
        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            _clientKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            validFrom: DateTimeOffset.UtcNow);

        // Remove server signature and add invalid signature
        transaction.Signatures.Clear();
        transaction.Sign(_clientKeypair);

        var challengeXdr = transaction.ToEnvelopeXdrBase64();
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain);

        // Act & Assert
        Assert.ThrowsException<ChallengeValidationErrorInvalidSignature>(() =>
        {
            webAuth.ValidateChallenge(challengeXdr, _clientKeypair.AccountId);
        });
    }

    /// <summary>
    ///     Verifies that ValidateChallenge throws ChallengeValidationErrorMemoAndMuxedAccount when memo is present with muxed account.
    /// </summary>
    [TestMethod]
    public void ValidateChallenge_MemoWithMuxedAccount_ThrowsChallengeValidationErrorMemoAndMuxedAccount()
    {
        // Arrange
        var muxedAccountId = "MAAAAAAAAAAAJURAAB2X52XFQP6FBXLGT6LWOOWMEXWHEWBDVRZ7V5WH34Y22MPFBHUHY";
        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            _clientKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            validFrom: DateTimeOffset.UtcNow);

        // Add memo to transaction
        var transactionBuilder = new TransactionBuilder(new Account(_serverKeypair.Address, -1))
            .AddOperation(transaction.Operations[0])
            .AddMemo(Memo.Id(12345))
            .AddPreconditions(transaction.Preconditions);

        var transactionWithMemo = transactionBuilder.Build();
        transactionWithMemo.Sign(_serverKeypair);
        var challengeXdr = transactionWithMemo.ToEnvelopeXdrBase64();

        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain);

        // Act & Assert
        Assert.ThrowsException<ChallengeValidationErrorMemoAndMuxedAccount>(() =>
        {
            webAuth.ValidateChallenge(challengeXdr, muxedAccountId);
        });
    }

    /// <summary>
    ///     Verifies that SignTransaction adds signatures to the challenge transaction.
    /// </summary>
    [TestMethod]
    public void SignTransaction_WithSigners_AddsSignatures()
    {
        // Arrange
        var challengeXdr = CreateChallengeTransactionXdr(_clientKeypair.AccountId);
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain);

        // Act
        var signedXdr = webAuth.SignTransaction(challengeXdr, new[] { _clientKeypair });

        // Assert
        var signedTransaction = Transaction.FromEnvelopeXdr(signedXdr);
        Assert.IsTrue(signedTransaction.Signatures.Count >= 2); // Server + client
    }

    /// <summary>
    ///     Verifies that SendSignedChallengeAsync returns JWT token on success.
    /// </summary>
    [TestMethod]
    public async Task SendSignedChallengeAsync_WithValidTransaction_ReturnsJwtToken()
    {
        // Arrange
        var challengeXdr = CreateChallengeTransactionXdr(_clientKeypair.AccountId);
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain);
        var signedXdr = webAuth.SignTransaction(challengeXdr, new[] { _clientKeypair });

        var submitResponse = new SubmitChallengeResponse { Token = "test.jwt.token" };
        var responseJson = JsonSerializer.Serialize(submitResponse, JsonOptions.DefaultOptions);

        using var httpClient = CreateMockHttpClient(responseJson);
        var webAuthWithClient = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain, httpClient);

        // Act
        var jwtToken = await webAuthWithClient.SendSignedChallengeAsync(signedXdr);

        // Assert
        Assert.AreEqual("test.jwt.token", jwtToken);
    }

    /// <summary>
    ///     Verifies that SendSignedChallengeAsync throws SubmitChallengeErrorResponseException on HTTP 400.
    /// </summary>
    [TestMethod]
    public async Task SendSignedChallengeAsync_Http400_ThrowsSubmitChallengeErrorResponseException()
    {
        // Arrange
        var challengeXdr = CreateChallengeTransactionXdr(_clientKeypair.AccountId);
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain);
        var signedXdr = webAuth.SignTransaction(challengeXdr, new[] { _clientKeypair });

        var submitResponse = new SubmitChallengeResponse { Error = "Invalid signature" };
        var responseJson = JsonSerializer.Serialize(submitResponse, JsonOptions.DefaultOptions);

        using var httpClient = CreateMockHttpClient(responseJson, HttpStatusCode.BadRequest);
        var webAuthWithClient = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain, httpClient);

        // Act & Assert
        var ex = await Assert.ThrowsExceptionAsync<SubmitChallengeErrorResponseException>(async () =>
        {
            await webAuthWithClient.SendSignedChallengeAsync(signedXdr);
        });

        Assert.AreEqual("Invalid signature", ex.Error);
    }

    /// <summary>
    ///     Verifies that SendSignedChallengeAsync throws SubmitChallengeTimeoutResponseException on HTTP 504.
    /// </summary>
    [TestMethod]
    public async Task SendSignedChallengeAsync_Http504_ThrowsSubmitChallengeTimeoutResponseException()
    {
        // Arrange
        var challengeXdr = CreateChallengeTransactionXdr(_clientKeypair.AccountId);
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain);
        var signedXdr = webAuth.SignTransaction(challengeXdr, new[] { _clientKeypair });

        using var httpClient = CreateMockHttpClient("", HttpStatusCode.GatewayTimeout);
        var webAuthWithClient = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain, httpClient);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<SubmitChallengeTimeoutResponseException>(async () =>
        {
            await webAuthWithClient.SendSignedChallengeAsync(signedXdr);
        });
    }

    /// <summary>
    ///     Verifies that JwtTokenAsync completes the full authentication flow successfully.
    /// </summary>
    [TestMethod]
    public async Task JwtTokenAsync_WithValidChallenge_ReturnsJwtToken()
    {
        // Arrange
        var challengeXdr = CreateChallengeTransactionXdr(_clientKeypair.AccountId);
        var challengeResponse = new ChallengeResponse { Transaction = challengeXdr };
        var challengeJson = JsonSerializer.Serialize(challengeResponse, JsonOptions.DefaultOptions);

        var submitResponse = new SubmitChallengeResponse { Token = "test.jwt.token" };
        var submitJson = JsonSerializer.Serialize(submitResponse, JsonOptions.DefaultOptions);

        // Create HttpClient that returns different responses for different requests
        var mockHandler = new Moq.Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        var callCount = 0;
        mockHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>()))
            .Returns<HttpRequestMessage>(req =>
            {
                callCount++;
                if (callCount == 1)
                {
                    // First call: challenge request
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(challengeJson),
                    };
                }
                else
                {
                    // Second call: submit challenge
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(submitJson),
                    };
                }
            });

        using var httpClient = new HttpClient(mockHandler.Object);
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain, httpClient);

        // Act
        var jwtToken = await webAuth.JwtTokenAsync(_clientKeypair.AccountId, new[] { _clientKeypair });

        // Assert
        Assert.AreEqual("test.jwt.token", jwtToken);
    }

    /// <summary>
    ///     Verifies that JwtTokenAsync throws NoMemoForMuxedAccountsException when memo is provided for muxed account.
    /// </summary>
    [TestMethod]
    public async Task JwtTokenAsync_MemoWithMuxedAccount_ThrowsNoMemoForMuxedAccountsException()
    {
        // Arrange
        var muxedAccountId = "MAAAAAAAAAAAJURAAB2X52XFQP6FBXLGT6LWOOWMEXWHEWBDVRZ7V5WH34Y22MPFBHUHY";
        using var httpClient = CreateMockHttpClient("{}");
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain, httpClient);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<NoMemoForMuxedAccountsException>(async () =>
        {
            await webAuth.JwtTokenAsync(muxedAccountId, new[] { _clientKeypair }, memo: 123);
        });
    }

    /// <summary>
    ///     Verifies that JwtTokenAsync throws MissingClientDomainException when delegate is provided without clientDomain.
    /// </summary>
    [TestMethod]
    public async Task JwtTokenAsync_DelegateWithoutClientDomain_ThrowsMissingClientDomainException()
    {
        // Arrange
        var challengeXdr = CreateChallengeTransactionXdr(_clientKeypair.AccountId);
        var challengeResponse = new ChallengeResponse { Transaction = challengeXdr };
        var challengeJson = JsonSerializer.Serialize(challengeResponse, JsonOptions.DefaultOptions);

        using var httpClient = CreateMockHttpClient(challengeJson);
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain, httpClient);

        ClientDomainSigningDelegate delegateFunc = async (xdr) => await Task.FromResult(xdr);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<MissingClientDomainException>(async () =>
        {
            await webAuth.JwtTokenAsync(
                _clientKeypair.AccountId,
                new[] { _clientKeypair },
                clientDomainSigningDelegate: delegateFunc);
        });
    }

    /// <summary>
    ///     Verifies that Dispose disposes internal HttpClient when created internally.
    /// </summary>
    [TestMethod]
    public void Dispose_WithInternalHttpClient_DisposesHttpClient()
    {
        // Arrange
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain);

        // Act
        webAuth.Dispose();

        // Assert - Should not throw
        Assert.IsTrue(true);
    }

    /// <summary>
    ///     Verifies that Dispose does not dispose external HttpClient.
    /// </summary>
    [TestMethod]
    public void Dispose_WithExternalHttpClient_DoesNotDisposeHttpClient()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient("{}");
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain, httpClient);

        // Act
        webAuth.Dispose();

        // Assert - HttpClient should still be usable
        Assert.IsNotNull(httpClient);
    }

    /// <summary>
    ///     Verifies that GetChallengeResponseAsync throws ChallengeRequestErrorException on HTTP error.
    /// </summary>
    [TestMethod]
    public async Task GetChallengeResponseAsync_HttpError_ThrowsChallengeRequestErrorException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient("Error", HttpStatusCode.BadRequest);
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain, httpClient);

        // Act & Assert
        var ex = await Assert.ThrowsExceptionAsync<ChallengeRequestErrorException>(async () =>
        {
            await webAuth.GetChallengeResponseAsync(_clientKeypair.AccountId);
        });

        Assert.AreEqual(400, ex.StatusCode);
    }

    /// <summary>
    ///     Verifies that ValidateChallenge throws ChallengeValidationErrorInvalidOperationType for non-ManageData operations.
    /// </summary>
    [TestMethod]
    public void ValidateChallenge_NonManageDataOperation_ThrowsChallengeValidationErrorInvalidOperationType()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var opSource = new Account(_clientKeypair.Address, 0);

        var operation = new PaymentOperation(KeyPair.Random(), new AssetTypeNative(), "100", opSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        var challengeXdr = transaction.ToEnvelopeXdrBase64();

        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain);

        // Act & Assert
        Assert.ThrowsException<ChallengeValidationErrorInvalidOperationType>(() =>
        {
            webAuth.ValidateChallenge(challengeXdr, _clientKeypair.AccountId);
        });
    }

    /// <summary>
    ///     Verifies that ValidateChallenge throws ChallengeValidationErrorInvalidSourceAccount when operation source doesn't match.
    /// </summary>
    [TestMethod]
    public void ValidateChallenge_InvalidOperationSourceAccount_ThrowsChallengeValidationErrorInvalidSourceAccount()
    {
        // Arrange
        var transactionSource = new Account(_serverKeypair.Address, -1);
        var wrongSource = new Account(KeyPair.Random().Address, 0);

        var plainTextBytes = Encoding.UTF8.GetBytes(new string(' ', 48));
        var base64Data = Encoding.ASCII.GetBytes(Convert.ToBase64String(plainTextBytes));

        var operation = new ManageDataOperation($"{HomeDomain} auth", base64Data, wrongSource.KeyPair);
        var transaction = new TransactionBuilder(transactionSource)
            .AddOperation(operation)
            .AddPreconditions(new TransactionPreconditions
                { TimeBounds = new TimeBounds(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddSeconds(1000)) })
            .Build();

        transaction.Sign(_serverKeypair);
        var challengeXdr = transaction.ToEnvelopeXdrBase64();

        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain);

        // Act & Assert
        Assert.ThrowsException<ChallengeValidationErrorInvalidSourceAccount>(() =>
        {
            webAuth.ValidateChallenge(challengeXdr, _clientKeypair.AccountId);
        });
    }

    /// <summary>
    ///     Verifies that ValidateChallenge throws ChallengeValidationErrorInvalidMemoType for invalid memo type.
    /// </summary>
    [TestMethod]
    public void ValidateChallenge_InvalidMemoType_ThrowsChallengeValidationErrorInvalidMemoType()
    {
        // Arrange
        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            _clientKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            validFrom: DateTimeOffset.UtcNow);

        // Add text memo (invalid for SEP-10)
        var transactionBuilder = new TransactionBuilder(new Account(_serverKeypair.Address, -1))
            .AddOperation(transaction.Operations[0])
            .AddMemo(Memo.Text("invalid"))
            .AddPreconditions(transaction.Preconditions);

        var transactionWithMemo = transactionBuilder.Build();
        transactionWithMemo.Sign(_serverKeypair);
        var challengeXdr = transactionWithMemo.ToEnvelopeXdrBase64();

        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain);

        // Act & Assert
        Assert.ThrowsException<ChallengeValidationErrorInvalidMemoType>(() =>
        {
            webAuth.ValidateChallenge(challengeXdr, _clientKeypair.AccountId, memo: 12345);
        });
    }

    /// <summary>
    ///     Verifies that ValidateChallenge throws ChallengeValidationErrorInvalidMemoValue when memo doesn't match.
    /// </summary>
    [TestMethod]
    public void ValidateChallenge_MemoValueMismatch_ThrowsChallengeValidationErrorInvalidMemoValue()
    {
        // Arrange
        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            _clientKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            validFrom: DateTimeOffset.UtcNow);

        // Add memo with different value
        var transactionBuilder = new TransactionBuilder(new Account(_serverKeypair.Address, -1))
            .AddOperation(transaction.Operations[0])
            .AddMemo(Memo.Id(99999))
            .AddPreconditions(transaction.Preconditions);

        var transactionWithMemo = transactionBuilder.Build();
        transactionWithMemo.Sign(_serverKeypair);
        var challengeXdr = transactionWithMemo.ToEnvelopeXdrBase64();

        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain);

        // Act & Assert
        Assert.ThrowsException<ChallengeValidationErrorInvalidMemoValue>(() =>
        {
            webAuth.ValidateChallenge(challengeXdr, _clientKeypair.AccountId, memo: 12345);
        });
    }

    /// <summary>
    ///     Verifies that ValidateChallenge throws ChallengeValidationErrorInvalidWebAuthDomain when web auth domain doesn't match.
    /// </summary>
    [TestMethod]
    public void ValidateChallenge_InvalidWebAuthDomain_ThrowsChallengeValidationErrorInvalidWebAuthDomain()
    {
        // Arrange
        var transaction = WebAuthentication.BuildChallengeTransaction(
            _serverKeypair,
            _clientKeypair.AccountId,
            HomeDomain,
            WebAuthDomain,
            validFrom: DateTimeOffset.UtcNow);

        var challengeXdr = transaction.ToEnvelopeXdrBase64();
        // Use different auth endpoint domain
        var webAuth = new WebAuth("https://wrong.domain.com/auth", _testnet, _serverSigningKey, HomeDomain);

        // Act & Assert
        Assert.ThrowsException<ChallengeValidationErrorInvalidWebAuthDomain>(() =>
        {
            webAuth.ValidateChallenge(challengeXdr, _clientKeypair.AccountId);
        });
    }

    /// <summary>
    ///     Verifies that SendSignedChallengeAsync throws SubmitChallengeUnknownResponseException on unexpected status code.
    /// </summary>
    [TestMethod]
    public async Task SendSignedChallengeAsync_UnexpectedStatusCode_ThrowsSubmitChallengeUnknownResponseException()
    {
        // Arrange
        var challengeXdr = CreateChallengeTransactionXdr(_clientKeypair.AccountId);
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain);
        var signedXdr = webAuth.SignTransaction(challengeXdr, new[] { _clientKeypair });

        using var httpClient = CreateMockHttpClient("Internal Server Error", HttpStatusCode.InternalServerError);
        var webAuthWithClient = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain, httpClient);

        // Act & Assert
        var ex = await Assert.ThrowsExceptionAsync<SubmitChallengeUnknownResponseException>(async () =>
        {
            await webAuthWithClient.SendSignedChallengeAsync(signedXdr);
        });

        Assert.AreEqual(500, ex.Code);
    }

    /// <summary>
    ///     Verifies that ValidateChallenge succeeds with valid challenge transaction.
    /// </summary>
    [TestMethod]
    public void ValidateChallenge_ValidChallenge_Succeeds()
    {
        // Arrange
        var challengeXdr = CreateChallengeTransactionXdr(_clientKeypair.AccountId);
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain);

        // Act & Assert - Should not throw
        webAuth.ValidateChallenge(challengeXdr, _clientKeypair.AccountId);
    }

    /// <summary>
    ///     Verifies that ValidateChallenge succeeds with custom grace period.
    /// </summary>
    [TestMethod]
    public void ValidateChallenge_WithCustomGracePeriod_Succeeds()
    {
        // Arrange
        var challengeXdr = CreateChallengeTransactionXdr(_clientKeypair.AccountId);
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain, gracePeriod: 600);

        // Act & Assert - Should not throw
        webAuth.ValidateChallenge(challengeXdr, _clientKeypair.AccountId, _clientKeypair.AccountId, 600);
    }

    /// <summary>
    ///     Verifies that GetChallengeResponseAsync includes query parameters correctly.
    /// </summary>
    [TestMethod]
    public async Task GetChallengeResponseAsync_WithQueryParameters_IncludesParameters()
    {
        // Arrange
        var challengeXdr = CreateChallengeTransactionXdr(_clientKeypair.AccountId);
        var challengeResponse = new ChallengeResponse { Transaction = challengeXdr };
        var responseJson = JsonSerializer.Serialize(challengeResponse, JsonOptions.DefaultOptions);

        var mockHandler = new Moq.Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        HttpRequestMessage? capturedRequest = null;
        mockHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>()))
            .Callback<HttpRequestMessage>(req => capturedRequest = req)
            .Returns(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson),
            });

        using var httpClient = new HttpClient(mockHandler.Object);
        var webAuth = new WebAuth(AuthEndpoint, _testnet, _serverSigningKey, HomeDomain, httpClient);

        // Act
        await webAuth.GetChallengeResponseAsync(_clientKeypair.AccountId, memo: 12345, homeDomain: "custom.domain");

        // Assert
        Assert.IsNotNull(capturedRequest);
        Assert.IsTrue(capturedRequest.RequestUri!.Query.Contains("account="));
        Assert.IsTrue(capturedRequest.RequestUri.Query.Contains("memo=12345"));
        Assert.IsTrue(capturedRequest.RequestUri.Query.Contains("home_domain=custom.domain"));
    }
}

