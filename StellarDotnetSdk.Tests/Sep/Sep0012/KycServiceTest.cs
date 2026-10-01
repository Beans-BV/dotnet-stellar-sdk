using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Converters;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Sep.Sep0009;
using StellarDotnetSdk.Sep.Sep0012;
using StellarDotnetSdk.Sep.Sep0012.Exceptions;
using StellarDotnetSdk.Sep.Sep0012.Requests;
using StellarDotnetSdk.Sep.Sep0012.Responses;
using StellarDotnetSdk.Tests.Sep.Sep0009.Fixtures;

namespace StellarDotnetSdk.Tests.Sep.Sep0012;

/// <summary>
///     Unit tests for the SEP-12 <see cref="KycService" />, driven through a recording fake HTTP handler so each test
///     asserts both what goes on the wire and how the anchor's answer is parsed.
/// </summary>
[TestClass]
public class KycServiceTest
{
    private const string KycServerUrl = "https://kyc.example.com/sep12";
    private const string Jwt = "test-jwt-token";
    private const string CustomerId = "d1ce2f48-3ff1-495d-9240-7a50d806cfed";
    private const string Account = "GBORFR3GDNVZ5PLUTBDQHKGWVD26CQUHORO2T3SDQ2JPLGLUJCCA5GK6";

    private static string ReadTestData(string fileName)
    {
        return File.ReadAllText(Utils.GetTestDataAbsolutePath(fileName));
    }

