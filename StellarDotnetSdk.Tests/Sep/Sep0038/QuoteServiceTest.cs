using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Sep.Sep0038;
using StellarDotnetSdk.Sep.Sep0038.Exceptions;
using StellarDotnetSdk.Sep.Sep0038.Requests;
using StellarDotnetSdk.Sep.Sep0038.Responses;

namespace StellarDotnetSdk.Tests.Sep.Sep0038;

/// <summary>
///     Unit tests for the SEP-38 <see cref="QuoteService" /> against a stubbed HTTP handler.
/// </summary>
[TestClass]
public class QuoteServiceTest
{
    private const string ServiceAddress = "https://api.example.com/sep38";
    private const string Jwt = "test-jwt-token";
    private const string QuoteId = "de762cda-a193-4961-861e-57b31fed6eb3";
    private const string Usdc = "stellar:USDC:GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN";
    private const string Brl = "iso4217:BRL";
    private const int MaxResponseBodyBytes = 1024 * 1024;

    private static string ReadTestData(string fileName)
    {
        return File.ReadAllText(Utils.GetTestDataAbsolutePath(fileName));
    }

    private static (QuoteService Service, StubHandler Handler) CreateService(
        string body,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        Dictionary<string, string>? headers = null)
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(statusCode) { Content = new StringContent(body) });
        return (new QuoteService(ServiceAddress, new HttpClient(handler), httpRequestHeaders: headers), handler);
    }

    private static (QuoteService Service, StubHandler Handler) CreateService(
        Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var handler = new StubHandler(respond);
        return (new QuoteService(ServiceAddress, new HttpClient(handler)), handler);
    }

    private static QuoteRequest ValidQuoteRequest()
    {
        return new QuoteRequest
        {
            Context = QuoteContext.Sep6,
            SellAsset = Brl,
            BuyAsset = Usdc,
            SellAmount = 542m,
            Jwt = Jwt,
        };
    }

    #region Construction and discovery

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("/relative/path")]
    [DataRow("ftp://api.example.com/sep38")]
    [DataRow("not a url")]
    [DataRow("http://api.example.com/sep38")]
    [DataRow("https://api.example.com/sep38?key=value")]
    [DataRow("https://api.example.com/sep38#fragment")]
    [DataRow("https://user:secret@api.example.com/sep38")]
    public void Constructor_WithInvalidAddress_ThrowsArgumentException(string address)
    {
        Assert.ThrowsException<ArgumentException>(() => new QuoteService(address, new HttpClient()));
    }

    [TestMethod]
    [DataRow("http://localhost:8080/sep38")]
    [DataRow("http://127.0.0.1:8080/sep38")]
    [DataRow("http://[::1]:8080/sep38")]
    [DataRow("http://LOCALHOST:8080/sep38")]
    public void Constructor_WithPlainHttpOnLoopback_IsAccepted(string address)
    {
        using var service = new QuoteService(address, new HttpClient());
    }

    [TestMethod]
    public async Task Constructor_WithSurroundingWhitespace_UsesTheParsedAddress()
    {
        // A padded stellar.toml value is a common copy-paste slip; it must not reach every request URI.
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ReadTestData("info-response.json")),
        });
        var service = new QuoteService(" https://api.example.com/sep38/ \t", new HttpClient(handler));

        await service.InfoAsync();

        Assert.AreEqual("https://api.example.com/sep38/info", handler.Requests.Single().RequestUri!.ToString());
    }

    [TestMethod]
    public void Constructor_WithNullAddress_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(() => new QuoteService(null!, new HttpClient()));
    }

    [TestMethod]
    public async Task FromDomainAsync_WithAnchorQuoteServer_UsesTheAddressFromStellarToml()
    {
        var info = ReadTestData("info-response.json");
        var handler = new StubHandler(request =>
        {
            var body = request.RequestUri!.AbsolutePath == "/.well-known/stellar.toml"
                ? "ANCHOR_QUOTE_SERVER=\"https://quotes.example.com/sep38/\""
                : info;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) };
        });
        using var httpClient = new HttpClient(handler);

        var service = await QuoteService.FromDomainAsync("example.com", httpClient: httpClient);
        var result = await service.InfoAsync();

        Assert.AreEqual(3, result.Assets.Count);
        Assert.AreEqual("https://example.com/.well-known/stellar.toml", handler.Requests[0].RequestUri!.ToString());
        // The trailing slash of the toml value must not produce "//info".
        Assert.AreEqual("https://quotes.example.com/sep38/info", handler.Requests[1].RequestUri!.ToString());
    }

    [TestMethod]
    public async Task FromDomainAsync_WithoutAnchorQuoteServer_ThrowsNoAnchorQuoteServerFoundException()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("TRANSFER_SERVER_SEP0024=\"https://api.example.com/sep24\""),
        });
        using var httpClient = new HttpClient(handler);

        var ex = await Assert.ThrowsExceptionAsync<NoAnchorQuoteServerFoundException>(() =>
            QuoteService.FromDomainAsync("example.com", httpClient: httpClient));
        Assert.AreEqual("example.com", ex.Domain);
        Assert.IsInstanceOfType(ex, typeof(QuoteServerException));
        Assert.AreEqual(1, handler.Requests.Count);
    }

    #endregion

    #region GET /info

    [TestMethod]
    public async Task InfoAsync_ParsesEveryField()
    {
        var (service, _) = CreateService(ReadTestData("info-response.json"));

        var result = await service.InfoAsync();

        Assert.AreEqual(3, result.Assets.Count);
        var usdc = result.Assets[0];
        Assert.AreEqual(Usdc, usdc.Asset);
        Assert.IsNull(usdc.SellDeliveryMethods);
        Assert.IsNull(usdc.BuyDeliveryMethods);
        Assert.IsNull(usdc.CountryCodes);

        var brl = result.Assets[2];
        Assert.AreEqual(Brl, brl.Asset);
        CollectionAssert.AreEqual(new[] { "BR" }, brl.CountryCodes!.ToArray());
        Assert.AreEqual(3, brl.SellDeliveryMethods!.Count);
        Assert.AreEqual("cash", brl.SellDeliveryMethods[0].Name);
        Assert.AreEqual("Deposit cash BRL at one of our agent locations.", brl.SellDeliveryMethods[0].Description);
        Assert.AreEqual("PIX", brl.SellDeliveryMethods[2].Name);
        Assert.AreEqual(3, brl.BuyDeliveryMethods!.Count);
        Assert.AreEqual("ACH", brl.BuyDeliveryMethods[1].Name);
        Assert.AreEqual("Have BRL sent directly to your bank account.", brl.BuyDeliveryMethods[1].Description);
    }

    [TestMethod]
    public async Task InfoAsync_WithoutJwt_SendsAnUnauthenticatedGet()
    {
        var (service, handler) = CreateService(ReadTestData("info-response.json"));

        await service.InfoAsync();

        var request = handler.Requests.Single();
        Assert.AreEqual(HttpMethod.Get, request.Method);
        Assert.AreEqual(ServiceAddress + "/info", request.RequestUri!.ToString());
        Assert.IsNull(request.Headers.Authorization);
        Assert.AreEqual("application/json", request.Headers.Accept.Single().MediaType);
    }

    [TestMethod]
    public async Task InfoAsync_WithJwt_SendsBearerToken()
    {
        var (service, handler) = CreateService(ReadTestData("info-response.json"));

        await service.InfoAsync(Jwt);

        var authorization = handler.Requests.Single().Headers.Authorization!;
        Assert.AreEqual("Bearer", authorization.Scheme);
        Assert.AreEqual(Jwt, authorization.Parameter);
    }

    [TestMethod]
    public async Task InfoAsync_WithCustomHeaders_SendsThem()
    {
        var headers = new Dictionary<string, string> { ["X-Client-Name"] = "sdk-test" };
        var (service, handler) = CreateService(ReadTestData("info-response.json"), headers: headers);

        await service.InfoAsync();

        CollectionAssert.AreEqual(new[] { "sdk-test" },
            handler.Requests.Single().Headers.GetValues("X-Client-Name").ToArray());
    }

    [TestMethod]
    public async Task InfoAsync_JwtArgument_OverridesAConfiguredAuthorizationHeader()
    {
        var headers = new Dictionary<string, string> { ["Authorization"] = "Bearer configured" };
        var (service, handler) = CreateService(ReadTestData("info-response.json"), headers: headers);

        await service.InfoAsync(Jwt);

        Assert.AreEqual(Jwt, handler.Requests.Single().Headers.Authorization!.Parameter);
    }

    #endregion

    #region GET /prices

    [TestMethod]
    public async Task PricesAsync_SellSide_SendsSellParametersAndParsesBuyAssets()
    {
        var (service, handler) = CreateService(ReadTestData("prices-sell-response.json"));

        var result = await service.PricesAsync(new PricesRequest
        {
            SellAsset = Usdc,
            SellAmount = 100m,
            BuyDeliveryMethod = "ACH",
            CountryCode = "BR",
        });

        Assert.AreEqual(
            ServiceAddress + "/prices?sell_asset=stellar%3AUSDC%3AGA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN" +
            "&sell_amount=100&buy_delivery_method=ACH&country_code=BR",
            handler.Requests.Single().RequestUri!.AbsoluteUri);
        Assert.IsNull(handler.Requests.Single().Headers.Authorization);
        Assert.IsNull(result.SellAssets);
        Assert.AreEqual(2, result.BuyAssets!.Count);
        Assert.AreEqual(Brl, result.BuyAssets[0].Asset);
        Assert.AreEqual(0.18m, result.BuyAssets[0].Price);
        Assert.AreEqual(2, result.BuyAssets[0].Decimals);
        // Exact: the trailing zeros survive, so the anchor's scale is preserved.
        Assert.AreEqual("0.1795000", result.BuyAssets[1].Price.ToString(CultureInfo.InvariantCulture));
        Assert.AreEqual(7, result.BuyAssets[1].Decimals);
    }

    [TestMethod]
    public async Task PricesAsync_BuySide_SendsBuyParametersAndParsesSellAssets()
    {
        var (service, handler) = CreateService(ReadTestData("prices-buy-response.json"));

        var result = await service.PricesAsync(new PricesRequest
        {
            BuyAsset = Usdc,
            BuyAmount = 100m,
            SellDeliveryMethod = "PIX",
            Jwt = Jwt,
        });

        var request = handler.Requests.Single();
        Assert.AreEqual(
            ServiceAddress + "/prices?buy_asset=stellar%3AUSDC%3AGA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN" +
            "&buy_amount=100&sell_delivery_method=PIX",
            request.RequestUri!.AbsoluteUri);
        Assert.AreEqual(Jwt, request.Headers.Authorization!.Parameter);
        Assert.IsNull(result.BuyAssets);
        Assert.AreEqual(1, result.SellAssets!.Count);
        Assert.AreEqual(5.42m, result.SellAssets[0].Price);
    }

    [TestMethod]
    [DataRow(true, true, true, false, DisplayName = "Both assets")]
    [DataRow(false, false, false, false, DisplayName = "Neither asset")]
    [DataRow(true, false, false, false, DisplayName = "Sell asset without sell amount")]
    [DataRow(true, false, true, true, DisplayName = "Sell asset with buy amount")]
    [DataRow(false, true, false, false, DisplayName = "Buy asset without buy amount")]
    [DataRow(false, true, true, true, DisplayName = "Buy asset with sell amount")]
    public async Task PricesAsync_WithInvalidSideCombination_ThrowsBeforeSending(bool sellAsset, bool buyAsset,
        bool sellAmount, bool buyAmount)
    {
        var (service, handler) = CreateService(ReadTestData("prices-sell-response.json"));
        var request = new PricesRequest
        {
            SellAsset = sellAsset ? Usdc : null,
            BuyAsset = buyAsset ? Brl : null,
            SellAmount = sellAmount ? 100m : null,
            BuyAmount = buyAmount ? 100m : null,
        };

        await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.PricesAsync(request));
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task PricesAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        var (service, _) = CreateService("{}");
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => service.PricesAsync(null!));
    }

    #endregion

    #region GET /price

    [TestMethod]
    public async Task PriceAsync_WithSellAmount_SendsQueryAndParsesPriceAndFee()
    {
        var (service, handler) = CreateService(ReadTestData("price-response.json"));

        var result = await service.PriceAsync(new PriceRequest
        {
            Context = QuoteContext.Sep6,
            SellAsset = Usdc,
            BuyAsset = Brl,
            SellAmount = 90m,
            BuyDeliveryMethod = "PIX",
            CountryCode = "BR",
            Jwt = Jwt,
        });

        var request = handler.Requests.Single();
        Assert.AreEqual(
            ServiceAddress + "/price?sell_asset=stellar%3AUSDC%3AGA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN" +
            "&buy_asset=iso4217%3ABRL&sell_amount=90&buy_delivery_method=PIX&country_code=BR&context=sep6",
            request.RequestUri!.AbsoluteUri);
        Assert.AreEqual(Jwt, request.Headers.Authorization!.Parameter);
        Assert.AreEqual(0.20m, result.TotalPrice);
        Assert.AreEqual("0.20", result.TotalPrice.ToString(CultureInfo.InvariantCulture));
        Assert.AreEqual(0.18m, result.Price);
        Assert.AreEqual(100m, result.SellAmount);
        Assert.AreEqual(500m, result.BuyAmount);
        Assert.AreEqual(55.5556m, result.Fee.Total);
        Assert.AreEqual(Brl, result.Fee.Asset);
        var detail = result.Fee.Details!.Single();
        Assert.AreEqual("PIX fee", detail.Name);
        Assert.AreEqual("Fee charged in order to process the outgoing PIX transaction.", detail.Description);
        Assert.AreEqual(55.5556m, detail.Amount);
    }

    [TestMethod]
    public async Task PriceAsync_WithBuyAmount_SendsBuyAmountAndSep31Context()
    {
        var (service, handler) = CreateService(ReadTestData("price-response.json"));

        await service.PriceAsync(new PriceRequest
        {
            Context = QuoteContext.Sep31,
            SellAsset = Brl,
            BuyAsset = Usdc,
            BuyAmount = 100m,
        });

        var query = handler.Requests.Single().RequestUri!.Query;
        StringAssert.Contains(query, "&buy_amount=100&");
        Assert.IsFalse(query.Contains("sell_amount"));
        StringAssert.EndsWith(query, "&context=sep31");
        Assert.IsNull(handler.Requests.Single().Headers.Authorization);
    }

    [TestMethod]
    public async Task PriceAsync_FormatsAmountsWithTheInvariantCultureAndKeepsTheirScale()
    {
        var (service, handler) = CreateService(ReadTestData("price-response.json"));
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            // A culture whose decimal separator is a comma: a culture-sensitive format would send "1234,50".
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            await service.PriceAsync(new PriceRequest
            {
                Context = QuoteContext.Sep6,
                SellAsset = Brl,
                BuyAsset = Usdc,
                SellAmount = 1234.50m,
            });
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }

        StringAssert.Contains(handler.Requests.Single().RequestUri!.Query, "&sell_amount=1234.50&");
    }

    [TestMethod]
    [DataRow(true, true)]
    [DataRow(false, false)]
    public async Task PriceAsync_WithoutExactlyOneAmount_ThrowsBeforeSending(bool sellAmount, bool buyAmount)
    {
        var (service, handler) = CreateService(ReadTestData("price-response.json"));

        await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.PriceAsync(new PriceRequest
        {
            Context = QuoteContext.Sep6,
            SellAsset = Brl,
            BuyAsset = Usdc,
            SellAmount = sellAmount ? 1m : null,
            BuyAmount = buyAmount ? 1m : null,
        }));
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    [DataRow("prices", true, "0")]
    [DataRow("prices", false, "-1")]
    [DataRow("price", true, "-0.01")]
    [DataRow("price", false, "0.00")]
    [DataRow("quote", true, "-542")]
    [DataRow("quote", false, "0")]
    public async Task WithAnAmountThatIsNotPositive_ThrowsBeforeSending(string endpoint, bool sellSide, string amountText)
    {
        var amount = decimal.Parse(amountText, CultureInfo.InvariantCulture);
        var (service, handler) = CreateService("{}");
        decimal? sell = sellSide ? amount : null;
        decimal? buy = sellSide ? null : amount;
        Func<Task> call = endpoint switch
        {
            "prices" => () => service.PricesAsync(sellSide
                ? new PricesRequest { SellAsset = Brl, SellAmount = sell }
                : new PricesRequest { BuyAsset = Usdc, BuyAmount = buy }),
            "price" => () => service.PriceAsync(new PriceRequest
            {
                Context = QuoteContext.Sep6, SellAsset = Brl, BuyAsset = Usdc, SellAmount = sell, BuyAmount = buy,
            }),
            _ => () => service.PostQuoteAsync(new QuoteRequest
            {
                Context = QuoteContext.Sep6, SellAsset = Brl, BuyAsset = Usdc, SellAmount = sell, BuyAmount = buy,
                Jwt = Jwt,
            }),
        };

        var ex = await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(call);

        Assert.AreEqual(sellSide ? "SellAmount" : "BuyAmount", ex.ParamName);
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task PriceAsync_WithSep24Context_ThrowsBecauseTheSpecificationAllowsOnlySep6AndSep31()
    {
        var (service, handler) = CreateService(ReadTestData("price-response.json"));

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.PriceAsync(new PriceRequest
        {
            Context = QuoteContext.Sep24,
            SellAsset = Brl,
            BuyAsset = Usdc,
            SellAmount = 1m,
        }));
        Assert.AreEqual("Context", ex.ParamName);
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task PriceAsync_WithUndefinedContext_ThrowsArgumentOutOfRangeException()
    {
        var (service, handler) = CreateService(ReadTestData("price-response.json"));

        await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => service.PriceAsync(new PriceRequest
        {
            Context = (QuoteContext)42,
            SellAsset = Brl,
            BuyAsset = Usdc,
            SellAmount = 1m,
        }));
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    [DataRow("", Usdc)]
    [DataRow(Brl, " ")]
    [DataRow(null, Usdc)]
    public async Task PriceAsync_WithMissingAsset_ThrowsBeforeSending(string? sellAsset, string? buyAsset)
    {
        var (service, handler) = CreateService(ReadTestData("price-response.json"));

        await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.PriceAsync(new PriceRequest
        {
            Context = QuoteContext.Sep6,
            SellAsset = sellAsset!,
            BuyAsset = buyAsset!,
            SellAmount = 1m,
        }));
        Assert.AreEqual(0, handler.Requests.Count);
    }

    #endregion

    #region POST /quote

    [TestMethod]
    public async Task PostQuoteAsync_SendsJsonBodyWithBearerTokenAndParsesTheQuote()
    {
        var (service, handler) = CreateService(ReadTestData("quote-response.json"), HttpStatusCode.Created);

        var result = await service.PostQuoteAsync(new QuoteRequest
        {
            Context = QuoteContext.Sep24,
            SellAsset = Brl,
            BuyAsset = Usdc,
            SellAmount = 542.00m,
            ExpireAfter = new DateTimeOffset(2021, 4, 30, 9, 42, 23, TimeSpan.FromHours(2)),
            SellDeliveryMethod = "PIX",
            CountryCode = "BR",
            Jwt = Jwt,
        });

        var request = handler.Requests.Single();
        Assert.AreEqual(HttpMethod.Post, request.Method);
        Assert.AreEqual(ServiceAddress + "/quote", request.RequestUri!.ToString());
        Assert.AreEqual("Bearer", request.Headers.Authorization!.Scheme);
        Assert.AreEqual(Jwt, request.Headers.Authorization.Parameter);
        Assert.AreEqual("application/json", handler.ContentTypes.Single());
        // Amounts are strings with their scale kept, and the expiry is converted to UTC.
        Assert.AreEqual(
            "{\"sell_asset\":\"iso4217:BRL\"," +
            "\"buy_asset\":\"stellar:USDC:GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN\"," +
            "\"sell_amount\":\"542.00\",\"expire_after\":\"2021-04-30T07:42:23Z\",\"sell_delivery_method\":\"PIX\"," +
            "\"country_code\":\"BR\",\"context\":\"sep24\"}",
            handler.Bodies.Single());

        Assert.AreEqual("de762cda-a193-4961-861e-57b31fed6eb3", result.Id);
        Assert.AreEqual(new DateTimeOffset(2021, 4, 30, 7, 42, 23, TimeSpan.Zero), result.ExpiresAt);
        Assert.AreEqual(TimeSpan.Zero, result.ExpiresAt.Offset);
        Assert.AreEqual(5.42m, result.TotalPrice);
        Assert.AreEqual(5.00m, result.Price);
        Assert.AreEqual(Brl, result.SellAsset);
        Assert.AreEqual(542m, result.SellAmount);
        Assert.AreEqual("PIX", result.SellDeliveryMethod);
        Assert.AreEqual(Usdc, result.BuyAsset);
        Assert.AreEqual(100m, result.BuyAmount);
        Assert.IsNull(result.BuyDeliveryMethod);
        Assert.AreEqual(42.00m, result.Fee.Total);
        Assert.AreEqual(Brl, result.Fee.Asset);
        Assert.AreEqual(3, result.Fee.Details!.Count);
        Assert.IsNull(result.Fee.Details[2].Description);
        Assert.AreEqual(result.Fee.Total, result.Fee.Details.Sum(d => d.Amount));
    }

    [TestMethod]
    public async Task PostQuoteAsync_WithBuyAmountAndNoOptionalFields_SendsOnlyTheRequiredFields()
    {
        var (service, handler) = CreateService(ReadTestData("quote-response.json"));

        await service.PostQuoteAsync(new QuoteRequest
        {
            Context = QuoteContext.Sep31,
            SellAsset = Brl,
            BuyAsset = Usdc,
            BuyAmount = 100m,
            Jwt = Jwt,
        });

        Assert.AreEqual(
            "{\"sell_asset\":\"iso4217:BRL\"," +
            "\"buy_asset\":\"stellar:USDC:GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN\"," +
            "\"buy_amount\":\"100\",\"context\":\"sep31\"}",
            handler.Bodies.Single());
    }

    [TestMethod]
    [DataRow(true, true)]
    [DataRow(false, false)]
    public async Task PostQuoteAsync_WithoutExactlyOneAmount_ThrowsBeforeSending(bool sellAmount, bool buyAmount)
    {
        var (service, handler) = CreateService(ReadTestData("quote-response.json"));

        await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.PostQuoteAsync(ValidQuoteRequest() with
        {
            SellAmount = sellAmount ? 1m : null,
            BuyAmount = buyAmount ? 1m : null,
        }));
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task PostQuoteAsync_WithBothDeliveryMethods_ThrowsBeforeSending()
    {
        var (service, handler) = CreateService(ReadTestData("quote-response.json"));

        await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.PostQuoteAsync(ValidQuoteRequest() with
        {
            SellDeliveryMethod = "PIX",
            BuyDeliveryMethod = "ACH",
        }));
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public async Task PostQuoteAsync_WithoutJwt_ThrowsBeforeSending(string? jwt)
    {
        var (service, handler) = CreateService(ReadTestData("quote-response.json"));

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            service.PostQuoteAsync(ValidQuoteRequest() with { Jwt = jwt! }));
        Assert.AreEqual("Jwt", ex.ParamName);
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task PostQuoteAsync_OnForbidden_ThrowsPermissionDeniedException()
    {
        var (service, _) = CreateService("{\"error\":\"The account is not KYC'ed.\"}", HttpStatusCode.Forbidden);

        var ex = await Assert.ThrowsExceptionAsync<PermissionDeniedException>(() =>
            service.PostQuoteAsync(ValidQuoteRequest()));
        Assert.AreEqual(403, ex.StatusCode);
        Assert.AreEqual("The account is not KYC'ed.", ex.Error);
    }

    [TestMethod]
    public async Task PostQuoteAsync_WithCustomContentHeader_AttachesItToTheContent()
    {
        var headers = new Dictionary<string, string> { ["Content-Language"] = "pt-BR" };
        var (service, handler) = CreateService(ReadTestData("quote-response.json"), headers: headers);

        await service.PostQuoteAsync(ValidQuoteRequest());

        CollectionAssert.AreEqual(new[] { "pt-BR" }, handler.ContentLanguages.Single());
    }

    [TestMethod]
    [DataRow("not a url")]
    [DataRow("ftp://quotes.example.com/sep38")]
    [DataRow("http://quotes.example.com/sep38")]
    [DataRow("https://quotes.example.com/sep38?key=value")]
    [DataRow("http://127.0.0.1:8080/sep38")]
    public async Task FromDomainAsync_WithUnusableAnchorQuoteServer_ThrowsNoAnchorQuoteServerFoundException(
        string address)
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"ANCHOR_QUOTE_SERVER=\"{address}\""),
        });
        using var httpClient = new HttpClient(handler);

        var ex = await Assert.ThrowsExceptionAsync<NoAnchorQuoteServerFoundException>(() =>
            QuoteService.FromDomainAsync("example.com", httpClient: httpClient));

        Assert.AreEqual("example.com", ex.Domain);
        Assert.AreEqual(address, ex.DeclaredAddress);
        StringAssert.Contains(ex.Message, "unusable ANCHOR_QUOTE_SERVER");
        // The caller never passed a serviceAddress argument, so the message must not name one.
        Assert.IsFalse(ex.Message.Contains("serviceAddress"), ex.Message);
    }

    [TestMethod]
    public async Task FromDomainAsync_WithHostileAnchorQuoteServer_EscapesAndClampsItInTheMessage()
    {
        var hostile = "ftp://x\\nFORGED LOG LINE \\u001b[31m" + new string('A', 500);
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"ANCHOR_QUOTE_SERVER=\"{hostile}\""),
        });
        using var httpClient = new HttpClient(handler);

        var ex = await Assert.ThrowsExceptionAsync<NoAnchorQuoteServerFoundException>(() =>
            QuoteService.FromDomainAsync("example.com", httpClient: httpClient));

        StringAssert.Contains(ex.DeclaredAddress, "\n");
        Assert.IsFalse(ex.Message.Contains('\n'), ex.Message);
        Assert.IsFalse(ex.Message.Contains('\u001b'), ex.Message);
        StringAssert.Contains(ex.Message, "truncated");
    }

    [TestMethod]
    public async Task FromDomainAsync_SendsTheCustomHeadersToTheQuoteServerToo()
    {
        var info = ReadTestData("info-response.json");
        var handler = new StubHandler(request =>
        {
            var body = request.RequestUri!.AbsolutePath == "/.well-known/stellar.toml"
                ? "ANCHOR_QUOTE_SERVER=\"https://quotes.example.com/sep38\""
                : info;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) };
        });
        using var httpClient = new HttpClient(handler);
        var headers = new Dictionary<string, string> { ["X-Api-Key"] = "key" };

        var service = await QuoteService.FromDomainAsync("example.com", httpClient: httpClient,
            httpRequestHeaders: headers);
        await service.InfoAsync();

        Assert.AreEqual(2, handler.Requests.Count);
        Assert.IsTrue(handler.Requests.All(r => r.Headers.GetValues("X-Api-Key").Single() == "key"));
    }

    #endregion

    #region GET /quote/:id

    [TestMethod]
    public async Task GetQuoteAsync_SendsAuthenticatedGetAndParsesTheQuote()
    {
        var (service, handler) = CreateService(ReadTestData("quote-response.json"));

        var result = await service.GetQuoteAsync("de762cda-a193-4961-861e-57b31fed6eb3", Jwt);

        var request = handler.Requests.Single();
        Assert.AreEqual(HttpMethod.Get, request.Method);
        Assert.AreEqual(ServiceAddress + "/quote/de762cda-a193-4961-861e-57b31fed6eb3",
            request.RequestUri!.ToString());
        Assert.AreEqual(Jwt, request.Headers.Authorization!.Parameter);
        Assert.AreEqual("de762cda-a193-4961-861e-57b31fed6eb3", result.Id);
        Assert.AreEqual("PIX", result.SellDeliveryMethod);
    }

    [TestMethod]
    public async Task GetQuoteAsync_EscapesTheIdAsASinglePathSegment()
    {
        var (service, handler) = CreateService(ReadTestData("quote-response.json")
            .Replace(QuoteId, "../info?x=1#y"));

        await service.GetQuoteAsync("../info?x=1#y", Jwt);

        Assert.AreEqual("/sep38/quote/..%2Finfo%3Fx%3D1%23y", handler.Requests.Single().RequestUri!.AbsolutePath);
    }

    [TestMethod]
    [DataRow("", Jwt)]
    [DataRow(null, Jwt)]
    [DataRow("id", "")]
    [DataRow("id", null)]
    public async Task GetQuoteAsync_WithMissingIdOrJwt_ThrowsBeforeSending(string? id, string? jwt)
    {
        var (service, handler) = CreateService(ReadTestData("quote-response.json"));

        await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.GetQuoteAsync(id!, jwt!));
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    [DataRow(".")]
    [DataRow("..")]
    public async Task GetQuoteAsync_WithDotSegmentId_ThrowsBeforeSending(string id)
    {
        // Uri.EscapeDataString leaves '.' alone and the URI parser resolves dot segments, so "..' would request the
        // service root and "." the quote collection, each with the JWT.
        var (service, handler) = CreateService(ReadTestData("quote-response.json"));

        await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.GetQuoteAsync(id, Jwt));
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task GetQuoteAsync_WhenTheAnchorReturnsAnotherQuote_Throws()
    {
        var (service, _) = CreateService(ReadTestData("quote-response.json"));

        var ex = await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() =>
            service.GetQuoteAsync("another-id", Jwt));

        StringAssert.Contains(ex.Message, QuoteId);
        Assert.AreEqual(200, ex.StatusCode);
        Assert.IsNotNull(ex.ResponseBody);
    }

    [TestMethod]
    public async Task GetQuoteAsync_WhenTheReturnedIdDiffersOnlyInCase_Throws()
    {
        // Quote ids are opaque: the spec says the response carries "the id specified in the request", so the match
        // is ordinal.
        var (service, _) = CreateService(ReadTestData("quote-response.json"));

        await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() =>
            service.GetQuoteAsync(QuoteId.ToUpperInvariant(), Jwt));
    }

    [TestMethod]
    public async Task GetQuoteAsync_WithoutTotalPrice_IsAccepted()
    {
        // The GET /quote/:id response table in SEP-38 does not list total_price.
        var (service, _) = CreateService(WithoutProperty(ReadTestData("quote-response.json"), "total_price"));

        var result = await service.GetQuoteAsync(QuoteId, Jwt);

        Assert.IsNull(result.TotalPrice);
        Assert.AreEqual(5.00m, result.Price);
    }

    [TestMethod]
    public async Task PostQuoteAsync_WithoutTotalPrice_Throws()
    {
        var (service, _) = CreateService(WithoutProperty(ReadTestData("quote-response.json"), "total_price"),
            HttpStatusCode.Created);

        var ex = await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() =>
            service.PostQuoteAsync(ValidQuoteRequest()));

        StringAssert.Contains(ex.Message, "total_price");
        // The anchor already created the quote: its status and body, id included, must survive the rejection.
        Assert.AreEqual(201, ex.StatusCode);
        StringAssert.Contains(ex.ResponseBody, QuoteId);
    }

    [TestMethod]
    [DataRow("total_price")]
    [DataRow("price")]
    [DataRow("sell_amount")]
    [DataRow("buy_amount")]
    public async Task GetQuoteAsync_WithAnAmountThatWouldBeRounded_RejectsTheResponse(string field)
    {
        // Each amount property carries the exact-decimal converter on its own; a lossy value in any of them must
        // be rejected, not rounded by the default decimal reader.
        var json = SetProperty(ReadTestData("quote-response.json"), field,
            "\"0.1234567890123456789012345678901\"");
        var (service, _) = CreateService(json);

        await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() => service.GetQuoteAsync(QuoteId, Jwt));
    }

    [TestMethod]
    public void ResponseDecimals_AllUseTheExactDecimalConverter()
    {
        var responseTypes = typeof(QuoteResponse).Assembly.GetTypes()
            .Where(t => t.Namespace == typeof(QuoteResponse).Namespace);
        var decimals = responseTypes
            .SelectMany(t => t.GetProperties())
            .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?))
            .ToList();

        Assert.IsTrue(decimals.Count >= 10, $"Only {decimals.Count} decimal properties found.");
        foreach (var property in decimals)
        {
            var attribute = property.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonConverterAttribute),
                false).Cast<System.Text.Json.Serialization.JsonConverterAttribute>().SingleOrDefault();
            Assert.AreEqual(typeof(StellarDotnetSdk.Converters.ExactDecimalJsonConverter), attribute?.ConverterType,
                $"{property.DeclaringType!.Name}.{property.Name}");
        }
    }

    /// <summary>
    ///     A consumer's source-generated <see cref="System.Text.Json.Serialization.JsonSerializerContext" /> can only
    ///     instantiate a property's converter when the converter is public with a public parameterless constructor;
    ///     otherwise the generator reports SYSLIB1220 and falls back to the default converter, which rejects the
    ///     spec's string amounts. This test assembly sees the SDK's internals, so a context compiled here would pass
    ///     either way; reflection is what checks the consumer's view.
    /// </summary>
    [TestMethod]
    public void ResponseConverters_AreUsableFromAConsumersSourceGeneratedContext()
    {
        var converters = typeof(QuoteResponse).Assembly.GetTypes()
            .Where(t => t.Namespace == typeof(QuoteResponse).Namespace)
            .SelectMany(t => t.GetProperties())
            .SelectMany(p => p.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonConverterAttribute), false)
                .Cast<System.Text.Json.Serialization.JsonConverterAttribute>())
            .Select(a => a.ConverterType)
            .OfType<Type>()
            .Distinct()
            .ToList();

        Assert.IsTrue(converters.Count >= 7, $"Only {converters.Count} converters found.");
        foreach (var converter in converters)
        {
            Assert.IsTrue(converter.IsPublic, $"{converter.Name} is not public.");
            Assert.IsNotNull(converter.GetConstructor(Type.EmptyTypes), $"{converter.Name} has no public parameterless constructor.");
        }
    }

    [TestMethod]
    public async Task GetQuoteAsync_WithExpiryOnlyTheJsonReaderAccepts_ThrowsUnexpectedResponseException()
    {
        var json = ReadTestData("quote-response.json")
            .Replace("\"2021-04-30T07:42:23\"", "\"2021-04-30T07:42:23.Z\"");
        var (service, _) = CreateService(json);

        var ex = await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() =>
            service.GetQuoteAsync(QuoteId, Jwt));

        Assert.IsInstanceOfType(ex.InnerException, typeof(JsonException));
    }

    [TestMethod]
    public async Task GetQuoteAsync_OnNotFound_ThrowsNotFoundException()
    {
        var (service, _) = CreateService("{\"error\":\"quote not found\"}", HttpStatusCode.NotFound);

        var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(() => service.GetQuoteAsync("missing", Jwt));
        Assert.AreEqual(404, ex.StatusCode);
        Assert.AreEqual("quote not found", ex.Error);
    }

    [TestMethod]
    public async Task GetQuoteAsync_WithExplicitOffset_ReadsThatInstant()
    {
        var json = ReadTestData("quote-response.json")
            .Replace("\"2021-04-30T07:42:23\"", "\"2021-04-30T09:42:23.5+02:00\"");
        var (service, _) = CreateService(json);

        var result = await service.GetQuoteAsync(QuoteId, Jwt);

        Assert.AreEqual(new DateTimeOffset(2021, 4, 30, 7, 42, 23, 500, TimeSpan.Zero), result.ExpiresAt);
        Assert.AreEqual(TimeSpan.FromHours(2), result.ExpiresAt.Offset);
    }

    #endregion

    #region Error responses

    [TestMethod]
    public async Task OnBadRequest_ThrowsBadRequestExceptionWithTheAnchorsError()
    {
        var body = ReadTestData("error-response.json");
        var (service, _) = CreateService(body, HttpStatusCode.BadRequest);

        var ex = await Assert.ThrowsExceptionAsync<BadRequestException>(() => service.InfoAsync());

        Assert.AreEqual(400, ex.StatusCode);
        Assert.AreEqual("The requested asset is not supported. See GET /prices for supported assets.", ex.Error);
        Assert.AreEqual(body, ex.ResponseBody);
        StringAssert.Contains(ex.Message, "HTTP 400");
        // Longer than the default 64-unit echo clamp, and reproduced whole (apostrophes/quotes aside, which are
        // escaped).
        StringAssert.Contains(ex.Message, "See GET /prices for supported assets.");
    }

    [TestMethod]
    public async Task OnServerError_ThrowsUnexpectedResponseException()
    {
        var (service, _) = CreateService("{\"error\":\"database unavailable\"}", HttpStatusCode.InternalServerError);

        var ex = await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() => service.InfoAsync());

        Assert.AreEqual(500, ex.StatusCode);
        Assert.AreEqual("database unavailable", ex.Error);
        Assert.IsNull(ex.RetryAfterDelay);
    }

    [TestMethod]
    public async Task PostQuoteAsync_OnTooManyRequests_ExposesTheRetryAfterDelay()
    {
        var (service, _) = CreateService(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Content = new StringContent("{\"error\":\"slow down\"}"),
            };
            response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(30));
            return response;
        });

        var ex = await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() =>
            service.PostQuoteAsync(ValidQuoteRequest()));

        Assert.AreEqual(429, ex.StatusCode);
        Assert.AreEqual(TimeSpan.FromSeconds(30), ex.RetryAfterDelay);
    }

    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.NotFound)]
    public async Task OnAStatusWithADedicatedException_LeavesRetryAfterDelayNull(HttpStatusCode statusCode)
    {
        var (service, _) = CreateService(_ =>
        {
            var response = new HttpResponseMessage(statusCode) { Content = new StringContent("{\"error\":\"no\"}") };
            response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(30));
            return response;
        });

        try
        {
            await service.InfoAsync();
            Assert.Fail("Expected a QuoteServerException.");
        }
        catch (QuoteServerException ex)
        {
            Assert.AreEqual((int)statusCode, ex.StatusCode);
            Assert.IsNotInstanceOfType(ex, typeof(UnexpectedResponseException));
            Assert.IsNull(ex.RetryAfterDelay);
        }
    }

    [TestMethod]
    public async Task OnServiceUnavailableWithAnHttpDate_ExposesTheRetryAfterDelay()
    {
        var (service, _) = CreateService(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            response.Headers.RetryAfter = new RetryConditionHeaderValue(DateTimeOffset.UtcNow.AddMinutes(10));
            return response;
        });

        var ex = await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() => service.InfoAsync());

        Assert.AreEqual(503, ex.StatusCode);
        Assert.IsNotNull(ex.RetryAfterDelay);
        Assert.IsTrue(ex.RetryAfterDelay > TimeSpan.FromMinutes(9) && ex.RetryAfterDelay <= TimeSpan.FromMinutes(10),
            $"RetryAfterDelay was {ex.RetryAfterDelay}.");
    }

    [TestMethod]
    [DataRow("<html><body>Bad Gateway</body></html>")]
    [DataRow("{\"error\":42}")]
    [DataRow("[\"error\"]")]
    [DataRow("{\"error\":\"a\",\"error\":\"b\"}")]
    [DataRow("")]
    public async Task OnErrorWithoutAUsableErrorField_LeavesErrorNull(string body)
    {
        var (service, _) = CreateService(body, HttpStatusCode.BadRequest);

        var ex = await Assert.ThrowsExceptionAsync<BadRequestException>(() => service.InfoAsync());

        Assert.IsNull(ex.Error);
        Assert.AreEqual(body, ex.ResponseBody);
        Assert.AreEqual("The quote server returned HTTP 400.", ex.Message);
    }

    [TestMethod]
    public async Task ErrorMessage_EscapesControlCharactersFromTheAnchor()
    {
        var (service, _) = CreateService("{\"error\":\"line one\\nFAKE LOG LINE\"}", HttpStatusCode.BadRequest);

        var ex = await Assert.ThrowsExceptionAsync<BadRequestException>(() => service.InfoAsync());

        Assert.AreEqual("line one\nFAKE LOG LINE", ex.Error);
        Assert.IsFalse(ex.Message.Contains('\n'));
        StringAssert.Contains(ex.Message, "line one\\u000aFAKE LOG LINE");
    }

    [TestMethod]
    public async Task ErrorMessage_ClampsAnOverlongError()
    {
        var error = new string('x', 10_000);
        var (service, _) = CreateService($"{{\"error\":\"{error}\"}}", HttpStatusCode.BadRequest);

        var ex = await Assert.ThrowsExceptionAsync<BadRequestException>(() => service.InfoAsync());

        Assert.AreEqual(error, ex.Error);
        Assert.IsTrue(ex.Message.Length < 400, $"Message length was {ex.Message.Length}.");
        StringAssert.Contains(ex.Message, "(truncated, 10000 UTF-16 code units)");
    }

    #endregion

    #region Response validation

    [TestMethod]
    [DataRow("{\"total_price\":\"0.20\",\"price\":\"0.18\",\"price\":\"0.01\",\"sell_amount\":\"100\"," +
             "\"buy_amount\":\"500\",\"fee\":{\"total\":\"1\",\"asset\":\"iso4217:BRL\"}}",
        DisplayName = "Top-level duplicate")]
    [DataRow("{\"total_price\":\"0.20\",\"price\":\"0.18\",\"sell_amount\":\"100\",\"buy_amount\":\"500\"," +
             "\"fee\":{\"total\":\"1\",\"total\":\"99\",\"asset\":\"iso4217:BRL\"}}",
        DisplayName = "Nested duplicate")]
    [DataRow("{\"total_price\":\"0.20\",\"price\":\"0.18\",\"sell_amount\":\"100\",\"buy_amount\":\"500\"," +
             "\"fee\":{\"total\":\"1\",\"asset\":\"iso4217:BRL\",\"details\":[{\"name\":\"a\",\"amount\":\"1\"," +
             "\"amount\":\"2\"}]}}",
        DisplayName = "Duplicate inside an array element")]
    public async Task PriceAsync_WithDuplicatedProperty_RejectsTheResponse(string json)
    {
        var (service, _) = CreateService(json);

        var ex = await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() => service.PriceAsync(
            new PriceRequest { Context = QuoteContext.Sep6, SellAsset = Brl, BuyAsset = Usdc, SellAmount = 1m }));

        Assert.IsInstanceOfType(ex.InnerException, typeof(JsonException));
        StringAssert.Contains(ex.InnerException!.Message, "Duplicate");
        Assert.AreEqual(200, ex.StatusCode);
        Assert.AreEqual(json, ex.ResponseBody);
    }

    [TestMethod]
    [DataRow("id")]
    [DataRow("expires_at")]
    [DataRow("price")]
    [DataRow("sell_asset")]
    [DataRow("sell_amount")]
    [DataRow("buy_asset")]
    [DataRow("buy_amount")]
    [DataRow("fee")]
    public async Task GetQuoteAsync_WithMissingRequiredField_RejectsTheResponse(string field)
    {
        using var document = JsonDocument.Parse(ReadTestData("quote-response.json"));
        var json = JsonSerializer.Serialize(document.RootElement.EnumerateObject()
            .Where(p => p.Name != field)
            .ToDictionary(p => p.Name, p => p.Value));
        var (service, _) = CreateService(json);

        var ex = await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() =>
            service.GetQuoteAsync(QuoteId, Jwt));

        Assert.IsInstanceOfType(ex.InnerException, typeof(JsonException));
        StringAssert.Contains(ex.InnerException!.Message, field);
    }

    [TestMethod]
    [DataRow("{\"assets\":null}", DisplayName = "Null list")]
    [DataRow("{\"assets\":[null]}", DisplayName = "Null asset")]
    [DataRow("{\"assets\":[{\"asset\":null}]}", DisplayName = "Null asset identifier")]
    [DataRow("{\"assets\":[{\"asset\":\"iso4217:BRL\",\"country_codes\":[\"BR\",null]}]}",
        DisplayName = "Null country code")]
    [DataRow("{\"assets\":[{\"asset\":\"iso4217:BRL\",\"sell_delivery_methods\":[{\"name\":\"PIX\"}]}]}",
        DisplayName = "Delivery method without description")]
    [DataRow("{}", DisplayName = "Missing assets")]
    [DataRow("null", DisplayName = "JSON null")]
    [DataRow("{\"assets\":[", DisplayName = "Truncated JSON")]
    [DataRow("", DisplayName = "Empty body")]
    public async Task InfoAsync_WithMalformedSuccessBody_ThrowsUnexpectedResponseException(string json)
    {
        var (service, _) = CreateService(json);

        var ex = await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() => service.InfoAsync());

        Assert.AreEqual(200, ex.StatusCode);
    }

    [TestMethod]
    [DataRow("\"0.1234567890123456789012345678901\"", DisplayName = "More digits than System.Decimal holds")]
    [DataRow("\"79228162514264337593543950336\"", DisplayName = "Overflow")]
    [DataRow("\"1e-29\"", DisplayName = "Exponent beyond System.Decimal's scale")]
    [DataRow("\"1,5\"", DisplayName = "Comma separator")]
    [DataRow("\"\"", DisplayName = "Empty")]
    [DataRow("true", DisplayName = "Boolean")]
    public async Task PricesAsync_WithInexactOrInvalidPrice_RejectsTheResponse(string price)
    {
        var json = $"{{\"buy_assets\":[{{\"asset\":\"iso4217:BRL\",\"price\":{price},\"decimals\":2}}]}}";
        var (service, _) = CreateService(json);

        await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() =>
            service.PricesAsync(new PricesRequest { SellAsset = Usdc, SellAmount = 1m }));
    }

    [TestMethod]
    public async Task PricesAsync_WithPriceAsABareJsonNumber_ReadsItExactly()
    {
        const string json = "{\"buy_assets\":[{\"asset\":\"iso4217:BRL\",\"price\":0.1795000,\"decimals\":2}]}";
        var (service, _) = CreateService(json);

        var result = await service.PricesAsync(new PricesRequest { SellAsset = Usdc, SellAmount = 1m });

        Assert.AreEqual("0.1795000", result.BuyAssets![0].Price.ToString(CultureInfo.InvariantCulture));
    }

    #endregion

    #region Response validation

    [TestMethod]
    [DataRow("{}", DisplayName = "Empty object")]
    [DataRow("{\"sell_assets\":[]}", DisplayName = "Only the other side")]
    public async Task PricesAsync_SellSideWithoutBuyAssets_ThrowsUnexpectedResponseException(string json)
    {
        var (service, _) = CreateService(json);

        await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() =>
            service.PricesAsync(new PricesRequest { SellAsset = Brl, SellAmount = 500m }));
    }

    [TestMethod]
    public async Task PricesAsync_BuySideWithoutSellAssets_ThrowsUnexpectedResponseException()
    {
        var (service, _) = CreateService("{\"buy_assets\":[]}");

        await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() =>
            service.PricesAsync(new PricesRequest { BuyAsset = Usdc, BuyAmount = 100m }));
    }

    [TestMethod]
    public async Task PricesAsync_WithExactExponentPrice_ReadsIt()
    {
        var json = "{\"buy_assets\":[{\"asset\":\"iso4217:BRL\",\"price\":\"1E-7\",\"decimals\":2}]}";
        var (service, _) = CreateService(json);

        var result = await service.PricesAsync(new PricesRequest { SellAsset = Usdc, SellAmount = 1m });

        Assert.AreEqual(0.0000001m, result.BuyAssets!.Single().Price);
    }

    [TestMethod]
    [DataRow("{\"error\":\"a\\ud800b\"}", DisplayName = "Lone surrogate escape")]
    [DataRow(null, DisplayName = "Invalid UTF-8")]
    public async Task ErrorResponse_WithUndecodableErrorText_KeepsTheStatusMapping(string? json)
    {
        var body = json == null
            ? Encoding.UTF8.GetBytes("{\"error\":\"a").Concat(new byte[] { 0xFF, 0xFE })
                .Concat(Encoding.UTF8.GetBytes("b\"}")).ToArray()
            : Encoding.UTF8.GetBytes(json);
        var (service, _) = CreateService(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new ByteArrayContent(body),
        });

        var ex = await Assert.ThrowsExceptionAsync<BadRequestException>(() => service.InfoAsync());

        Assert.IsNull(ex.Error);
    }

    [TestMethod]
    public void EndpointMethods_ReportInvalidArgumentsThroughTheReturnedTask()
    {
        var (service, _) = CreateService(ReadTestData("info-response.json"));

        // Called without await: an async method must hand back a faulted task, not throw at the call site.
        var prices = service.PricesAsync(new PricesRequest { SellAsset = Brl, BuyAsset = Usdc, SellAmount = 1m });
        var price = service.PriceAsync(null!);
        var quote = service.PostQuoteAsync(ValidQuoteRequest() with { SellAmount = null });
        var get = service.GetQuoteAsync("", Jwt);

        Assert.IsTrue(prices.IsFaulted && prices.Exception!.InnerException is ArgumentException);
        Assert.IsTrue(price.IsFaulted && price.Exception!.InnerException is ArgumentNullException);
        Assert.IsTrue(quote.IsFaulted && quote.Exception!.InnerException is ArgumentException);
        Assert.IsTrue(get.IsFaulted && get.Exception!.InnerException is ArgumentException);
    }

    private static string WithoutProperty(string json, string name)
    {
        using var document = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(document.RootElement.EnumerateObject()
            .Where(p => p.Name != name)
            .ToDictionary(p => p.Name, p => p.Value));
    }

    private static string SetProperty(string json, string name, string rawValue)
    {
        using var document = JsonDocument.Parse(json);
        var properties = document.RootElement.EnumerateObject()
            .Select(p => $"{JsonSerializer.Serialize(p.Name)}:{(p.Name == name ? rawValue : p.Value.GetRawText())}");
        return "{" + string.Join(",", properties) + "}";
    }

    #endregion

    #region Request formatting

    [TestMethod]
    public void RequestToString_RedactsTheJwtAndPrintsEveryOtherMember()
    {
        const string secret = "eyJ.SECRET.JWT";
        object[] requests =
        {
            new PricesRequest { SellAsset = Brl, SellAmount = 1m, Jwt = secret },
            new PriceRequest { Context = QuoteContext.Sep6, SellAsset = Brl, BuyAsset = Usdc, SellAmount = 1m, Jwt = secret },
            ValidQuoteRequest() with { Jwt = secret },
        };

        foreach (var request in requests)
        {
            var text = request.ToString()!;
            Assert.IsFalse(text.Contains(secret), text);
            StringAssert.Contains(text, "Jwt = <redacted>");
            // Every public property is printed, so a member added later cannot silently drop out.
            foreach (var property in request.GetType().GetProperties())
            {
                StringAssert.Contains(text, property.Name + " = ", $"{request.GetType().Name}.{property.Name}");
            }
        }
    }

    #endregion

    #region Timeouts and transport failures

    [TestMethod]
    [Timeout(10000)]
    public async Task StalledBody_IsBoundedByTheHttpClientTimeout()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ScriptedContent(Encoding.UTF8.GetBytes("{\"ass"), stall: true),
        });
        using var httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(300) };
        var service = new QuoteService(ServiceAddress, httpClient);

        var ex = await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => service.InfoAsync());

        Assert.IsInstanceOfType(ex.InnerException, typeof(TimeoutException));
    }

    [TestMethod]
    [Timeout(10000)]
    public async Task StalledBody_IsCanceledByTheCallerToken()
    {
        var (service, _) = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ScriptedContent(Encoding.UTF8.GetBytes("{\"ass"), stall: true),
        });
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

        var ex = await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => service.InfoAsync(null, cts.Token));

        Assert.IsNotInstanceOfType(ex.InnerException, typeof(TimeoutException));
    }

    [TestMethod]
    [Timeout(10000)]
    public async Task StalledBody_IsBoundedByTheInternalClientRequestTimeout()
    {
        // The internal client cannot take a stub handler, so this runs against a loopback socket that sends the
        // headers and a few body bytes, then goes silent. HttpClient.Timeout (100 s) would never fire in time.
        using var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var server = Task.Run(async () =>
        {
            using var client = await listener.AcceptTcpClientAsync();
            var stream = client.GetStream();
            var buffer = new byte[8192];
            await stream.ReadAsync(buffer, 0, buffer.Length);
            var head = Encoding.ASCII.GetBytes(
                "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: 1000\r\n\r\n{\"ass");
            await stream.WriteAsync(head, 0, head.Length);
            await Task.Delay(TimeSpan.FromSeconds(9));
        });
        using var service = new QuoteService($"http://127.0.0.1:{port}/sep38", null,
            new StellarDotnetSdk.Requests.HttpResilienceOptions { RequestTimeout = TimeSpan.FromMilliseconds(300) });

        var ex = await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => service.InfoAsync());

        Assert.IsInstanceOfType(ex.InnerException, typeof(TimeoutException));
        listener.Stop();
    }

    [TestMethod]
    public async Task TruncatedSuccessBody_SurfacesAsHttpRequestException()
    {
        var (service, _) = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ScriptedContent(Encoding.UTF8.GetBytes("{\"ass"), stall: false),
        });

        var ex = await Assert.ThrowsExceptionAsync<HttpRequestException>(() => service.InfoAsync());

        Assert.IsInstanceOfType(ex.InnerException, typeof(IOException));
    }

    [TestMethod]
    public async Task UndecompressibleSuccessBody_SurfacesAsHttpRequestException()
    {
        // What a decompressing handler raises for a corrupt gzip body.
        var (service, _) = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ScriptedContent(Encoding.UTF8.GetBytes("{\"ass"), stall: false,
                failure: new InvalidDataException("The archive entry was compressed using an unsupported method.")),
        });

        var ex = await Assert.ThrowsExceptionAsync<HttpRequestException>(() => service.InfoAsync());

        Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidDataException));
    }

    [TestMethod]
    [Timeout(10000)]
    public async Task ResilienceTimeoutOfACallerOwnedClient_ReportsItsOwnDuration()
    {
        // The caller's handler chain times the attempt out after 200 ms, well inside HttpClient.Timeout (100 s);
        // the message must name the timeout that fired.
        using var httpClient = new StellarDotnetSdk.Requests.DefaultStellarSdkHttpClient(
            resilienceOptions: new StellarDotnetSdk.Requests.HttpResilienceOptions
            {
                RequestTimeout = TimeSpan.FromMilliseconds(200),
            },
            innerHandler: new HangingHandler());
        var service = new QuoteService(ServiceAddress, httpClient);

        var ex = await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => service.InfoAsync());

        Assert.IsInstanceOfType(ex.InnerException, typeof(TimeoutException));
        StringAssert.Contains(ex.Message, "0.2 seconds");
    }

    [TestMethod]
    public async Task TruncatedErrorBody_StillMapsTheStatus()
    {
        var (service, _) = CreateService(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new ScriptedContent(Encoding.UTF8.GetBytes("{\"err"), stall: false),
        });

        var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(() => service.InfoAsync());

        Assert.IsNull(ex.ResponseBody);
    }

    [TestMethod]
    public async Task BodyUnderTheLimit_IsReadWithoutHttpClientBuffering()
    {
        // HttpClient's own buffering (ResponseContentRead) enforces MaxResponseContentBufferSize; the service reads
        // the stream itself, so only its 1 MiB limit applies. This pins the streamed read the size limit relies on.
        var (_, handler) = CreateService(PadJson("{\"assets\":[]}", 64 * 1024));
        using var httpClient = new HttpClient(handler) { MaxResponseContentBufferSize = 1024 };
        var service = new QuoteService(ServiceAddress, httpClient);

        var result = await service.InfoAsync();

        Assert.AreEqual(0, result.Assets.Count);
    }

    [TestMethod]
    [Timeout(10000)]
    public async Task EndlessBody_IsAbandonedAtTheSizeLimit()
    {
        var content = new EndlessContent();
        var (service, _) = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = content });

        await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() => service.InfoAsync());

        // Read in 16 KiB chunks: at most one chunk past the limit, never the whole (infinite) body.
        Assert.IsTrue(content.BytesServed <= MaxResponseBodyBytes + (64 * 1024), $"{content.BytesServed} bytes read");
    }

    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.BadRequest)]
    public async Task Response_IsDisposedAfterTheCall(HttpStatusCode status)
    {
        var content = new DisposeTrackingContent(ReadTestData("info-response.json"));
        var (service, _) = CreateService(_ => new HttpResponseMessage(status) { Content = content });

        try
        {
            await service.InfoAsync();
        }
        catch (BadRequestException)
        {
        }

        Assert.IsTrue(content.Disposed);
    }

    #endregion

    #region Response size limit

    [TestMethod]
    public async Task ResponseAtTheSizeLimit_IsAccepted()
    {
        var (service, _) = CreateService(PadJson("{\"assets\":[]}", MaxResponseBodyBytes));

        var result = await service.InfoAsync();

        Assert.AreEqual(0, result.Assets.Count);
    }

    [TestMethod]
    public async Task ResponseOverTheSizeLimit_WithContentLength_IsRejected()
    {
        var (service, _) = CreateService(PadJson("{\"assets\":[]}", MaxResponseBodyBytes + 1));

        var ex = await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() => service.InfoAsync());

        StringAssert.Contains(ex.Message, "limit");
        Assert.IsNull(ex.ResponseBody);
    }

    [TestMethod]
    public async Task DeclaredContentLengthOverTheLimit_IsRejectedBeforeTheBodyIsRead()
    {
        // The body itself is small and valid: only the Content-Length check can reject it, so this pins that the
        // declared length is honoured up front rather than after reading (and buffering) the body.
        var (service, _) = CreateService(_ =>
        {
            var content = new StringContent("{\"assets\":[]}");
            content.Headers.ContentLength = MaxResponseBodyBytes + 1L;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        });

        var ex = await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() => service.InfoAsync());

        StringAssert.Contains(ex.Message, "limit");
    }

    [TestMethod]
    public async Task ResponseOverTheSizeLimit_WithoutContentLength_IsRejected()
    {
        var payload = Encoding.UTF8.GetBytes(PadJson("{\"assets\":[]}", MaxResponseBodyBytes + 1));
        var (service, _) = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new UnknownLengthContent(payload),
        });

        var ex = await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() => service.InfoAsync());

        StringAssert.Contains(ex.Message, "limit");
    }

    [TestMethod]
    public async Task ErrorResponseOverTheSizeLimit_IsRejectedWithItsStatusCode()
    {
        var (service, _) = CreateService(new string(' ', MaxResponseBodyBytes + 1), HttpStatusCode.BadRequest);

        var ex = await Assert.ThrowsExceptionAsync<UnexpectedResponseException>(() => service.InfoAsync());

        Assert.AreEqual(400, ex.StatusCode);
    }

    private static string PadJson(string json, int totalBytes)
    {
        return json + new string(' ', totalBytes - Encoding.UTF8.GetByteCount(json));
    }

    #endregion

    #region Cancellation and disposal

    [TestMethod]
    public async Task CanceledToken_CancelsTheRequest()
    {
        var (service, handler) = CreateService(ReadTestData("info-response.json"));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => service.InfoAsync(null, cts.Token));
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task Dispose_LeavesACallerOwnedHttpClientUsable()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ReadTestData("info-response.json")),
        });
        using var httpClient = new HttpClient(handler);

        new QuoteService(ServiceAddress, httpClient).Dispose();
        using var second = new QuoteService(ServiceAddress, httpClient);

        Assert.AreEqual(3, (await second.InfoAsync()).Assets.Count);
    }

    [TestMethod]
    public void Dispose_WithAnInternalHttpClient_DoesNotThrow()
    {
        var service = new QuoteService(ServiceAddress);
        service.Dispose();
        service.Dispose();
    }

    #endregion

    /// <summary>
    ///     Answers every request with the given factory and records what was sent. The body and content headers
    ///     are captured during the call, because <see cref="QuoteService" /> disposes the request afterwards.
    /// </summary>
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
        {
            _respond = respond;
        }

        public List<HttpRequestMessage> Requests { get; } = new();
        public List<string?> Bodies { get; } = new();
        public List<string?> ContentTypes { get; } = new();
        public List<string[]> ContentLanguages { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request);
            Bodies.Add(request.Content == null ? null : await request.Content.ReadAsStringAsync());
            ContentTypes.Add(request.Content?.Headers.ContentType?.MediaType);
            ContentLanguages.Add(request.Content?.Headers.ContentLanguage.ToArray() ?? Array.Empty<string>());
            return _respond(request);
        }
    }

    /// <summary>
    ///     Content that does not report its length, as a chunked response does, so only the bytes actually read
    ///     can enforce the size limit.
    /// </summary>
    private sealed class UnknownLengthContent : HttpContent
    {
        private readonly byte[] _payload;

        public UnknownLengthContent(byte[] payload)
        {
            _payload = payload;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            return stream.WriteAsync(_payload, 0, _payload.Length);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }

    /// <summary>
    ///     Serves a prefix, then either stalls until canceled or fails the way a dropped connection does.
    /// </summary>
    private sealed class ScriptedContent : HttpContent
    {
        private readonly Exception? _failure;
        private readonly byte[] _prefix;
        private readonly bool _stall;

        public ScriptedContent(byte[] prefix, bool stall, Exception? failure = null)
        {
            _prefix = prefix;
            _stall = stall;
            _failure = failure;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            throw new NotSupportedException();
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            return Task.FromResult<Stream>(new ScriptedStream(_prefix, _stall, _failure));
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }

        private sealed class ScriptedStream : Stream
        {
            private readonly Exception? _failure;
            private readonly byte[] _prefix;
            private readonly bool _stall;
            private int _position;

            public ScriptedStream(byte[] prefix, bool stall, Exception? failure)
            {
                _prefix = prefix;
                _stall = stall;
                _failure = failure;
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count,
                CancellationToken cancellationToken)
            {
                if (_position < _prefix.Length)
                {
                    var n = Math.Min(count, _prefix.Length - _position);
                    Array.Copy(_prefix, _position, buffer, offset, n);
                    _position += n;
                    return n;
                }

                if (_stall)
                {
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                }

                throw _failure ?? new IOException("The response ended prematurely.");
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return ReadAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();
            }

            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
        }
    }

    /// <summary>A body that never ends, counting the bytes handed out.</summary>
    private sealed class EndlessContent : HttpContent
    {
        public long BytesServed { get; private set; }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            throw new NotSupportedException();
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            return Task.FromResult<Stream>(new EndlessStream(this));
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }

        private sealed class EndlessStream : Stream
        {
            private readonly EndlessContent _owner;

            public EndlessStream(EndlessContent owner)
            {
                _owner = owner;
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                Array.Fill(buffer, (byte)' ', offset, count);
                _owner.BytesServed += count;
                return count;
            }

            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
        }
    }

    /// <summary>A string body that records whether it was disposed.</summary>
    private sealed class DisposeTrackingContent : StringContent
    {
        public DisposeTrackingContent(string content)
            : base(content)
        {
        }

        public bool Disposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }

    /// <summary>Never answers, until the request is canceled.</summary>
    private sealed class HangingHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new InvalidOperationException("Unreachable.");
        }
    }
}
