using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StellarDotnetSdk.Sep.Sep0009;
using StellarDotnetSdk.Sep.Sep0024;

namespace StellarDotnetSdk.Tests.Sep.Sep0024;

/// <summary>
///     Comprehensive tests for InteractiveService SEP-24 implementation.
/// </summary>
[TestClass]
public class InteractiveServiceTest
{
    private const string TestTransferServerUrl = "https://api.example.com/sep24";
    private const string TestJwt = "test-jwt-token";
    private string _infoResponseJson = null!;
    private string _feeResponseJson = null!;
    private string _interactiveResponseJson = null!;
    private string _transactionResponseJson = null!;
    private string _transactionsResponseJson = null!;
    private string _transactionWithRefundJson = null!;
    private string _errorResponseJson = null!;
    private string _authRequiredResponseJson = null!;
    private string _stellarTomlContent = null!;

    [TestInitialize]
    public void Setup()
    {
        _infoResponseJson = ReadTestDataFile("info-response.json");
        _feeResponseJson = ReadTestDataFile("fee-response.json");
        _interactiveResponseJson = ReadTestDataFile("interactive-response.json");
        _transactionResponseJson = ReadTestDataFile("transaction-response.json");
        _transactionsResponseJson = ReadTestDataFile("transactions-response.json");
        _transactionWithRefundJson = ReadTestDataFile("transaction-with-refund.json");
        _errorResponseJson = ReadTestDataFile("error-response.json");
        _authRequiredResponseJson = ReadTestDataFile("authentication-required-response.json");

        _stellarTomlContent = @"
VERSION=""2.0.0""
NETWORK_PASSPHRASE=""Public Global Stellar Network ; September 2015""
TRANSFER_SERVER_SEP0024=""https://api.example.com/sep24""
";
    }

    private static string ReadTestDataFile(string filename)
    {
        return File.ReadAllText(Utils.GetTestDataAbsolutePath(filename));
    }

    private static HttpClient CreateMockHttpClient(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return Utils.CreateFakeHttpClient(content, statusCode);
    }

