using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Sep.Sep0045;
using StellarDotnetSdk.Sep.Sep0045.Exceptions;

namespace StellarDotnetSdk.Tests.Sep.Sep0045;

[TestClass]
public class ClientWebAuthContractTest
{
    private const string AuthEndpoint = "https://auth.example.com/auth";
    private const string SorobanRpcUrl = "https://rpc.example.com";
    private const string HomeDomain = "sep45.example.com";
    private static string ContractId => TestChallengeBuilder.DefaultWebAuthContractId;

    [TestMethod]
    public async Task FromDomainAsync_Throws_WhenContractEndpointMissing()
    {
        var signer = KeyPair.Random();
        var toml = $"SIGNING_KEY=\"{signer.AccountId}\"\nWEB_AUTH_CONTRACT_ID=\"{ContractId}\"\n";
        var handler = BuildTomlHandler(toml);
        using var client = new HttpClient(handler.Object);

        await Assert.ThrowsExceptionAsync<NoWebAuthContractEndpointFoundException>(() =>
            ClientWebAuthContract.FromDomainAsync(
                HomeDomain, Network.Test(), SorobanRpcUrl, httpClient: client));
    }

    [TestMethod]
    public async Task FromDomainAsync_Throws_WhenContractIdMissing()
    {
        var signer = KeyPair.Random();
        var toml = $"SIGNING_KEY=\"{signer.AccountId}\"\nWEB_AUTH_FOR_CONTRACTS_ENDPOINT=\"{AuthEndpoint}\"\n";
        var handler = BuildTomlHandler(toml);
        using var client = new HttpClient(handler.Object);

        await Assert.ThrowsExceptionAsync<NoWebAuthContractIdFoundException>(() =>
            ClientWebAuthContract.FromDomainAsync(
                HomeDomain, Network.Test(), SorobanRpcUrl, httpClient: client));
    }

    [TestMethod]
    public async Task FromDomainAsync_Throws_WhenSigningKeyMissing()
    {
        var toml = $"WEB_AUTH_FOR_CONTRACTS_ENDPOINT=\"{AuthEndpoint}\"\nWEB_AUTH_CONTRACT_ID=\"{ContractId}\"\n";
        var handler = BuildTomlHandler(toml);
        using var client = new HttpClient(handler.Object);

        await Assert.ThrowsExceptionAsync<StellarDotnetSdk.Sep.Sep0010.Exceptions.NoWebAuthServerSigningKeyFoundException>(() =>
            ClientWebAuthContract.FromDomainAsync(
                HomeDomain, Network.Test(), SorobanRpcUrl, httpClient: client));
    }

    [TestMethod]
    public async Task FromDomainAsync_ReturnsInstance_WhenTomlComplete()
    {
        var signer = KeyPair.Random();
        var toml =
            $"SIGNING_KEY=\"{signer.AccountId}\"\n" +
            $"WEB_AUTH_FOR_CONTRACTS_ENDPOINT=\"{AuthEndpoint}\"\n" +
            $"WEB_AUTH_CONTRACT_ID=\"{ContractId}\"\n";
        var handler = BuildTomlHandler(toml);
        using var client = new HttpClient(handler.Object);

        using var instance = await ClientWebAuthContract.FromDomainAsync(
            HomeDomain, Network.Test(), SorobanRpcUrl, httpClient: client);
        Assert.IsNotNull(instance);
    }

