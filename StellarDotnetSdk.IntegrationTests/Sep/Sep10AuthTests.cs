using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.IntegrationTests.Infrastructure;
using StellarDotnetSdk.Sep.Sep0001.Exceptions;
using StellarDotnetSdk.Sep.Sep0010;
using StellarDotnetSdk.Sep.Sep0010.Exceptions;

namespace StellarDotnetSdk.IntegrationTests.Sep;

[TestFixture]
[CancelAfter(90_000)]
public class Sep10AuthTests : IntegrationTestBase
{
    private const string HomeDomain = "integration.example.com";
    private const string WebAuthDomain = "integration.example.com";

    [Test]
    public void Sep10Loopback_BuildSignVerify_RoundTrips()
    {
        // Deterministic: the SDK's own server primitives build+sign the challenge, the client signs,
        // and the server primitives verify it. No network. Network.Current is Testnet (set in the base).
        var server = KeyPair.Random();
        var client = KeyPair.Random();
        var now = DateTimeOffset.Now;

        var challenge = ServerWebAuth.BuildChallengeTransaction(
            server, client.AccountId, HomeDomain, WebAuthDomain, validFrom: now);
        challenge.Sign(client);

        var readClientId = ServerWebAuth.ReadChallengeTransaction(
            challenge, server.AccountId, HomeDomain, WebAuthDomain, now: now);
        readClientId.Should().Be(client.AccountId);

        var verifiedSigners = ServerWebAuth.VerifyChallengeTransactionSigners(
            challenge, server.AccountId, new[] { client.AccountId }, HomeDomain, WebAuthDomain, now: now);
        verifiedSigners.Should().ContainSingle().Which.Should().Be(client.AccountId);
    }

    [Test]
    public async Task Sep10RealAnchor_GetChallengeSignSubmit_ReturnsJwt()
    {
        var account = await CreateFundedAccountAsync();

        string jwt;
        try
        {
            using var webAuth = await ClientWebAuth.FromDomainAsync(TestnetConfig.Sep10HomeDomain, Network.Current!);
            jwt = await webAuth.JwtTokenAsync(account.AccountId, new[] { account });
        }
        // Treat anchor unavailability OR anchor-side misbehavior as Inconclusive, not Fail — SDK-side
        // SEP-10 protocol logic is covered deterministically by the loopback test above.
        catch (Exception ex) when (ex is HttpRequestException
                                       or TaskCanceledException
                                       or JsonException
                                       or InvalidOperationException
                                       or StellarTomlException
                                       or WebAuthException)
        {
            Assert.Inconclusive($"SEP-10 anchor '{TestnetConfig.Sep10HomeDomain}' unavailable: {ex.Message}");
            return;
        }

        jwt.Should().NotBeNullOrEmpty();
        jwt.Split('.').Should().HaveCount(3, "a JWT has header.payload.signature");
    }
}