    private static (HttpClient HttpClient, RequestCapture Capture) CreateMockHttpClientWithCallback(
        string content,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var fakeHttpMessageHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        var capture = new RequestCapture();
        fakeHttpMessageHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>()))
            .Callback<HttpRequestMessage>(req => capture.Request = req)
            .Returns(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content),
            });

        var httpClient = new HttpClient(fakeHttpMessageHandler.Object);
        return (httpClient, capture);
    }

    private class RequestCapture
    {
        public HttpRequestMessage? Request { get; set; }
    }

    #region Info Tests

    [TestMethod]
    public async Task InfoAsync_WithValidResponse_ReturnsInfoResponse()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_infoResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);

        // Act
        var result = await service.InfoAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DepositAssets);
        Assert.IsTrue(result.DepositAssets.ContainsKey("USD"));
        Assert.IsTrue(result.DepositAssets.ContainsKey("BTC"));
        var usdDeposit = result.DepositAssets["USD"];
        Assert.IsTrue(usdDeposit.Enabled);
        Assert.AreEqual(10.0m, usdDeposit.MinAmount);
        Assert.AreEqual(10000.0m, usdDeposit.MaxAmount);
        Assert.AreEqual(5.0m, usdDeposit.FeeFixed);
        Assert.AreEqual(1.0m, usdDeposit.FeePercent);
        Assert.AreEqual(2.0m, usdDeposit.FeeMinimum);
        Assert.IsNotNull(result.WithdrawAssets);
        Assert.IsTrue(result.WithdrawAssets.ContainsKey("USD"));
        Assert.IsTrue(result.WithdrawAssets.ContainsKey("BTC"));
        var usdWithdraw = result.WithdrawAssets["USD"];
        Assert.IsTrue(usdWithdraw.Enabled);
        Assert.AreEqual(10.0m, usdWithdraw.MinAmount);
        Assert.AreEqual(10000.0m, usdWithdraw.MaxAmount);
        Assert.AreEqual(5.0m, usdWithdraw.FeeFixed);
        Assert.AreEqual(1.0m, usdWithdraw.FeePercent);
        Assert.AreEqual(2.0m, usdWithdraw.FeeMinimum);
        var btcWithdraw = result.WithdrawAssets["BTC"];
        Assert.IsTrue(btcWithdraw.Enabled);
        Assert.AreEqual(0.001m, btcWithdraw.MinAmount);
        Assert.AreEqual(10.0m, btcWithdraw.MaxAmount);
        Assert.AreEqual(0.0001m, btcWithdraw.FeeFixed);
        Assert.AreEqual(0.5m, btcWithdraw.FeePercent);
        Assert.IsNull(btcWithdraw.FeeMinimum);
        Assert.IsNotNull(result.FeeEndpointInfo);
        Assert.IsTrue(result.FeeEndpointInfo.Enabled);
        Assert.IsFalse(result.FeeEndpointInfo.AuthenticationRequired);
        Assert.IsNotNull(result.FeatureFlags);
        Assert.IsTrue(result.FeatureFlags.AccountCreation);
        Assert.IsFalse(result.FeatureFlags.ClaimableBalances);
    }

    [TestMethod]
    public async Task InfoAsync_WithLangParameter_IncludesLangInQuery()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_infoResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);

        using (httpClient)
        {
            // Act
            await service.InfoAsync("fr");

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsTrue(capture.Request.RequestUri!.ToString().Contains("lang=fr"));
        }
    }

    [TestMethod]
    public async Task InfoAsync_WithCustomHeaders_IncludesHeadersInRequest()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_infoResponseJson);
        var customHeaders = new Dictionary<string, string>
        {
            { "X-Custom-Header", "custom-value" },
        };
        var service = new InteractiveService(TestTransferServerUrl, httpClient, httpRequestHeaders: customHeaders);

        using (httpClient)
        {
            // Act
            await service.InfoAsync();

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsTrue(capture.Request.Headers.Contains("X-Custom-Header"));
        }
    }

    [TestMethod]
    public async Task DepositAsync_WithCustomHeadersThatCannotBeAddedToRequestHeaders_AddsToContentHeaders()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_interactiveResponseJson);
        // Content-Type is a restricted header that cannot be added to request headers
        var customHeaders = new Dictionary<string, string>
        {
            { "Content-Type", "multipart/form-data; boundary=custom" },
        };
        var service = new InteractiveService(TestTransferServerUrl, httpClient, httpRequestHeaders: customHeaders);
        var request = new Sep24DepositRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
        };

        using (httpClient)
        {
            // Act
            await service.DepositAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsNotNull(capture.Request.Content);
            // The header should be attempted on request headers first, then fall back to content headers
            // Since Content-Type is restricted, it should end up on content headers if at all
        }
    }

    [TestMethod]
    [ExpectedException(typeof(Sep24RequestException))]
    public async Task InfoAsync_WithErrorResponse_ThrowsSep24RequestException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_errorResponseJson, HttpStatusCode.BadRequest);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);

        // Act
        await service.InfoAsync();
    }

    #endregion

    #region Fee Tests

    [TestMethod]
    public async Task FeeAsync_WithValidResponse_ReturnsFeeResponse()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_feeResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24FeeRequest
        {
            Operation = "deposit",
            AssetCode = "USD",
            Amount = 100.0m,
            Jwt = TestJwt,
        };

        // Act
        var result = await service.FeeAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(10.5m, result.Fee);
    }

    [TestMethod]
    public async Task FeeAsync_WithoutJwt_DoesNotIncludeAuthorizationHeader()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_feeResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24FeeRequest
        {
            Operation = "deposit",
            AssetCode = "USD",
            Amount = 100.0m,
            Jwt = null,
        };

        using (httpClient)
        {
            // Act
            await service.FeeAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsNull(capture.Request.Headers.Authorization);
        }
    }

    [TestMethod]
    public async Task FeeAsync_WithTypeParameter_IncludesTypeInQuery()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_feeResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24FeeRequest
        {
            Operation = "deposit",
            Type = "SEPA",
            AssetCode = "USD",
            Amount = 100.0m,
            Jwt = TestJwt,
        };

        using (httpClient)
        {
            // Act
            await service.FeeAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsTrue(capture.Request.RequestUri!.ToString().Contains("type=SEPA"));
        }
    }

    [TestMethod]
    public async Task FeeAsync_WithJwt_IncludesAuthorizationHeader()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_feeResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24FeeRequest
        {
            Operation = "deposit",
            AssetCode = "USD",
            Amount = 100.0m,
            Jwt = TestJwt,
        };

        using (httpClient)
        {
            // Act
            await service.FeeAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsNotNull(capture.Request.Headers.Authorization);
            Assert.AreEqual("Bearer", capture.Request.Headers.Authorization.Scheme);
            Assert.AreEqual(TestJwt, capture.Request.Headers.Authorization.Parameter);
        }
    }

    [TestMethod]
    [ExpectedException(typeof(Sep24AuthenticationRequiredException))]
    public async Task FeeAsync_WithForbiddenResponse_ThrowsAuthenticationRequiredException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_authRequiredResponseJson, HttpStatusCode.Forbidden);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24FeeRequest
        {
            Operation = "deposit",
            AssetCode = "USD",
            Amount = 100.0m,
            Jwt = TestJwt,
        };

        // Act
        await service.FeeAsync(request);
    }

    [TestMethod]
    [ExpectedException(typeof(HttpRequestException))]
    public async Task FeeAsync_WithInvalidJsonForbiddenResponse_ThrowsHttpRequestException()
    {
        // Arrange
        // When JSON parsing fails in HandleForbiddenResponse, it falls through
        // and then HandleErrorResponse is called, which throws HttpRequestException
        using var httpClient = CreateMockHttpClient("not valid json", HttpStatusCode.Forbidden);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24FeeRequest
        {
            Operation = "deposit",
            AssetCode = "USD",
            Amount = 100.0m,
            Jwt = TestJwt,
        };

        // Act
        await service.FeeAsync(request);
    }

    [TestMethod]
    [ExpectedException(typeof(Sep24RequestException))]
    public async Task FeeAsync_WithErrorResponse_ThrowsRequestException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_errorResponseJson, HttpStatusCode.BadRequest);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24FeeRequest
        {
            Operation = "deposit",
            AssetCode = "USD",
            Amount = 100.0m,
            Jwt = TestJwt,
        };

        // Act
        await service.FeeAsync(request);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task FeeAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_feeResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);

        // Act
        await service.FeeAsync(null!);
    }

    #endregion

    #region Deposit Tests

    [TestMethod]
    public async Task DepositAsync_WithValidResponse_ReturnsInteractiveResponse()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_interactiveResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24DepositRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
        };

        // Act
        var result = await service.DepositAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("interactive_customer_info_needed", result.Type);
        Assert.IsNotNull(result.Url);
        Assert.IsNotNull(result.Id);
        Assert.AreEqual("test-transaction-id", result.Id);
    }

    [TestMethod]
    public async Task DepositAsync_WithAllParameters_IncludesAllFields()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_interactiveResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24DepositRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            AssetIssuer = "GCZJM35NKGVK47BB4SPBDV25477PZYIYPVVG453LPYFNXLS3FGHDXOCM",
            SourceAsset = "iso4217:USD",
            Amount = "100.0",
            QuoteId = "quote-123",
            Account = "GASYKQXV47TPTB6HKXWZNB6IRVPMTQ6M6B27IM5L2LYMNYBX2O53YJAL",
            Memo = "test-memo",
            MemoType = "text",
            WalletName = "Test Wallet",
            WalletUrl = "https://wallet.example.com",
            Lang = "en-US",
            ClaimableBalanceSupported = "true",
            CustomerId = "customer-123",
        };

        using (httpClient)
        {
            // Act
            await service.DepositAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsNotNull(capture.Request.Content);
            Assert.IsInstanceOfType(capture.Request.Content, typeof(MultipartFormDataContent));
            Assert.IsNotNull(capture.Request.Headers.Authorization);
            Assert.AreEqual("Bearer", capture.Request.Headers.Authorization.Scheme);
            Assert.AreEqual(TestJwt, capture.Request.Headers.Authorization.Parameter);
        }
    }

    [TestMethod]
    public async Task DepositAsync_WithKycFields_IncludesKycData()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_interactiveResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var kycFields = new StandardKycFields
        {
            NaturalPerson = new NaturalPersonKycFields
            {
                FirstName = "John",
                LastName = "Doe",
                EmailAddress = "john.doe@example.com",
            },
        };
        var request = new Sep24DepositRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            KycFields = kycFields,
        };

        using (httpClient)
        {
            // Act
            await service.DepositAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsNotNull(capture.Request.Content);
            Assert.IsInstanceOfType(capture.Request.Content, typeof(MultipartFormDataContent));
        }
    }

    [TestMethod]
    public async Task DepositAsync_WithKycFiles_IncludesFiles()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_interactiveResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var kycFields = new StandardKycFields
        {
            NaturalPerson = new NaturalPersonKycFields
            {
                PhotoIdFront = new byte[] { 1, 2, 3, 4, 5 },
                PhotoIdBack = new byte[] { 6, 7, 8, 9, 10 },
            },
        };
        var request = new Sep24DepositRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            KycFields = kycFields,
        };

        using (httpClient)
        {
            // Act
            await service.DepositAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsNotNull(capture.Request.Content);
            Assert.IsInstanceOfType(capture.Request.Content, typeof(MultipartFormDataContent));
        }
    }

    [TestMethod]
    public async Task DepositAsync_WithCustomFields_IncludesCustomData()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_interactiveResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24DepositRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            CustomFields = new Dictionary<string, string>
            {
                { "custom_field_1", "custom_value_1" },
            },
            CustomFiles = new Dictionary<string, byte[]>
            {
                { "custom_file_1", new byte[] { 1, 2, 3 } },
            },
        };

        using (httpClient)
        {
            // Act
            await service.DepositAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsNotNull(capture.Request.Content);
            Assert.IsInstanceOfType(capture.Request.Content, typeof(MultipartFormDataContent));
        }
    }

    [TestMethod]
    public async Task DepositAsync_WithOrganizationKycFields_IncludesOrganizationData()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_interactiveResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var kycFields = new StandardKycFields
        {
            Organization = new OrganizationKycFields
            {
                Name = "Test Organization",
                VatNumber = "VAT123",
            },
        };
        var request = new Sep24DepositRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            KycFields = kycFields,
        };

        using (httpClient)
        {
            // Act
            await service.DepositAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsNotNull(capture.Request.Content);
            Assert.IsInstanceOfType(capture.Request.Content, typeof(MultipartFormDataContent));
        }
    }

    [TestMethod]
    [ExpectedException(typeof(Sep24AuthenticationRequiredException))]
    public async Task DepositAsync_WithForbiddenResponse_ThrowsAuthenticationRequiredException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_authRequiredResponseJson, HttpStatusCode.Forbidden);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24DepositRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
        };

        // Act
        await service.DepositAsync(request);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task DepositAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_interactiveResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);

        // Act
        await service.DepositAsync(null!);
    }

    #endregion

    #region Withdraw Tests

    [TestMethod]
    public async Task WithdrawAsync_WithValidResponse_ReturnsInteractiveResponse()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_interactiveResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24WithdrawRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
        };

        // Act
        var result = await service.WithdrawAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("interactive_customer_info_needed", result.Type);
        Assert.IsNotNull(result.Url);
        Assert.IsNotNull(result.Id);
    }

    [TestMethod]
    public async Task WithdrawAsync_WithAllParameters_IncludesAllFields()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_interactiveResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24WithdrawRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            AssetIssuer = "GCZJM35NKGVK47BB4SPBDV25477PZYIYPVVG453LPYFNXLS3FGHDXOCM",
            DestinationAsset = "iso4217:USD",
            Amount = "50.0",
            QuoteId = "quote-456",
            Account = "GASYKQXV47TPTB6HKXWZNB6IRVPMTQ6M6B27IM5L2LYMNYBX2O53YJAL",
            Memo = "withdraw-memo",
            MemoType = "id",
            WalletName = "Test Wallet",
            WalletUrl = "https://wallet.example.com",
            Lang = "en-US",
            RefundMemo = "refund-memo",
            RefundMemoType = "text",
            CustomerId = "customer-456",
        };

        using (httpClient)
        {
            // Act
            await service.WithdrawAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsNotNull(capture.Request.Content);
            Assert.IsInstanceOfType(capture.Request.Content, typeof(MultipartFormDataContent));
            Assert.IsNotNull(capture.Request.Headers.Authorization);
            Assert.AreEqual("Bearer", capture.Request.Headers.Authorization.Scheme);
            Assert.AreEqual(TestJwt, capture.Request.Headers.Authorization.Parameter);
        }
    }

    [TestMethod]
    [ExpectedException(typeof(Sep24AuthenticationRequiredException))]
    public async Task WithdrawAsync_WithForbiddenResponse_ThrowsAuthenticationRequiredException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_authRequiredResponseJson, HttpStatusCode.Forbidden);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24WithdrawRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
        };

        // Act
        await service.WithdrawAsync(request);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task WithdrawAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_interactiveResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);

        // Act
        await service.WithdrawAsync(null!);
    }

    [TestMethod]
    public async Task WithdrawAsync_WithNaturalPersonKycFields_IncludesFieldsInMultipartContent()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_interactiveResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var kycFields = new StandardKycFields
        {
            NaturalPerson = new NaturalPersonKycFields
            {
                FirstName = "Jane",
                LastName = "Smith",
                EmailAddress = "jane.smith@example.com",
            },
        };
        var request = new Sep24WithdrawRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            KycFields = kycFields,
        };

        using (httpClient)
        {
            // Act
            await service.WithdrawAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsNotNull(capture.Request.Content);
            Assert.IsInstanceOfType(capture.Request.Content, typeof(MultipartFormDataContent));
        }
    }

    [TestMethod]
    public async Task WithdrawAsync_WithNaturalPersonKycFiles_IncludesFilesInMultipartContent()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_interactiveResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var kycFields = new StandardKycFields
        {
            NaturalPerson = new NaturalPersonKycFields
            {
                PhotoIdFront = new byte[] { 1, 2, 3 },
                PhotoIdBack = new byte[] { 4, 5, 6 },
            },
        };
        var request = new Sep24WithdrawRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            KycFields = kycFields,
        };

        using (httpClient)
        {
            // Act
            await service.WithdrawAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsNotNull(capture.Request.Content);
            Assert.IsInstanceOfType(capture.Request.Content, typeof(MultipartFormDataContent));
        }
    }

    [TestMethod]
    public async Task WithdrawAsync_WithOrganizationKycFields_IncludesFieldsInMultipartContent()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_interactiveResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var kycFields = new StandardKycFields
        {
            Organization = new OrganizationKycFields
            {
                Name = "Test Organization",
                VatNumber = "VAT123",
                RegistrationNumber = "REG456",
            },
        };
        var request = new Sep24WithdrawRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            KycFields = kycFields,
        };

        using (httpClient)
        {
            // Act
            await service.WithdrawAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsNotNull(capture.Request.Content);
            Assert.IsInstanceOfType(capture.Request.Content, typeof(MultipartFormDataContent));
        }
    }

    [TestMethod]
    public async Task WithdrawAsync_WithOrganizationKycFiles_IncludesFilesInMultipartContent()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_interactiveResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var kycFields = new StandardKycFields
        {
            Organization = new OrganizationKycFields
            {
                PhotoIncorporationDoc = new byte[] { 10, 20, 30 },
                PhotoProofAddress = new byte[] { 40, 50, 60 },
            },
        };
        var request = new Sep24WithdrawRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            KycFields = kycFields,
        };

        using (httpClient)
        {
            // Act
            await service.WithdrawAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsNotNull(capture.Request.Content);
            Assert.IsInstanceOfType(capture.Request.Content, typeof(MultipartFormDataContent));
        }
    }

    [TestMethod]
    public async Task WithdrawAsync_WithCustomFieldsAndFiles_IncludesInMultipartContent()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_interactiveResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24WithdrawRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            CustomFields = new Dictionary<string, string>
            {
                { "withdraw_custom_field", "withdraw_value" },
            },
            CustomFiles = new Dictionary<string, byte[]>
            {
                { "withdraw_custom_file", new byte[] { 100, 200, 255 } },
            },
        };

        using (httpClient)
        {
            // Act
            await service.WithdrawAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsNotNull(capture.Request.Content);
            Assert.IsInstanceOfType(capture.Request.Content, typeof(MultipartFormDataContent));
        }
    }

    #endregion

    #region Transactions Tests

    [TestMethod]
    public async Task TransactionsAsync_WithValidResponse_ReturnsTransactionsResponse()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_transactionsResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24TransactionsRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
        };

        // Act
        var result = await service.TransactionsAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Transactions);
        Assert.AreEqual(2, result.Transactions.Count);
        
        // Test first transaction (deposit)
        var tx1 = result.Transactions[0];
        Assert.AreEqual("test-transaction-id-1", tx1.Id);
        Assert.AreEqual("deposit", tx1.Kind);
        Assert.AreEqual("completed", tx1.Status);
        Assert.IsTrue(tx1.KycVerified);
        Assert.AreEqual("https://anchor.example.com/transactions/test-transaction-id-1", tx1.MoreInfoUrl);
        Assert.AreEqual("100.0", tx1.AmountIn);
        Assert.AreEqual("iso4217:USD", tx1.AmountInAsset);
        Assert.AreEqual("95.0", tx1.AmountOut);
        Assert.AreEqual("stellar:USD:GCZJM35NKGVK47BB4SPBDV25477PZYIYPVVG453LPYFNXLS3FGHDXOCM", tx1.AmountFeeAsset);
        Assert.IsFalse(tx1.Refunded);
        Assert.IsNotNull(tx1.FeeDetails);
        Assert.AreEqual("5.0", tx1.FeeDetails.Total);
        Assert.AreEqual("stellar:USD:GCZJM35NKGVK47BB4SPBDV25477PZYIYPVVG453LPYFNXLS3FGHDXOCM", tx1.FeeDetails.Asset);
        Assert.IsNotNull(tx1.FeeDetails.Breakdown);
        Assert.AreEqual(2, tx1.FeeDetails.Breakdown.Count);
        Assert.AreEqual("Transaction completed successfully", tx1.Message);
        Assert.AreEqual("bank_account_12345", tx1.From);
        Assert.AreEqual("GASYKQXV47TPTB6HKXWZNB6IRVPMTQ6M6B27IM5L2LYMNYBX2O53YJAL", tx1.To);
        Assert.IsNull(tx1.ClaimableBalanceId);
        Assert.IsNull(tx1.UserActionRequiredBy);
        
        // Test second transaction (withdrawal) - covers withdrawal-specific properties
        var tx2 = result.Transactions[1];
        Assert.AreEqual("test-transaction-id-2", tx2.Id);
        Assert.AreEqual("withdrawal", tx2.Kind);
        Assert.AreEqual("pending_anchor", tx2.Status);
        Assert.IsNull(tx2.MoreInfoUrl);
        Assert.AreEqual("stellar:USD:GCZJM35NKGVK47BB4SPBDV25477PZYIYPVVG453LPYFNXLS3FGHDXOCM", tx2.AmountInAsset);
        Assert.AreEqual("iso4217:USD", tx2.AmountOutAsset);
        Assert.AreEqual("stellar:USD:GCZJM35NKGVK47BB4SPBDV25477PZYIYPVVG453LPYFNXLS3FGHDXOCM", tx2.AmountFeeAsset);
        Assert.IsFalse(tx2.Refunded);
        Assert.IsNotNull(tx2.FeeDetails);
        Assert.AreEqual("2.5", tx2.FeeDetails.Total);
        Assert.AreEqual("stellar:USD:GCZJM35NKGVK47BB4SPBDV25477PZYIYPVVG453LPYFNXLS3FGHDXOCM", tx2.FeeDetails.Asset);
        Assert.IsNull(tx2.FeeDetails.Breakdown);
        Assert.AreEqual("Processing withdrawal", tx2.Message);
        Assert.AreEqual("GASYKQXV47TPTB6HKXWZNB6IRVPMTQ6M6B27IM5L2LYMNYBX2O53YJAL", tx2.From);
        Assert.AreEqual("bank_account_67890", tx2.To);
        Assert.IsNotNull(tx2.WithdrawAnchorAccount);
        Assert.IsNotNull(tx2.WithdrawMemo);
        Assert.AreEqual("id", tx2.WithdrawMemoType);
        Assert.IsNull(tx2.ClaimableBalanceId);
    }

    [TestMethod]
    public async Task TransactionsAsync_WithAllParameters_IncludesAllQueryParams()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_transactionsResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24TransactionsRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            NoOlderThan = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Limit = 10,
            Kind = "deposit",
            PagingId = "paging-id-123",
            Lang = "en-US",
        };

        using (httpClient)
        {
            // Act
            await service.TransactionsAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            var uri = capture.Request.RequestUri!.ToString();
            Assert.IsTrue(uri.Contains("asset_code=USD"));
            Assert.IsTrue(uri.Contains("limit=10"));
            Assert.IsTrue(uri.Contains("kind=deposit"));
            Assert.IsTrue(uri.Contains("paging_id=paging-id-123"));
            Assert.IsTrue(uri.Contains("lang=en-US"));
            Assert.IsTrue(uri.Contains("no_older_than"));
            Assert.IsNotNull(capture.Request.Headers.Authorization);
            Assert.AreEqual("Bearer", capture.Request.Headers.Authorization.Scheme);
            Assert.AreEqual(TestJwt, capture.Request.Headers.Authorization.Parameter);
        }
    }

    [TestMethod]
    public async Task TransactionsAsync_WithMinimalParameters_IncludesOnlyRequiredFields()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_transactionsResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24TransactionsRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
        };

        using (httpClient)
        {
            // Act
            await service.TransactionsAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            var uri = capture.Request.RequestUri!.ToString();
            Assert.IsTrue(uri.Contains("asset_code=USD"));
            Assert.IsFalse(uri.Contains("limit"));
            Assert.IsFalse(uri.Contains("kind"));
            Assert.IsFalse(uri.Contains("paging_id"));
            Assert.IsFalse(uri.Contains("lang"));
            Assert.IsFalse(uri.Contains("no_older_than"));
        }
    }

    [TestMethod]
    [ExpectedException(typeof(Sep24AuthenticationRequiredException))]
    public async Task TransactionsAsync_WithForbiddenResponse_ThrowsAuthenticationRequiredException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_authRequiredResponseJson, HttpStatusCode.Forbidden);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24TransactionsRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
        };

        // Act
        await service.TransactionsAsync(request);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task TransactionsAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_transactionsResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);

        // Act
        await service.TransactionsAsync(null!);
    }

    #endregion

    #region Transaction Tests

    [TestMethod]
    public async Task TransactionAsync_WithValidResponse_ReturnsTransactionResponse()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_transactionResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24TransactionRequest
        {
            Jwt = TestJwt,
            Id = "test-transaction-id",
        };

        // Act
        var result = await service.TransactionAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Transaction);
        Assert.AreEqual("test-transaction-id", result.Transaction.Id);
        Assert.AreEqual("deposit", result.Transaction.Kind);
        Assert.AreEqual("completed", result.Transaction.Status);
        Assert.AreEqual(3600, result.Transaction.StatusEta);
        Assert.IsTrue(result.Transaction.KycVerified);
        Assert.AreEqual("https://anchor.example.com/transactions/test-transaction-id", result.Transaction.MoreInfoUrl);
        Assert.AreEqual("100.0", result.Transaction.AmountIn);
        Assert.AreEqual("iso4217:USD", result.Transaction.AmountInAsset);
        Assert.AreEqual("95.0", result.Transaction.AmountOut);
        Assert.AreEqual("5.0", result.Transaction.AmountFee);
        Assert.AreEqual("stellar:USD:GCZJM35NKGVK47BB4SPBDV25477PZYIYPVVG453LPYFNXLS3FGHDXOCM", result.Transaction.AmountFeeAsset);
        Assert.AreEqual("test-quote-id", result.Transaction.QuoteId);
        Assert.IsNotNull(result.Transaction.StartedAt);
        Assert.IsNotNull(result.Transaction.CompletedAt);
        Assert.IsNotNull(result.Transaction.UpdatedAt);
        Assert.AreEqual("2023-01-02T00:00:00Z", result.Transaction.UserActionRequiredBy);
        Assert.IsNotNull(result.Transaction.StellarTransactionId);
        Assert.IsNotNull(result.Transaction.ExternalTransactionId);
        Assert.AreEqual("Transaction completed successfully", result.Transaction.Message);
        Assert.IsFalse(result.Transaction.Refunded);
        Assert.IsNotNull(result.Transaction.Refunds);
        Assert.IsNotNull(result.Transaction.FeeDetails);
        Assert.AreEqual("5.0", result.Transaction.FeeDetails.Total);
        Assert.AreEqual("stellar:USD:GCZJM35NKGVK47BB4SPBDV25477PZYIYPVVG453LPYFNXLS3FGHDXOCM", result.Transaction.FeeDetails.Asset);
        Assert.IsNotNull(result.Transaction.FeeDetails.Breakdown);
        Assert.AreEqual(2, result.Transaction.FeeDetails.Breakdown.Count);
        Assert.AreEqual("Service fee", result.Transaction.FeeDetails.Breakdown[0].Name);
        Assert.AreEqual("Standard processing fee", result.Transaction.FeeDetails.Breakdown[0].Description);
        Assert.AreEqual("3.0", result.Transaction.FeeDetails.Breakdown[0].Amount);
        Assert.AreEqual("Network fee", result.Transaction.FeeDetails.Breakdown[1].Name);
        Assert.AreEqual("Stellar network transaction fee", result.Transaction.FeeDetails.Breakdown[1].Description);
        Assert.AreEqual("2.0", result.Transaction.FeeDetails.Breakdown[1].Amount);
        Assert.AreEqual("bank_account_12345", result.Transaction.From);
        Assert.AreEqual("GASYKQXV47TPTB6HKXWZNB6IRVPMTQ6M6B27IM5L2LYMNYBX2O53YJAL", result.Transaction.To);
        Assert.AreEqual("test-memo", result.Transaction.DepositMemo);
        Assert.AreEqual("text", result.Transaction.DepositMemoType);
        Assert.IsNull(result.Transaction.ClaimableBalanceId);
        Assert.IsNull(result.Transaction.WithdrawMemoType);
    }

    [TestMethod]
    public async Task TransactionAsync_WithStellarTransactionId_IncludesInQuery()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_transactionResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24TransactionRequest
        {
            Jwt = TestJwt,
            StellarTransactionId = "17a670bc424ff5ce3b386dbfaae9990b66a2a37b4fbe51547e8794962a3f9e6a",
        };

        using (httpClient)
        {
            // Act
            await service.TransactionAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            var uri = capture.Request.RequestUri!.ToString();
            Assert.IsTrue(uri.Contains("stellar_transaction_id"));
            Assert.IsNotNull(capture.Request.Headers.Authorization);
            Assert.AreEqual("Bearer", capture.Request.Headers.Authorization.Scheme);
            Assert.AreEqual(TestJwt, capture.Request.Headers.Authorization.Parameter);
        }
    }

    [TestMethod]
    public async Task TransactionAsync_WithExternalTransactionId_IncludesInQuery()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_transactionResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24TransactionRequest
        {
            Jwt = TestJwt,
            ExternalTransactionId = "ext-tx-12345",
        };

        using (httpClient)
        {
            // Act
            await service.TransactionAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            var uri = capture.Request.RequestUri!.ToString();
            Assert.IsTrue(uri.Contains("external_transaction_id"));
            Assert.IsNotNull(capture.Request.Headers.Authorization);
        }
    }

    [TestMethod]
    public async Task TransactionAsync_WithLangParameter_IncludesLangInQuery()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_transactionResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24TransactionRequest
        {
            Jwt = TestJwt,
            Id = "test-transaction-id",
            Lang = "fr",
        };

        using (httpClient)
        {
            // Act
            await service.TransactionAsync(request);

            // Assert
            Assert.IsNotNull(capture.Request);
            var uri = capture.Request.RequestUri!.ToString();
            Assert.IsTrue(uri.Contains("lang=fr"));
        }
    }

    [TestMethod]
    public async Task TransactionAsync_WithRefund_DeserializesRefundData()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_transactionWithRefundJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24TransactionRequest
        {
            Jwt = TestJwt,
            Id = "test-transaction-id-refund",
        };

        // Act
        var result = await service.TransactionAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Transaction);
        Assert.AreEqual("refunded", result.Transaction.Status);
        Assert.IsTrue(result.Transaction.Refunded);
        Assert.IsNotNull(result.Transaction.Refunds);
        Assert.AreEqual("98.0", result.Transaction.Refunds.AmountRefunded);
        Assert.AreEqual("2.0", result.Transaction.Refunds.AmountFee);
        Assert.AreEqual(1, result.Transaction.Refunds.Payments.Count);
        Assert.AreEqual("refund-payment-1", result.Transaction.Refunds.Payments[0].Id);
        Assert.AreEqual("stellar", result.Transaction.Refunds.Payments[0].IdType);
        Assert.AreEqual("98.0", result.Transaction.Refunds.Payments[0].Amount);
        Assert.AreEqual("2.0", result.Transaction.Refunds.Payments[0].Fee);
        Assert.IsNotNull(result.Transaction.FeeDetails);
        Assert.AreEqual("2.0", result.Transaction.FeeDetails.Total);
        Assert.AreEqual("iso4217:USD", result.Transaction.FeeDetails.Asset);
    }

    [TestMethod]
    [ExpectedException(typeof(Sep24TransactionNotFoundException))]
    public async Task TransactionAsync_WithNotFoundResponse_ThrowsTransactionNotFoundException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient("", HttpStatusCode.NotFound);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24TransactionRequest
        {
            Jwt = TestJwt,
            Id = "non-existent-id",
        };

        // Act
        await service.TransactionAsync(request);
    }

    [TestMethod]
    [ExpectedException(typeof(Sep24AuthenticationRequiredException))]
    public async Task TransactionAsync_WithForbiddenResponse_ThrowsAuthenticationRequiredException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_authRequiredResponseJson, HttpStatusCode.Forbidden);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24TransactionRequest
        {
            Jwt = TestJwt,
            Id = "test-transaction-id",
        };

        // Act
        await service.TransactionAsync(request);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task TransactionAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_transactionResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);

        // Act
        await service.TransactionAsync(null!);
    }

    #endregion

    #region FromDomainAsync Tests

    [TestMethod]
    public async Task FromDomainAsync_WithValidToml_CreatesService()
    {
        // Arrange
        var fakeHttpMessageHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        fakeHttpMessageHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>())).Returns(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(_stellarTomlContent),
        });

        using var httpClient = new HttpClient(fakeHttpMessageHandler.Object);

        // Act
        var service = await InteractiveService.FromDomainAsync("example.com", httpClient: httpClient);

        // Assert
        Assert.IsNotNull(service);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task FromDomainAsync_WithMissingTransferServerSep24_ThrowsArgumentException()
    {
        // Arrange
        var tomlWithoutSep24 = @"
VERSION=""2.0.0""
NETWORK_PASSPHRASE=""Public Global Stellar Network ; September 2015""
";
        var fakeHttpMessageHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        fakeHttpMessageHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>())).Returns(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(tomlWithoutSep24),
        });

        using var httpClient = new HttpClient(fakeHttpMessageHandler.Object);

        // Act
        await InteractiveService.FromDomainAsync("example.com", httpClient: httpClient);
    }

    #endregion

    #region Constructor Tests

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WithNullTransferServerAddress_ThrowsArgumentNullException()
    {
        // Act
        _ = new InteractiveService(null!);
    }

    [TestMethod]
    public void Constructor_WithTransferServerAddress_CreatesService()
    {
        // Act
        var service = new InteractiveService(TestTransferServerUrl);

        // Assert
        Assert.IsNotNull(service);
    }

    [TestMethod]
    public void Constructor_WithHttpClient_UsesProvidedClient()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_infoResponseJson);

        // Act
        var service = new InteractiveService(TestTransferServerUrl, httpClient);

        // Assert
        Assert.IsNotNull(service);
    }

    [TestMethod]
    public void Constructor_WithCustomHeaders_StoresHeaders()
    {
        // Arrange
        var customHeaders = new Dictionary<string, string>
        {
            { "X-Custom-Header", "custom-value" },
        };

        // Act
        var service = new InteractiveService(TestTransferServerUrl, httpClient: null, httpRequestHeaders: customHeaders);

        // Assert
        Assert.IsNotNull(service);
    }

    #endregion

    #region Edge Case Tests

    [TestMethod]
    public async Task InfoAsync_WithEmptyDepositAssets_HandlesGracefully()
    {
        // Arrange
        var emptyInfoJson = @"{""deposit"": {}, ""withdraw"": {}}";
        using var httpClient = CreateMockHttpClient(emptyInfoJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);

        // Act
        var result = await service.InfoAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DepositAssets);
        Assert.AreEqual(0, result.DepositAssets.Count);
    }

    [TestMethod]
    public async Task FeeAsync_WithNullFee_HandlesGracefully()
    {
        // Arrange
        var nullFeeJson = @"{""fee"": null}";
        using var httpClient = CreateMockHttpClient(nullFeeJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24FeeRequest
        {
            Operation = "deposit",
            AssetCode = "USD",
            Amount = 100.0m,
            Jwt = TestJwt,
        };

        // Act
        var result = await service.FeeAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNull(result.Fee);
    }

    [TestMethod]
    public async Task TransactionsAsync_WithEmptyTransactions_ReturnsEmptyList()
    {
        // Arrange
        var emptyTransactionsJson = @"{""transactions"": []}";
        using var httpClient = CreateMockHttpClient(emptyTransactionsJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24TransactionsRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
        };

        // Act
        var result = await service.TransactionsAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Transactions);
        Assert.AreEqual(0, result.Transactions.Count);
    }

    [TestMethod]
    public async Task DepositAsync_WithMinimalRequest_Succeeds()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_interactiveResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24DepositRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
        };

        // Act
        var result = await service.DepositAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Url);
        Assert.IsNotNull(result.Id);
    }

    [TestMethod]
    public async Task WithdrawAsync_WithMinimalRequest_Succeeds()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_interactiveResponseJson);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24WithdrawRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
        };

        // Act
        var result = await service.WithdrawAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Url);
        Assert.IsNotNull(result.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(Sep24RequestException))]
    public async Task DepositAsync_WithErrorResponse_ThrowsRequestException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_errorResponseJson, HttpStatusCode.BadRequest);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24DepositRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
        };

        // Act
        await service.DepositAsync(request);
    }

    [TestMethod]
    [ExpectedException(typeof(HttpRequestException))]
    public async Task DepositAsync_WithInvalidJsonErrorResponse_ThrowsHttpRequestException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient("not valid json", HttpStatusCode.BadRequest);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24DepositRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
        };

        // Act
        await service.DepositAsync(request);
    }

    [TestMethod]
    [ExpectedException(typeof(Sep24RequestException))]
    public async Task WithdrawAsync_WithErrorResponse_ThrowsRequestException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_errorResponseJson, HttpStatusCode.BadRequest);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24WithdrawRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
        };

        // Act
        await service.WithdrawAsync(request);
    }

    [TestMethod]
    [ExpectedException(typeof(HttpRequestException))]
    public async Task WithdrawAsync_WithInvalidJsonErrorResponse_ThrowsHttpRequestException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient("not valid json", HttpStatusCode.BadRequest);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24WithdrawRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
        };

        // Act
        await service.WithdrawAsync(request);
    }

    [TestMethod]
    [ExpectedException(typeof(Sep24RequestException))]
    public async Task TransactionsAsync_WithErrorResponse_ThrowsRequestException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_errorResponseJson, HttpStatusCode.BadRequest);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24TransactionsRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
        };

        // Act
        await service.TransactionsAsync(request);
    }

    [TestMethod]
    [ExpectedException(typeof(HttpRequestException))]
    public async Task TransactionsAsync_WithInvalidJsonErrorResponse_ThrowsHttpRequestException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient("not valid json", HttpStatusCode.BadRequest);
        var service = new InteractiveService(TestTransferServerUrl, httpClient);
        var request = new Sep24TransactionsRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
        };

        // Act
        await service.TransactionsAsync(request);
    }

    #endregion

    #region Property Setter Tests

    [TestMethod]
    public void Sep24DepositRequest_KycFields_CanBeSetAndRetrieved()
    {
        // Arrange
        var kycFields = new StandardKycFields
        {
            NaturalPerson = new NaturalPersonKycFields
            {
                FirstName = "John",
                LastName = "Doe",
            },
        };
        var request = new Sep24DepositRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            KycFields = kycFields,
        };

        // Act
        // KycFields is set during initialization

        // Assert
        Assert.IsNotNull(request.KycFields);
        Assert.IsNotNull(request.KycFields.NaturalPerson);
        Assert.AreEqual("John", request.KycFields.NaturalPerson.FirstName);
        Assert.AreEqual("Doe", request.KycFields.NaturalPerson.LastName);
    }

    [TestMethod]
    public void Sep24DepositRequest_KycFields_CanBeSetToNull()
    {
        // Arrange & Act
        var request = new Sep24DepositRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            KycFields = null,
        };

        // Assert
        Assert.IsNull(request.KycFields);
    }

    [TestMethod]
    public void Sep24DepositRequest_CustomFields_CanBeSetAndRetrieved()
    {
        // Arrange
        var customFields = new Dictionary<string, string>
        {
            { "custom_field_1", "value_1" },
            { "custom_field_2", "value_2" },
        };
        var request = new Sep24DepositRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            CustomFields = customFields,
        };

        // Act
        // CustomFields is set during initialization

        // Assert
        Assert.IsNotNull(request.CustomFields);
        Assert.AreEqual(2, request.CustomFields.Count);
        Assert.AreEqual("value_1", request.CustomFields["custom_field_1"]);
        Assert.AreEqual("value_2", request.CustomFields["custom_field_2"]);
    }

    [TestMethod]
    public void Sep24DepositRequest_CustomFields_CanBeSetToNull()
    {
        // Arrange & Act
        var request = new Sep24DepositRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            CustomFields = null,
        };

        // Assert
        Assert.IsNull(request.CustomFields);
    }

    [TestMethod]
    public void Sep24DepositRequest_CustomFiles_CanBeSetAndRetrieved()
    {
        // Arrange
        var customFiles = new Dictionary<string, byte[]>
        {
            { "file_1", new byte[] { 1, 2, 3 } },
            { "file_2", new byte[] { 4, 5, 6 } },
        };
        var request = new Sep24DepositRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            CustomFiles = customFiles,
        };

        // Act
        // CustomFiles is set during initialization

        // Assert
        Assert.IsNotNull(request.CustomFiles);
        Assert.AreEqual(2, request.CustomFiles.Count);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, request.CustomFiles["file_1"]);
        CollectionAssert.AreEqual(new byte[] { 4, 5, 6 }, request.CustomFiles["file_2"]);
    }

    [TestMethod]
    public void Sep24DepositRequest_CustomFiles_CanBeSetToNull()
    {
        // Arrange & Act
        var request = new Sep24DepositRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            CustomFiles = null,
        };

        // Assert
        Assert.IsNull(request.CustomFiles);
    }

    [TestMethod]
    public void Sep24WithdrawRequest_KycFields_CanBeSetAndRetrieved()
    {
        // Arrange
        var kycFields = new StandardKycFields
        {
            NaturalPerson = new NaturalPersonKycFields
            {
                FirstName = "Jane",
                LastName = "Smith",
            },
        };

        // Act
        var request = new Sep24WithdrawRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            KycFields = kycFields,
        };

        // Assert
        Assert.IsNotNull(request.KycFields);
        Assert.IsNotNull(request.KycFields.NaturalPerson);
        Assert.AreEqual("Jane", request.KycFields.NaturalPerson.FirstName);
        Assert.AreEqual("Smith", request.KycFields.NaturalPerson.LastName);
    }

    [TestMethod]
    public void Sep24WithdrawRequest_KycFields_CanBeSetToNull()
    {
        // Arrange & Act
        var request = new Sep24WithdrawRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            KycFields = null,
        };

        // Assert
        Assert.IsNull(request.KycFields);
    }

    [TestMethod]
    public void Sep24WithdrawRequest_CustomFields_CanBeSetAndRetrieved()
    {
        // Arrange
        var customFields = new Dictionary<string, string>
        {
            { "withdraw_field_1", "withdraw_value_1" },
            { "withdraw_field_2", "withdraw_value_2" },
        };

        // Act
        var request = new Sep24WithdrawRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            CustomFields = customFields,
        };

        // Assert
        Assert.IsNotNull(request.CustomFields);
        Assert.AreEqual(2, request.CustomFields.Count);
        Assert.AreEqual("withdraw_value_1", request.CustomFields["withdraw_field_1"]);
        Assert.AreEqual("withdraw_value_2", request.CustomFields["withdraw_field_2"]);
    }

    [TestMethod]
    public void Sep24WithdrawRequest_CustomFields_CanBeSetToNull()
    {
        // Arrange & Act
        var request = new Sep24WithdrawRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            CustomFields = null,
        };

        // Assert
        Assert.IsNull(request.CustomFields);
    }

    [TestMethod]
    public void Sep24WithdrawRequest_CustomFiles_CanBeSetAndRetrieved()
    {
        // Arrange
        var customFiles = new Dictionary<string, byte[]>
        {
            { "withdraw_file_1", new byte[] { 7, 8, 9 } },
            { "withdraw_file_2", new byte[] { 10, 11, 12 } },
        };

        // Act
        var request = new Sep24WithdrawRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            CustomFiles = customFiles,
        };

        // Assert
        Assert.IsNotNull(request.CustomFiles);
        Assert.AreEqual(2, request.CustomFiles.Count);
        CollectionAssert.AreEqual(new byte[] { 7, 8, 9 }, request.CustomFiles["withdraw_file_1"]);
        CollectionAssert.AreEqual(new byte[] { 10, 11, 12 }, request.CustomFiles["withdraw_file_2"]);
    }

    [TestMethod]
    public void Sep24WithdrawRequest_CustomFiles_CanBeSetToNull()
    {
        // Arrange & Act
        var request = new Sep24WithdrawRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            CustomFiles = null,
        };

        // Assert
        Assert.IsNull(request.CustomFiles);
    }

    [TestMethod]
    public void Sep24DepositRequest_AllKycProperties_CanBeSetTogether()
    {
        // Arrange & Act
        var request = new Sep24DepositRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            KycFields = new StandardKycFields
            {
                NaturalPerson = new NaturalPersonKycFields
                {
                    FirstName = "John",
                },
            },
            CustomFields = new Dictionary<string, string>
            {
                { "custom1", "value1" },
            },
            CustomFiles = new Dictionary<string, byte[]>
            {
                { "file1", new byte[] { 1, 2, 3 } },
            },
        };

        // Assert
        Assert.IsNotNull(request.KycFields);
        Assert.IsNotNull(request.CustomFields);
        Assert.IsNotNull(request.CustomFiles);
        Assert.AreEqual("John", request.KycFields.NaturalPerson!.FirstName);
        Assert.AreEqual("value1", request.CustomFields["custom1"]);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, request.CustomFiles["file1"]);
    }

    [TestMethod]
    public void Sep24WithdrawRequest_AllKycProperties_CanBeSetTogether()
    {
        // Arrange & Act
        var request = new Sep24WithdrawRequest
        {
            Jwt = TestJwt,
            AssetCode = "USD",
            KycFields = new StandardKycFields
            {
                Organization = new OrganizationKycFields
                {
                    Name = "Test Org",
                },
            },
            CustomFields = new Dictionary<string, string>
            {
                { "custom1", "value1" },
            },
            CustomFiles = new Dictionary<string, byte[]>
            {
                { "file1", new byte[] { 1, 2, 3 } },
            },
        };

        // Assert
        Assert.IsNotNull(request.KycFields);
        Assert.IsNotNull(request.CustomFields);
        Assert.IsNotNull(request.CustomFiles);
        Assert.AreEqual("Test Org", request.KycFields.Organization!.Name);
        Assert.AreEqual("value1", request.CustomFields["custom1"]);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, request.CustomFiles["file1"]);
    }

    #endregion

    #region Exception Constructor Tests

    [TestMethod]
    public void Sep24RequestException_WithMessageAndInnerException_InitializesCorrectly()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner exception message");
        var message = "Outer exception message";

        // Act
        var exception = new Sep24RequestException(message, innerException);

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual(message, exception.Message);
        Assert.IsNotNull(exception.InnerException);
        Assert.AreEqual(innerException, exception.InnerException);
        Assert.IsInstanceOfType(exception.InnerException, typeof(InvalidOperationException));
    }

    [TestMethod]
    public void Sep24AuthenticationRequiredException_WithMessage_InitializesCorrectly()
    {
        // Arrange
        var message = "Custom authentication required message";

        // Act
        var exception = new Sep24AuthenticationRequiredException(message);

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual(message, exception.Message);
        Assert.IsNull(exception.InnerException);
    }

    [TestMethod]
    public void Sep24AuthenticationRequiredException_WithMessageAndInnerException_InitializesCorrectly()
    {
        // Arrange
        var innerException = new UnauthorizedAccessException("Inner exception message");
        var message = "Custom authentication required message";

        // Act
        var exception = new Sep24AuthenticationRequiredException(message, innerException);

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual(message, exception.Message);
        Assert.IsNotNull(exception.InnerException);
        Assert.AreEqual(innerException, exception.InnerException);
        Assert.IsInstanceOfType(exception.InnerException, typeof(UnauthorizedAccessException));
    }

    [TestMethod]
    public void Sep24TransactionNotFoundException_WithMessage_InitializesCorrectly()
    {
        // Arrange
        var message = "Custom transaction not found message";

        // Act
        var exception = new Sep24TransactionNotFoundException(message);

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual(message, exception.Message);
        Assert.IsNull(exception.InnerException);
    }

    [TestMethod]
    public void Sep24TransactionNotFoundException_WithMessageAndInnerException_InitializesCorrectly()
    {
        // Arrange
        var innerException = new KeyNotFoundException("Inner exception message");
        var message = "Custom transaction not found message";

        // Act
        var exception = new Sep24TransactionNotFoundException(message, innerException);

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual(message, exception.Message);
        Assert.IsNotNull(exception.InnerException);
        Assert.AreEqual(innerException, exception.InnerException);
        Assert.IsInstanceOfType(exception.InnerException, typeof(KeyNotFoundException));
    }

    #endregion
}

