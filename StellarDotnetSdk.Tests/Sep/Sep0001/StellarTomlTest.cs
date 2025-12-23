using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Sep.Sep0001;
using StellarDotnetSdk.Sep.Sep0001.Exceptions;

namespace StellarDotnetSdk.Tests.Sep.Sep0001;

/// <summary>
///     Tests for StellarToml class functionality.
/// </summary>
[TestClass]
public class StellarTomlTest
{
    private string _sampleTomlContent = null!;
    private string _linkedCurrencyTomlContent = null!;

    [TestInitialize]
    public void Setup()
    {
        var testDataPath = Utils.GetTestDataAbsolutePath("stellar-toml-sample.toml");
        _sampleTomlContent = File.ReadAllText(testDataPath);

        var linkedCurrencyTomlPath = Utils.GetTestDataAbsolutePath("stellar-toml-linked-currency.toml");
        _linkedCurrencyTomlContent = File.ReadAllText(linkedCurrencyTomlPath);
    }

    /// <summary>
    ///     Creates a mock HttpClient that returns the specified content and status code.
    /// </summary>
    private static HttpClient CreateMockHttpClient(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return Utils.CreateFakeHttpClient(content, statusCode);
    }

    /// <summary>
    ///     Creates a mock HttpClient with a callback to capture the request message.
    ///     Returns both the HttpClient and a wrapper object that holds the captured request.
    /// </summary>
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

    /// <summary>
    ///     Wrapper class to hold captured HTTP request messages.
    /// </summary>
    private class RequestCapture
    {
        public HttpRequestMessage? Request { get; set; }
    }