    [TestMethod]
    public async Task GetChallengeAsync_BuildsCorrectUrl_WithAllParams()
    {
        var serverKp = KeyPair.Random();
        HttpRequestMessage? captured = null;
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"authorization_entries\":\"aGk=\",\"network_passphrase\":\"Test SDF Network ; September 2015\"}"),
            });
        using var client = new HttpClient(handler.Object);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, ContractId, Network.Test(), serverKp.AccountId,
            HomeDomain, SorobanRpcUrl, httpClient: client);

        var result = await auth.GetChallengeAsync(
            TestChallengeBuilder.DefaultWebAuthContractId, "home.example", "client.example");

        Assert.IsNotNull(captured);
        var query = captured!.RequestUri!.Query;
        StringAssert.Contains(query, "account=");
        StringAssert.Contains(query, "home_domain=home.example");
        StringAssert.Contains(query, "client_domain=client.example");
        Assert.AreEqual("aGk=", result.AuthorizationEntries);
    }

    [TestMethod]
    public async Task GetChallengeAsync_Throws_OnNon200Response()
    {
        var serverKp = KeyPair.Random();
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("bad account"),
            });
        using var client = new HttpClient(handler.Object);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, ContractId, Network.Test(), serverKp.AccountId,
            HomeDomain, SorobanRpcUrl, httpClient: client);

        await Assert.ThrowsExceptionAsync<ChallengeForContractsRequestErrorException>(() =>
            auth.GetChallengeAsync(TestChallengeBuilder.DefaultWebAuthContractId));
    }

    [TestMethod]
    public async Task GetChallengeAsync_Throws_WhenAuthorizationEntriesMissing()
    {
        var serverKp = KeyPair.Random();
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"network_passphrase\":\"x\"}"),
            });
        using var client = new HttpClient(handler.Object);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, ContractId, Network.Test(), serverKp.AccountId,
            HomeDomain, SorobanRpcUrl, httpClient: client);

        await Assert.ThrowsExceptionAsync<MissingAuthorizationEntriesInChallengeResponseException>(() =>
            auth.GetChallengeAsync(TestChallengeBuilder.DefaultWebAuthContractId));
    }

    [TestMethod]
    public void ValidateChallenge_Passes_WhenValid()
    {
        var result = TestChallengeBuilder.Build();
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, TestChallengeBuilder.DefaultWebAuthContractId,
            Network.Test(), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultHomeDomain, SorobanRpcUrl,
            httpClient: new HttpClient());
        auth.ValidateChallenge(xdr, result.ClientContractId);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidClientAccountException))]
    public void ValidateChallenge_Throws_WhenClientAccountMismatch()
    {
        var result = TestChallengeBuilder.Build();
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, TestChallengeBuilder.DefaultWebAuthContractId,
            Network.Test(), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultHomeDomain, SorobanRpcUrl,
            httpClient: new HttpClient());
        // Use an all-zero contract id as a definitely-different client account
        var differentClientCid = StellarDotnetSdk.StrKey.EncodeContractId(new byte[32]);
        auth.ValidateChallenge(xdr, differentClientCid);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidClientDomainException))]
    public void ValidateChallenge_Throws_WhenClientDomainAccountMismatch()
    {
        var cdKp = KeyPair.Random();
        var result = TestChallengeBuilder.Build(clientDomain: "c.example", clientDomainKeyPair: cdKp);
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, TestChallengeBuilder.DefaultWebAuthContractId,
            Network.Test(), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultHomeDomain, SorobanRpcUrl,
            httpClient: new HttpClient());
        var differentKp = KeyPair.Random();
        auth.ValidateChallenge(xdr, result.ClientContractId, differentKp.AccountId);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidClientDomainException))]
    public void ValidateChallenge_Throws_WhenUnexpectedClientDomainPresent()
    {
        var cdKp = KeyPair.Random();
        var result = TestChallengeBuilder.Build(clientDomain: "c.example", clientDomainKeyPair: cdKp);
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, TestChallengeBuilder.DefaultWebAuthContractId,
            Network.Test(), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultHomeDomain, SorobanRpcUrl,
            httpClient: new HttpClient());
        // Caller passes null (no client domain expected) but challenge contains one → must throw
        auth.ValidateChallenge(xdr, result.ClientContractId);
    }

    [TestMethod]
    public async Task SignAuthorizationEntries_ProducesVerifiableSignature_ForClientSigner()
    {
        // Use the client account's G-address as the client-contract identifier so that the
        // signer's AccountId matches the credentials address.
        var signer = KeyPair.Random();
        var result = TestChallengeBuilder.Build(clientContractId: signer.AccountId);
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);

        using var auth = new TestableClientWebAuthContract(
            AuthEndpoint, TestChallengeBuilder.DefaultWebAuthContractId,
            Network.Test(), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultHomeDomain, SorobanRpcUrl,
            httpClient: new HttpClient(), latestLedgerOverride: 500);

        var signed = await auth.SignAuthorizationEntriesAsync(xdr, new[] { signer });
        var entries = TestChallengeBuilder.DecodeEntries(signed);
        // Client entry (index 1) should now carry a non-empty signature vector.
        Assert.IsTrue(entries[1].Credentials.Address.Signature.Vec!.InnerValue.Length > 0);
    }

    [TestMethod]
    public async Task SignAuthorizationEntries_SignsClientDomain_WithLocalKeyPair()
    {
        var cdKp = KeyPair.Random();
        var clientKp = KeyPair.Random();
        var result = TestChallengeBuilder.Build(
            clientContractId: clientKp.AccountId,
            clientDomain: "c.example",
            clientDomainKeyPair: cdKp);
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        using var auth = new TestableClientWebAuthContract(
            AuthEndpoint, TestChallengeBuilder.DefaultWebAuthContractId,
            Network.Test(), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultHomeDomain, SorobanRpcUrl,
            httpClient: new HttpClient(), latestLedgerOverride: 500);

        var signed = await auth.SignAuthorizationEntriesAsync(xdr, new[] { clientKp }, cdKp);
        var entries = TestChallengeBuilder.DecodeEntries(signed);
        Assert.IsTrue(entries[2].Credentials.Address.Signature.Vec!.InnerValue.Length > 0);
    }

    [TestMethod]
    public async Task SignAuthorizationEntries_SignsClientDomain_WithRemoteDelegate()
    {
        var cdKp = KeyPair.Random();
        var clientKp = KeyPair.Random();
        var result = TestChallengeBuilder.Build(
            clientContractId: clientKp.AccountId,
            clientDomain: "c.example",
            clientDomainKeyPair: cdKp);
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        using var auth = new TestableClientWebAuthContract(
            AuthEndpoint, TestChallengeBuilder.DefaultWebAuthContractId,
            Network.Test(), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultHomeDomain, SorobanRpcUrl,
            httpClient: new HttpClient(), latestLedgerOverride: 500);

        ClientDomainEntrySigningDelegate del = e =>
        {
            // Reuse TestChallengeBuilder's in-place signer to install a signature.
            TestChallengeBuilder.SignEntryInPlace(e, cdKp, Network.Test());
            return Task.FromResult(e);
        };

        var signed = await auth.SignAuthorizationEntriesAsync(
            xdr, new[] { clientKp }, clientDomainSigningDelegate: del);
        var entries = TestChallengeBuilder.DecodeEntries(signed);
        Assert.IsTrue(entries[2].Credentials.Address.Signature.Vec!.InnerValue.Length > 0);
    }

    [TestMethod]
    public async Task SendSignedChallenge_FormEncoded_ReturnsJwtOn200()
    {
        var serverKp = KeyPair.Random();
        string? capturedContentType = null;
        string? capturedBody = null;
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((r, _) =>
            {
                capturedContentType = r.Content!.Headers.ContentType!.MediaType;
                capturedBody = r.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"token\":\"abc.def.ghi\"}"),
            });
        using var client = new HttpClient(handler.Object);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, ContractId, Network.Test(), serverKp.AccountId,
            HomeDomain, SorobanRpcUrl, httpClient: client);

        var token = await auth.SendSignedChallengeAsync("aGVsbG8=", useFormUrlEncoded: true);

        Assert.AreEqual("abc.def.ghi", token);
        Assert.AreEqual("application/x-www-form-urlencoded", capturedContentType);
        StringAssert.Contains(capturedBody!, "authorization_entries=");
    }

    [TestMethod]
    public async Task SendSignedChallenge_Json_ReturnsJwtOn200()
    {
        var serverKp = KeyPair.Random();
        string? capturedContentType = null;
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((r, _) =>
            {
                capturedContentType = r.Content!.Headers.ContentType!.MediaType;
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"token\":\"t\"}"),
            });
        using var client = new HttpClient(handler.Object);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, ContractId, Network.Test(), serverKp.AccountId,
            HomeDomain, SorobanRpcUrl, httpClient: client);

        var token = await auth.SendSignedChallengeAsync("aGk=", useFormUrlEncoded: false);

        Assert.AreEqual("t", token);
        Assert.AreEqual("application/json", capturedContentType);
    }

    [TestMethod]
    [ExpectedException(typeof(SubmitSignedChallengeForContractsErrorResponseException))]
    public async Task SendSignedChallenge_Throws_On400WithError()
    {
        var serverKp = KeyPair.Random();
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"error\":\"bad sig\"}"),
            });
        using var client = new HttpClient(handler.Object);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, ContractId, Network.Test(), serverKp.AccountId,
            HomeDomain, SorobanRpcUrl, httpClient: client);
        await auth.SendSignedChallengeAsync("x");
    }

    [TestMethod]
    [ExpectedException(typeof(SubmitSignedChallengeForContractsTimeoutResponseException))]
    public async Task SendSignedChallenge_Throws_On504()
    {
        var serverKp = KeyPair.Random();
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.GatewayTimeout)
            {
                Content = new StringContent(""),
            });
        using var client = new HttpClient(handler.Object);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, ContractId, Network.Test(), serverKp.AccountId,
            HomeDomain, SorobanRpcUrl, httpClient: client);
        await auth.SendSignedChallengeAsync("x");
    }

    [TestMethod]
    [ExpectedException(typeof(SubmitSignedChallengeForContractsUnknownResponseException))]
    public async Task SendSignedChallenge_Throws_OnOtherStatus()
    {
        var serverKp = KeyPair.Random();
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("oops"),
            });
        using var client = new HttpClient(handler.Object);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, ContractId, Network.Test(), serverKp.AccountId,
            HomeDomain, SorobanRpcUrl, httpClient: client);
        await auth.SendSignedChallengeAsync("x");
    }

    private static Mock<HttpMessageHandler> BuildTomlHandler(string tomlBody)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.EndsWith("stellar.toml")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(tomlBody, Encoding.UTF8, "text/plain"),
            });
        return handler;
    }

    [TestMethod]
    public async Task JwtTokenAsync_FullFlow_ReturnsToken()
    {
        var signer = KeyPair.Random();
        var result = TestChallengeBuilder.Build(clientContractId: signer.AccountId);
        var challengeXdr = TestChallengeBuilder.EncodeEntries(result.Entries);

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"authorization_entries\":\"" + challengeXdr + "\"}"),
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"token\":\"final.jwt.token\"}"),
            });

        using var client = new HttpClient(handler.Object);
        using var auth = new TestableClientWebAuthContract(
            AuthEndpoint, TestChallengeBuilder.DefaultWebAuthContractId,
            Network.Test(), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultHomeDomain, SorobanRpcUrl,
            httpClient: client, latestLedgerOverride: 500);

        var jwt = await auth.JwtTokenAsync(signer.AccountId, new[] { signer });
        Assert.AreEqual("final.jwt.token", jwt);
    }

    [TestMethod]
    [ExpectedException(typeof(MissingClientDomainException))]
    public async Task JwtTokenAsync_Throws_WhenDelegateWithoutClientDomain()
    {
        var signer = KeyPair.Random();
        var serverKp = KeyPair.Random();
        using var auth = new TestableClientWebAuthContract(
            AuthEndpoint, ContractId, Network.Test(), serverKp.AccountId,
            HomeDomain, SorobanRpcUrl, httpClient: new HttpClient(), latestLedgerOverride: 500);

        ClientDomainEntrySigningDelegate del = e => Task.FromResult(e);
        await auth.JwtTokenAsync(signer.AccountId, new[] { signer },
            clientDomainSigningDelegate: del);
    }
}

internal sealed class TestableClientWebAuthContract : ClientWebAuthContract
{
    private readonly uint _override;
    public TestableClientWebAuthContract(
        string authEndpoint, string webAuthContractId, Network network,
        string serverSigningKey, string serverHomeDomain, string sorobanRpcUrl,
        HttpClient httpClient, uint latestLedgerOverride)
        : base(authEndpoint, webAuthContractId, network, serverSigningKey, serverHomeDomain,
            sorobanRpcUrl, httpClient)
    {
        _override = latestLedgerOverride;
    }

    internal override Task<uint> GetLatestLedgerSequenceAsync() => Task.FromResult(_override);
}