    private static (KycService Service, RecordingHandler Handler) CreateService(
        string? responseBody,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        Dictionary<string, string>? httpRequestHeaders = null)
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(statusCode)
        {
            Content = responseBody == null ? null : new StringContent(responseBody, Encoding.UTF8, "application/json"),
        });
        return (new KycService(KycServerUrl, new HttpClient(handler), httpRequestHeaders: httpRequestHeaders),
            handler);
    }

    private static async Task<T> AssertThrowsAsync<T>(Func<Task> action) where T : Exception
    {
        try
        {
            await action();
        }
        catch (T ex)
        {
            return ex;
        }
        catch (Exception ex)
        {
            Assert.Fail($"Expected {typeof(T).Name} but got {ex.GetType().Name}: {ex.Message}");
        }

        Assert.Fail($"Expected {typeof(T).Name} but no exception was thrown.");
        return null!;
    }

    private static void AssertAuthorized(RecordedRequest request)
    {
        Assert.AreEqual("Bearer", request.AuthorizationScheme);
        Assert.AreEqual(Jwt, request.AuthorizationParameter);
    }

    #region Construction and discovery

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("kyc.example.com")]
    [DataRow("/customer")]
    [DataRow("ftp://kyc.example.com")]
    [DataRow("http://evil.com\\@localhost/", DisplayName = "backslash before '@', which Uri and the as-written host read differently")]
    public void Constructor_WithInvalidAddress_ThrowsArgumentException(string address)
    {
        Assert.ThrowsException<ArgumentException>(() => new KycService(address, new HttpClient()));
    }

    [TestMethod]
    public void Constructor_TrimsTrailingSlash()
    {
        using var service = new KycService(KycServerUrl + "/", new HttpClient());
        Assert.AreEqual(KycServerUrl, service.ServiceAddress);
    }

    [TestMethod]
    public async Task Constructor_WithoutHttpClient_CreatesAndDisposesInternalClient()
    {
        var service = new KycService(KycServerUrl);
        var internalClient = (HttpClient)typeof(KycService)
            .GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(service)!;

        service.Dispose();
        service.Dispose();

        await AssertThrowsAsync<ObjectDisposedException>(() =>
            internalClient.GetAsync("https://kyc.example.com/"));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("a b")]
    [DataRow("a\u00e9")]
    public async Task Dispose_ThenCallWithInvalidJwt_ReportsTheInvalidArgument(string jwt)
    {
        var (service, _) = CreateService(ReadTestData("customer-rejected.json"));
        service.Dispose();

        await AssertThrowsAsync<ArgumentException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = jwt }));
    }

    [TestMethod]
    public async Task Dispose_ThenCall_ThrowsObjectDisposedException()
    {
        var (service, handler) = CreateService(ReadTestData("customer-rejected.json"));
        service.Dispose();

        await AssertThrowsAsync<ObjectDisposedException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public void Dispose_MarksTheServiceDisposedBeforeReleasingTheInternalClient()
    {
        var service = new KycService(KycServerUrl);
        var disposedField = typeof(KycService).GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var clientField = typeof(KycService).GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance)!;
        using var internalClient = (HttpClient)clientField.GetValue(service)!;
        var client = new DisposeObservingHttpClient(() => (bool)disposedField.GetValue(service)!);
        clientField.SetValue(service, client);

        service.Dispose();

        Assert.AreEqual(true, client.ServiceDisposedWhenReleased,
            "A call starting while the client is released would report HttpClient, not KycService, as disposed.");
    }

    [TestMethod]
    [DataRow("http://kyc.example.com")]
    [DataRow("http://10.0.0.5/sep12")]
    [DataRow("http://loopback/sep12", DisplayName = "the bare name 'loopback' is not a loopback address")]
    [DataRow("http://localhost.evil.example/sep12")]
    [DataRow("http://0177.0.0.1/sep12", DisplayName = "octal IPv4, which some parsers read as 177.0.0.1")]
    [DataRow("http://0x7f.1/sep12", DisplayName = "hex IPv4")]
    [DataRow("http://2130706433/sep12", DisplayName = "integer IPv4")]
    [DataRow("http://127.1/sep12", DisplayName = "short IPv4")]
    [DataRow("http://[::1]x/sep12", DisplayName = "text after an IPv6 literal, which Uri reads as path")]
    public void Constructor_WithHttpForNonLoopbackHost_ThrowsArgumentException(string address)
    {
        var ex = Assert.ThrowsException<ArgumentException>(() => new KycService(address, new HttpClient()));

        StringAssert.Contains(ex.Message, "https");
    }

    [TestMethod]
    [DataRow("http://localhost:8000/kyc")]
    [DataRow("http://127.0.0.1/kyc")]
    [DataRow("http://[::1]:8000")]
    [DataRow("http://LOCALHOST:8000")]
    [DataRow("http://127.0.0.2")]
    [DataRow("http://[::1]")]
    [DataRow("http://[::FFFF:127.0.0.1]:8000")]
    public void Constructor_WithHttpForLoopbackHost_IsAccepted(string address)
    {
        using var service = new KycService(address, new HttpClient());

        Assert.AreEqual(address, service.ServiceAddress);
    }

    [TestMethod]
    [DataRow("https://kyc.example.com/sep12?tenant=1")]
    [DataRow("https://kyc.example.com/sep12#frag")]
    [DataRow("https://kyc.example.com/sep12#")]
    [DataRow("https://user:secret@kyc.example.com/sep12")]
    public void Constructor_WithQueryFragmentOrUserInfo_ThrowsArgumentException(string address)
    {
        Assert.ThrowsException<ArgumentException>(() => new KycService(address, new HttpClient()));
    }

    [TestMethod]
    public void Constructor_WithNullAddress_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => new KycService(null!, new HttpClient()));
    }

    [TestMethod]
    [DataRow("https:\\\\kyc.example.com/sep12", DisplayName = "backslash authority")]
    [DataRow("https:kyc.example.com/sep12", DisplayName = "no slashes")]
    public void Constructor_WithoutSchemeSlashes_ThrowsArgumentException(string address)
    {
        Assert.ThrowsException<ArgumentException>(() => new KycService(address, new HttpClient()));
    }

    [TestMethod]
    [DataRow("KYC_SERVER=\"http://kyc.example.com/sep12\"", DisplayName = "http")]
    [DataRow("KYC_SERVER=\"http://127.0.0.1:8080\"", DisplayName = "http loopback from a remote toml")]
    [DataRow("KYC_SERVER=\"https://kyc.example.com/sep12?x=1\"", DisplayName = "query")]
    [DataRow("KYC_SERVER=\"/sep12\"", DisplayName = "relative")]
    public async Task FromDomainAsync_WithUnusableDeclaredServer_ThrowsKycServiceException(string toml)
    {
        using var httpClient = CreateTomlClient(toml);

        var ex = await AssertThrowsAsync<KycServiceException>(() =>
            KycService.FromDomainAsync("example.com", httpClient: httpClient));

        Assert.IsNull(ex.StatusCode);
    }

    [TestMethod]
    public void Constructor_TrimsSurroundingWhitespace()
    {
        using var service = new KycService("  " + KycServerUrl + "/  ", new HttpClient());

        Assert.AreEqual(KycServerUrl, service.ServiceAddress);
    }

    [TestMethod]
    public async Task Dispose_DoesNotDisposeExternalHttpClient()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ReadTestData("customer-rejected.json")),
        });
        var httpClient = new HttpClient(handler);
        new KycService(KycServerUrl, httpClient).Dispose();

        using var second = new KycService(KycServerUrl, httpClient);
        var response = await second.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt });

        Assert.AreEqual(CustomerStatus.Rejected, response.Status);
    }

    private static HttpClient CreateTomlClient(string toml)
    {
        return new HttpClient(new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(toml),
        }));
    }

    [TestMethod]
    public async Task FromDomainAsync_WithKycServer_UsesKycServer()
    {
        using var httpClient = CreateTomlClient(@"
KYC_SERVER=""https://kyc.example.com/sep12""
TRANSFER_SERVER=""https://transfer.example.com/sep6""
");
        using var service = await KycService.FromDomainAsync("example.com", httpClient: httpClient);

        Assert.AreEqual("https://kyc.example.com/sep12", service.ServiceAddress);
    }

    [TestMethod]
    public async Task FromDomainAsync_WithoutKycServer_FallsBackToTransferServer()
    {
        using var httpClient = CreateTomlClient(@"
TRANSFER_SERVER=""https://transfer.example.com/sep6/""
");
        using var service = await KycService.FromDomainAsync("example.com", httpClient: httpClient);

        Assert.AreEqual("https://transfer.example.com/sep6", service.ServiceAddress);
    }

    [TestMethod]
    public async Task FromDomainAsync_WithBlankKycServer_FallsBackToTransferServer()
    {
        using var httpClient = CreateTomlClient(@"
KYC_SERVER=""  ""
TRANSFER_SERVER=""https://transfer.example.com/sep6""
");
        using var service = await KycService.FromDomainAsync("example.com", httpClient: httpClient);

        Assert.AreEqual("https://transfer.example.com/sep6", service.ServiceAddress);
    }

    [TestMethod]
    public async Task FromDomainAsync_WithNeitherServer_ThrowsKycServiceException()
    {
        using var httpClient = CreateTomlClient(@"
WEB_AUTH_ENDPOINT=""https://example.com/auth""
");
        var ex = await AssertThrowsAsync<KycServiceException>(() =>
            KycService.FromDomainAsync("example.com", httpClient: httpClient));

        Assert.IsNull(ex.StatusCode);
        StringAssert.Contains(ex.Message, "KYC_SERVER");
    }

    [TestMethod]
    public async Task FromDomainAsync_RequestsStellarTomlFromDomain()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("KYC_SERVER=\"https://kyc.example.com\""),
        });
        using var httpClient = new HttpClient(handler);

        using var service = await KycService.FromDomainAsync("example.com", httpClient: httpClient);

        Assert.AreEqual("https://example.com/.well-known/stellar.toml", handler.Requests.Single().Uri.ToString());
    }

    #endregion

    #region GET /customer

    [TestMethod]
    public async Task GetCustomerInfoAsync_Accepted_ParsesStatusAndProvidedFields()
    {
        var (service, handler) = CreateService(ReadTestData("customer-accepted.json"));

        var response = await service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt, Id = CustomerId });

        Assert.AreEqual(CustomerId, response.Id);
        Assert.AreEqual(CustomerStatus.Accepted, response.Status);
        Assert.IsNull(response.Fields);
        Assert.IsNull(response.Message);
        Assert.IsNotNull(response.ProvidedFields);
        Assert.AreEqual(3, response.ProvidedFields.Count);
        var firstName = response.ProvidedFields["first_name"];
        Assert.AreEqual(FieldType.String, firstName.Type);
        Assert.AreEqual("The customer's first name", firstName.Description);
        Assert.AreEqual(ProvidedFieldStatus.Accepted, firstName.Status);
        Assert.IsNull(firstName.Error);
        Assert.IsNull(firstName.Choices);
        Assert.IsFalse(firstName.Optional);

        var request = handler.Requests.Single();
        Assert.AreEqual(HttpMethod.Get, request.Method);
        Assert.AreEqual($"{KycServerUrl}/customer?id={CustomerId}", request.Uri.ToString());
        AssertAuthorized(request);
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_NeedsInfo_ParsesFieldsProvidedFieldsAndMessage()
    {
        var (service, _) = CreateService(ReadTestData("customer-needs-info.json"));

        var response = await service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt });

        Assert.AreEqual(CustomerStatus.NeedsInfo, response.Status);
        Assert.AreEqual("Please provide your mobile number and date of birth.", response.Message);
        Assert.IsNotNull(response.Fields);
        Assert.AreEqual(4, response.Fields.Count);
        Assert.AreEqual(FieldType.String, response.Fields["mobile_number"].Type);
        // Absent reads as the SEP-0012 default, false, the same as the explicit false on birth_date.
        Assert.IsFalse(response.Fields["mobile_number"].Optional);
        Assert.IsTrue(response.Fields["email_address"].Optional);
        Assert.AreEqual(FieldType.Date, response.Fields["birth_date"].Type);
        Assert.IsFalse(response.Fields["birth_date"].Optional);
        Assert.AreEqual(FieldType.Number, response.Fields["number_of_shareholders"].Type);

        Assert.IsNotNull(response.ProvidedFields);
        Assert.AreEqual(ProvidedFieldStatus.Accepted, response.ProvidedFields["first_name"].Status);
        var lastName = response.ProvidedFields["last_name"];
        Assert.AreEqual(ProvidedFieldStatus.Rejected, lastName.Status);
        Assert.AreEqual("The last name does not match the ID document.", lastName.Error);
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_UnknownCustomer_ParsesChoicesAndBinaryFields()
    {
        var (service, _) = CreateService(ReadTestData("customer-new-needs-info.json"));

        var response = await service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt });

        Assert.IsNull(response.Id);
        Assert.AreEqual(CustomerStatus.NeedsInfo, response.Status);
        Assert.IsNull(response.ProvidedFields);
        Assert.IsNotNull(response.Fields);
        CollectionAssert.AreEqual(new[] { "Passport", "Drivers License", "State ID" },
            response.Fields["id_type"].Choices);
        Assert.AreEqual(FieldType.Binary, response.Fields["photo_id_front"].Type);
        Assert.AreEqual("A clear photo of the front of the government issued ID",
            response.Fields["photo_id_front"].Description);
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_Processing_ParsesMessageAndProcessingField()
    {
        var (service, _) = CreateService(ReadTestData("customer-processing.json"));

        var response = await service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt });

        Assert.AreEqual(CustomerStatus.Processing, response.Status);
        StringAssert.StartsWith(response.Message, "Photo ID requires manual review.");
        Assert.IsNotNull(response.ProvidedFields);
        Assert.AreEqual(ProvidedFieldStatus.Processing, response.ProvidedFields["photo_id_front"].Status);
        Assert.AreEqual(FieldType.Binary, response.ProvidedFields["photo_id_front"].Type);
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_Rejected_ParsesMessage()
    {
        var (service, _) = CreateService(ReadTestData("customer-rejected.json"));

        var response = await service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt });

        Assert.AreEqual(CustomerStatus.Rejected, response.Status);
        Assert.AreEqual("This person is on a sanctions list", response.Message);
        Assert.IsNull(response.Fields);
        Assert.IsNull(response.ProvidedFields);
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_VerificationRequired_ParsesProvidedFieldStatus()
    {
        var (service, _) = CreateService(ReadTestData("customer-verification-required.json"));

        var response = await service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt });

        Assert.AreEqual(CustomerStatus.NeedsInfo, response.Status);
        Assert.IsNotNull(response.ProvidedFields);
        Assert.AreEqual(ProvidedFieldStatus.VerificationRequired, response.ProvidedFields["mobile_number"].Status);
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_SendsEveryParameterEscaped()
    {
        var (service, handler) = CreateService(ReadTestData("customer-accepted.json"));

        await service.GetCustomerInfoAsync(new GetCustomerInfoRequest
        {
            Jwt = Jwt,
            Id = CustomerId,
            Account = Account,
            Memo = "12345",
            MemoType = "id",
            Type = "sep31-sender",
            TransactionId = "tx 1&2",
            Lang = "es",
        });

        Assert.AreEqual(
            $"{KycServerUrl}/customer?id={CustomerId}&account={Account}&memo=12345&memo_type=id" +
            "&type=sep31-sender&transaction_id=tx%201%262&lang=es",
            handler.Requests.Single().Uri.AbsoluteUri);
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_WithoutParameters_SendsNoQueryString()
    {
        var (service, handler) = CreateService(ReadTestData("customer-new-needs-info.json"));

        await service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt, Memo = " " });

        Assert.AreEqual($"{KycServerUrl}/customer", handler.Requests.Single().Uri.ToString());
        Assert.IsNull(handler.Requests.Single().Body);
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_SendsCustomHeaders_AndJwtOverridesCustomAuthorization()
    {
        var headers = new Dictionary<string, string>
        {
            ["X-Client-Name"] = "test-wallet",
            ["Authorization"] = "Bearer stale-token",
        };
        var (service, handler) = CreateService(ReadTestData("customer-accepted.json"), httpRequestHeaders: headers);

        await service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt });

        var request = handler.Requests.Single();
        Assert.AreEqual("test-wallet", request.Headers["X-Client-Name"]);
        AssertAuthorized(request);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("  ")]
    public async Task GetCustomerInfoAsync_WithEmptyJwt_ThrowsBeforeSending(string jwt)
    {
        var (service, handler) = CreateService(ReadTestData("customer-accepted.json"));

        await AssertThrowsAsync<ArgumentException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = jwt }));

        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        var (service, _) = CreateService(ReadTestData("customer-accepted.json"));

        await AssertThrowsAsync<ArgumentNullException>(() => service.GetCustomerInfoAsync(null!));
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        var (service, _) = CreateService(ReadTestData("customer-accepted.json"));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await AssertThrowsAsync<OperationCanceledException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }, cts.Token));
    }

    #endregion

    #region Response validation

    [TestMethod]
    [DataRow("\"accepted\"", DisplayName = "lowercase status")]
    [DataRow("\"Accepted\"", DisplayName = "PascalCase status")]
    [DataRow("\"NeedsInfo\"", DisplayName = "C# member name")]
    [DataRow("\"APPROVED\"", DisplayName = "undefined status")]
    [DataRow("0", DisplayName = "ordinal of ACCEPTED")]
    [DataRow("null", DisplayName = "null status")]
    public async Task GetCustomerInfoAsync_WithInvalidStatus_ThrowsInvalidKycResponseException(string status)
    {
        var (service, _) = CreateService($"{{\"status\": {status}}}");

        var ex = await AssertThrowsAsync<InvalidKycResponseException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        Assert.AreEqual(200, ex.StatusCode);
        Assert.IsInstanceOfType(ex.InnerException, typeof(JsonException));
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_WithMissingStatus_ThrowsInvalidKycResponseException()
    {
        var (service, _) = CreateService($"{{\"id\": \"{CustomerId}\"}}");

        var ex = await AssertThrowsAsync<InvalidKycResponseException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        StringAssert.Contains(ex.InnerException!.Message, "status");
    }

    [TestMethod]
    [DataRow("{\"status\":\"NEEDS_INFO\",\"fields\":{\"x\":{\"type\":\"STRING\",\"description\":\"d\"}}}", DisplayName = "uppercase type")]
    [DataRow("{\"status\":\"NEEDS_INFO\",\"fields\":{\"x\":{\"type\":\"file\",\"description\":\"d\"}}}", DisplayName = "undefined type")]
    [DataRow("{\"status\":\"NEEDS_INFO\",\"fields\":{\"x\":{\"description\":\"d\"}}}", DisplayName = "missing type")]
    [DataRow("{\"status\":\"NEEDS_INFO\",\"fields\":{\"x\":null}}", DisplayName = "null field entry")]
    [DataRow("{\"status\":\"NEEDS_INFO\",\"fields\":{\"x\":{\"type\":\"string\",\"description\":\"d\",\"choices\":[\"a\",null]}}}",
        DisplayName = "null choice")]
    [DataRow("{\"status\":\"ACCEPTED\",\"provided_fields\":{\"x\":{\"type\":\"string\",\"description\":\"d\",\"status\":\"VERIFIED\"}}}",
        DisplayName = "undefined provided status")]
    [DataRow("{\"status\":\"ACCEPTED\",\"provided_fields\":{\"x\":{\"type\":\"string\",\"description\":\"d\",\"status\":\"verification_required\"}}}",
        DisplayName = "lowercase provided status")]
    [DataRow("{\"status\":\"ACCEPTED\",\"provided_fields\":{\"x\":null}}", DisplayName = "null provided entry")]
    [DataRow("{\"status\":\"NEEDS_INFO\",\"fields\":{\"x\":{\"type\":\"string\"}}}", DisplayName = "missing description")]
    [DataRow("{\"status\":\"NEEDS_INFO\",\"fields\":{\"x\":{\"type\":\"string\",\"description\":null}}}",
        DisplayName = "null description")]
    [DataRow("{\"status\":\"ACCEPTED\",\"provided_fields\":{\"x\":{\"type\":\"string\",\"status\":\"ACCEPTED\"}}}",
        DisplayName = "missing provided description")]
    [DataRow("{\"status\":\"ACCEPTED\",\"provided_fields\":{\"x\":{\"type\":\"string\",\"description\":null}}}",
        DisplayName = "null provided description")]
    [DataRow("{\"status\":\"NEEDS_INFO\",\"fields\":{\"x\":{\"type\":\"string\",\"description\":\"d\",\"optional\":\"true\"}}}",
        DisplayName = "string optional")]
    [DataRow("{\"status\":\"NEEDS_INFO\",\"fields\":{\"x\":{\"type\":\"string\",\"description\":\"d\",\"optional\":1}}}",
        DisplayName = "number optional")]
    [DataRow("{\"status\":\"ACCEPTED\",\"provided_fields\":{\"x\":{\"type\":\"string\",\"description\":\"d\",\"optional\":{}}}}",
        DisplayName = "object provided optional")]
    public async Task GetCustomerInfoAsync_WithInvalidField_ThrowsInvalidKycResponseException(string body)
    {
        var (service, _) = CreateService(body);

        await AssertThrowsAsync<InvalidKycResponseException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));
    }

    [TestMethod]
    public void FromJson_WithNullOptional_ReadsFalse()
    {
        // Anchors whose serializers write out null properties send "optional": null for "not set".
        var response = GetCustomerInfoResponse.FromJson(
            "{\"status\":\"NEEDS_INFO\"," +
            "\"fields\":{\"a\":{\"type\":\"string\",\"description\":\"d\",\"optional\":null}," +
            "\"b\":{\"type\":\"string\",\"description\":\"d\",\"optional\":true}}," +
            "\"provided_fields\":{\"c\":{\"type\":\"string\",\"description\":\"d\",\"optional\":null}}}");

        Assert.IsFalse(response.Fields!["a"].Optional);
        Assert.IsTrue(response.Fields["b"].Optional);
        Assert.IsFalse(response.ProvidedFields!["c"].Optional);
    }

    [TestMethod]
    [DataRow("{\"status\":\"REJECTED\",\"status\":\"ACCEPTED\"}", DisplayName = "duplicate status")]
    [DataRow("{\"status\":\"NEEDS_INFO\",\"fields\":{\"x\":{\"type\":\"string\",\"type\":\"binary\",\"description\":\"d\"}}}",
        DisplayName = "duplicate field type")]
    [DataRow("{\"status\":\"NEEDS_INFO\",\"fields\":{\"x\":{\"type\":\"string\",\"description\":\"d\"},\"x\":{\"type\":\"binary\",\"description\":\"d\"}}}",
        DisplayName = "duplicate fields key")]
    [DataRow("{\"status\":\"ACCEPTED\",\"provided_fields\":{\"x\":{\"type\":\"string\",\"description\":\"d\",\"status\":\"REJECTED\",\"status\":\"ACCEPTED\"}}}",
        DisplayName = "duplicate provided status")]
    // PropertyNameCaseInsensitive makes case variants the same property, so they must count as duplicates too.
    [DataRow("{\"status\":\"REJECTED\",\"Status\":\"ACCEPTED\"}", DisplayName = "case-variant status")]
    [DataRow("{\"status\":\"NEEDS_INFO\",\"fields\":{\"x\":{\"type\":\"string\",\"Type\":\"binary\",\"description\":\"d\"}}}",
        DisplayName = "case-variant field type")]
    [DataRow("{\"status\":\"ACCEPTED\",\"provided_fields\":{\"x\":{\"type\":\"string\",\"description\":\"d\",\"status\":\"REJECTED\",\"STATUS\":\"ACCEPTED\"}}}",
        DisplayName = "case-variant provided status")]
    public async Task GetCustomerInfoAsync_WithDuplicateProperty_ThrowsInvalidKycResponseException(string body)
    {
        var (service, _) = CreateService(body);

        var ex = await AssertThrowsAsync<InvalidKycResponseException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        StringAssert.Contains(ex.InnerException!.Message, "Duplicate");
    }

    [TestMethod]
    [DataRow("", DisplayName = "empty body")]
    [DataRow("   ", DisplayName = "whitespace body")]
    [DataRow("null", DisplayName = "JSON null")]
    [DataRow("<html>ok</html>", DisplayName = "not JSON")]
    [DataRow("[]", DisplayName = "array")]
    public async Task GetCustomerInfoAsync_WithMalformedBody_ThrowsInvalidKycResponseException(string body)
    {
        var (service, _) = CreateService(body);

        await AssertThrowsAsync<InvalidKycResponseException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_WithNoContent_ThrowsInvalidKycResponseException()
    {
        var (service, _) = CreateService(null);

        await AssertThrowsAsync<InvalidKycResponseException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_WithDeclaredOversizedBody_ThrowsWithoutReadingIt()
    {
        var stream = new TrackingStream(new byte[16]);
        var handler = new RecordingHandler(_ =>
        {
            var content = new StreamContent(stream);
            content.Headers.ContentLength = KycService.MaxResponseBodyBytes + 1L;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        });
        using var service = new KycService(KycServerUrl, new HttpClient(handler));

        var ex = await AssertThrowsAsync<InvalidKycResponseException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        StringAssert.Contains(ex.Message, "limit");
        Assert.AreEqual(0, stream.BytesRead);
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_WithUndeclaredOversizedBody_StopsReadingAtLimit()
    {
        var stream = new TrackingStream(new byte[KycService.MaxResponseBodyBytes * 4]);
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new UnknownLengthContent(stream),
        });
        using var service = new KycService(KycServerUrl, new HttpClient(handler));

        await AssertThrowsAsync<InvalidKycResponseException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        Assert.IsTrue(stream.BytesRead <= KycService.MaxResponseBodyBytes + (16 * 1024),
            $"Read {stream.BytesRead} bytes past the limit.");
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_WithBodyExactlyAtLimit_IsAccepted()
    {
        var prefix = "{\"status\":\"ACCEPTED\",\"message\":\"";
        const string suffix = "\"}";
        var padding = new string('a', KycService.MaxResponseBodyBytes - prefix.Length - suffix.Length);
        var (service, _) = CreateService(prefix + padding + suffix);

        var response = await service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt });

        Assert.AreEqual(padding.Length, response.Message!.Length);
    }

    [TestMethod]
    public void EnumConverters_WriteSep12WireLiterals()
    {
        var response = new GetCustomerInfoResponse
        {
            Status = CustomerStatus.NeedsInfo,
            ProvidedFields = new Dictionary<string, GetCustomerInfoProvidedField>
            {
                ["mobile_number"] = new()
                {
                    Type = FieldType.String,
                    Description = "mobile phone number of the customer",
                    Status = ProvidedFieldStatus.VerificationRequired,
                },
            },
        };

        var json = JsonSerializer.Serialize(response, JsonOptions.DefaultOptions);

        StringAssert.Contains(json, "\"status\":\"NEEDS_INFO\"");
        StringAssert.Contains(json, "\"type\":\"string\"");
        StringAssert.Contains(json, "\"status\":\"VERIFICATION_REQUIRED\"");
        Assert.AreEqual(CustomerStatus.NeedsInfo, GetCustomerInfoResponse.FromJson(json).Status);
    }

    [TestMethod]
    public void EnumConverters_RejectUndefinedValueOnWrite()
    {
        var response = new GetCustomerInfoResponse { Status = (CustomerStatus)42 };

        Assert.ThrowsException<JsonException>(() => JsonSerializer.Serialize(response, JsonOptions.DefaultOptions));
    }

    [TestMethod]
    public void FromJson_ParsesCallbackPayload()
    {
        var response = GetCustomerInfoResponse.FromJson(ReadTestData("customer-processing.json"));

        Assert.AreEqual("46116754-695e-43f6-84c4-8c05e50a7b12", response.Id);
        Assert.AreEqual(CustomerStatus.Processing, response.Status);
    }

    [TestMethod]
    [DataRow("null")]
    [DataRow("{\"status\":\"ACCEPTED\",\"status\":\"REJECTED\"}")]
    [DataRow("{\"status\":\"ACCEPTED\",\"fields\":{\"a\":null}}")]
    [DataRow("{\"id\":\"x\"}")]
    public void FromJson_WithInvalidPayload_ThrowsJsonException(string json)
    {
        Assert.ThrowsException<JsonException>(() => GetCustomerInfoResponse.FromJson(json));
    }

    [TestMethod]
    [DataRow("{\"status\":\"ACCEPTED\",\"Status\":\"REJECTED\"}", DisplayName = "case-variant status")]
    [DataRow("{\"status\":\"NEEDS_INFO\",\"fields\":{\"x\":{\"type\":\"string\",\"TYPE\":\"binary\",\"description\":\"d\"}}}",
        DisplayName = "case-variant field type")]
    [DataRow("{\"status\":\"ACCEPTED\",\"message\":\"a\",\"Message\":\"b\"}", DisplayName = "case-variant message")]
    public void FromJson_WithCaseVariantDuplicate_ThrowsJsonException(string json)
    {
        // The callback payload is parsed case-insensitively; a case variant must not overwrite the first value.
        var ex = Assert.ThrowsException<JsonException>(() => GetCustomerInfoResponse.FromJson(json));

        StringAssert.Contains(ex.Message, "Duplicate property");
    }

    #endregion

    #region Error responses

    [TestMethod]
    public async Task GetCustomerInfoAsync_404_ThrowsCustomerNotFoundWithAnchorError()
    {
        var (service, _) = CreateService(
            "{\"error\": \"customer not found for id: 7e285e7d-d984-412c-97bc-909d0e399fbf\"}",
            HttpStatusCode.NotFound);

        var ex = await AssertThrowsAsync<CustomerNotFoundException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt, Id = "7e285e7d" }));

        Assert.AreEqual(404, ex.StatusCode);
        Assert.AreEqual("customer not found for id: 7e285e7d-d984-412c-97bc-909d0e399fbf", ex.ErrorMessage);
        StringAssert.Contains(ex.Message, "customer not found for id");
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_400_ThrowsKycServiceExceptionWithAnchorError()
    {
        var (service, _) = CreateService(
            "{\"error\": \"unrecognized 'type' value. see valid values in the /info response\"}",
            HttpStatusCode.BadRequest);

        var ex = await AssertThrowsAsync<KycServiceException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt, Type = "bogus" }));

        Assert.AreEqual(typeof(KycServiceException), ex.GetType());
        Assert.AreEqual(400, ex.StatusCode);
        Assert.AreEqual("unrecognized 'type' value. see valid values in the /info response", ex.ErrorMessage);
        Assert.IsNull(ex.InnerException);
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_401_ThrowsAuthenticationRequired()
    {
        var (service, _) = CreateService("{\"error\": \"invalid jwt\"}", HttpStatusCode.Unauthorized);

        var ex = await AssertThrowsAsync<AuthenticationRequiredException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        Assert.AreEqual(401, ex.StatusCode);
        Assert.AreEqual("invalid jwt", ex.ErrorMessage);
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_403AuthenticationRequired_ThrowsAuthenticationRequired()
    {
        var (service, _) = CreateService("{\"type\": \"authentication_required\"}", HttpStatusCode.Forbidden);

        var ex = await AssertThrowsAsync<AuthenticationRequiredException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        Assert.AreEqual(403, ex.StatusCode);
        Assert.IsNull(ex.ErrorMessage);
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_403OtherType_ThrowsPlainKycServiceException()
    {
        var (service, _) = CreateService("{\"error\": \"forbidden\"}", HttpStatusCode.Forbidden);

        var ex = await AssertThrowsAsync<KycServiceException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        Assert.AreEqual(typeof(KycServiceException), ex.GetType());
        Assert.AreEqual(403, ex.StatusCode);
    }

    [TestMethod]
    [DataRow("{\"error\": \"a\", \"error\": \"b\"}", DisplayName = "duplicate error")]
    [DataRow("{\"type\": \"other\", \"type\": \"authentication_required\"}", DisplayName = "duplicate type")]
    [DataRow("{\"error\": \"a\", \"Error\": \"b\"}", DisplayName = "case-variant error")]
    [DataRow("{\"type\": \"other\", \"Type\": \"authentication_required\"}", DisplayName = "case-variant type")]
    public async Task ErrorResponse_WithDuplicateProperty_IsNotTrusted(string body)
    {
        var (service, _) = CreateService(body, HttpStatusCode.Forbidden);

        var ex = await AssertThrowsAsync<KycServiceException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        // A duplicated discriminator must not be resolved last-wins into AuthenticationRequiredException.
        Assert.AreEqual(typeof(KycServiceException), ex.GetType());
        Assert.IsNull(ex.ErrorMessage);
        Assert.IsInstanceOfType(ex.InnerException, typeof(JsonException));
        StringAssert.Contains(ex.InnerException!.Message, "Duplicate");
    }

    [TestMethod]
    [DataRow("<html><body>Bad Gateway</body></html>", DisplayName = "HTML")]
    [DataRow("{\"error\": {\"code\": 5}}", DisplayName = "non-string error")]
    public async Task ErrorResponse_WithUnparseableBody_KeepsStatusMapping(string body)
    {
        var (service, _) = CreateService(body, HttpStatusCode.NotFound);

        var ex = await AssertThrowsAsync<CustomerNotFoundException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        Assert.IsNull(ex.ErrorMessage);
        Assert.IsInstanceOfType(ex.InnerException, typeof(JsonException));
    }

    [TestMethod]
    public async Task ErrorResponse_WithEmptyBody_MapsStatusOnly()
    {
        var (service, _) = CreateService(null, HttpStatusCode.InternalServerError);

        var ex = await AssertThrowsAsync<KycServiceException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        Assert.AreEqual(500, ex.StatusCode);
        Assert.IsNull(ex.ErrorMessage);
        Assert.IsNull(ex.InnerException);
    }

    [TestMethod]
    public async Task ErrorResponse_SanitizesErrorTextInMessage_ButKeepsItVerbatim()
    {
        var error = "line1\r\nFAKE LOG ENTRY\u001b[31m" + new string('x', 1000);
        var (service, _) = CreateService(JsonSerializer.Serialize(new { error }), HttpStatusCode.BadRequest);

        var ex = await AssertThrowsAsync<KycServiceException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        Assert.AreEqual(error, ex.ErrorMessage);
        Assert.IsFalse(ex.Message.Any(char.IsControl), "Control characters leaked into the message.");
        Assert.IsTrue(ex.Message.Length < 400, $"Message length {ex.Message.Length}.");
        StringAssert.Contains(ex.Message, "(truncated)");
    }

    [TestMethod]
    public async Task ErrorResponse_WithOversizedBody_MapsStatusWithoutErrorText()
    {
        var body = "{\"error\":\"" + new string('e', KycService.MaxResponseBodyBytes) + "\"}";
        var (service, _) = CreateService(body, HttpStatusCode.NotFound);

        var ex = await AssertThrowsAsync<CustomerNotFoundException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        Assert.IsNull(ex.ErrorMessage);
    }

    #endregion

    #region PUT /customer

    private static readonly byte[] PhotoFront = Encoding.ASCII.GetBytes("front-image-bytes");
    private static readonly byte[] PhotoBack = Encoding.ASCII.GetBytes("back-image-bytes");
    private static readonly byte[] IncorporationDoc = Encoding.ASCII.GetBytes("incorporation-doc");

    [TestMethod]
    public async Task PutCustomerInfoAsync_SendsIdentificationAsMultipartAndParsesId()
    {
        var (service, handler) = CreateService(ReadTestData("put-customer.json"), HttpStatusCode.Accepted);

        var response = await service.PutCustomerInfoAsync(new PutCustomerInfoRequest
        {
            Jwt = Jwt,
            Id = CustomerId,
            Account = Account,
            Memo = "10638330804770506835",
            MemoType = "id",
            Type = "sep31-sender",
            TransactionId = "tx-1",
        });

        Assert.AreEqual("391fb415-c223-4608-b2f5-dd1e91e3a986", response.Id);
        var request = handler.Requests.Single();
        Assert.AreEqual(HttpMethod.Put, request.Method);
        Assert.AreEqual($"{KycServerUrl}/customer", request.Uri.ToString());
        AssertAuthorized(request);
        StringAssert.StartsWith(request.ContentType, "multipart/form-data");
        CollectionAssert.AreEqual(
            new[] { "id", "account", "memo", "memo_type", "type", "transaction_id" },
            request.Parts.Select(p => p.Name).ToArray());
        Assert.AreEqual(Account, request.Part("account").Value);
        Assert.AreEqual("10638330804770506835", request.Part("memo").Value);
    }

    [TestMethod]
    public async Task PutCustomerInfoAsync_SendsSep9FieldsAndFilesWithFilesLast()
    {
        var (service, handler) = CreateService(ReadTestData("put-customer.json"));

        await service.PutCustomerInfoAsync(new PutCustomerInfoRequest
        {
            Jwt = Jwt,
            Type = "sep6",
            KycFields = new StandardKycFields
            {
                NaturalPerson = new NaturalPersonKycFields
                {
                    FirstName = "John",
                    LastName = "Doe",
                    EmailAddress = "john@example.com",
                    BirthDate = KycTestDates.BirthDate,
                    PhotoIdFront = PhotoFront,
                    PhotoIdBack = PhotoBack,
                    FinancialAccount = new FinancialAccountKycFields { BankAccountNumber = "123456789" },
                },
                Organization = new OrganizationKycFields
                {
                    Name = "Acme Inc.",
                    PhotoIncorporationDoc = IncorporationDoc,
                },
            },
            CustomFields = new Dictionary<string, string> { ["referral_code"] = "abc" },
        });

        var request = handler.Requests.Single();
        Assert.AreEqual("sep6", request.Part("type").Value);
        Assert.AreEqual("John", request.Part(NaturalPersonKycFields.FirstNameFieldKey).Value);
        Assert.AreEqual("Doe", request.Part(NaturalPersonKycFields.LastNameFieldKey).Value);
        Assert.AreEqual("john@example.com", request.Part(NaturalPersonKycFields.EmailAddressFieldKey).Value);
        Assert.AreEqual("1990-01-15", request.Part(NaturalPersonKycFields.BirthDateFieldKey).Value);
        Assert.AreEqual("123456789", request.Part(FinancialAccountKycFields.BankAccountNumberFieldKey).Value);
        Assert.AreEqual("Acme Inc.", request.Part(OrganizationKycFields.NameFieldKey).Value);
        Assert.AreEqual("abc", request.Part("referral_code").Value);

        var front = request.Part(NaturalPersonKycFields.PhotoIdFrontFileKey);
        Assert.AreEqual("front-image-bytes", front.Value);
        Assert.AreEqual(NaturalPersonKycFields.PhotoIdFrontFileKey, front.FileName);
        Assert.AreEqual("application/octet-stream", front.ContentType);
        Assert.AreEqual("back-image-bytes", request.Part(NaturalPersonKycFields.PhotoIdBackFileKey).Value);
        Assert.AreEqual("incorporation-doc", request.Part(OrganizationKycFields.PhotoIncorporationDocFileKey).Value);

        // SEP-12: binary fields must come after every other field.
        var firstFileIndex = request.Parts.FindIndex(p => p.FileName != null);
        Assert.AreEqual(3, request.Parts.Count(p => p.FileName != null));
        Assert.IsTrue(request.Parts.Skip(firstFileIndex).All(p => p.FileName != null),
            "A text part was sent after a file part.");
    }

    [TestMethod]
    public async Task PutCustomerInfoAsync_QuotesPartNamesAndOmitsTextPartContentType()
    {
        var (service, handler) = CreateService(ReadTestData("put-customer.json"));

        await service.PutCustomerInfoAsync(new PutCustomerInfoRequest
        {
            Jwt = Jwt,
            CustomFields = new Dictionary<string, string> { ["organization.name"] = "Zürich AG" },
        });

        var request = handler.Requests.Single();
        StringAssert.Contains(request.Body, "Content-Disposition: form-data; name=\"organization.name\"");
        var part = request.Part("organization.name");
        Assert.IsNull(part.ContentType);
        Assert.AreEqual("Zürich AG", part.Value);
    }

    [TestMethod]
    public async Task PutCustomerInfoAsync_CustomFieldsAndFilesOverrideKycFields()
    {
        var (service, handler) = CreateService(ReadTestData("put-customer.json"));

        await service.PutCustomerInfoAsync(new PutCustomerInfoRequest
        {
            Jwt = Jwt,
            KycFields = new StandardKycFields
            {
                NaturalPerson = new NaturalPersonKycFields { FirstName = "John", PhotoIdFront = PhotoFront },
            },
            CustomFields = new Dictionary<string, string> { ["first_name"] = "Johnny" },
            CustomFiles = new Dictionary<string, byte[]>
            {
                ["photo_id_front"] = PhotoBack,
                ["proof_of_funds"] = IncorporationDoc,
            },
        });

        var request = handler.Requests.Single();
        Assert.AreEqual(1, request.Parts.Count(p => p.Name == "first_name"));
        Assert.AreEqual("Johnny", request.Part("first_name").Value);
        Assert.AreEqual("back-image-bytes", request.Part("photo_id_front").Value);
        Assert.AreEqual("incorporation-doc", request.Part("proof_of_funds").Value);
        Assert.AreEqual("proof_of_funds", request.Part("proof_of_funds").FileName);
    }

    [TestMethod]
    public async Task PutCustomerInfoAsync_WithNameUsedAsTextAndFile_ThrowsBeforeSending()
    {
        var requests = new[]
        {
            new PutCustomerInfoRequest
            {
                Jwt = Jwt,
                KycFields = new StandardKycFields
                {
                    NaturalPerson = new NaturalPersonKycFields { PhotoIdFront = PhotoFront },
                },
                CustomFields = new Dictionary<string, string> { ["photo_id_front"] = "text" },
            },
            new PutCustomerInfoRequest
            {
                Jwt = Jwt,
                Account = Account,
                CustomFiles = new Dictionary<string, byte[]> { ["account"] = PhotoFront },
            },
            new PutCustomerInfoRequest
            {
                Jwt = Jwt,
                KycFields = new StandardKycFields { NaturalPerson = new NaturalPersonKycFields { FirstName = "John" } },
                CustomFiles = new Dictionary<string, byte[]> { ["first_name"] = PhotoFront },
            },
            new PutCustomerInfoRequest
            {
                Jwt = Jwt,
                FileReferences = new Dictionary<string, string> { ["photo_id_front"] = "file_1" },
                CustomFiles = new Dictionary<string, byte[]> { ["photo_id_front_file_id"] = PhotoFront },
            },
            new PutCustomerInfoRequest
            {
                Jwt = Jwt,
                VerificationFields = new Dictionary<string, string> { ["mobile_number"] = "2735021" },
                CustomFiles = new Dictionary<string, byte[]> { ["mobile_number_verification"] = PhotoFront },
            },
        };
        var (service, handler) = CreateService(ReadTestData("put-customer.json"));

        foreach (var request in requests)
        {
            var ex = await AssertThrowsAsync<ArgumentException>(() => service.PutCustomerInfoAsync(request));
            StringAssert.Contains(ex.Message, "both as a text field and as a file");
        }

        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task PutCustomerInfoAsync_SendsVerificationFieldsWithSuffix()
    {
        var (service, handler) = CreateService(ReadTestData("put-customer.json"));

        await service.PutCustomerInfoAsync(new PutCustomerInfoRequest
        {
            Jwt = Jwt,
            Id = "391fb415-c223-4608-b2f5-dd1e91e3a986",
            VerificationFields = new Dictionary<string, string>
            {
                ["mobile_number"] = "123456",
                ["email_address_verification"] = "654321",
            },
        });

        var request = handler.Requests.Single();
        CollectionAssert.AreEqual(
            new[] { "id", "mobile_number_verification", "email_address_verification" },
            request.Parts.Select(p => p.Name).ToArray());
        Assert.AreEqual("123456", request.Part("mobile_number_verification").Value);
        Assert.AreEqual("654321", request.Part("email_address_verification").Value);
    }

    [TestMethod]
    public async Task PutCustomerInfoAsync_SendsFileReferencesWithSuffix()
    {
        var (service, handler) = CreateService(ReadTestData("put-customer.json"));

        await service.PutCustomerInfoAsync(new PutCustomerInfoRequest
        {
            Jwt = Jwt,
            Id = "2f417dab-18d2-4081-8c59-c9d3afb59d3f",
            FileReferences = new Dictionary<string, string>
            {
                ["photo_id_front"] = "file_abc",
                ["photo_id_back_file_id"] = "file_def",
            },
        });

        var request = handler.Requests.Single();
        Assert.AreEqual("file_abc", request.Part("photo_id_front_file_id").Value);
        Assert.AreEqual("file_def", request.Part("photo_id_back_file_id").Value);
        Assert.IsTrue(request.Parts.All(p => p.FileName == null));
    }

    [TestMethod]
    [DataRow("bad\"name")]
    [DataRow("bad\r\nX-Injected: 1")]
    [DataRow("bad\nname")]
    [DataRow("bad\0name")]
    [DataRow("")]
    [DataRow("bad\\")]
    [DataRow("na\u00efve")]
    [DataRow("tab\tname")]
    public async Task PutCustomerInfoAsync_WithInvalidFieldName_ThrowsBeforeSending(string name)
    {
        var (service, handler) = CreateService(ReadTestData("put-customer.json"));

        var ex = await AssertThrowsAsync<ArgumentException>(() => service.PutCustomerInfoAsync(
            new PutCustomerInfoRequest
            {
                Jwt = Jwt,
                CustomFields = new Dictionary<string, string> { [name] = "value" },
            }));

        // The SDK's own check, not a later System.Net.Http header validation that happens to catch some of these.
        StringAssert.Contains(ex.Message, "Invalid form field name");
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task PutCustomerInfoAsync_WithInvalidFileName_ThrowsBeforeSending()
    {
        var (service, handler) = CreateService(ReadTestData("put-customer.json"));

        await AssertThrowsAsync<ArgumentException>(() => service.PutCustomerInfoAsync(new PutCustomerInfoRequest
        {
            Jwt = Jwt,
            CustomFiles = new Dictionary<string, byte[]> { ["a\"b"] = PhotoFront },
        }));

        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task PutCustomerInfoAsync_WithNullValue_ThrowsArgumentException()
    {
        var (service, handler) = CreateService(ReadTestData("put-customer.json"));

        await AssertThrowsAsync<ArgumentException>(() => service.PutCustomerInfoAsync(new PutCustomerInfoRequest
        {
            Jwt = Jwt,
            CustomFields = new Dictionary<string, string> { ["x"] = null! },
        }));

        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task PutCustomerInfoAsync_413_ThrowsPayloadTooLarge()
    {
        var (service, _) = CreateService("{\"error\": \"file too large\"}", HttpStatusCode.RequestEntityTooLarge);

        var ex = await AssertThrowsAsync<PayloadTooLargeException>(() =>
            service.PutCustomerInfoAsync(new PutCustomerInfoRequest
            {
                Jwt = Jwt,
                CustomFiles = new Dictionary<string, byte[]> { ["photo_id_front"] = PhotoFront },
            }));

        Assert.AreEqual(413, ex.StatusCode);
        Assert.AreEqual("file too large", ex.ErrorMessage);
    }

    [TestMethod]
    public async Task PutCustomerInfoAsync_404_ThrowsCustomerNotFound()
    {
        var (service, _) = CreateService("{\"error\": \"customer with `id` not found\"}", HttpStatusCode.NotFound);

        var ex = await AssertThrowsAsync<CustomerNotFoundException>(() =>
            service.PutCustomerInfoAsync(new PutCustomerInfoRequest { Jwt = Jwt, Id = "missing" }));

        Assert.AreEqual("customer with `id` not found", ex.ErrorMessage);
    }

    [TestMethod]
    [DataRow("{}", DisplayName = "missing id")]
    [DataRow("{\"id\": null}", DisplayName = "null id")]
    [DataRow("{\"id\": \"a\", \"id\": \"b\"}", DisplayName = "duplicate id")]
    [DataRow("{\"id\": \"a\", \"ID\": \"b\"}", DisplayName = "case-variant id")]
    public async Task PutCustomerInfoAsync_WithInvalidResponse_ThrowsInvalidKycResponseException(string body)
    {
        var (service, _) = CreateService(body);

        await AssertThrowsAsync<InvalidKycResponseException>(() =>
            service.PutCustomerInfoAsync(new PutCustomerInfoRequest { Jwt = Jwt }));
    }

    #endregion

    #region PUT /customer/verification (deprecated)

#pragma warning disable CS0618 // exercising the deprecated endpoint on purpose
    [TestMethod]
    public async Task PutCustomerVerificationAsync_SendsIdAndSuffixedFields_ParsesCustomerResponse()
    {
        var (service, handler) = CreateService(ReadTestData("customer-accepted.json"));

        var response = await service.PutCustomerVerificationAsync(new PutCustomerVerificationRequest
        {
            Jwt = Jwt,
            Id = "391fb415-c223-4608-b2f5-dd1e91e3a986",
            VerificationFields = new Dictionary<string, string>
            {
                ["mobile_number"] = "2735021",
                ["email_address_verification"] = "T32U1",
            },
        });

        Assert.AreEqual(CustomerStatus.Accepted, response.Status);
        var request = handler.Requests.Single();
        Assert.AreEqual(HttpMethod.Put, request.Method);
        Assert.AreEqual($"{KycServerUrl}/customer/verification", request.Uri.ToString());
        AssertAuthorized(request);
        CollectionAssert.AreEqual(
            new[] { "id", "mobile_number_verification", "email_address_verification" },
            request.Parts.Select(p => p.Name).ToArray());
        Assert.AreEqual("2735021", request.Part("mobile_number_verification").Value);
    }

    [TestMethod]
    public async Task PutCustomerVerificationAsync_WithoutFields_ThrowsArgumentException()
    {
        var (service, handler) = CreateService(ReadTestData("customer-accepted.json"));

        await AssertThrowsAsync<ArgumentException>(() =>
            service.PutCustomerVerificationAsync(new PutCustomerVerificationRequest
            {
                Jwt = Jwt,
                Id = CustomerId,
                VerificationFields = new Dictionary<string, string>(),
            }));

        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task PutCustomerVerificationAsync_WithoutId_ThrowsArgumentException()
    {
        var (service, _) = CreateService(ReadTestData("customer-accepted.json"));

        await AssertThrowsAsync<ArgumentException>(() =>
            service.PutCustomerVerificationAsync(new PutCustomerVerificationRequest
            {
                Jwt = Jwt,
                Id = "",
                VerificationFields = new Dictionary<string, string> { ["mobile_number"] = "1" },
            }));
    }

    [TestMethod]
    public async Task PutCustomerVerificationAsync_WithInvalidCode_ThrowsKycServiceException()
    {
        var (service, _) = CreateService("{\"error\": \"The provided confirmation code was invalid.\"}",
            HttpStatusCode.BadRequest);

        var ex = await AssertThrowsAsync<KycServiceException>(() =>
            service.PutCustomerVerificationAsync(new PutCustomerVerificationRequest
            {
                Jwt = Jwt,
                Id = CustomerId,
                VerificationFields = new Dictionary<string, string> { ["mobile_number"] = "000" },
            }));

        Assert.AreEqual("The provided confirmation code was invalid.", ex.ErrorMessage);
    }
#pragma warning restore CS0618

    #endregion

    #region PUT /customer/callback

    [TestMethod]
    public async Task PutCustomerCallbackAsync_SendsUrlAndIdentification()
    {
        var (service, handler) = CreateService("");

        await service.PutCustomerCallbackAsync(new PutCustomerCallbackRequest
        {
            Jwt = Jwt,
            Url = "https://wallet.example.com/kyc-callback",
            Id = CustomerId,
            Account = Account,
            Memo = "123",
            MemoType = "id",
        });

        var request = handler.Requests.Single();
        Assert.AreEqual(HttpMethod.Put, request.Method);
        Assert.AreEqual($"{KycServerUrl}/customer/callback", request.Uri.ToString());
        AssertAuthorized(request);
        CollectionAssert.AreEqual(new[] { "url", "id", "account", "memo", "memo_type" },
            request.Parts.Select(p => p.Name).ToArray());
        Assert.AreEqual("https://wallet.example.com/kyc-callback", request.Part("url").Value);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("/relative/callback")]
    [DataRow("ftp://wallet.example.com/callback")]
    [DataRow("http://evil.com\\@localhost/", DisplayName = "backslash before '@', which Uri and the as-written host read differently")]
    public async Task PutCustomerCallbackAsync_WithInvalidUrl_ThrowsArgumentException(string url)
    {
        var (service, handler) = CreateService("");

        await AssertThrowsAsync<ArgumentException>(() =>
            service.PutCustomerCallbackAsync(new PutCustomerCallbackRequest { Jwt = Jwt, Url = url }));

        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    [DataRow("http://wallet.example.com/callback")]
    [DataRow("http://10.0.0.5/callback")]
    [DataRow("http://loopback/callback", DisplayName = "the bare name 'loopback' is not a loopback address")]
    [DataRow("http://localhost.evil.example/callback")]
    [DataRow("http://0177.0.0.1/callback", DisplayName = "octal IPv4, which some parsers read as 177.0.0.1")]
    [DataRow("http://0x7f.1/callback", DisplayName = "hex IPv4")]
    [DataRow("http://2130706433/callback", DisplayName = "integer IPv4")]
    [DataRow("http://127.1/callback", DisplayName = "short IPv4")]
    [DataRow("http://[::1]x/callback", DisplayName = "text after an IPv6 literal, which Uri reads as path")]
    public async Task PutCustomerCallbackAsync_WithHttpForNonLoopbackHost_ThrowsArgumentException(string url)
    {
        var (service, handler) = CreateService("");

        var ex = await AssertThrowsAsync<ArgumentException>(() =>
            service.PutCustomerCallbackAsync(new PutCustomerCallbackRequest { Jwt = Jwt, Url = url }));

        StringAssert.Contains(ex.Message, "https");
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    [DataRow("http://localhost:8000/callback")]
    [DataRow("http://127.0.0.1/callback")]
    [DataRow("http://[::1]:8000/callback")]
    [DataRow("http://LOCALHOST:8000/callback")]
    [DataRow("http://127.0.0.2/callback")]
    [DataRow("http://[::ffff:127.0.0.1]/callback")]
    [DataRow("http://[::1]/callback", DisplayName = "IPv6 without a port")]
    [DataRow("http://[::FFFF:127.0.0.1]/callback", DisplayName = "upper-case IPv6 literal")]
    public async Task PutCustomerCallbackAsync_WithHttpForLoopbackHost_IsSent(string url)
    {
        var (service, handler) = CreateService("");

        await service.PutCustomerCallbackAsync(new PutCustomerCallbackRequest { Jwt = Jwt, Url = url });

        Assert.AreEqual(url, handler.Requests.Single().Part("url").Value);
    }

    [TestMethod]
    public async Task PutCustomerCallbackAsync_404_ThrowsCustomerNotFound()
    {
        var (service, _) = CreateService("{\"error\": \"not found\"}", HttpStatusCode.NotFound);

        await AssertThrowsAsync<CustomerNotFoundException>(() =>
            service.PutCustomerCallbackAsync(new PutCustomerCallbackRequest
            {
                Jwt = Jwt,
                Url = "https://wallet.example.com/cb",
            }));
    }

    #endregion

    #region DELETE /customer/{account}

    [TestMethod]
    public async Task DeleteCustomerAsync_SendsDeleteToAccountPathWithoutBody()
    {
        var (service, handler) = CreateService(null);

        await service.DeleteCustomerAsync(new DeleteCustomerRequest { Jwt = Jwt, Account = Account });

        var request = handler.Requests.Single();
        Assert.AreEqual(HttpMethod.Delete, request.Method);
        Assert.AreEqual($"{KycServerUrl}/customer/{Account}", request.Uri.ToString());
        AssertAuthorized(request);
        Assert.IsNull(request.Body);
    }

    [TestMethod]
    public async Task DeleteCustomerAsync_WithMemo_SendsMemoInBody()
    {
        var (service, handler) = CreateService("{}");

        await service.DeleteCustomerAsync(new DeleteCustomerRequest
        {
            Jwt = Jwt,
            Account = Account,
            Memo = "10638330804770506835",
            MemoType = "id",
        });

        var request = handler.Requests.Single();
        CollectionAssert.AreEqual(new[] { "memo", "memo_type" }, request.Parts.Select(p => p.Name).ToArray());
        Assert.AreEqual("10638330804770506835", request.Part("memo").Value);
    }

    [TestMethod]
    public async Task DeleteCustomerAsync_EscapesAccountPathSegment()
    {
        var (service, handler) = CreateService(null, HttpStatusCode.NoContent);

        await service.DeleteCustomerAsync(new DeleteCustomerRequest { Jwt = Jwt, Account = "../files?x=1" });

        Assert.AreEqual($"{KycServerUrl}/customer/..%2Ffiles%3Fx%3D1",
            handler.Requests.Single().Uri.AbsoluteUri);
    }

    [TestMethod]
    public async Task DeleteCustomerAsync_404_ThrowsCustomerNotFound()
    {
        var (service, _) = CreateService(null, HttpStatusCode.NotFound);

        var ex = await AssertThrowsAsync<CustomerNotFoundException>(() =>
            service.DeleteCustomerAsync(new DeleteCustomerRequest { Jwt = Jwt, Account = Account }));

        Assert.AreEqual(404, ex.StatusCode);
    }

    [TestMethod]
    public async Task DeleteCustomerAsync_401_ThrowsAuthenticationRequired()
    {
        var (service, _) = CreateService(null, HttpStatusCode.Unauthorized);

        await AssertThrowsAsync<AuthenticationRequiredException>(() =>
            service.DeleteCustomerAsync(new DeleteCustomerRequest { Jwt = Jwt, Account = Account }));
    }

    [TestMethod]
    public async Task DeleteCustomerAsync_WithEmptyAccount_ThrowsArgumentException()
    {
        var (service, handler) = CreateService(null);

        await AssertThrowsAsync<ArgumentException>(() =>
            service.DeleteCustomerAsync(new DeleteCustomerRequest { Jwt = Jwt, Account = " " }));

        Assert.AreEqual(0, handler.Requests.Count);
    }

    #endregion

    #region POST /customer/files

    [TestMethod]
    public async Task PostCustomerFileAsync_UploadsFilePartAndParsesResponse()
    {
        var (service, handler) = CreateService(ReadTestData("customer-file.json"));

        var response = await service.PostCustomerFileAsync(new PostCustomerFileRequest
        {
            Jwt = Jwt,
            File = PhotoFront,
        });

        Assert.AreEqual("file_d3d54529-6683-4341-9b66-4ac7d7504238", response.FileId);
        Assert.AreEqual("image/jpeg", response.ContentType);
        Assert.AreEqual(4089371L, response.Size);
        Assert.AreEqual(new DateTimeOffset(2026, 10, 1, 12, 30, 0, TimeSpan.Zero), response.ExpiresAt);
        Assert.AreEqual("2bf95490-db23-442d-a1bd-c6fd5efb584e", response.CustomerId);

        var request = handler.Requests.Single();
        Assert.AreEqual(HttpMethod.Post, request.Method);
        Assert.AreEqual($"{KycServerUrl}/customer/files", request.Uri.ToString());
        AssertAuthorized(request);
        var part = request.Parts.Single();
        Assert.AreEqual("file", part.Name);
        Assert.AreEqual("file", part.FileName);
        Assert.AreEqual("application/octet-stream", part.ContentType);
        Assert.AreEqual("front-image-bytes", part.Value);
    }

    [TestMethod]
    public async Task PostCustomerFileAsync_WithFileNameAndContentType_SendsThem()
    {
        var (service, handler) = CreateService(ReadTestData("customer-file.json"));

        await service.PostCustomerFileAsync(new PostCustomerFileRequest
        {
            Jwt = Jwt,
            File = PhotoFront,
            FileName = "passport.jpg",
            ContentType = "image/jpeg",
        });

        var part = handler.Requests.Single().Parts.Single();
        Assert.AreEqual("file", part.Name);
        Assert.AreEqual("passport.jpg", part.FileName);
        Assert.AreEqual("image/jpeg", part.ContentType);
    }

    [TestMethod]
    public async Task PostCustomerFileAsync_WithoutOptionalResponseFields_ParsesNulls()
    {
        var (service, _) = CreateService(
            "{\"file_id\":\"file_1\",\"content_type\":\"image/png\",\"size\":10,\"customer_id\":null}");

        var response = await service.PostCustomerFileAsync(new PostCustomerFileRequest { Jwt = Jwt, File = PhotoFront });

        Assert.IsNull(response.ExpiresAt);
        Assert.IsNull(response.CustomerId);
    }

    [TestMethod]
    [DataRow("not a media type")]
    [DataRow("image/jpeg\r\nX-Injected: 1")]
    public async Task PostCustomerFileAsync_WithInvalidContentType_ThrowsArgumentException(string contentType)
    {
        var (service, handler) = CreateService(ReadTestData("customer-file.json"));

        await AssertThrowsAsync<ArgumentException>(() => service.PostCustomerFileAsync(new PostCustomerFileRequest
        {
            Jwt = Jwt,
            File = PhotoFront,
            ContentType = contentType,
        }));

        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task PostCustomerFileAsync_WithNullFile_ThrowsArgumentException()
    {
        var (service, _) = CreateService(ReadTestData("customer-file.json"));

        await AssertThrowsAsync<ArgumentException>(() =>
            service.PostCustomerFileAsync(new PostCustomerFileRequest { Jwt = Jwt, File = null! }));
    }

    [TestMethod]
    public async Task PostCustomerFileAsync_413_ThrowsPayloadTooLarge()
    {
        var (service, _) = CreateService("", HttpStatusCode.RequestEntityTooLarge);

        var ex = await AssertThrowsAsync<PayloadTooLargeException>(() =>
            service.PostCustomerFileAsync(new PostCustomerFileRequest { Jwt = Jwt, File = PhotoFront }));

        Assert.AreEqual(413, ex.StatusCode);
    }

    [TestMethod]
    public async Task PostCustomerFileAsync_400_ThrowsKycServiceException()
    {
        var (service, _) = CreateService("{\"error\": \"'photo_id_front' cannot be decoded. Must be jpg or png.\"}",
            HttpStatusCode.BadRequest);

        var ex = await AssertThrowsAsync<KycServiceException>(() =>
            service.PostCustomerFileAsync(new PostCustomerFileRequest { Jwt = Jwt, File = PhotoFront }));

        Assert.AreEqual("'photo_id_front' cannot be decoded. Must be jpg or png.", ex.ErrorMessage);
    }

    [TestMethod]
    public async Task PostCustomerFileAsync_404_IsNotACustomerNotFound()
    {
        var (service, _) = CreateService("{\"error\": \"no such route\"}", HttpStatusCode.NotFound);

        var ex = await AssertThrowsAsync<KycServiceException>(() =>
            service.PostCustomerFileAsync(new PostCustomerFileRequest { Jwt = Jwt, File = PhotoFront }));

        Assert.AreEqual(typeof(KycServiceException), ex.GetType());
        Assert.AreEqual(404, ex.StatusCode);
    }

    [TestMethod]
    [DataRow("{\"content_type\":\"image/png\",\"size\":1}", DisplayName = "missing file_id")]
    [DataRow("{\"file_id\":\"f\",\"size\":1}", DisplayName = "missing content_type")]
    [DataRow("{\"file_id\":\"f\",\"content_type\":\"image/png\"}", DisplayName = "missing size")]
    [DataRow("{\"file_id\":\"f\",\"content_type\":\"image/png\",\"size\":\"big\"}", DisplayName = "non-numeric size")]
    [DataRow("{\"file_id\":\"f\",\"content_type\":\"image/png\",\"size\":1,\"expires_at\":\"soon\"}",
        DisplayName = "invalid expires_at")]
    public async Task PostCustomerFileAsync_WithInvalidResponse_ThrowsInvalidKycResponseException(string body)
    {
        var (service, _) = CreateService(body);

        await AssertThrowsAsync<InvalidKycResponseException>(() =>
            service.PostCustomerFileAsync(new PostCustomerFileRequest { Jwt = Jwt, File = PhotoFront }));
    }

    #endregion

    #region GET /customer/files

    [TestMethod]
    public async Task GetCustomerFilesAsync_ByCustomerId_ParsesFiles()
    {
        var (service, handler) = CreateService(ReadTestData("customer-files.json"));

        var response = await service.GetCustomerFilesAsync(new GetCustomerFilesRequest
        {
            Jwt = Jwt,
            CustomerId = "2bf95490-db23-442d-a1bd-c6fd5efb584e",
        });

        Assert.AreEqual(2, response.Files.Length);
        Assert.AreEqual("file_d5c67b4c-173c-428c-baab-944f4b89a57f", response.Files[0].FileId);
        Assert.AreEqual("image/png", response.Files[0].ContentType);
        Assert.AreEqual(6134063L, response.Files[0].Size);
        Assert.IsNull(response.Files[0].ExpiresAt);
        Assert.AreEqual("image/jpeg", response.Files[1].ContentType);

        var request = handler.Requests.Single();
        Assert.AreEqual(HttpMethod.Get, request.Method);
        Assert.AreEqual($"{KycServerUrl}/customer/files?customer_id=2bf95490-db23-442d-a1bd-c6fd5efb584e",
            request.Uri.ToString());
        AssertAuthorized(request);
    }

    [TestMethod]
    public async Task GetCustomerFilesAsync_ByFileId_SendsFileId()
    {
        var (service, handler) = CreateService("{\"files\": []}");

        var response = await service.GetCustomerFilesAsync(new GetCustomerFilesRequest
        {
            Jwt = Jwt,
            FileId = "file_abc",
        });

        Assert.AreEqual(0, response.Files.Length);
        Assert.AreEqual($"{KycServerUrl}/customer/files?file_id=file_abc", handler.Requests.Single().Uri.ToString());
    }

    [TestMethod]
    public async Task GetCustomerFilesAsync_WithoutFileOrCustomerId_ThrowsArgumentException()
    {
        var (service, handler) = CreateService("{\"files\": []}");

        await AssertThrowsAsync<ArgumentException>(() =>
            service.GetCustomerFilesAsync(new GetCustomerFilesRequest { Jwt = Jwt }));

        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    [DataRow("{}", DisplayName = "missing files")]
    [DataRow("{\"files\": null}", DisplayName = "null files")]
    [DataRow("{\"files\": [null]}", DisplayName = "null file entry")]
    [DataRow("{\"files\": [], \"files\": []}", DisplayName = "duplicate files")]
    public async Task GetCustomerFilesAsync_WithInvalidResponse_ThrowsInvalidKycResponseException(string body)
    {
        var (service, _) = CreateService(body);

        await AssertThrowsAsync<InvalidKycResponseException>(() =>
            service.GetCustomerFilesAsync(new GetCustomerFilesRequest { Jwt = Jwt, FileId = "f" }));
    }

    #endregion

    #region Hardening

    [TestMethod]
    public async Task GetCustomerInfoAsync_WithStalledBody_TimesOutWithTheClientTimeout()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new StallingStream()),
        });
        using var service = new KycService(KycServerUrl,
            new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(300) });

        var call = service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt });
        var finished = await Task.WhenAny(call, Task.Delay(TimeSpan.FromSeconds(20)));

        Assert.AreSame(call, finished, "The body read ignored HttpClient.Timeout.");
        var ex = await AssertThrowsAsync<TaskCanceledException>(() => call);
        Assert.IsInstanceOfType(ex.InnerException, typeof(TimeoutException));
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_CanceledDuringBody_ThrowsCancellationWithoutTimeout()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new StallingStream()),
        });
        using var service = new KycService(KycServerUrl,
            new HttpClient(handler) { Timeout = System.Threading.Timeout.InfiniteTimeSpan });
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        var call = service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }, cts.Token);
        var finished = await Task.WhenAny(call, Task.Delay(TimeSpan.FromSeconds(20)));

        Assert.AreSame(call, finished, "The body read ignored the caller's cancellation token.");
        var ex = await AssertThrowsAsync<OperationCanceledException>(() => call);
        Assert.IsNotInstanceOfType(ex.InnerException, typeof(TimeoutException));
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_WhenSuccessBodyReadFails_ThrowsHttpRequestException()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new FailingStream(Encoding.UTF8.GetBytes("{\"status\":\"ACC"))),
        });
        using var service = new KycService(KycServerUrl, new HttpClient(handler));

        var ex = await AssertThrowsAsync<HttpRequestException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        Assert.IsInstanceOfType(ex.InnerException, typeof(IOException));
    }

    [TestMethod]
    public async Task ErrorResponse_WhenBodyReadFails_KeepsStatusMapping()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StreamContent(new FailingStream(Encoding.UTF8.GetBytes("{\"error\":\"bo"))),
        });
        using var service = new KycService(KycServerUrl, new HttpClient(handler));

        var ex = await AssertThrowsAsync<KycServiceException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        Assert.AreEqual(500, ex.StatusCode);
        Assert.IsNull(ex.ErrorMessage);
    }

    [TestMethod]
    public async Task Response_AfterRedirectToAnotherOrigin_IsRejected()
    {
        // A client that follows redirects rewrites the request URI to the final location.
        var handler = new RecordingHandler(request =>
        {
            request.RequestUri = new Uri("https://evil.example.com/steal");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":\"forged\"}"),
                RequestMessage = request,
            };
        });
        using var service = new KycService(KycServerUrl, new HttpClient(handler));

        var ex = await AssertThrowsAsync<KycServiceException>(() =>
            service.PutCustomerInfoAsync(new PutCustomerInfoRequest { Jwt = Jwt, Id = CustomerId }));

        StringAssert.Contains(ex.Message, "different origin");
    }

    [TestMethod]
    [DataRow("https://kyc.example.com:8443/sep12/customer", DisplayName = "same host, other port")]
    [DataRow("http://kyc.example.com/sep12/customer", DisplayName = "same host, other scheme")]
    [DataRow("http://kyc.example.com:443/sep12/customer", DisplayName = "same host and port, other scheme")]
    public async Task Response_AfterRedirectToAnotherPortOrScheme_IsRejected(string finalUri)
    {
        var handler = new RecordingHandler(request =>
        {
            request.RequestUri = new Uri(finalUri);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":\"forged\"}"),
                RequestMessage = request,
            };
        });
        using var service = new KycService(KycServerUrl, new HttpClient(handler));

        await AssertThrowsAsync<KycServiceException>(() =>
            service.PutCustomerInfoAsync(new PutCustomerInfoRequest { Jwt = Jwt, Id = CustomerId }));
    }

    [TestMethod]
    public async Task Response_AfterRedirectWithinTheSameOrigin_IsAccepted()
    {
        var handler = new RecordingHandler(request =>
        {
            request.RequestUri = new Uri("https://KYC.example.com:443/other/path");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":\"same\"}"),
                RequestMessage = request,
            };
        });
        using var service = new KycService(KycServerUrl, new HttpClient(handler));

        var response = await service.PutCustomerInfoAsync(new PutCustomerInfoRequest { Jwt = Jwt, Id = CustomerId });

        Assert.AreEqual("same", response.Id);
    }

    [TestMethod]
    public void InternalClient_DoesNotFollowRedirects()
    {
        using var service = new KycService(KycServerUrl);
        var client = (HttpClient)typeof(KycService)
            .GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(service)!;
        var handler = typeof(HttpMessageInvoker)
            .GetField("_handler", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(client);

        // The SDK build under test decides the handler type, not the test host's TFM.
#if TEST_SDK_NETSTANDARD21
        Assert.IsFalse(((HttpClientHandler)handler!).AllowAutoRedirect);
#else
        Assert.IsFalse(((SocketsHttpHandler)handler!).AllowAutoRedirect);
#endif
    }

    [TestMethod]
    public async Task InvalidResponse_WithHostileFieldKey_HasABoundedSingleLineMessage()
    {
        var key = "K\nFORGED LOG LINE\u2028" + new string('B', 5000);
        var body = "{\"status\":\"NEEDS_INFO\",\"fields\":{" + JsonSerializer.Serialize(key) +
                   ":{\"type\":\"string\",\"description\":\"d\",\"optional\":\"not-a-bool\"}}}";
        var (service, _) = CreateService(body);

        var ex = await AssertThrowsAsync<InvalidKycResponseException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        var text = ex.ToString();
        Assert.IsFalse(text.Contains("FORGED LOG LINE\u2028"), "Unsanitized key text reached the exception.");
        Assert.IsFalse(text.Contains(new string('B', 1000)), "The key was not clamped.");
        Assert.IsFalse(ex.Message.Contains('\n'));
    }

    [TestMethod]
    public void FromJson_WithHostileFieldKey_HasABoundedSingleLineMessage()
    {
        var key = "K\r\nFORGED" + new string('B', 5000);
        var body = "{\"status\":\"NEEDS_INFO\",\"fields\":{" + JsonSerializer.Serialize(key) +
                   ":{\"type\":\"string\",\"description\":\"d\",\"optional\":\"not-a-bool\"}}}";

        var ex = Assert.ThrowsException<JsonException>(() => GetCustomerInfoResponse.FromJson(body));

        Assert.IsTrue(ex.Message.Length < 1000, $"Message length {ex.Message.Length}.");
        Assert.IsFalse(ex.ToString().Contains("\nFORGED"));
    }

    [TestMethod]
    public void FromJson_WithNull_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => GetCustomerInfoResponse.FromJson(null!));
    }

    [TestMethod]
    public async Task ErrorResponse_SanitizesLineSeparatorsBidiAndZeroWidthCharacters()
    {
        const string error = "a\u2028b\u2029c\u202Ed\u200Be\u0085f";
        var (service, _) = CreateService(JsonSerializer.Serialize(new { error }), HttpStatusCode.BadRequest);

        var ex = await AssertThrowsAsync<KycServiceException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        Assert.AreEqual(error, ex.ErrorMessage);
        StringAssert.EndsWith(ex.Message, "a b c d e f");
    }

    [TestMethod]
    public async Task ErrorResponse_SanitizesSupplementaryPlaneFormatCharacters()
    {
        // Invisible Unicode tag characters (U+E0049 U+E0047 = "IG") and a musical-symbol format character (U+1D173).
        const string error = "a\U000E0049\U000E0047b\U0001D173c\U0001F600";
        var (service, _) = CreateService(JsonSerializer.Serialize(new { error }), HttpStatusCode.BadRequest);

        var ex = await AssertThrowsAsync<KycServiceException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        Assert.AreEqual(error, ex.ErrorMessage);
        StringAssert.EndsWith(ex.Message, "a  b c\U0001F600", "Format characters survived, or an emoji was lost.");
    }

    [TestMethod]
    public async Task ErrorResponse_TruncationDoesNotSplitASurrogatePair()
    {
        var error = new string('a', 255) + "\U0001F600" + new string('b', 100);
        var (service, _) = CreateService(JsonSerializer.Serialize(new { error }), HttpStatusCode.BadRequest);

        var ex = await AssertThrowsAsync<KycServiceException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        for (var i = 0; i < ex.Message.Length; i++)
        {
            if (char.IsHighSurrogate(ex.Message[i]))
            {
                Assert.IsTrue(i + 1 < ex.Message.Length && char.IsLowSurrogate(ex.Message[i + 1]),
                    "A lone high surrogate was left in the message.");
            }

            Assert.IsFalse(char.IsLowSurrogate(ex.Message[i]) && (i == 0 || !char.IsHighSurrogate(ex.Message[i - 1])),
                "A lone low surrogate was left in the message.");
        }
    }

    [TestMethod]
    [DataRow(".")]
    [DataRow("..")]
    public async Task DeleteCustomerAsync_WithDotSegmentAccount_ThrowsBeforeSending(string account)
    {
        var (service, handler) = CreateService(null, HttpStatusCode.OK);

        await AssertThrowsAsync<ArgumentException>(() =>
            service.DeleteCustomerAsync(new DeleteCustomerRequest { Jwt = Jwt, Account = account }));

        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task PostCustomerFileAsync_WithNonAsciiOrQuotedFileName_PercentEncodesIt()
    {
        var (service, handler) = CreateService(ReadTestData("customer-file.json"));

        await service.PostCustomerFileAsync(new PostCustomerFileRequest
        {
            Jwt = Jwt,
            File = PhotoFront,
            FileName = "r\u00E9sum\u00E9 \"1\"\\100%.pdf",
        });

        var part = handler.Requests.Single().Part("file");
        Assert.AreEqual("r%C3%A9sum%C3%A9 %221%22%5C100%25.pdf", part.FileName);
    }

    [TestMethod]
    public async Task PostCustomerFileAsync_WithWhitespaceFileName_UsesDefaultName()
    {
        var (service, handler) = CreateService(ReadTestData("customer-file.json"));

        await service.PostCustomerFileAsync(new PostCustomerFileRequest
        {
            Jwt = Jwt,
            File = PhotoFront,
            FileName = "   ",
        });

        Assert.AreEqual("file", handler.Requests.Single().Part("file").FileName);
    }

    [TestMethod]
    public async Task Uploads_SendFileBytesVerbatim()
    {
        var allBytes = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        var (service, handler) = CreateService(ReadTestData("put-customer.json"));
        await service.PutCustomerInfoAsync(new PutCustomerInfoRequest
        {
            Jwt = Jwt,
            CustomFiles = new Dictionary<string, byte[]> { ["photo_id_front"] = allBytes },
        });
        var (fileService, fileHandler) = CreateService(ReadTestData("customer-file.json"));
        await fileService.PostCustomerFileAsync(new PostCustomerFileRequest { Jwt = Jwt, File = allBytes });

        Assert.IsTrue(ContainsSequence(handler.Requests.Single().BodyBytes!, allBytes),
            "PUT /customer did not send the file bytes verbatim.");
        Assert.IsTrue(ContainsSequence(fileHandler.Requests.Single().BodyBytes!, allBytes),
            "POST /customer/files did not send the file bytes verbatim.");
    }

    [TestMethod]
    [DataRow("2030-01-01T00:00:00", 0, DisplayName = "no zone designator")]
    [DataRow("2030-01-01", 0, DisplayName = "date only")]
    [DataRow("2030-01-01T00:00:00Z", 0, DisplayName = "Z")]
    [DataRow("2030-01-01T02:00:00+02:00", 2, DisplayName = "offset")]
    public async Task GetCustomerFilesAsync_ReadsExpiresAtAsUtcUnlessAnOffsetIsGiven(string expiresAt, int offset)
    {
        var body = "{\"files\":[{\"file_id\":\"a\",\"content_type\":\"image/png\",\"size\":1,\"expires_at\":\"" +
                   expiresAt + "\"}]}";
        var (service, _) = CreateService(body);

        var response = await service.GetCustomerFilesAsync(new GetCustomerFilesRequest { Jwt = Jwt, FileId = "a" });

        var value = response.Files[0].ExpiresAt!.Value;
        Assert.AreEqual(TimeSpan.FromHours(offset), value.Offset);
        Assert.AreEqual(new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc), value.UtcDateTime);
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_NumericChoices_AreKeptAsTheirJsonText()
    {
        const string body =
            "{\"status\":\"NEEDS_INFO\",\"fields\":{\"n\":{\"type\":\"number\",\"description\":\"d\",\"choices\":[1,2.5,\"x\"]}}}";
        var (service, _) = CreateService(body);

        var response = await service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt });

        CollectionAssert.AreEqual(new[] { "1", "2.5", "x" }, response.Fields!["n"].Choices);
    }

    [TestMethod]
    public void ChoicesJsonConverter_WithReaderEndingInsideTheArray_ThrowsJsonException()
    {
        var reader = new Utf8JsonReader("[\"a\", 1"u8, false, default);
        Assert.IsTrue(reader.Read());

        try
        {
            new ChoicesJsonConverter().Read(ref reader, typeof(string[]), JsonSerializerOptions.Default);
            Assert.Fail("A reader that ran out inside the array was read as a complete array.");
        }
        catch (JsonException ex)
        {
            StringAssert.Contains(ex.Message, "closing bracket");
        }
    }

    [TestMethod]
    [DataRow("[true]", DisplayName = "boolean")]
    [DataRow("[{}]", DisplayName = "object")]
    [DataRow("[[1]]", DisplayName = "array")]
    [DataRow("\"a\"", DisplayName = "not an array")]
    public async Task GetCustomerInfoAsync_WithUnsupportedChoiceElement_ThrowsInvalidKycResponseException(
        string choices)
    {
        var body = "{\"status\":\"NEEDS_INFO\",\"fields\":{\"n\":{\"type\":\"string\",\"description\":\"d\",\"choices\":" + choices + "}}}";
        var (service, _) = CreateService(body);

        await AssertThrowsAsync<InvalidKycResponseException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));
    }

    [TestMethod]
    public async Task Response_WithUtf8ByteOrderMark_IsParsed()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(WithBom("{\"id\":\"x\"}")),
        });
        using var service = new KycService(KycServerUrl, new HttpClient(handler));

        var response = await service.PutCustomerInfoAsync(new PutCustomerInfoRequest { Jwt = Jwt, Id = "x" });

        Assert.AreEqual("x", response.Id);
    }

    [TestMethod]
    public async Task ErrorResponse_WithUtf8ByteOrderMark_KeepsTheErrorText()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new ByteArrayContent(WithBom("{\"error\":\"bad field\"}")),
        });
        using var service = new KycService(KycServerUrl, new HttpClient(handler));

        var ex = await AssertThrowsAsync<KycServiceException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        Assert.AreEqual("bad field", ex.ErrorMessage);
    }

    [TestMethod]
    public async Task Response_WithMultiByteUtf8_IsParsed()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            // Two-, three- and four-byte sequences, sent as raw UTF-8 rather than JSON escapes.
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes(
                "{\"status\":\"REJECTED\",\"message\":\"Z\u00fcrich \u6771\u4eac \ud83d\ude00\"}")),
        });
        using var service = new KycService(KycServerUrl, new HttpClient(handler));

        var response = await service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt });

        Assert.AreEqual("Z\u00fcrich \u6771\u4eac \ud83d\ude00", response.Message);
    }

    [TestMethod]
    public async Task Response_WithMalformedUtf8_ThrowsInvalidKycResponseException()
    {
        // A replacement-fallback decoder would read the two bytes as U+FFFD U+FFFD and accept the body.
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(WithInvalidUtf8("{\"status\":\"REJECTED\",\"message\":\"a", "b\"}")),
        });
        using var service = new KycService(KycServerUrl, new HttpClient(handler));

        var ex = await AssertThrowsAsync<InvalidKycResponseException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        StringAssert.Contains(ex.Message, "not valid UTF-8");
        Assert.IsInstanceOfType(ex.InnerException, typeof(DecoderFallbackException));
    }

    [TestMethod]
    public async Task ErrorResponse_WithMalformedUtf8_IsStillMappedByItsType()
    {
        // A 403 maps to AuthenticationRequiredException only through the body's "type", so error bodies stay
        // lenient: rejecting the bytes would turn an expired JWT into a plain KycServiceException.
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new ByteArrayContent(
                WithInvalidUtf8("{\"type\":\"authentication_required\",\"error\":\"a", "b\"}")),
        });
        using var service = new KycService(KycServerUrl, new HttpClient(handler));

        var ex = await AssertThrowsAsync<AuthenticationRequiredException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        Assert.AreEqual("a\ufffd\ufffdb", ex.ErrorMessage);
    }

    [TestMethod]
    public void StatusEnums_OnTheirOwn_RejectOrdinalsAndWrongCase()
    {
        foreach (var options in new[] { JsonOptions.DefaultOptions, new JsonSerializerOptions() })
        {
            Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<CustomerStatus>("0", options));
            Assert.ThrowsException<JsonException>(() =>
                JsonSerializer.Deserialize<CustomerStatus>("\"accepted\"", options));
            Assert.ThrowsException<JsonException>(() =>
                JsonSerializer.Deserialize<ProvidedFieldStatus>("\"Rejected\"", options));
            Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<FieldType>("1", options));
            Assert.AreEqual(CustomerStatus.NeedsInfo,
                JsonSerializer.Deserialize<CustomerStatus>("\"NEEDS_INFO\"", options));
            Assert.AreEqual("\"VERIFICATION_REQUIRED\"",
                JsonSerializer.Serialize(ProvidedFieldStatus.VerificationRequired, options));
        }
    }

    [TestMethod]
    public void StatusEnums_DefaultValue_IsNotADefinedMember()
    {
        Assert.IsFalse(Enum.IsDefined(typeof(CustomerStatus), default(CustomerStatus)));
        Assert.IsFalse(Enum.IsDefined(typeof(ProvidedFieldStatus), default(ProvidedFieldStatus)));
        Assert.IsFalse(Enum.IsDefined(typeof(FieldType), default(FieldType)));
    }

    [TestMethod]
    public async Task DeleteAndCallback_IgnoreTheSuccessBody()
    {
        var stream = new TrackingStream(new byte[16]);
        var handler = new RecordingHandler(_ =>
        {
            var content = new StreamContent(stream);
            content.Headers.ContentLength = KycService.MaxResponseBodyBytes * 2L;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        });
        using var service = new KycService(KycServerUrl, new HttpClient(handler));

        await service.DeleteCustomerAsync(new DeleteCustomerRequest { Jwt = Jwt, Account = Account });
        await service.PutCustomerCallbackAsync(new PutCustomerCallbackRequest
        {
            Jwt = Jwt,
            Url = "https://wallet.example.com/callback",
        });

        Assert.AreEqual(0, stream.BytesRead);
    }

    [TestMethod]
    public async Task GetCustomerInfoAsync_WithTransactionIdButNoType_ThrowsBeforeSending()
    {
        var (service, handler) = CreateService(ReadTestData("customer-accepted.json"));

        await AssertThrowsAsync<ArgumentException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt, TransactionId = "t1" }));
        await AssertThrowsAsync<ArgumentException>(() =>
            service.PutCustomerInfoAsync(new PutCustomerInfoRequest { Jwt = Jwt, TransactionId = "t1" }));

        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task Requests_WithMemoForContractAccount_ThrowBeforeSending()
    {
        var contract = StrKey.EncodeContractId(new byte[32]);
        var (service, handler) = CreateService(ReadTestData("customer-accepted.json"));

        await AssertThrowsAsync<ArgumentException>(() => service.GetCustomerInfoAsync(
            new GetCustomerInfoRequest { Jwt = Jwt, Account = contract, Memo = "1" }));
        await AssertThrowsAsync<ArgumentException>(() => service.PutCustomerInfoAsync(
            new PutCustomerInfoRequest { Jwt = Jwt, Account = contract, Memo = "1" }));
        await AssertThrowsAsync<ArgumentException>(() => service.PutCustomerCallbackAsync(
            new PutCustomerCallbackRequest
                { Jwt = Jwt, Url = "https://wallet.example.com/cb", Account = contract, Memo = "1" }));
        await AssertThrowsAsync<ArgumentException>(() => service.DeleteCustomerAsync(
            new DeleteCustomerRequest { Jwt = Jwt, Account = contract, Memo = "1" }));

        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task Requests_WithMemoForClassicAccount_AreSent()
    {
        var (service, handler) = CreateService(ReadTestData("customer-accepted.json"));

        await service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt, Account = Account, Memo = "1" });

        Assert.AreEqual(1, handler.Requests.Count);
    }

    [TestMethod]
    public async Task Requests_WithUnknownMemoType_ThrowBeforeSending()
    {
        var (service, handler) = CreateService(ReadTestData("customer-accepted.json"));

        await AssertThrowsAsync<ArgumentException>(() => service.GetCustomerInfoAsync(
            new GetCustomerInfoRequest { Jwt = Jwt, Memo = "1", MemoType = "ID" }));

        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task PutCustomerInfoAsync_WithEmptyVerificationOrFileReferenceKey_ThrowsBeforeSending()
    {
        var (service, handler) = CreateService(ReadTestData("put-customer.json"));

        await AssertThrowsAsync<ArgumentException>(() => service.PutCustomerInfoAsync(new PutCustomerInfoRequest
        {
            Jwt = Jwt,
            VerificationFields = new Dictionary<string, string> { [""] = "123456" },
        }));
        await AssertThrowsAsync<ArgumentException>(() => service.PutCustomerInfoAsync(new PutCustomerInfoRequest
        {
            Jwt = Jwt,
            FileReferences = new Dictionary<string, string> { [""] = "file_abc" },
        }));

        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task PutCustomerInfoAsync_WithNullFileContent_ThrowsBeforeSending()
    {
        var (service, handler) = CreateService(ReadTestData("put-customer.json"));

        var ex = await AssertThrowsAsync<ArgumentException>(() => service.PutCustomerInfoAsync(
            new PutCustomerInfoRequest
            {
                Jwt = Jwt,
                CustomFiles = new Dictionary<string, byte[]> { ["photo"] = null! },
            }));

        StringAssert.Contains(ex.Message, "must not be null");
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task CustomContentTypeHeader_DoesNotCorruptTheMultipartBody()
    {
        var (service, handler) = CreateService(ReadTestData("put-customer.json"),
            httpRequestHeaders: new Dictionary<string, string> { ["Content-Type"] = "application/json" });

        await service.PutCustomerInfoAsync(new PutCustomerInfoRequest { Jwt = Jwt, Id = CustomerId });

        // The raw header values, not the parsed ContentType, which shows only the first of several values.
        var rawValues = handler.Requests.Single().RawContentTypeValues;
        Assert.AreEqual(1, rawValues.Count, string.Join(" | ", rawValues));
        StringAssert.StartsWith(rawValues[0], "multipart/form-data");
    }

    [TestMethod]
    public async Task CustomHeaderWithAnInvalidName_IsIgnoredOnEveryMethod()
    {
        var headers = new Dictionary<string, string> { ["Bad Name"] = "v" };
        var (getService, _) = CreateService(ReadTestData("customer-accepted.json"), httpRequestHeaders: headers);
        var (putService, putHandler) = CreateService(ReadTestData("put-customer.json"), httpRequestHeaders: headers);

        var (postService, _) = CreateService(ReadTestData("customer-file.json"), httpRequestHeaders: headers);
        var (deleteService, deleteHandler) = CreateService(null, httpRequestHeaders: headers);

        await getService.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt });
        await putService.PutCustomerInfoAsync(new PutCustomerInfoRequest { Jwt = Jwt, Id = CustomerId });
        await postService.PostCustomerFileAsync(new PostCustomerFileRequest { Jwt = Jwt, File = PhotoFront });
        await deleteService.DeleteCustomerAsync(
            new DeleteCustomerRequest { Jwt = Jwt, Account = Account, Memo = "1" });

        Assert.AreEqual(1, putHandler.Requests.Count);
        Assert.AreEqual(1, deleteHandler.Requests.Count);
    }

    [TestMethod]
    public async Task PutCustomerInfoAsync_ChecksTheSep12RulesOnTheMergedFields()
    {
        var contract = StrKey.EncodeContractId(new byte[32]);
        var (service, handler) = CreateService(ReadTestData("put-customer.json"));

        await AssertThrowsAsync<ArgumentException>(() => service.PutCustomerInfoAsync(new PutCustomerInfoRequest
        {
            Jwt = Jwt,
            Account = Account,
            CustomFields = new Dictionary<string, string> { ["account"] = contract, ["memo"] = "1" },
        }));
        await AssertThrowsAsync<ArgumentException>(() => service.PutCustomerInfoAsync(new PutCustomerInfoRequest
        {
            Jwt = Jwt,
            CustomFields = new Dictionary<string, string> { ["transaction_id"] = "t1" },
        }));
        await AssertThrowsAsync<ArgumentException>(() => service.PutCustomerInfoAsync(new PutCustomerInfoRequest
        {
            Jwt = Jwt,
            Memo = "1",
            CustomFields = new Dictionary<string, string> { ["memo_type"] = "bogus" },
        }));

        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public void Sep12Types_NameOnlyPublicConverters()
    {
        // A consumer's source-generated JsonSerializerContext cannot use an internal converter (SYSLIB1220), and
        // then drops the enum's metadata or silently falls back to the default DateTimeOffset handling.
        var types = typeof(KycService).Assembly.GetExportedTypes()
            .Where(t => t.Namespace?.StartsWith("StellarDotnetSdk.Sep.Sep0012", StringComparison.Ordinal) == true);
        var converters = types
            .SelectMany(t => new MemberInfo[] { t }.Concat(t.GetProperties()))
            .Select(m => m.GetCustomAttribute<System.Text.Json.Serialization.JsonConverterAttribute>())
            .Where(a => a?.ConverterType != null)
            .Select(a => a!.ConverterType!)
            .Distinct()
            .ToList();

        Assert.IsTrue(converters.Count >= 6, $"Found only {converters.Count} converters.");
        foreach (var converter in converters)
        {
            Assert.IsTrue(converter.IsPublic, $"{converter.Name} is not public.");
        }

        Assert.AreEqual(typeof(UtcDateTimeOffsetJsonConverter), typeof(CustomerFileResponse)
            .GetProperty(nameof(CustomerFileResponse.ExpiresAt))!
            .GetCustomAttribute<System.Text.Json.Serialization.JsonConverterAttribute>()!.ConverterType);
    }

    [TestMethod]
    public void StatusEnums_AsDictionaryKeys_UseTheWireLiterals()
    {
        foreach (var options in new[] { JsonOptions.DefaultOptions, new JsonSerializerOptions() })
        {
            var map = JsonSerializer.Deserialize<Dictionary<CustomerStatus, int>>("{\"NEEDS_INFO\":1}", options)!;
            Assert.AreEqual(1, map[CustomerStatus.NeedsInfo]);
            Assert.AreEqual("{\"REJECTED\":2}",
                JsonSerializer.Serialize(new Dictionary<CustomerStatus, int> { [CustomerStatus.Rejected] = 2 }, options));
            Assert.ThrowsException<JsonException>(() =>
                JsonSerializer.Deserialize<Dictionary<CustomerStatus, int>>("{\"0\":1}", options));
            Assert.ThrowsException<JsonException>(() =>
                JsonSerializer.Deserialize<Dictionary<ProvidedFieldStatus, int>>("{\"accepted\":1}", options));
            Assert.AreEqual(FieldType.Binary,
                JsonSerializer.Deserialize<Dictionary<FieldType, int>>("{\"binary\":1}", options)!.Keys.Single());
            Assert.ThrowsException<JsonException>(() =>
                JsonSerializer.Deserialize<Dictionary<FieldType, int>>("{\"Binary\":1}", options));
        }
    }

    [TestMethod]
    public void FromJson_WithALoneSurrogate_ThrowsJsonException()
    {
        Assert.ThrowsException<JsonException>(() => GetCustomerInfoResponse.FromJson("{\"status\":\"\ud800\"}"));
    }

    [TestMethod]
    public async Task Requests_WithARealisticJwt_AreSent()
    {
        const string jwt = "eyJhbGciOiJFZERTQSIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJHQUJDIiwiaWF0IjoxfQ.A-_b9~";
        var (service, handler) = CreateService(ReadTestData("customer-accepted.json"));

        await service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = jwt });

        Assert.AreEqual(jwt, handler.Requests.Single().AuthorizationParameter);
    }

    [TestMethod]
    [DataRow("a b\u00e9")]
    [DataRow("a b")]
    [DataRow("a\tb")]
    public async Task Requests_WithNonPrintableAsciiJwt_ThrowArgumentException(string jwt)
    {
        var (service, handler) = CreateService(ReadTestData("customer-accepted.json"));

        await AssertThrowsAsync<ArgumentException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = jwt }));

        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task PutCustomerCallbackAsync_WithBackslashUrl_ThrowsArgumentException()
    {
        var (service, handler) = CreateService(null);

        await AssertThrowsAsync<ArgumentException>(() => service.PutCustomerCallbackAsync(
            new PutCustomerCallbackRequest { Jwt = Jwt, Url = "https:\\\\wallet.example.com/cb" }));

        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task InternalClient_RequestTimeout_BoundsAStalledBodyAndStalledHeaders()
    {
        foreach (var sendHeaders in new[] { true, false })
        {
            using var server = new StallingServer(sendHeaders);
            using var service = new KycService($"http://127.0.0.1:{server.Port}", resilienceOptions:
                new HttpResilienceOptions { RequestTimeout = TimeSpan.FromMilliseconds(500) });

            var call = service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt });
            var finished = await Task.WhenAny(call, Task.Delay(TimeSpan.FromSeconds(20)));

            Assert.AreSame(call, finished, $"RequestTimeout was not applied (headers sent: {sendHeaders}).");
            var ex = await AssertThrowsAsync<TaskCanceledException>(() => call);
            Assert.IsInstanceOfType(ex.InnerException, typeof(TimeoutException));
        }
    }

    [TestMethod]
    public async Task ResponseContent_IsDisposedWhenTheBodyIsNotRead()
    {
        var content = new TrackingContent("{\"ignored\":true}");
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        using var service = new KycService(KycServerUrl, new HttpClient(handler));

        await service.DeleteCustomerAsync(new DeleteCustomerRequest { Jwt = Jwt, Account = Account });

        Assert.IsTrue(content.Disposed, "The unread success response was not disposed.");
    }

    [TestMethod]
    public async Task RequestContent_IsDisposedWhenTheRequestMessageCannotBeCreated()
    {
        // The public path is DeleteCustomerAsync with a memo and an account long enough to exceed the runtime's URL
        // limit, which only .NET 8 enforces; an out-of-range port makes the same constructor throw the same
        // UriFormatException on every runtime.
        var (service, handler) = CreateService("");
        var content = new TrackingContent("memo");
        var sendAsync = typeof(KycService).GetMethod("SendAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;

        await AssertThrowsAsync<UriFormatException>(() => (Task)sendAsync.Invoke(service,
            new object?[] { HttpMethod.Delete, "https://kyc.example.com:99999/customer", content, Jwt, true, false,
                CancellationToken.None })!);

        Assert.IsTrue(content.Disposed, "The request content was not disposed.");
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    [DataRow(HttpStatusCode.TooManyRequests)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    public async Task ErrorResponse_WithRetryAfter_ExposesIt(HttpStatusCode status)
    {
        var handler = new RecordingHandler(_ =>
        {
            var response = new HttpResponseMessage(status) { Content = new StringContent("{\"error\":\"slow down\"}") };
            response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(5));
            return response;
        });
        using var service = new KycService(KycServerUrl, new HttpClient(handler));

        var ex = await AssertThrowsAsync<KycServiceException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt }));

        Assert.AreEqual(TimeSpan.FromSeconds(5), ex.RetryAfterDelay);
        Assert.AreEqual((int)status, ex.StatusCode);
    }

    [TestMethod]
    [DataRow("abc\r\nX-Injected: 1")]
    [DataRow("abc\ndef")]
    public async Task Requests_WithLineBreakInJwt_ThrowArgumentException(string jwt)
    {
        var (service, handler) = CreateService(ReadTestData("customer-accepted.json"));

        await AssertThrowsAsync<ArgumentException>(() =>
            service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = jwt }));

        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public void RequestToString_NeutralizesLineBreaksAndBidiControlsInEveryStringProperty()
    {
        // A transaction ID usually comes from an anchor and a memo from an end user; neither may forge or reorder
        // the log line a request is formatted into. Each string property is set on its own, so a single call site
        // that skips sanitizing fails here.
        const string hostile = "1\r\n[INFO] forged\u2028line \u202Eevil\u200B\U000E0041";
        var unsafeChars = new[] { '\r', '\n', '\u2028', '\u202E', '\u200B', '\uDB40' };
        var requestTypes = typeof(GetCustomerInfoRequest).Assembly.GetExportedTypes()
            .Where(t => t.Namespace == typeof(GetCustomerInfoRequest).Namespace && !t.IsAbstract)
            .ToList();
        Assert.AreEqual(7, requestTypes.Count, string.Join(", ", requestTypes.Select(t => t.Name)));

        var checkedProperties = 0;
        foreach (var type in requestTypes)
        {
            foreach (var property in type.GetProperties().Where(p => p.PropertyType == typeof(string) && p.CanWrite))
            {
                var request = Activator.CreateInstance(type)!;
                property.SetValue(request, hostile);

                var text = request.ToString()!;

                var where = $"{type.Name}.{property.Name}";
                foreach (var c in unsafeChars)
                {
                    Assert.IsFalse(text.Contains(c), $"{where} printed U+{(int)c:X4}: {text}");
                }

                Assert.IsTrue(text.Contains("1  [INFO] forged line  evil  ") || text.Contains("[redacted]"),
                    $"{where} printed neither the sanitized value nor a placeholder: {text}");
                checkedProperties++;
            }
        }

        Assert.IsTrue(checkedProperties >= 25, $"Only {checkedProperties} properties checked.");

        // Uri keeps some format and separator characters in a host, so the printed origin is sanitized too.
        foreach (var url in new[] { "https://host\u2028.example", "https://host\u200B.example", "https://host\U000E0041.example" })
        {
            var urlText = new PutCustomerCallbackRequest { Jwt = Jwt, Url = url }.ToString();
            foreach (var c in unsafeChars)
            {
                Assert.IsFalse(urlText.Contains(c), $"Url printed U+{(int)c:X4}: {urlText}");
            }

            StringAssert.Contains(urlText, "Url = https://host", url);
        }

        // An ordinary identifier is printed unchanged.
        StringAssert.Contains(new GetCustomerInfoRequest { Jwt = Jwt, Memo = "12345" }.ToString(), "Memo = 12345,");
    }

    [TestMethod]
    public void RequestToString_RedactsTheJwtAndCustomerData()
    {
        var request = new PutCustomerInfoRequest
        {
            Jwt = "eyJ.secret.jwt",
            Id = CustomerId,
            KycFields = new StandardKycFields { NaturalPerson = new NaturalPersonKycFields { EmailAddress = "a@b.c" } },
            CustomFields = new Dictionary<string, string> { ["ssn"] = "123-45-6789" },
        };

        var text = request.ToString();

        StringAssert.StartsWith(text, "PutCustomerInfoRequest { Jwt = [redacted], Id = " + CustomerId + ",");
        Assert.IsFalse(text.Contains("eyJ.secret.jwt"));
        Assert.IsFalse(text.Contains("a@b.c"));
        StringAssert.Contains(text, "CustomFields = [1 entry, [redacted]]");
        StringAssert.EndsWith(text, " }");
        foreach (var other in new object[]
                 {
                     new GetCustomerInfoRequest { Jwt = "eyJ.secret.jwt" },
                     new DeleteCustomerRequest { Jwt = "eyJ.secret.jwt", Account = Account },
                     new PutCustomerCallbackRequest { Jwt = "eyJ.secret.jwt", Url = "https://w.example" },
                     new PostCustomerFileRequest { Jwt = "eyJ.secret.jwt", File = new byte[3] },
                     new GetCustomerFilesRequest { Jwt = "eyJ.secret.jwt", FileId = "f" },
#pragma warning disable CS0618
                     new PutCustomerVerificationRequest
                     {
                         Jwt = "eyJ.secret.jwt", Id = "i",
                         VerificationFields = new Dictionary<string, string> { ["mobile_number"] = "1" },
                     },
#pragma warning restore CS0618
                 })
        {
            Assert.IsFalse(other.ToString()!.Contains("eyJ.secret.jwt"), other.GetType().Name);
            StringAssert.Contains(other.ToString(), " { Jwt = [redacted]");
        }

        var callback = new PutCustomerCallbackRequest
        {
            Jwt = "eyJ.secret.jwt",
            Url = "https://user:pw@wallet.example.com/cb?token=SECRETTOKEN#frag",
        };
        var callbackText = callback.ToString();
        StringAssert.Contains(callbackText, "Url = https://wallet.example.com [redacted]");
        Assert.IsFalse(callbackText.Contains("SECRETTOKEN") || callbackText.Contains("pw@"), callbackText);
        var pathToken = new PutCustomerCallbackRequest
            { Jwt = "j.w.t", Url = "https://wallet.example.com/hook/SECRETPATHTOKEN" }.ToString();
        Assert.IsFalse(pathToken.Contains("SECRETPATHTOKEN"), pathToken);
        StringAssert.Contains(pathToken, "Url = https://wallet.example.com [redacted]");
        StringAssert.Contains(new PutCustomerCallbackRequest { Jwt = "j.w.t", Url = "https://wallet.example.com" }
            .ToString(), "Url = https://wallet.example.com,");
        foreach (var url in new[]
                 {
                     "https://wallet.example.com?t=SECRET", "https://wallet.example.com#SECRET",
                     "https://u:SECRET@wallet.example.com/",
                 })
        {
            var urlText = new PutCustomerCallbackRequest { Jwt = "j.w.t", Url = url }.ToString();
            StringAssert.Contains(urlText, "Url = https://wallet.example.com [redacted]", url);
            Assert.IsFalse(urlText.Contains("SECRET"), urlText);
        }
    }

    [TestMethod]
    public void DeprecatedVerificationEndpoint_IsMarkedObsolete()
    {
        Assert.IsNotNull(typeof(KycService).GetMethod(nameof(KycService.PutCustomerVerificationAsync))!
            .GetCustomAttribute<ObsoleteAttribute>());
#pragma warning disable CS0618
        Assert.IsNotNull(typeof(PutCustomerVerificationRequest).GetCustomAttribute<ObsoleteAttribute>());
#pragma warning restore CS0618
    }

    [TestMethod]
    public async Task PutCustomerInfoAsync_With202Accepted_ParsesTheId()
    {
        var (service, _) = CreateService(ReadTestData("put-customer.json"), HttpStatusCode.Accepted);

        var response = await service.PutCustomerInfoAsync(new PutCustomerInfoRequest { Jwt = Jwt, Id = CustomerId });

        Assert.IsFalse(string.IsNullOrEmpty(response.Id));
    }

    [TestMethod]
    public async Task FromDomainAsync_UsesTheGivenClientAndHeadersForTomlAndKycRequests()
    {
        var handler = new RecordingHandler(request => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(request.RequestUri!.AbsolutePath.EndsWith("stellar.toml")
                ? "KYC_SERVER=\"https://kyc.example.com/sep12\""
                : ReadTestData("customer-accepted.json")),
        });
        using var httpClient = new HttpClient(handler);
        using var service = await KycService.FromDomainAsync("example.com", httpClient: httpClient,
            httpRequestHeaders: new Dictionary<string, string> { ["X-Wallet"] = "w1" });

        await service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt });

        Assert.AreEqual(2, handler.Requests.Count);
        Assert.IsTrue(handler.Requests.All(r => r.Headers.TryGetValue("X-Wallet", out var v) && v == "w1"));
    }

    [TestMethod]
    public async Task ResponseStreams_AreDisposedOnEveryOutcome()
    {
        foreach (var (status, body) in new[]
                 {
                     (HttpStatusCode.OK, ReadTestData("customer-accepted.json")),
                     (HttpStatusCode.BadRequest, "{\"error\":\"x\"}"),
                     (HttpStatusCode.OK, "{\"status\":\"BOGUS\"}"),
                 })
        {
            var stream = new TrackingStream(Encoding.UTF8.GetBytes(body));
            var handler = new RecordingHandler(_ => new HttpResponseMessage(status)
            {
                Content = new StreamContent(stream),
            });
            using var service = new KycService(KycServerUrl, new HttpClient(handler));

            try
            {
                await service.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = Jwt });
            }
            catch (KycServiceException)
            {
            }

            Assert.IsTrue(stream.Disposed, $"Response stream not disposed for {status} {body}.");
        }
    }

    private static byte[] WithBom(string text)
    {
        return new byte[] { 0xEF, 0xBB, 0xBF }.Concat(Encoding.UTF8.GetBytes(text)).ToArray();
    }

    private static byte[] WithInvalidUtf8(string before, string after)
    {
        // 0xFF never occurs in UTF-8; 0xC3 starts a two-byte sequence that the next ASCII byte cannot continue.
        return Encoding.UTF8.GetBytes(before).Concat(new byte[] { 0xFF, 0xC3 })
            .Concat(Encoding.UTF8.GetBytes(after)).ToArray();
    }

    private static bool ContainsSequence(byte[] haystack, byte[] needle)
    {
        for (var i = 0; i <= haystack.Length - needle.Length; i++)
        {
            if (haystack.AsSpan(i, needle.Length).SequenceEqual(needle))
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Test infrastructure

    /// <summary>A request as it went on the wire, captured before the SDK disposes it.</summary>
    private sealed class RecordedRequest
    {
        public HttpMethod Method { get; init; } = null!;
        public Uri Uri { get; init; } = null!;
        public string? AuthorizationScheme { get; init; }
        public string? AuthorizationParameter { get; init; }
        public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);
        public string? ContentType { get; init; }
        public string? Body { get; init; }
        public byte[]? BodyBytes { get; init; }
        public List<string> RawContentTypeValues { get; init; } = new();
        public List<MultipartPart> Parts { get; init; } = new();

        public MultipartPart Part(string name)
        {
            var matches = Parts.Where(p => p.Name == name).ToList();
            Assert.AreEqual(1, matches.Count, $"Expected exactly one part named '{name}'.");
            return matches[0];
        }
    }

    private sealed record MultipartPart(string Name, string? FileName, string? ContentType, string Value);

    /// <summary>Records every request and answers each with a response built by the supplied factory.</summary>
    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

        public RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
        {
            _respond = respond;
        }

        public List<RecordedRequest> Requests { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string? body = null;
            byte[]? bodyBytes = null;
            var rawContentTypes = new List<string>();
            string? contentType = null;
            var parts = new List<MultipartPart>();
            if (request.Content != null)
            {
                bodyBytes = await request.Content.ReadAsByteArrayAsync();
                body = await request.Content.ReadAsStringAsync();
                contentType = request.Content.Headers.ContentType?.ToString();
                if (request.Content.Headers.NonValidated.TryGetValues("Content-Type", out var rawValues))
                {
                    rawContentTypes.AddRange(rawValues);
                }
                var boundary = request.Content.Headers.ContentType?.Parameters
                    .FirstOrDefault(p => p.Name == "boundary")?.Value?.Trim('"');
                if (boundary != null)
                {
                    parts = ParseMultipart(body, boundary);
                }
            }

            var recorded = new RecordedRequest
            {
                Method = request.Method,
                Uri = request.RequestUri!,
                AuthorizationScheme = request.Headers.Authorization?.Scheme,
                AuthorizationParameter = request.Headers.Authorization?.Parameter,
                ContentType = contentType,
                Body = body,
                BodyBytes = bodyBytes,
                RawContentTypeValues = rawContentTypes,
                Parts = parts,
            };
            foreach (var header in request.Headers)
            {
                recorded.Headers[header.Key] = string.Join(",", header.Value);
            }

            Requests.Add(recorded);
            return _respond(request);
        }

        private static List<MultipartPart> ParseMultipart(string body, string boundary)
        {
            var result = new List<MultipartPart>();
            var sections = body.Split(new[] { "--" + boundary }, StringSplitOptions.None);
            // sections[0] is the preamble; the last section starts with "--" (the closing delimiter).
            foreach (var section in sections.Skip(1))
            {
                if (section.StartsWith("--", StringComparison.Ordinal))
                {
                    break;
                }

                var headerEnd = section.IndexOf("\r\n\r\n", StringComparison.Ordinal);
                var headers = section.Substring(0, headerEnd).Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                var value = section.Substring(headerEnd + 4);
                if (value.EndsWith("\r\n", StringComparison.Ordinal))
                {
                    value = value.Substring(0, value.Length - 2);
                }

                string? name = null;
                string? fileName = null;
                string? partContentType = null;
                foreach (var header in headers)
                {
                    if (header.StartsWith("Content-Disposition:", StringComparison.OrdinalIgnoreCase))
                    {
                        name = ExtractParameter(header, "name");
                        fileName = ExtractParameter(header, "filename");
                    }
                    else if (header.StartsWith("Content-Type:", StringComparison.OrdinalIgnoreCase))
                    {
                        partContentType = header.Substring("Content-Type:".Length).Trim();
                    }
                }

                result.Add(new MultipartPart(name!, fileName, partContentType, value));
            }

            return result;
        }

        private static string? ExtractParameter(string header, string parameter)
        {
            foreach (var segment in header.Split(';').Skip(1))
            {
                var trimmed = segment.Trim();
                if (trimmed.StartsWith(parameter + "=", StringComparison.Ordinal))
                {
                    return trimmed.Substring(parameter.Length + 1).Trim('"');
                }
            }

            return null;
        }
    }

    /// <summary>A read-only stream that counts how many bytes the SDK pulled from it.</summary>
    private sealed class TrackingStream : MemoryStream
    {
        public TrackingStream(byte[] buffer)
            : base(buffer, false)
        {
        }

        public long BytesRead { get; private set; }

        public bool Disposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = base.Read(buffer, offset, count);
            BytesRead += read;
            return read;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return Task.FromResult(Read(buffer, offset, count));
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var read = base.Read(buffer.Span);
            BytesRead += read;
            return new ValueTask<int>(read);
        }
    }

    /// <summary>An HttpClient that records whether its owning service was already marked disposed.</summary>
    private sealed class DisposeObservingHttpClient : HttpClient
    {
        private readonly Func<bool> _isServiceDisposed;

        public DisposeObservingHttpClient(Func<bool> isServiceDisposed)
        {
            _isServiceDisposed = isServiceDisposed;
        }

        public bool? ServiceDisposedWhenReleased { get; private set; }

        protected override void Dispose(bool disposing)
        {
            ServiceDisposedWhenReleased ??= _isServiceDisposed();
            base.Dispose(disposing);
        }
    }

    /// <summary>Content that records whether it was disposed.</summary>
    private sealed class TrackingContent : StringContent
    {
        public TrackingContent(string content)
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

    /// <summary>
    ///     A real loopback HTTP server that reads each request and then stalls: after sending response headers and
    ///     one body byte, or before sending anything. Used where the behaviour under test lives in the real socket
    ///     handler (the internal client), which a fake HttpMessageHandler would bypass.
    /// </summary>
    private sealed class StallingServer : IDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource _stop = new();

        public StallingServer(bool sendHeaders)
        {
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            _ = Task.Run(async () =>
            {
                while (!_stop.IsCancellationRequested)
                {
                    TcpClient client;
                    try
                    {
                        client = await _listener.AcceptTcpClientAsync();
                    }
                    catch (Exception)
                    {
                        return;
                    }

                    _ = Task.Run(async () =>
                    {
                        using var _ = client;
                        var stream = client.GetStream();
                        var buffer = new byte[8192];
                        try
                        {
                            await stream.ReadAsync(buffer, 0, buffer.Length, _stop.Token);
                            if (sendHeaders)
                            {
                                var head = Encoding.ASCII.GetBytes(
                                    "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: 100\r\n\r\n{");
                                await stream.WriteAsync(head, 0, head.Length, _stop.Token);
                            }

                            await Task.Delay(Timeout.Infinite, _stop.Token);
                        }
                        catch (Exception)
                        {
                            // Stopped, or the client gave up.
                        }
                    });
                }
            });
        }

        public int Port { get; }

        public void Dispose()
        {
            _stop.Cancel();
            _listener.Stop();
        }
    }

    /// <summary>A response stream that never delivers a byte, like a server that stalls after its headers.</summary>
    private sealed class StallingStream : MemoryStream
    {
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken)
        {
            await Task.Delay(System.Threading.Timeout.Infinite, cancellationToken);
            return 0;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(System.Threading.Timeout.Infinite, cancellationToken);
            return 0;
        }
    }

    /// <summary>A response stream whose connection drops after the first chunk, as a truncated body does.</summary>
    private sealed class FailingStream : MemoryStream
    {
        private bool _served;

        public FailingStream(byte[] firstChunk)
            : base(firstChunk, false)
        {
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Next(buffer.AsSpan(offset, count)));
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return new ValueTask<int>(Next(buffer.Span));
        }

        private int Next(Span<byte> buffer)
        {
            if (_served)
            {
                throw new IOException("The response ended prematurely.");
            }

            _served = true;
            return base.Read(buffer);
        }
    }

    /// <summary>Content that does not declare a length, like a chunked response.</summary>
    private sealed class UnknownLengthContent : HttpContent
    {
        private readonly Stream _stream;

        public UnknownLengthContent(Stream stream)
        {
            _stream = stream;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            return _stream.CopyToAsync(stream);
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            return Task.FromResult(_stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }

    #endregion
}