    /// <summary>
    ///     Creates a mock HttpClient that throws the specified exception.
    /// </summary>
    private static HttpClient CreateMockHttpClientWithException(Exception exception)
    {
        var fakeHttpMessageHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        fakeHttpMessageHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>()))
            .Throws(exception);

        return new HttpClient(fakeHttpMessageHandler.Object);
    }

    /// <summary>
    ///     Asserts common currency properties.
    /// </summary>
    private static void AssertCurrencyProperties(
        Currency currency,
        string expectedCode,
        string expectedIssuer,
        int expectedDisplayDecimals,
        string? expectedName = null,
        string? expectedDesc = null,
        string? expectedConditions = null,
        string? expectedImage = null,
        int? expectedFixedNumber = null)
    {
        Assert.IsNotNull(currency);
        Assert.AreEqual(expectedCode, currency.Code);
        Assert.AreEqual(expectedIssuer, currency.Issuer);
        Assert.AreEqual(expectedDisplayDecimals, currency.DisplayDecimals);
        if (expectedName != null)
        {
            Assert.AreEqual(expectedName, currency.Name);
        }

        if (expectedDesc != null)
        {
            Assert.AreEqual(expectedDesc, currency.Desc);
        }

        if (expectedConditions != null)
        {
            Assert.AreEqual(expectedConditions, currency.Conditions);
        }

        if (expectedImage != null)
        {
            Assert.AreEqual(expectedImage, currency.Image);
        }

        if (expectedFixedNumber.HasValue)
        {
            Assert.AreEqual(expectedFixedNumber.Value, currency.FixedNumber);
        }
    }

    /// <summary>
    ///     Verifies that StellarToml constructor parses TOML content correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidToml_ParsesAllSections()
    {
        // Act
        var stellarToml = new StellarToml(_sampleTomlContent);

        // Assert - General Information
        var generalInfo = stellarToml.GeneralInformation;
        Assert.AreEqual("2.0.0", generalInfo.Version);
        Assert.AreEqual("Public Global Stellar Network ; September 2015", generalInfo.NetworkPassphrase);
        Assert.AreEqual("https://stellarid.io/federation/", generalInfo.FederationServer);
        Assert.AreEqual("https://api.domain.com/auth", generalInfo.AuthServer);
        Assert.AreEqual("https://api.domain.com", generalInfo.TransferServer);
        Assert.AreEqual("https://api.domain.com/sep24", generalInfo.TransferServerSep24);
        Assert.AreEqual("https://api.domain.com/kyc", generalInfo.KycServer);
        Assert.AreEqual("https://api.domain.com/auth", generalInfo.WebAuthEndpoint);
        Assert.AreEqual("GBBHQ7H4V6RRORKYLHTCAWP6MOHNORRFJSDPXDFYDGJB2LPZUFPXUEW3", generalInfo.SigningKey);
        Assert.AreEqual("https://horizon.domain.com", generalInfo.HorizonUrl);
        Assert.AreEqual(3, generalInfo.Accounts.Count);
        Assert.IsTrue(generalInfo.Accounts.Contains("GD5DJQDDBKGAYNEAXU562HYGOOSYAEOO6AS53PZXBOZGCP5M2OPGMZV3"));
        Assert.IsTrue(generalInfo.Accounts.Contains("GAENZLGHJGJRCMX5VCHOLHQXU3EMCU5XWDNU4BGGJFNLI2EL354IVBK7"));
        Assert.IsTrue(generalInfo.Accounts.Contains("GAOO3LWBC4XF6VWRP5ESJ6IBHAISVJMSBTALHOQM2EZG7Q477UWA6L7U"));
        Assert.AreEqual("GBWMCCC3NHSKLAOJDBKKYW7SSH2PFTTNVFKWSGLWGDLEBKLOVP5JLBBP", generalInfo.UriRequestSigningKey);
        Assert.AreEqual("https://test.direct-payment.com", generalInfo.DirectPaymentServer);
        Assert.AreEqual("https://test.anchor-quote.com", generalInfo.AnchorQuoteServer);
        Assert.AreEqual("https://api.example.com:8001/contracts/auth", generalInfo.WebAuthForContractsEndpoint);
        Assert.AreEqual("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC", generalInfo.WebAuthContractId);

        // Assert - Documentation
        Assert.IsNotNull(stellarToml.Documentation);
        var doc = stellarToml.Documentation!;
        Assert.AreEqual("Organization Name", doc.OrgName);
        Assert.AreEqual("Organization DBA", doc.OrgDba);
        Assert.AreEqual("https://www.domain.com", doc.OrgUrl);
        Assert.AreEqual("https://www.domain.com/awesomelogo.png", doc.OrgLogo);
        Assert.AreEqual("Description of issuer", doc.OrgDescription);
        Assert.AreEqual("123 Sesame Street, New York, NY 12345, United States", doc.OrgPhysicalAddress);
        Assert.AreEqual("https://www.domain.com/address_attestation.jpg", doc.OrgPhysicalAddressAttestation);
        Assert.AreEqual("+14155552671", doc.OrgPhoneNumber);
        Assert.AreEqual("https://www.domain.com/phone_attestation.jpg", doc.OrgPhoneNumberAttestation);
        Assert.AreEqual("accountname", doc.OrgKeybase);
        Assert.AreEqual("orgtweet", doc.OrgTwitter);
        Assert.AreEqual("orgcode", doc.OrgGithub);
        Assert.AreEqual("info@domain.com", doc.OrgOfficialEmail);
        Assert.AreEqual("support@domain.com", doc.OrgSupportEmail);
        Assert.AreEqual("State Banking Authority", doc.OrgLicensingAuthority);
        Assert.AreEqual("Money Transmitter", doc.OrgLicenseType);
        Assert.AreEqual("MT-12345", doc.OrgLicenseNumber);

        // Assert - Points of Contact
        Assert.IsNotNull(stellarToml.PointsOfContact);
        Assert.AreEqual(1, stellarToml.PointsOfContact!.Count);
        var pointOfContact = stellarToml.PointsOfContact[0];
        Assert.AreEqual("Jane Jedidiah Johnson", pointOfContact.Name);
        Assert.AreEqual("jane@domain.com", pointOfContact.Email);
        Assert.AreEqual("crypto_jane", pointOfContact.Keybase);
        Assert.AreEqual("crypto_jane", pointOfContact.Twitter);
        Assert.AreEqual("crypto_jane", pointOfContact.Github);
        Assert.IsNotNull(pointOfContact.IdPhotoHash);
        Assert.IsNotNull(pointOfContact.VerificationPhotoHash);

        // Assert - Currencies
        Assert.IsNotNull(stellarToml.Currencies);
        Assert.AreEqual(4, stellarToml.Currencies!.Count);

        var usdCurrency = stellarToml.Currencies[0];
        Assert.AreEqual("USD", usdCurrency.Code);
        Assert.AreEqual("GCZJM35NKGVK47BB4SPBDV25477PZYIYPVVG453LPYFNXLS3FGHDXOCM", usdCurrency.Issuer);
        Assert.AreEqual(2, usdCurrency.DisplayDecimals);

        var btcCurrency = stellarToml.Currencies[1];
        Assert.AreEqual("BTC", btcCurrency.Code);
        Assert.AreEqual("GAOO3LWBC4XF6VWRP5ESJ6IBHAISVJMSBTALHOQM2EZG7Q477UWA6L7U", btcCurrency.Issuer);
        Assert.AreEqual(7, btcCurrency.DisplayDecimals);
        Assert.AreEqual("crypto", btcCurrency.AnchorAssetType);
        Assert.AreEqual("BTC", btcCurrency.AnchorAsset);
        Assert.AreEqual("Use SEP6 with our federation server", btcCurrency.RedemptionInstructions);
        Assert.IsNotNull(btcCurrency.CollateralAddresses);
        Assert.AreEqual(1, btcCurrency.CollateralAddresses!.Count);
        Assert.IsTrue(btcCurrency.CollateralAddresses.Contains("2C1mCx3ukix1KfegAY5zgQJV7sanAciZpv"));
        Assert.IsNotNull(btcCurrency.CollateralAddressSignatures);
        Assert.AreEqual(1, btcCurrency.CollateralAddressSignatures!.Count);

        var goatCurrency = stellarToml.Currencies[2];
        Assert.AreEqual("GOAT", goatCurrency.Code);
        Assert.AreEqual("GD5T6IPRNCKFOHQWT264YPKOZAWUMMZOLZBJ6BNQMUGPWGRLBK3U7ZNP", goatCurrency.Issuer);
        Assert.AreEqual(2, goatCurrency.DisplayDecimals);
        Assert.AreEqual("goat share", goatCurrency.Name);
        Assert.IsNotNull(goatCurrency.Desc);
        Assert.IsNotNull(goatCurrency.Conditions);
        Assert.IsNotNull(goatCurrency.Image);
        Assert.AreEqual(10000, goatCurrency.FixedNumber);

        var contractCurrency = stellarToml.Currencies[3];
        Assert.AreEqual("CCRT", contractCurrency.Code);
        Assert.AreEqual("CC4DZNN2TPLUOAIRBI3CY7TGRFFCCW6GNVVRRQ3QIIBY6TM6M2RVMBMC", contractCurrency.Contract);

        // Assert - Validators
        Assert.IsNotNull(stellarToml.Validators);
        Assert.AreEqual(3, stellarToml.Validators!.Count);

        var validator1 = stellarToml.Validators[0];
        Assert.AreEqual("domain-au", validator1.Alias);
        Assert.AreEqual("Domain Australia", validator1.DisplayName);
        Assert.AreEqual("core-au.domain.com:11625", validator1.Host);
        Assert.AreEqual("GD5DJQDDBKGAYNEAXU562HYGOOSYAEOO6AS53PZXBOZGCP5M2OPGMZV3", validator1.PublicKey);
        Assert.AreEqual("http://history.domain.com/prd/core-live/core_live_001/", validator1.History);
    }

    /// <summary>
    ///     Verifies that StellarToml constructor handles malformed TOML headers correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_WithMalformedHeaders_CorrectsHeaders()
    {
        // Arrange - TOML with incorrect headers (using array-of-tables syntax where single table is expected)
        var malformedToml = @"
VERSION=""2.0.0""
NETWORK_PASSPHRASE=""Public Global Stellar Network ; September 2015""

[[DOCUMENTATION]]
ORG_NAME=""Test Org""
ORG_DBA=""Test DBA""

[PRINCIPALS]
name=""Test Principal""

[CURRENCIES]
code=""USD""
issuer=""GCZJM35NKGVK47BB4SPBDV25477PZYIYPVVG453LPYFNXLS3FGHDXOCM""

[VALIDATORS]
ALIAS=""test-validator""
PUBLIC_KEY=""GD5DJQDDBKGAYNEAXU562HYGOOSYAEOO6AS53PZXBOZGCP5M2OPGMZV3""
";

        // Act
        var stellarToml = new StellarToml(malformedToml);

        // Assert - Should parse without errors
        Assert.IsNotNull(stellarToml.GeneralInformation);
        Assert.IsNotNull(stellarToml.Documentation);
        Assert.AreEqual("Test Org", stellarToml.Documentation!.OrgName);
        // Note: The safeguard logic corrects headers, so [[DOCUMENTATION]] becomes [DOCUMENTATION]
        // and only the first one is parsed
    }

    /// <summary>
    ///     Verifies that FromDomainAsync fetches and parses stellar.toml correctly.
    /// </summary>
    [TestMethod]
    public async Task FromDomainAsync_WithValidDomain_FetchesAndParsesToml()
    {
        // Arrange
        var fakeHttpMessageHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        fakeHttpMessageHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>())).Returns(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(_sampleTomlContent),
        });

        using var httpClient = new HttpClient(fakeHttpMessageHandler.Object);

        // Act
        var stellarToml = await StellarToml.FromDomainAsync("example.com", httpClient);

        // Assert
        Assert.IsNotNull(stellarToml);
        Assert.AreEqual("2.0.0", stellarToml.GeneralInformation.Version);
        Assert.AreEqual("https://stellarid.io/federation/", stellarToml.GeneralInformation.FederationServer);

        fakeHttpMessageHandler.Verify(a => a.Send(It.IsAny<HttpRequestMessage>()));
        Assert.AreEqual(new Uri("https://example.com/.well-known/stellar.toml"),
            fakeHttpMessageHandler.Object.RequestUri);
    }

    /// <summary>
    ///     Verifies that FromDomainAsync throws exception when stellar.toml is not found.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(StellarTomlException))]
    public async Task FromDomainAsync_WithNotFound_ThrowsException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient("Not Found", HttpStatusCode.NotFound);

        // Act
        await StellarToml.FromDomainAsync("example.com", httpClient);
    }

    /// <summary>
    ///     Verifies that CurrencyFromUrlAsync loads currency from external TOML file.
    /// </summary>
    [TestMethod]
    public async Task CurrencyFromUrlAsync_WithValidUrl_LoadsCurrency()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_linkedCurrencyTomlContent);

        // Act
        var currency = await StellarToml.CurrencyFromUrlAsync("https://example.com/.well-known/TESTC.toml", httpClient);

        // Assert
        AssertCurrencyProperties(
            currency,
            "TESTC",
            "GCPWPTAX6QVJQIQARN2WESISHVLN65D4HAGQECHLCAV22UST3W2Q6QTA",
            2,
            "test currency",
            "TESTC description",
            "TESTC conditions",
            "https://soneso.com/123.png",
            10000);
    }

    /// <summary>
    ///     Verifies that CurrencyFromUrlAsync throws exception when currency TOML is not found.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(StellarTomlException))]
    public async Task CurrencyFromUrlAsync_WithNotFound_ThrowsException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient("Not Found", HttpStatusCode.NotFound);

        // Act
        await StellarToml.CurrencyFromUrlAsync("https://example.com/.well-known/CURRENCY.toml", httpClient);
    }

    /// <summary>
    ///     Verifies that CurrencyFromUrlAsync correctly handles custom HTTP headers.
    /// </summary>
    [TestMethod]
    public async Task CurrencyFromUrlAsync_WithCustomHeaders_IncludesHeadersInRequest()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_linkedCurrencyTomlContent);
        var customHeaders = new Dictionary<string, string>
        {
            { "X-Custom-Header", "custom-value" },
            { "Authorization", "Bearer token123" },
        };

        using (httpClient)
        {
            // Act
            await StellarToml.CurrencyFromUrlAsync("https://example.com/.well-known/TESTC.toml", httpClient, customHeaders);

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsTrue(capture.Request.Headers.Contains("X-Custom-Header"));
            Assert.IsTrue(capture.Request.Headers.Contains("Authorization"));
        }
    }

    /// <summary>
    ///     Verifies that FromDomainAsync throws exception when domain is null or empty.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task FromDomainAsync_WithNullDomain_ThrowsArgumentException()
    {
        // Act
        await StellarToml.FromDomainAsync(null!);
    }

    /// <summary>
    ///     Verifies that CurrencyFromUrlAsync throws exception when URL is invalid.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task CurrencyFromUrlAsync_WithInvalidUrl_ThrowsArgumentException()
    {
        // Act
        await StellarToml.CurrencyFromUrlAsync("not-a-valid-url");
    }

    /// <summary>
    ///     Verifies that CurrencyFromUrlAsync throws exception when URL is null or empty.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task CurrencyFromUrlAsync_WithEmptyUrl_ThrowsArgumentException()
    {
        // Act
        await StellarToml.CurrencyFromUrlAsync("");
    }

    /// <summary>
    ///     Verifies that FromDomainAsync correctly handles custom HTTP headers.
    /// </summary>
    [TestMethod]
    public async Task FromDomainAsync_WithCustomHeaders_IncludesHeadersInRequest()
    {
        // Arrange
        var (httpClient, capture) = CreateMockHttpClientWithCallback(_sampleTomlContent);
        var customHeaders = new Dictionary<string, string>
        {
            { "X-Custom-Header", "custom-value" },
            { "Authorization", "Bearer token123" },
        };

        using (httpClient)
        {
            // Act
            await StellarToml.FromDomainAsync("example.com", httpClient, customHeaders);

            // Assert
            Assert.IsNotNull(capture.Request);
            Assert.IsTrue(capture.Request.Headers.Contains("X-Custom-Header"));
            Assert.IsTrue(capture.Request.Headers.Contains("Authorization"));
        }
    }

    /// <summary>
    ///     Verifies that FromDomainAsync overload with bearerToken and resilienceOptions works correctly.
    ///     This indirectly tests GetOrCreateHttpClient by verifying the overload accepts these parameters.
    /// </summary>
    [TestMethod]
    public async Task FromDomainAsync_WithBearerTokenAndResilienceOptions_WorksCorrectly()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_sampleTomlContent);
        var resilienceOptions = HttpResilienceOptionsPresets.WithConnectionRetries();

        // Act - Call with bearerToken and resilienceOptions (GetOrCreateHttpClient is called internally when httpClient is null,
        // but we provide httpClient here to avoid real network calls. The test verifies the overload signature works.)
        var stellarToml = await StellarToml.FromDomainAsync("example.com", resilienceOptions, "test-bearer-token", httpClient);

        // Assert - Request succeeded, verifying the overload works correctly
        Assert.IsNotNull(stellarToml);
        Assert.AreEqual("2.0.0", stellarToml.GeneralInformation.Version);
    }

    /// <summary>
    ///     Verifies that CurrencyFromUrlAsync overload with bearerToken and resilienceOptions works correctly.
    ///     This indirectly tests GetOrCreateHttpClient by verifying the overload accepts these parameters.
    /// </summary>
    [TestMethod]
    public async Task CurrencyFromUrlAsync_WithBearerTokenAndResilienceOptions_WorksCorrectly()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient(_linkedCurrencyTomlContent);
        var resilienceOptions = HttpResilienceOptionsPresets.WithConnectionRetries();

        // Act - Call with bearerToken and resilienceOptions (GetOrCreateHttpClient is called internally when httpClient is null,
        // but we provide httpClient here to avoid real network calls. The test verifies the overload signature works.)
        var currency = await StellarToml.CurrencyFromUrlAsync(
            "https://example.com/.well-known/TESTC.toml",
            resilienceOptions,
            "test-bearer-token",
            httpClient);

        // Assert - Request succeeded, verifying the overload works correctly
        Assert.IsNotNull(currency);
        Assert.AreEqual("TESTC", currency.Code);
    }

    /// <summary>
    ///     Verifies that CurrencyFromUrlAsync handles HttpRequestException correctly.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(StellarTomlException))]
    public async Task CurrencyFromUrlAsync_WithHttpRequestException_WrapsException()
    {
        // Arrange
        using var httpClient = CreateMockHttpClientWithException(new HttpRequestException("Network error"));

        try
        {
            // Act
            await StellarToml.CurrencyFromUrlAsync("https://example.com/currency.toml", httpClient);
        }
        catch (StellarTomlException ex)
        {
            // Assert
            Assert.IsTrue(ex.Message.Contains("Failed to fetch currency TOML"));
            Assert.IsNotNull(ex.InnerException);
            Assert.IsInstanceOfType(ex.InnerException, typeof(HttpRequestException));
            throw;
        }
    }

    /// <summary>
    ///     Verifies that StellarToml parses TOML correctly when optional sections (PRINCIPALS, CURRENCIES, VALIDATORS) are missing.
    /// </summary>
    [TestMethod]
    public void Constructor_WithOptionalSectionsMissing_ParsesSuccessfully()
    {
        // Arrange - TOML with only required sections
        var minimalToml = @"
VERSION=""2.0.0""
NETWORK_PASSPHRASE=""Public Global Stellar Network ; September 2015""
FEDERATION_SERVER=""https://stellarid.io/federation/""

[DOCUMENTATION]
ORG_NAME=""Test Org""
";

        // Act
        var stellarToml = new StellarToml(minimalToml);

        // Assert
        Assert.IsNotNull(stellarToml.GeneralInformation);
        Assert.IsNotNull(stellarToml.Documentation);
        Assert.IsNull(stellarToml.PointsOfContact); // PRINCIPALS missing
        Assert.IsNull(stellarToml.Currencies); // CURRENCIES missing
        Assert.IsNull(stellarToml.Validators); // VALIDATORS missing
    }

    /// <summary>
    ///     Verifies that StellarToml parses all extended currency fields correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_WithExtendedCurrencyFields_ParsesAllFields()
    {
        // Arrange - TOML with currency containing all optional fields
        var extendedCurrencyToml = @"
VERSION=""2.0.0""
NETWORK_PASSPHRASE=""Public Global Stellar Network ; September 2015""

[[CURRENCIES]]
code=""EXTENDED""
code_template=""EXT*""
issuer=""GCZJM35NKGVK47BB4SPBDV25477PZYIYPVVG453LPYFNXLS3FGHDXOCM""
status=""live""
display_decimals=2
toml=""https://example.com/.well-known/EXTENDED.toml""
max_number=1000000
is_unlimited=false
is_asset_anchored=true
attestation_of_reserve=""https://example.com/attestation.pdf""
collateral_address_messages=[""Message 1"", ""Message 2""]
regulated=true
approval_server=""https://example.com/approval""
approval_criteria=""Must be verified""
";

        // Act
        var stellarToml = new StellarToml(extendedCurrencyToml);

        // Assert
        Assert.IsNotNull(stellarToml.Currencies);
        Assert.AreEqual(1, stellarToml.Currencies!.Count);
        var currency = stellarToml.Currencies[0];
        Assert.AreEqual("EXTENDED", currency.Code);
        Assert.AreEqual("EXT*", currency.CodeTemplate);
        Assert.AreEqual("live", currency.Status);
        Assert.AreEqual("https://example.com/.well-known/EXTENDED.toml", currency.Toml);
        Assert.AreEqual(1000000, currency.MaxNumber);
        Assert.AreEqual(false, currency.IsUnlimited);
        Assert.AreEqual(true, currency.IsAssetAnchored);
        Assert.AreEqual("https://example.com/attestation.pdf", currency.AttestationOfReserve);
        Assert.IsNotNull(currency.CollateralAddressMessages);
        Assert.AreEqual(2, currency.CollateralAddressMessages!.Count);
        Assert.IsTrue(currency.CollateralAddressMessages.Contains("Message 1"));
        Assert.IsTrue(currency.CollateralAddressMessages.Contains("Message 2"));
        Assert.AreEqual(true, currency.Regulated);
        Assert.AreEqual("https://example.com/approval", currency.ApprovalServer);
        Assert.AreEqual("Must be verified", currency.ApprovalCriteria);
    }

    /// <summary>
    ///     Verifies that StellarToml parses PointOfContact with Telegram field correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_WithPointOfContactTelegram_ParsesTelegramField()
    {
        // Arrange - TOML with PRINCIPALS containing telegram field
        var tomlWithTelegram = @"
VERSION=""2.0.0""
NETWORK_PASSPHRASE=""Public Global Stellar Network ; September 2015""

[[PRINCIPALS]]
name=""John Doe""
email=""john@example.com""
telegram=""@johndoe""
keybase=""johndoe""
";

        // Act
        var stellarToml = new StellarToml(tomlWithTelegram);

        // Assert
        Assert.IsNotNull(stellarToml.PointsOfContact);
        Assert.AreEqual(1, stellarToml.PointsOfContact!.Count);
        var pointOfContact = stellarToml.PointsOfContact[0];
        Assert.AreEqual("John Doe", pointOfContact.Name);
        Assert.AreEqual("john@example.com", pointOfContact.Email);
        Assert.AreEqual("@johndoe", pointOfContact.Telegram);
        Assert.AreEqual("johndoe", pointOfContact.Keybase);
    }

    /// <summary>
    ///     Verifies that FromDomainAsync does not dispose externally provided HttpClient.
    /// </summary>
    [TestMethod]
    public async Task FromDomainAsync_WithExternalHttpClient_DoesNotDisposeHttpClient()
    {
        // Arrange
        var fakeHttpMessageHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        fakeHttpMessageHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>())).Returns(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(_sampleTomlContent),
        });

        var trackingHttpClient = new DisposableTrackingHttpClient(fakeHttpMessageHandler.Object);

        // Act
        await StellarToml.FromDomainAsync("example.com", trackingHttpClient);

        // Assert - External HttpClient should NOT be disposed
        Assert.IsFalse(trackingHttpClient.IsDisposed, "External HttpClient should not be disposed by StellarToml");
    }

    /// <summary>
    ///     Verifies that CurrencyFromUrlAsync does not dispose externally provided HttpClient.
    /// </summary>
    [TestMethod]
    public async Task CurrencyFromUrlAsync_WithExternalHttpClient_DoesNotDisposeHttpClient()
    {
        // Arrange
        var fakeHttpMessageHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        fakeHttpMessageHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>())).Returns(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(_linkedCurrencyTomlContent),
        });

        var trackingHttpClient = new DisposableTrackingHttpClient(fakeHttpMessageHandler.Object);

        // Act
        await StellarToml.CurrencyFromUrlAsync("https://example.com/.well-known/TESTC.toml", trackingHttpClient);

        // Assert - External HttpClient should NOT be disposed
        Assert.IsFalse(trackingHttpClient.IsDisposed, "External HttpClient should not be disposed by StellarToml");
    }

    /// <summary>
    ///     HttpClient wrapper that tracks disposal for testing purposes.
    /// </summary>
    private class DisposableTrackingHttpClient : HttpClient
    {
        public DisposableTrackingHttpClient(HttpMessageHandler handler) : base(handler)
        {
        }

        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}

