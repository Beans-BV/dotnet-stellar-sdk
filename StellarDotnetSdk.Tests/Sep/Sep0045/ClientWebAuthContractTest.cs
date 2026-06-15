using System.Collections.Generic;
using System.Linq;
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
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;

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
    public async Task GetChallengeAsync_AcceptsCamelCaseResponseFields()
    {
        // The SEP-45 spec uses snake_case, but the Flutter peer SDK also accepts camelCase. Match it
        // so a camelCase-emitting server interoperates instead of failing with a misleading
        // "missing authorization_entries" error.
        var serverKp = KeyPair.Random();
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"authorizationEntries\":\"aGk=\",\"networkPassphrase\":\"Test SDF Network ; September 2015\"}"),
            });
        using var client = new HttpClient(handler.Object);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, ContractId, Network.Test(), serverKp.AccountId,
            HomeDomain, SorobanRpcUrl, httpClient: client);

        var result = await auth.GetChallengeAsync(TestChallengeBuilder.DefaultWebAuthContractId);

        Assert.AreEqual("aGk=", result.AuthorizationEntries);
        Assert.AreEqual("Test SDF Network ; September 2015", result.NetworkPassphrase);
    }

    [TestMethod]
    public async Task GetChallengeAsync_WrapsMalformedJson_NotRawException()
    {
        // Boundary guard for the custom converter: a 200 with a non-JSON body must still surface as the
        // documented exception (via the JsonException catch), never a raw parser exception.
        var serverKp = KeyPair.Random();
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("not json {{{"),
            });
        using var client = new HttpClient(handler.Object);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, ContractId, Network.Test(), serverKp.AccountId,
            HomeDomain, SorobanRpcUrl, httpClient: client);

        await Assert.ThrowsExceptionAsync<ChallengeForContractsRequestErrorException>(() =>
            auth.GetChallengeAsync(TestChallengeBuilder.DefaultWebAuthContractId));
    }

    [TestMethod]
    public async Task GetChallengeAsync_RejectsDuplicateAuthorizationEntries()
    {
        // The SDK rejects duplicate JSON properties (JsonOptions.AllowDuplicateProperties = false) so an
        // adversarial server cannot silently override the signed authorization_entries blob. The custom
        // converter must preserve that guard rather than silently taking last-wins.
        var serverKp = KeyPair.Random();
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"authorization_entries\":\"aGk=\",\"authorization_entries\":\"ZXZpbA==\"}"),
            });
        using var client = new HttpClient(handler.Object);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, ContractId, Network.Test(), serverKp.AccountId,
            HomeDomain, SorobanRpcUrl, httpClient: client);

        await Assert.ThrowsExceptionAsync<ChallengeForContractsRequestErrorException>(() =>
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
            webAuthDomain: TestChallengeBuilder.DefaultWebAuthDomain,
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
            webAuthDomain: TestChallengeBuilder.DefaultWebAuthDomain,
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
            webAuthDomain: TestChallengeBuilder.DefaultWebAuthDomain,
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
            webAuthDomain: TestChallengeBuilder.DefaultWebAuthDomain,
            httpClient: new HttpClient());
        // Caller passes null (no client domain expected) but challenge contains one → must throw
        auth.ValidateChallenge(xdr, result.ClientContractId);
    }

    [TestMethod]
    public async Task SignAuthorizationEntries_SignsClientContractEntry_Verifiably()
    {
        // Realistic SEP-45: the client account is a C... contract; the wallet signs its entry
        // with a G... ed25519 keypair that is a signer of that contract.
        var signer = KeyPair.Random();
        var result = TestChallengeBuilder.Build(); // default: random C... client account
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);

        using var auth = new TestableClientWebAuthContract(
            AuthEndpoint, TestChallengeBuilder.DefaultWebAuthContractId,
            Network.Test(), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultHomeDomain, SorobanRpcUrl,
            httpClient: new HttpClient(), latestLedgerOverride: 500);

        var signed = await auth.SignAuthorizationEntriesAsync(xdr, result.ClientContractId, new[] { signer });
        var entries = TestChallengeBuilder.DecodeEntries(signed);
        var clientEntry = entries[1]; // order: [server, client, (client-domain)]

        // Expiration stamped to latestLedger (500) + DefaultSignatureExpirationLedgers (10).
        Assert.AreEqual(510u, clientEntry.Credentials.Address.SignatureExpirationLedger.InnerValue);

        var sigs = ReadSignatures(clientEntry);
        Assert.AreEqual(1, sigs.Length);
        CollectionAssert.AreEqual(signer.PublicKey, sigs[0].PublicKey);
        // Signature must verify against the hash of the FINAL stamped entry (catches sign-before-stamp).
        var hash = Sep45Challenge.ComputeAuthorizationHash(clientEntry, Network.Test());
        Assert.IsTrue(signer.Verify(hash, sigs[0].Signature));

        // The server entry must be left untouched.
        Sep45Challenge.VerifyServerSignature(entries[0], result.ServerKeyPair.AccountId, Network.Test());
    }

    [TestMethod]
    public async Task SignAuthorizationEntries_Multisig_AppendsVerifiableSignatures()
    {
        // A contract account requiring M-of-N: every signer must produce a distinct, verifiable map.
        var s1 = KeyPair.Random();
        var s2 = KeyPair.Random();
        var result = TestChallengeBuilder.Build();
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        using var auth = new TestableClientWebAuthContract(
            AuthEndpoint, TestChallengeBuilder.DefaultWebAuthContractId,
            Network.Test(), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultHomeDomain, SorobanRpcUrl,
            httpClient: new HttpClient(), latestLedgerOverride: 500);

        var signed = await auth.SignAuthorizationEntriesAsync(xdr, result.ClientContractId, new[] { s1, s2 });
        var clientEntry = TestChallengeBuilder.DecodeEntries(signed)[1];
        var sigs = ReadSignatures(clientEntry);

        Assert.AreEqual(2, sigs.Length);
        var hash = Sep45Challenge.ComputeAuthorizationHash(clientEntry, Network.Test());
        foreach (var kp in new[] { s1, s2 })
        {
            var match = sigs.FirstOrDefault(x => x.PublicKey.SequenceEqual(kp.PublicKey));
            Assert.IsNotNull(match.Signature, $"no signature found for {kp.AccountId}");
            Assert.IsTrue(kp.Verify(hash, match.Signature), $"signature does not verify for {kp.AccountId}");
        }
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidArgumentsException))]
    public async Task SignAuthorizationEntries_Throws_WhenEntryHasNoSigner()
    {
        // Challenge carries a client-domain entry, but the caller supplies no way to sign it.
        // The old code silently skipped it and let the server reject the half-signed challenge.
        var cdKp = KeyPair.Random();
        var result = TestChallengeBuilder.Build(clientDomain: "c.example", clientDomainKeyPair: cdKp);
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        using var auth = new TestableClientWebAuthContract(
            AuthEndpoint, TestChallengeBuilder.DefaultWebAuthContractId,
            Network.Test(), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultHomeDomain, SorobanRpcUrl,
            httpClient: new HttpClient(), latestLedgerOverride: 500);

        await auth.SignAuthorizationEntriesAsync(xdr, result.ClientContractId, new[] { KeyPair.Random() });
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidArgumentsException))]
    public async Task SignAuthorizationEntries_Throws_WhenDelegateSuppliedWithoutAccountId()
    {
        // A remote client-domain delegate is useless without clientDomainAccountId (which locates the
        // entry to hand it) and with no local keypair fallback. The paired-null guard must reject this
        // up front rather than silently leaving the client-domain entry unsigned.
        var cdKp = KeyPair.Random();
        var result = TestChallengeBuilder.Build(clientDomain: "c.example", clientDomainKeyPair: cdKp);
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        using var auth = new TestableClientWebAuthContract(
            AuthEndpoint, TestChallengeBuilder.DefaultWebAuthContractId,
            Network.Test(), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultHomeDomain, SorobanRpcUrl,
            httpClient: new HttpClient(), latestLedgerOverride: 500);

        await auth.SignAuthorizationEntriesAsync(
            xdr, result.ClientContractId, new[] { KeyPair.Random() },
            clientDomainSigningDelegate: e => Task.FromResult(e)); // clientDomainAccountId omitted
    }

    [TestMethod]
    public async Task SignAuthorizationEntries_SignsClientDomain_WithLocalKeyPair()
    {
        var clientSigner = KeyPair.Random();
        var cdKp = KeyPair.Random();
        var result = TestChallengeBuilder.Build(clientDomain: "c.example", clientDomainKeyPair: cdKp);
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        using var auth = new TestableClientWebAuthContract(
            AuthEndpoint, TestChallengeBuilder.DefaultWebAuthContractId,
            Network.Test(), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultHomeDomain, SorobanRpcUrl,
            httpClient: new HttpClient(), latestLedgerOverride: 500);

        var signed = await auth.SignAuthorizationEntriesAsync(
            xdr, result.ClientContractId, new[] { clientSigner }, cdKp);
        var entries = TestChallengeBuilder.DecodeEntries(signed);

        // Client (contract) entry signed by clientSigner.
        var clientSigs = ReadSignatures(entries[1]);
        Assert.AreEqual(1, clientSigs.Length);
        CollectionAssert.AreEqual(clientSigner.PublicKey, clientSigs[0].PublicKey);

        // Client-domain entry (index 2) signed by cdKp, verifiably.
        var cdEntry = entries[2];
        var cdSigs = ReadSignatures(cdEntry);
        Assert.AreEqual(1, cdSigs.Length);
        CollectionAssert.AreEqual(cdKp.PublicKey, cdSigs[0].PublicKey);
        var cdHash = Sep45Challenge.ComputeAuthorizationHash(cdEntry, Network.Test());
        Assert.IsTrue(cdKp.Verify(cdHash, cdSigs[0].Signature));
    }

    [TestMethod]
    public async Task SignAuthorizationEntries_SignsClientDomain_WithRemoteDelegate()
    {
        var clientSigner = KeyPair.Random();
        var cdKp = KeyPair.Random();
        var result = TestChallengeBuilder.Build(clientDomain: "c.example", clientDomainKeyPair: cdKp);
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        using var auth = new TestableClientWebAuthContract(
            AuthEndpoint, TestChallengeBuilder.DefaultWebAuthContractId,
            Network.Test(), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultHomeDomain, SorobanRpcUrl,
            httpClient: new HttpClient(), latestLedgerOverride: 500);

        var delegated = new List<string>();
        ClientDomainEntrySigningDelegate del = e =>
        {
            delegated.Add(CredentialsAddress(e));
            TestChallengeBuilder.SignEntryInPlace(e, cdKp, Network.Test());
            return Task.FromResult(e);
        };

        var signed = await auth.SignAuthorizationEntriesAsync(
            xdr, result.ClientContractId, new[] { clientSigner },
            clientDomainSigningDelegate: del, clientDomainAccountId: cdKp.AccountId);

        // The delegate must be invoked exactly once, and only for the client-domain entry —
        // never for the client contract entry (the old exclusion-based routing did the latter).
        Assert.AreEqual(1, delegated.Count);
        Assert.AreEqual(cdKp.AccountId, delegated[0]);
        var entries = TestChallengeBuilder.DecodeEntries(signed);
        Assert.AreEqual(1, ReadSignatures(entries[2]).Length);
        // Client entry still signed locally by clientSigner.
        Assert.AreEqual(1, ReadSignatures(entries[1]).Length);
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

    [TestMethod]
    public void ValidateChallenge_Passes_WhenHomeDomainOverrideMatches()
    {
        // Auth server serves multiple home domains; the challenge carries a non-default one.
        var result = TestChallengeBuilder.Build(homeDomain: "other.example");
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, TestChallengeBuilder.DefaultWebAuthContractId,
            Network.Test(), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultHomeDomain, SorobanRpcUrl,
            webAuthDomain: TestChallengeBuilder.DefaultWebAuthDomain,
            httpClient: new HttpClient());

        auth.ValidateChallenge(xdr, result.ClientContractId, null, homeDomain: "other.example");
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidHomeDomainException))]
    public void ValidateChallenge_Throws_WhenHomeDomainNotDefaultAndNoOverride()
    {
        var result = TestChallengeBuilder.Build(homeDomain: "other.example");
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, TestChallengeBuilder.DefaultWebAuthContractId,
            Network.Test(), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultHomeDomain, SorobanRpcUrl,
            webAuthDomain: TestChallengeBuilder.DefaultWebAuthDomain,
            httpClient: new HttpClient());

        auth.ValidateChallenge(xdr, result.ClientContractId); // no override → "other.example" not allowed
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidWebAuthDomainException))]
    public void ValidateChallenge_Throws_WhenWebAuthDomainDoesNotMatchConfigured()
    {
        // Proves web_auth_domain is validated against the configured value (not the endpoint host
        // and not the home domain): the challenge's web_auth_domain differs from the configured one.
        var result = TestChallengeBuilder.Build();
        var xdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, TestChallengeBuilder.DefaultWebAuthContractId,
            Network.Test(), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultHomeDomain, SorobanRpcUrl,
            webAuthDomain: "different.example",
            httpClient: new HttpClient());

        auth.ValidateChallenge(xdr, result.ClientContractId);
    }

    [TestMethod]
    public async Task GetChallengeAsync_WrapsTimeout_WithInnerException()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("timed out"));
        using var client = new HttpClient(handler.Object);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, ContractId, Network.Test(), KeyPair.Random().AccountId,
            HomeDomain, SorobanRpcUrl, httpClient: client);

        var ex = await Assert.ThrowsExceptionAsync<ChallengeForContractsRequestErrorException>(() =>
            auth.GetChallengeAsync(TestChallengeBuilder.DefaultWebAuthContractId));
        Assert.IsInstanceOfType(ex.InnerException, typeof(TaskCanceledException));
        Assert.AreEqual(0, ex.StatusCode);
    }

    [TestMethod]
    public async Task GetChallengeAsync_PreservesInnerException_OnTransportFailure()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("connection refused"));
        using var client = new HttpClient(handler.Object);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, ContractId, Network.Test(), KeyPair.Random().AccountId,
            HomeDomain, SorobanRpcUrl, httpClient: client);

        var ex = await Assert.ThrowsExceptionAsync<ChallengeForContractsRequestErrorException>(() =>
            auth.GetChallengeAsync(TestChallengeBuilder.DefaultWebAuthContractId));
        Assert.IsInstanceOfType(ex.InnerException, typeof(HttpRequestException));
    }

    [TestMethod]
    public async Task SendSignedChallengeAsync_WrapsClientTimeout_AsTimeoutException()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("timed out"));
        using var client = new HttpClient(handler.Object);
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, ContractId, Network.Test(), KeyPair.Random().AccountId,
            HomeDomain, SorobanRpcUrl, httpClient: client);

        var ex = await Assert.ThrowsExceptionAsync<SubmitSignedChallengeForContractsTimeoutResponseException>(
            () => auth.SendSignedChallengeAsync("aGk="));
        Assert.IsInstanceOfType(ex.InnerException, typeof(TaskCanceledException));
    }

    [TestMethod]
    public async Task JwtTokenAsync_DoesNotLeakAuthHeaders_ToClientDomainToml()
    {
        // The auth server's request headers (which may carry credentials) must not be forwarded to
        // the client domain — a different origin — when resolving its stellar.toml SIGNING_KEY.
        var cdKp = KeyPair.Random();
        HttpRequestMessage? tomlRequest = null;
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.EndsWith("stellar.toml")),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((r, _) => tomlRequest = r)
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"SIGNING_KEY=\"{cdKp.AccountId}\"\n", Encoding.UTF8, "text/plain"),
            });
        // Any non-toml request (the challenge GET) fails fast — we only care that the toml fetch above
        // already happened without the auth header.
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => !r.RequestUri!.AbsolutePath.EndsWith("stellar.toml")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("stop"),
            });

        using var client = new HttpClient(handler.Object);
        var headers = new Dictionary<string, string> { ["Authorization"] = "Bearer super-secret" };
        using var auth = new ClientWebAuthContract(
            AuthEndpoint, ContractId, Network.Test(), KeyPair.Random().AccountId,
            HomeDomain, SorobanRpcUrl, httpClient: client, httpRequestHeaders: headers);

        await Assert.ThrowsExceptionAsync<ChallengeForContractsRequestErrorException>(() =>
            auth.JwtTokenAsync(
                TestChallengeBuilder.DefaultWebAuthContractId, new[] { KeyPair.Random() },
                clientDomain: "wallet.example",
                clientDomainSigningDelegate: e => Task.FromResult(e)));

        Assert.IsNotNull(tomlRequest);
        Assert.IsFalse(tomlRequest!.Headers.Contains("Authorization"),
            "auth-server Authorization header must not be forwarded to the client-domain toml fetch");
    }

    /// <summary>Extracts the (public_key, signature) byte pairs from an entry's SCV_VEC signature.</summary>
    private static (byte[] PublicKey, byte[] Signature)[] ReadSignatures(SorobanAuthorizationEntry entry)
    {
        var vec = entry.Credentials.Address.Signature.Vec!.InnerValue;
        var result = new (byte[] PublicKey, byte[] Signature)[vec.Length];
        for (var i = 0; i < vec.Length; i++)
        {
            byte[]? pub = null, sig = null;
            foreach (var kv in vec[i].Map!.InnerValue)
            {
                var name = kv.Key.Sym.InnerValue;
                if (name == "public_key") pub = kv.Val.Bytes.InnerValue;
                else if (name == "signature") sig = kv.Val.Bytes.InnerValue;
            }
            result[i] = (pub!, sig!);
        }
        return result;
    }

    /// <summary>Returns the strkey of an entry's credentials address (mirrors the production helper).</summary>
    private static string CredentialsAddress(SorobanAuthorizationEntry entry)
    {
        return ScAddress.FromXdr(entry.Credentials.Address.Address) switch
        {
            ScAccountId acc => acc.InnerValue,
            ScContractId con => con.InnerValue,
            _ => "?",
        };
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
        var result = TestChallengeBuilder.Build(); // C... client account, signed by a G... signer
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
            httpClient: client, latestLedgerOverride: 500,
            webAuthDomain: TestChallengeBuilder.DefaultWebAuthDomain);

        var jwt = await auth.JwtTokenAsync(result.ClientContractId, new[] { signer });
        Assert.AreEqual("final.jwt.token", jwt);
    }

    [TestMethod]
    public async Task JwtTokenAsync_Throws_WhenResponseNetworkPassphraseMismatch()
    {
        // The server issued the challenge for a different network. Fail fast with a clear error rather
        // than letting it surface later as an opaque server-signature failure. Matches the Flutter peer.
        var signer = KeyPair.Random();
        var result = TestChallengeBuilder.Build(); // built for Network.Test()
        var challengeXdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        var handler = new Mock<HttpMessageHandler>();
        // Fresh response per call (a single shared instance would be disposed by the challenge GET,
        // masking the behaviour under test if the flow ever reaches a second request).
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"authorization_entries\":\"" + challengeXdr +
                    "\",\"network_passphrase\":\"Public Global Stellar Network ; September 2015\"}"),
            });
        using var client = new HttpClient(handler.Object);
        using var auth = new TestableClientWebAuthContract(
            AuthEndpoint, TestChallengeBuilder.DefaultWebAuthContractId,
            Network.Test(), result.ServerKeyPair.AccountId,
            TestChallengeBuilder.DefaultHomeDomain, SorobanRpcUrl,
            httpClient: client, latestLedgerOverride: 500,
            webAuthDomain: TestChallengeBuilder.DefaultWebAuthDomain);

        await Assert.ThrowsExceptionAsync<InvalidNetworkPassphraseException>(() =>
            auth.JwtTokenAsync(result.ClientContractId, new[] { signer }));
    }

    [TestMethod]
    public async Task JwtTokenAsync_Succeeds_WhenResponseNetworkPassphraseMatches()
    {
        // Boundary guard: a network_passphrase that IS present and matches must NOT be rejected
        // (the check validates only present-and-mismatched values).
        var signer = KeyPair.Random();
        var result = TestChallengeBuilder.Build();
        var challengeXdr = TestChallengeBuilder.EncodeEntries(result.Entries);
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"authorization_entries\":\"" + challengeXdr +
                    "\",\"network_passphrase\":\"Test SDF Network ; September 2015\"}"),
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
            httpClient: client, latestLedgerOverride: 500,
            webAuthDomain: TestChallengeBuilder.DefaultWebAuthDomain);

        var jwt = await auth.JwtTokenAsync(result.ClientContractId, new[] { signer });
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
        HttpClient httpClient, uint latestLedgerOverride, string? webAuthDomain = null)
        : base(authEndpoint, webAuthContractId, network, serverSigningKey, serverHomeDomain,
            sorobanRpcUrl, webAuthDomain: webAuthDomain, httpClient: httpClient)
    {
        _override = latestLedgerOverride;
    }

    internal override Task<uint> GetLatestLedgerSequenceAsync() => Task.FromResult(_override);
}
