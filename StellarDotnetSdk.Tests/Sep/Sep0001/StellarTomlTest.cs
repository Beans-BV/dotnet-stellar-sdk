using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StellarDotnetSdk.Sep.Sep0001;

namespace StellarDotnetSdk.Tests.Sep.Sep0001;

/// <summary>
///     Tests for StellarToml class functionality.
/// </summary>
[TestClass]
public class StellarTomlTest
{
    private string _sampleTomlContent = null!;

    [TestInitialize]
    public void Setup()
    {
        var testDataPath = Utils.GetTestDataPath("stellar-toml-sample.toml");
        _sampleTomlContent = File.ReadAllText(testDataPath);
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
        // Arrange - TOML with incorrect headers
        var malformedToml = @"
VERSION=""2.0.0""
NETWORK_PASSPHRASE=""Public Global Stellar Network ; September 2015""

[DOCUMENTATION]
ORG_NAME=""Test Org""

[[DOCUMENTATION]]
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

        var httpClient = new HttpClient(fakeHttpMessageHandler.Object);

        try
        {
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
        finally
        {
            httpClient.Dispose();
        }
    }

    /// <summary>
    ///     Verifies that FromDomainAsync throws exception when stellar.toml is not found.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(StellarTomlException))]
    public async Task FromDomainAsync_WithNotFound_ThrowsException()
    {
        // Arrange
        var fakeHttpMessageHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        fakeHttpMessageHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>())).Returns(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound,
            Content = new StringContent("Not Found"),
        });

        var httpClient = new HttpClient(fakeHttpMessageHandler.Object);

        try
        {
            // Act
            await StellarToml.FromDomainAsync("example.com", httpClient);
        }
        finally
        {
            httpClient.Dispose();
        }
    }

    /// <summary>
    ///     Verifies that CurrencyFromUrlAsync loads currency from external TOML file.
    /// </summary>
    [TestMethod]
    public async Task CurrencyFromUrlAsync_WithValidUrl_LoadsCurrency()
    {
        // Arrange
        var linkedCurrencyTomlPath = Utils.GetTestDataPath("stellar-toml-linked-currency.toml");
        var linkedCurrencyToml = File.ReadAllText(linkedCurrencyTomlPath);

        var fakeHttpMessageHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        fakeHttpMessageHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>())).Returns(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(linkedCurrencyToml),
        });

        var httpClient = new HttpClient(fakeHttpMessageHandler.Object);

        try
        {
            // Act
            var currency = await StellarToml.CurrencyFromUrlAsync("https://example.com/.well-known/TESTC.toml", httpClient);

            // Assert
            Assert.IsNotNull(currency);
            Assert.AreEqual("TESTC", currency.Code);
            Assert.AreEqual("GCPWPTAX6QVJQIQARN2WESISHVLN65D4HAGQECHLCAV22UST3W2Q6QTA", currency.Issuer);
            Assert.AreEqual(2, currency.DisplayDecimals);
            Assert.AreEqual("test currency", currency.Name);
            Assert.AreEqual("TESTC description", currency.Desc);
            Assert.AreEqual("TESTC conditions", currency.Conditions);
            Assert.AreEqual("https://soneso.com/123.png", currency.Image);
            Assert.AreEqual(10000, currency.FixedNumber);
        }
        finally
        {
            httpClient.Dispose();
        }
    }

    /// <summary>
    ///     Verifies that CurrencyFromUrlAsync throws exception when currency TOML is not found.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(StellarTomlException))]
    public async Task CurrencyFromUrlAsync_WithNotFound_ThrowsException()
    {
        // Arrange
        var fakeHttpMessageHandler = new Mock<Utils.FakeHttpMessageHandler> { CallBase = true };
        fakeHttpMessageHandler.Setup(a => a.Send(It.IsAny<HttpRequestMessage>())).Returns(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound,
            Content = new StringContent("Not Found"),
        });

        var httpClient = new HttpClient(fakeHttpMessageHandler.Object);

        try
        {
            // Act
            await StellarToml.CurrencyFromUrlAsync("https://example.com/.well-known/CURRENCY.toml", httpClient);
        }
        finally
        {
            httpClient.Dispose();
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
}

