using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly.Timeout;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Requests;
using StellarDotnetSdk.Sep.Sep0007;
using StellarDotnetSdk.Sep.Sep0007.Exceptions;
using StellarDotnetSdk.Tests.Sep.Sep0007.Fixtures;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Tests.Sep.Sep0007;

/// <summary>
///     The networked half of SEP-7: verifying <c>origin_domain</c> against the domain's stellar.toml, and handing a
///     signed transaction to the callback or to Horizon.
/// </summary>
[TestClass]
public class UriSchemeHttpTest
{
    private const int MaxResponseBodyBytes = 512 * 1024;

    // ---- origin_domain verification ----

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_SpecVector_ReturnsSigningKey()
    {
        var handler = TomlHandler($"URI_REQUEST_SIGNING_KEY=\"{Sep7TestVectors.SpecSigningAccountId}\"");
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var key = await uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri);

        Assert.AreEqual(Sep7TestVectors.SpecSigningAccountId, key);
        var request = handler.Requests.Single();
        Assert.AreEqual(HttpMethod.Get, request.Method);
        Assert.AreEqual("https://somedomain.com/.well-known/stellar.toml", request.RequestUri!.ToString());
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_TypescriptVector_Verifies()
    {
        var handler = TomlHandler($"URI_REQUEST_SIGNING_KEY=\"{Sep7TestVectors.TsSigningAccountId}\"");
        using var uriScheme = new UriScheme(new HttpClient(handler));

        Assert.AreEqual(Sep7TestVectors.TsSigningAccountId,
            await uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.TsSignedPayUri));
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_SignedWithOtherKey_ThrowsInvalidSignature()
    {
        // Soneso "signature mismatch" toml: publishes a key other than the one that signed the URI.
        var handler = TomlHandler($"URI_REQUEST_SIGNING_KEY=\"{Sep7TestVectors.SonesoMismatchAccountId}\"");
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var ex = await Assert.ThrowsExceptionAsync<InvalidSep7SignatureException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));

        Assert.AreEqual("someDomain.com", ex.Domain);
        Assert.AreEqual(Sep7TestVectors.SonesoMismatchAccountId, ex.SigningKey);
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_SonesoSigner_RoundTrips()
    {
        using var signer = KeyPair.FromSecretSeed(Sep7TestVectors.SonesoSeed);
        Assert.AreEqual(Sep7TestVectors.SonesoAccountId, signer.AccountId);
        var uri = UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr,
            callback: "https://examplePost.com", originDomain: "place.domain.com");
        var signed = UriScheme.SignUri(uri, signer);
        var handler = TomlHandler($"URI_REQUEST_SIGNING_KEY=\"{Sep7TestVectors.SonesoAccountId}\"");
        using var uriScheme = new UriScheme(new HttpClient(handler));

        Assert.AreEqual(Sep7TestVectors.SonesoAccountId, await uriScheme.VerifyOriginDomainSignatureAsync(signed));
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_TamperedUri_ThrowsInvalidSignature()
    {
        var handler = TomlHandler($"URI_REQUEST_SIGNING_KEY=\"{Sep7TestVectors.SpecSigningAccountId}\"");
        using var uriScheme = new UriScheme(new HttpClient(handler));
        var tampered = Sep7TestVectors.SpecSignedPayUri.Replace(
            "GCALNQQBXAPZ2WIRSDDBMSTAKCUH5SG6U76YBFLQLIXJTF7FE5AX7AOO", Sep7TestVectors.SonesoAccountId);

        await Assert.ThrowsExceptionAsync<InvalidSep7SignatureException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(tampered));
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_NoSigningKeyInToml_Throws()
    {
        var handler = TomlHandler("SIGNING_KEY=\"GCALNQQBXAPZ2WIRSDDBMSTAKCUH5SG6U76YBFLQLIXJTF7FE5AX7AOO\"");
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var ex = await Assert.ThrowsExceptionAsync<NoUriRequestSigningKeyFoundException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));
        Assert.AreEqual("someDomain.com", ex.Domain);
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_InvalidSigningKeyInToml_Throws()
    {
        var handler = TomlHandler("URI_REQUEST_SIGNING_KEY=\"GABC\"");
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var ex = await Assert.ThrowsExceptionAsync<InvalidUriRequestSigningKeyException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));
        Assert.AreEqual("GABC", ex.SigningKey);
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_PinnedKeyMatches_Verifies()
    {
        var handler = TomlHandler($"URI_REQUEST_SIGNING_KEY=\"{Sep7TestVectors.SpecSigningAccountId}\"");
        using var uriScheme = new UriScheme(new HttpClient(handler));

        Assert.AreEqual(Sep7TestVectors.SpecSigningAccountId,
            await uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri,
                Sep7TestVectors.SpecSigningAccountId));
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_PinnedKeyChanged_Throws()
    {
        // The toml key changed since the wallet pinned it; SEP-7 requires alerting the user.
        var handler = TomlHandler($"URI_REQUEST_SIGNING_KEY=\"{Sep7TestVectors.SpecSigningAccountId}\"");
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var ex = await Assert.ThrowsExceptionAsync<UriRequestSigningKeyChangedException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri,
                Sep7TestVectors.SonesoMismatchAccountId));

        Assert.AreEqual(Sep7TestVectors.SonesoMismatchAccountId, ex.PinnedSigningKey);
        Assert.AreEqual(Sep7TestVectors.SpecSigningAccountId, ex.CurrentSigningKey);
        Assert.AreEqual("someDomain.com", ex.Domain);
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_NoOriginDomain_ThrowsWithoutFetching()
    {
        var handler = TomlHandler("");
        using var uriScheme = new UriScheme(new HttpClient(handler));

        await Assert.ThrowsExceptionAsync<MissingOriginDomainException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecPayExample1));
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_NoSignature_ThrowsWithoutFetching()
    {
        var handler = TomlHandler("");
        using var uriScheme = new UriScheme(new HttpClient(handler));

        await Assert.ThrowsExceptionAsync<MissingSignatureException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecUnsignedPayUri));
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_OriginDomainNotFqdn_ThrowsWithoutFetching()
    {
        var handler = TomlHandler("");
        using var uriScheme = new UriScheme(new HttpClient(handler));
        var uri = Sep7TestVectors.SpecSignedPayUri.Replace("origin_domain=someDomain.com",
            "origin_domain=evil.com%2F%40someDomain.com");

        await Assert.ThrowsExceptionAsync<InvalidOriginDomainException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(uri));
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_InvalidUri_Throws()
    {
        using var uriScheme = new UriScheme(new HttpClient(TomlHandler("")));

        await Assert.ThrowsExceptionAsync<InvalidSep7UriException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync("web+stellar:pay"));
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_TomlNotFound_Throws()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var ex = await Assert.ThrowsExceptionAsync<OriginDomainStellarTomlException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));

        Assert.AreEqual("someDomain.com", ex.Domain);
        StringAssert.Contains(ex.Message, "404");
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_NetworkFailure_WrapsException()
    {
        var handler = new RecordingHandler(_ => throw new HttpRequestException("connection refused"));
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var ex = await Assert.ThrowsExceptionAsync<OriginDomainStellarTomlException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));

        Assert.IsInstanceOfType(ex.InnerException, typeof(HttpRequestException));
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_UnparseableToml_Throws()
    {
        var handler = TomlHandler("URI_REQUEST_SIGNING_KEY = = [[[ not toml");
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var ex = await Assert.ThrowsExceptionAsync<OriginDomainStellarTomlException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));

        StringAssert.Contains(ex.Message, "could not be parsed");
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_TomlOverDeclaredLimit_Throws()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(new string('#', MaxResponseBodyBytes + 1)),
        });
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var ex = await Assert.ThrowsExceptionAsync<OriginDomainStellarTomlException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));

        Assert.IsInstanceOfType(ex.InnerException, typeof(HttpRequestException));
        StringAssert.Contains(ex.InnerException!.Message, "limit");
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_TomlStreamedPastLimit_StopsReading()
    {
        // No Content-Length: the cap must hold while streaming, and the read must stop soon after it.
        var content = new EndlessContent();
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var ex = await Assert.ThrowsExceptionAsync<OriginDomainStellarTomlException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));

        Assert.IsInstanceOfType(ex.InnerException, typeof(HttpRequestException));
        Assert.IsTrue(content.BytesServed <= MaxResponseBodyBytes + 64 * 1024,
            $"read {content.BytesServed} bytes past a {MaxResponseBodyBytes}-byte cap");
    }

    [TestMethod]
    public async Task IsValidSignedUriAsync_ReportsValidAndReason()
    {
        var good = TomlHandler($"URI_REQUEST_SIGNING_KEY=\"{Sep7TestVectors.SpecSigningAccountId}\"");
        var bad = TomlHandler($"URI_REQUEST_SIGNING_KEY=\"{Sep7TestVectors.SonesoMismatchAccountId}\"");
        using var goodScheme = new UriScheme(new HttpClient(good));
        using var badScheme = new UriScheme(new HttpClient(bad));

        var valid = await goodScheme.IsValidSignedUriAsync(Sep7TestVectors.SpecSignedPayUri);
        var invalid = await badScheme.IsValidSignedUriAsync(Sep7TestVectors.SpecSignedPayUri);
        var unsigned = await goodScheme.IsValidSignedUriAsync(Sep7TestVectors.SpecUnsignedPayUri);
        var malformed = await goodScheme.IsValidSignedUriAsync("web+stellar:pay");

        Assert.IsTrue(valid.IsValid);
        Assert.IsFalse(invalid.IsValid);
        StringAssert.Contains(invalid.Reason, "does not verify");
        Assert.IsFalse(unsigned.IsValid);
        StringAssert.Contains(unsigned.Reason, "'signature'");
        Assert.IsFalse(malformed.IsValid);
    }

    // ---- callback and submission ----

    [TestMethod]
    public async Task SignAndSubmitTransactionAsync_WithCallback_PostsSignedEnvelopeAsForm()
    {
        using var signer = KeyPair.Random();
        var uri = UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr,
            callback: "https://cb.example.com/submit?id=42", publicKey: signer.AccountId,
            networkPassphrase: Network.TestnetPassphrase);
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("accepted"),
        });
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var result = await uriScheme.SignAndSubmitTransactionAsync(uri, signer);

        Assert.IsTrue(result.SubmittedToCallback);
        Assert.AreEqual(HttpStatusCode.OK, result.CallbackStatusCode);
        Assert.AreEqual("accepted", result.CallbackResponseBody);
        Assert.IsNull(result.HorizonResponse);

        var request = handler.Requests.Single();
        Assert.AreEqual(HttpMethod.Post, request.Method);
        Assert.AreEqual("https://cb.example.com/submit?id=42", request.RequestUri!.ToString());
        Assert.AreEqual("application/x-www-form-urlencoded", handler.ContentTypes.Single());

        var body = handler.Bodies.Single()!;
        StringAssert.StartsWith(body, "xdr=");
        var posted = TransactionBuilder.FromEnvelopeXdr(WebUtility.UrlDecode(body.Substring(4)));
        var signature = posted.Signatures.Single();
        Assert.IsTrue(signer.Verify(posted.Hash(Network.Test()), signature.Signature.InnerValue),
            "the transaction must be signed for the request's network_passphrase");
    }

    [TestMethod]
    public async Task SignAndSubmitTransactionAsync_WithoutCallback_SubmitsToHorizon()
    {
        using var signer = KeyPair.Random();
        var uri = UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr,
            networkPassphrase: Network.TestnetPassphrase);
        using var server = await Utils.CreateTestServerWithJson("Responses/serverSuccess.json");
        using var uriScheme = new UriScheme(new HttpClient(new RecordingHandler(_ =>
            throw new AssertFailedException("no callback request expected"))));

        var result = await uriScheme.SignAndSubmitTransactionAsync(uri, signer, server);

        Assert.IsFalse(result.SubmittedToCallback);
        Assert.IsNull(result.CallbackStatusCode);
        Assert.IsNull(result.CallbackResponseBody);
        Assert.IsNotNull(result.HorizonResponse);
        Assert.IsTrue(result.HorizonResponse!.IsSuccess);
    }

    [TestMethod]
    public async Task SignAndSubmitTransactionAsync_WithoutCallbackOrServer_Throws()
    {
        using var signer = KeyPair.Random();
        var uri = UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr);
        using var uriScheme = new UriScheme(new HttpClient(new RecordingHandler(_ => new HttpResponseMessage())));

        var ex = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            uriScheme.SignAndSubmitTransactionAsync(uri, signer));
        Assert.AreEqual("horizonServer", ex.ParamName);
    }

    [TestMethod]
    public async Task SignAndSubmitTransactionAsync_SignerNotPubkey_Throws()
    {
        using var signer = KeyPair.Random();
        using var uriScheme = new UriScheme(new HttpClient(new RecordingHandler(_ => new HttpResponseMessage())));

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            uriScheme.SignAndSubmitTransactionAsync(Sep7TestVectors.SpecTxExample1, signer));
        StringAssert.Contains(ex.Message, "GAU2ZSYYEYO5S5ZQSMMUENJ2TANY4FPXYGGIMU6GMGKTNVDG5QYFW6JS");
    }

    [TestMethod]
    public async Task SignAndSubmitTransactionAsync_WithReplace_Throws()
    {
        using var signer = KeyPair.Random();
        using var uriScheme = new UriScheme(new HttpClient(new RecordingHandler(_ => new HttpResponseMessage())));

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            uriScheme.SignAndSubmitTransactionAsync(Sep7TestVectors.SpecTxExample2, signer));
    }

    [TestMethod]
    public async Task SignAndSubmitTransactionAsync_PayRequest_Throws()
    {
        using var signer = KeyPair.Random();
        using var uriScheme = new UriScheme(new HttpClient(new RecordingHandler(_ => new HttpResponseMessage())));

        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            uriScheme.SignAndSubmitTransactionAsync(Sep7TestVectors.SpecPayExample1, signer));
    }

    [TestMethod]
    public async Task SubmitTransactionAsync_AfterApplyingReplacements_PostsToCallback()
    {
        // The replace flow: take the transaction, fill the fields in (here: re-sign as-is), then submit.
        using var signer = KeyPair.Random();
        var request = UriScheme.ParseUri(UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr,
            new[] { new Sep7Replacement("X", "sourceAccount", "source") }, "https://cb.example.com"));
        var transaction = request.GetTransaction();
        transaction.Sign(signer, request.GetNetwork());
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.Accepted));
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var result = await uriScheme.SubmitTransactionAsync(request, transaction);

        Assert.AreEqual(HttpStatusCode.Accepted, result.CallbackStatusCode);
        Assert.AreEqual("xdr=" + WebUtility.UrlEncode(transaction.ToEnvelopeXdrBase64()), handler.Bodies.Single());
    }

    [TestMethod]
    public async Task SubmitToCallbackAsync_ErrorStatus_IsReturnedNotThrown()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("bad tx"),
        });
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var result = await uriScheme.SubmitToCallbackAsync("url:https://cb.example.com", "AAAA");

        Assert.AreEqual(HttpStatusCode.BadRequest, result.CallbackStatusCode);
        Assert.AreEqual("bad tx", result.CallbackResponseBody);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("https://cb.example.com/", handler.Requests.Single().RequestUri!.ToString());
    }

    [TestMethod]
    [DataRow("http://cb.example.com")]
    [DataRow("url:http://cb.example.com")]
    [DataRow("ftp://cb.example.com")]
    [DataRow("cb.example.com/path")]
    public async Task SubmitToCallbackAsync_NonHttpsCallback_Throws(string callback)
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage());
        using var uriScheme = new UriScheme(new HttpClient(handler));

        await Assert.ThrowsExceptionAsync<ArgumentException>(() => uriScheme.SubmitToCallbackAsync(callback, "AAAA"));
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    [DataRow("https://user:pw@cb.example.com/x")]
    [DataRow("url:https://user@cb.example.com/x")]
    [DataRow("https://@cb.example.com/x")]
    [DataRow(" https://cb.example.com/x")]
    [DataRow("https://cb.example.com/x\n")]
    [DataRow("https://cb.example.com/x\u200B")]
    [DataRow("https:\\\\@cb.example.com/x")] // normalized to https://@cb.example.com/x
    [DataRow("https://cb.example.com/x#frag")]
    [DataRow("https://cb.example.com/x#")]
    [DataRow("https://cb.example.com/%ZZ")]
    [DataRow("https://cb.example.com/a%2")]
    [DataRow("https://cb.example.com/%2G")]
    [DataRow("https://cb.example.com/%G2")]
    [DataRow("https://cb.example.com/%41%ZZ")]
    public async Task SubmitToCallbackAsync_CallbackARequestCannotCarry_Throws(string callback)
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage());
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            uriScheme.SubmitToCallbackAsync(callback, "AAAA"));
        StringAssert.Contains(ex.Message, "must not contain");
        Assert.AreEqual(0, handler.Requests.Count);
        // One rule for both paths: the same callback makes a request invalid.
        var prefixed = callback.StartsWith(UriScheme.CallbackUrlPrefix) ? callback : UriScheme.CallbackUrlPrefix + callback;
        Assert.IsFalse(UriScheme.ValidateUri(Sep7TestVectors.SpecPayExample1 + "&callback=" +
                                             Uri.EscapeDataString(prefixed)).IsValid);
    }

    [TestMethod]
    public async Task SubmitToCallbackAsync_LoopbackHttp_IsAllowed()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var result = await uriScheme.SubmitToCallbackAsync("http://127.0.0.1:8000/cb", "AAAA");

        Assert.AreEqual(HttpStatusCode.OK, result.CallbackStatusCode);
    }

    [TestMethod]
    public async Task SubmitToCallbackAsync_ResponseOverLimit_Throws()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new EndlessContent(),
        });
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var ex = await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            uriScheme.SubmitToCallbackAsync("https://cb.example.com", "AAAA"));
        // The callback answered before the body ran over the cap, so it has the transaction.
        StringAssert.Contains(ex.Message, "after the callback answered HTTP 200");
    }

    [TestMethod]
    public void Dispose_LeavesCallerOwnedHttpClientUsable()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var client = new HttpClient(handler);

        new UriScheme(client).Dispose();
        new UriScheme().Dispose();

        using var response = client.GetAsync("https://example.com").GetAwaiter().GetResult();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    // ---- hostile stellar.toml (F1) ----

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_TomlThatOverflowsTheGeneralParser_StillVerifies()
    {
        // 20,000 nested arrays overflow the general stellar.toml parser's stack, which kills the process. The
        // origin domain is picked by whoever wrote the URI, so this must not reach that parser.
        var handler = TomlHandler($"URI_REQUEST_SIGNING_KEY=\"{Sep7TestVectors.SpecSigningAccountId}\"\n" +
                                  "X = " + new string('[', 20_000) + new string(']', 20_000));
        using var uriScheme = new UriScheme(new HttpClient(handler));

        Assert.AreEqual(Sep7TestVectors.SpecSigningAccountId,
            await uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));
    }

    // ---- stellar.toml redirects (F2) ----

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_TomlRedirects_AreFollowedOverHttps()
    {
        var handler = new RecordingHandler(request => request.RequestUri!.AbsoluteUri switch
        {
            "https://somedomain.com/.well-known/stellar.toml" => Redirect(HttpStatusCode.MovedPermanently,
                new Uri("/moved/stellar.toml", UriKind.Relative)),
            "https://somedomain.com/moved/stellar.toml" => Redirect(HttpStatusCode.Redirect,
                new Uri("https://www.somedomain.com/stellar.toml")),
            "https://www.somedomain.com/stellar.toml" => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"URI_REQUEST_SIGNING_KEY=\"{Sep7TestVectors.SpecSigningAccountId}\""),
            },
            _ => new HttpResponseMessage(HttpStatusCode.NotFound),
        });
        using var uriScheme = new UriScheme(new HttpClient(handler));

        Assert.AreEqual(Sep7TestVectors.SpecSigningAccountId,
            await uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));
        Assert.AreEqual(3, handler.Requests.Count);
    }

    [TestMethod]
    [DataRow(HttpStatusCode.SeeOther)]
    [DataRow(HttpStatusCode.PermanentRedirect)]
    public async Task VerifyOriginDomainSignatureAsync_TomlRedirect_AllRedirectStatusesAreFollowed(HttpStatusCode status)
    {
        var handler = new RecordingHandler(request => request.RequestUri!.AbsolutePath == "/moved"
            ? new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"URI_REQUEST_SIGNING_KEY=\"{Sep7TestVectors.SpecSigningAccountId}\""),
            }
            : Redirect(status, new Uri("/moved", UriKind.Relative)));
        using var uriScheme = new UriScheme(new HttpClient(handler));

        Assert.AreEqual(Sep7TestVectors.SpecSigningAccountId,
            await uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("//")]
    [DataRow("//:")]
    [DataRow("http:")]
    [DataRow("//host:99999/x")]
    public async Task VerifyOriginDomainSignatureAsync_TomlRedirectWithoutUsableLocation_IsATomlException(
        string? location)
    {
        var handler = new RecordingHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.Found);
            if (location != null)
            {
                response.Headers.TryAddWithoutValidation("Location", location);
            }
            return response;
        });
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var ex = await Assert.ThrowsExceptionAsync<OriginDomainStellarTomlException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));
        StringAssert.Contains(ex.Message, "Location");
        Assert.IsFalse((await uriScheme.IsValidSignedUriAsync(Sep7TestVectors.SpecSignedPayUri)).IsValid);
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_MalformedLocation_FollowingClient_IsATomlException()
    {
        // A client that follows redirects itself throws UriFormatException from SendAsync for such a Location.
        using var server = new LoopbackHttpServer((_, stream, cancellationToken) => LoopbackHttpServer.WriteAsync(
            stream, "HTTP/1.1 302 Found\r\nLocation: //:\r\nContent-Length: 0\r\nConnection: close\r\n\r\n",
            cancellationToken));
        using var uriScheme = new UriScheme(new HttpClient(new RedirectToLoopbackHandler(server.Port, true)));

        await Assert.ThrowsExceptionAsync<OriginDomainStellarTomlException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));
        Assert.IsFalse((await uriScheme.IsValidSignedUriAsync(Sep7TestVectors.SpecSignedPayUri)).IsValid);
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_TomlRedirectToHttp_Throws()
    {
        var handler = new RecordingHandler(_ => Redirect(HttpStatusCode.Found,
            new Uri("http://somedomain.com/.well-known/stellar.toml")));
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var ex = await Assert.ThrowsExceptionAsync<OriginDomainStellarTomlException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));
        StringAssert.Contains(ex.Message, "not https");
        Assert.AreEqual(1, handler.Requests.Count);
    }

    [TestMethod]
    [DataRow("https://127.0.0.1/.well-known/stellar.toml")]
    [DataRow("https://[::1]/.well-known/stellar.toml")]
    [DataRow("https://169.254.169.254/latest/meta-data/")]
    [DataRow("https://intranet/.well-known/stellar.toml")]
    [DataRow("https://www.somedomain.com:8443/.well-known/stellar.toml")]
    [DataRow("https://user:pw@www.somedomain.com/.well-known/stellar.toml")]
    [DataRow("https://@www.somedomain.com/.well-known/stellar.toml")]
    [DataRow("https://foo.localhost/.well-known/stellar.toml")] // RFC 6761: resolves to loopback
    [DataRow("https://www.somedomain.com\uFF0Fx/t")] // a host IDNA cannot map
    public async Task VerifyOriginDomainSignatureAsync_TomlRedirectBeyondADomainName_Throws(string location)
    {
        // The origin domain is attacker-chosen: a redirect must not reach what naming a domain could not.
        var handler = new RecordingHandler(_ => Redirect(HttpStatusCode.Found, new Uri(location)));
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var ex = await Assert.ThrowsExceptionAsync<OriginDomainStellarTomlException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));
        StringAssert.Contains(ex.Message, "not a fully qualified domain name");
        Assert.AreEqual(1, handler.Requests.Count);
    }

    [TestMethod]
    [DataRow("https://www.somedomain.com:443/stellar.toml")]
    [DataRow("https://bücher.example/stellar.toml")]
    public async Task VerifyOriginDomainSignatureAsync_TomlRedirectToADomainName_IsFollowed(string location)
    {
        var handler = new RecordingHandler(request => request.RequestUri!.AbsolutePath == "/.well-known/stellar.toml"
            ? Redirect(HttpStatusCode.Found, new Uri(location))
            : new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"URI_REQUEST_SIGNING_KEY=\"{Sep7TestVectors.SpecSigningAccountId}\""),
            });
        using var uriScheme = new UriScheme(new HttpClient(handler));

        Assert.AreEqual(Sep7TestVectors.SpecSigningAccountId,
            await uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));
        Assert.AreEqual(2, handler.Requests.Count);
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_TomlRedirectLoop_Throws()
    {
        var handler = new RecordingHandler(_ => Redirect(HttpStatusCode.TemporaryRedirect,
            new Uri("https://somedomain.com/.well-known/stellar.toml")));
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var ex = await Assert.ThrowsExceptionAsync<OriginDomainStellarTomlException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));
        StringAssert.Contains(ex.Message, "more than 5 times");
        Assert.AreEqual(6, handler.Requests.Count);
    }

    // ---- callback redirects (F2), against a real socket so the real handler's redirect logic runs ----

    [TestMethod]
    [DataRow(301)]
    [DataRow(302)]
    [DataRow(303)]
    [DataRow(307)]
    [DataRow(308)]
    public async Task SubmitToCallbackAsync_Redirect_IsReturnedNotFollowed(int status)
    {
        using var server = RedirectingCallbackServer(status);
        using var uriScheme = new UriScheme();

        var result = await uriScheme.SubmitToCallbackAsync($"url:http://127.0.0.1:{server.Port}/cb", "AAAA");

        Assert.AreEqual(status, (int)result.CallbackStatusCode!);
        CollectionAssert.AreEqual(new[] { "POST /cb HTTP/1.1 [xdr]" }, server.Requests.ToArray(),
            "the signed transaction must reach the callback once and nothing else");
    }

    [TestMethod]
    [DataRow(302)]
    [DataRow(307)]
    public async Task SubmitToCallbackAsync_ClientThatFollowsRedirects_Throws(int status)
    {
        using var server = RedirectingCallbackServer(status);
        using var client = new HttpClient(); // follows redirects, as a default HttpClient does
        using var uriScheme = new UriScheme(client);

        var ex = await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            uriScheme.SubmitToCallbackAsync($"http://127.0.0.1:{server.Port}/cb", "AAAA"));

        StringAssert.Contains(ex.Message, "redirect");
        Assert.AreEqual(2, server.Requests.Count, "the client did follow the redirect; it is reported, not hidden");
    }

    [TestMethod]
    public async Task SubmitToCallbackAsync_ClientThatFollowsARedirectToTheSameUrl_Throws()
    {
        // A 302 to itself turns the POST into a GET on the very same URL: only the method gives it away.
        using var server = new LoopbackHttpServer((requestLine, stream, cancellationToken) =>
            LoopbackHttpServer.WriteAsync(stream, requestLine.StartsWith("POST")
                    ? "HTTP/1.1 302 Found\r\nLocation: /cb\r\nContent-Length: 0\r\nConnection: close\r\n\r\n"
                    : "HTTP/1.1 200 OK\r\nContent-Length: 2\r\nConnection: close\r\n\r\nok",
                cancellationToken));
        using var client = new HttpClient();
        using var uriScheme = new UriScheme(client);

        var ex = await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            uriScheme.SubmitToCallbackAsync($"http://127.0.0.1:{server.Port}/cb", "AAAA"));

        StringAssert.Contains(ex.Message, "(GET)");
    }

    [TestMethod]
    public async Task SubmitToCallbackAsync_MalformedLocation_FollowingClient_IsAnHttpRequestException()
    {
        using var server = new LoopbackHttpServer((_, stream, cancellationToken) => LoopbackHttpServer.WriteAsync(
            stream, "HTTP/1.1 307 Temporary Redirect\r\nLocation: //:\r\nContent-Length: 0\r\nConnection: close\r\n\r\n",
            cancellationToken));
        using var client = new HttpClient();
        using var uriScheme = new UriScheme(client);

        var ex = await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            uriScheme.SubmitToCallbackAsync($"http://127.0.0.1:{server.Port}/cb", "AAAA"));
        StringAssert.Contains(ex.Message, "after the callback answered with a redirect");
    }

    // ---- origin_domain verification before signing (F4) ----

    [TestMethod]
    public async Task SignAndSubmitTransactionAsync_SignedOriginDomain_VerifiesBeforePosting()
    {
        var (uri, wallet) = SignedTxRequestWithCallback();
        var handler = TomlAndCallbackHandler(Sep7TestVectors.SpecSigningAccountId);
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var result = await uriScheme.SignAndSubmitTransactionAsync(uri, wallet,
            pinnedSigningKey: Sep7TestVectors.SpecSigningAccountId);

        Assert.AreEqual(HttpStatusCode.OK, result.CallbackStatusCode);
        Assert.AreEqual(2, handler.Requests.Count);
        Assert.AreEqual(HttpMethod.Get, handler.Requests[0].Method);
        Assert.AreEqual("/.well-known/stellar.toml", handler.Requests[0].RequestUri!.AbsolutePath);
        Assert.AreEqual(HttpMethod.Post, handler.Requests[1].Method);
    }

    [TestMethod]
    public async Task SignAndSubmitTransactionAsync_OriginDomainSignatureDoesNotVerify_SendsNothing()
    {
        var (uri, wallet) = SignedTxRequestWithCallback();
        var handler = TomlAndCallbackHandler(Sep7TestVectors.SonesoMismatchAccountId);
        using var uriScheme = new UriScheme(new HttpClient(handler));

        await Assert.ThrowsExceptionAsync<InvalidSep7SignatureException>(() =>
            uriScheme.SignAndSubmitTransactionAsync(uri, wallet));
        Assert.IsFalse(handler.Requests.Any(r => r.Method == HttpMethod.Post));
    }

    [TestMethod]
    public async Task SignAndSubmitTransactionAsync_OriginDomainKeyChanged_SendsNothing()
    {
        var (uri, wallet) = SignedTxRequestWithCallback();
        var handler = TomlAndCallbackHandler(Sep7TestVectors.SpecSigningAccountId);
        using var uriScheme = new UriScheme(new HttpClient(handler));

        await Assert.ThrowsExceptionAsync<UriRequestSigningKeyChangedException>(() =>
            uriScheme.SignAndSubmitTransactionAsync(uri, wallet,
                pinnedSigningKey: Sep7TestVectors.SonesoMismatchAccountId));
        Assert.IsFalse(handler.Requests.Any(r => r.Method == HttpMethod.Post));
    }

    [TestMethod]
    public async Task SignAndSubmitTransactionAsync_PinnedKeyWithoutOriginDomain_SendsNothing()
    {
        using var wallet = KeyPair.Random();
        var uri = UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr,
            callback: "https://cb.example.com/submit");
        var handler = TomlAndCallbackHandler(Sep7TestVectors.SpecSigningAccountId);
        using var uriScheme = new UriScheme(new HttpClient(handler));

        await Assert.ThrowsExceptionAsync<MissingOriginDomainException>(() =>
            uriScheme.SignAndSubmitTransactionAsync(uri, wallet,
                pinnedSigningKey: Sep7TestVectors.SpecSigningAccountId));
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_NullUri_Throws()
    {
        using var uriScheme = new UriScheme(new HttpClient(TomlHandler("")));

        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(null!));
    }

    [TestMethod]
    public async Task SignAndSubmitTransactionAsync_OriginDomainWithoutSignature_SendsNothing()
    {
        using var wallet = KeyPair.Random();
        var uri = UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr,
            callback: "https://cb.example.com/submit", originDomain: "someDomain.com");
        var handler = TomlAndCallbackHandler(Sep7TestVectors.SpecSigningAccountId);
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var ex = await Assert.ThrowsExceptionAsync<InvalidSep7UriException>(() =>
            uriScheme.SignAndSubmitTransactionAsync(uri, wallet));
        StringAssert.Contains(ex.Message, "'signature'");
        Assert.AreEqual(0, handler.Requests.Count);
    }

    // ---- time bound on the response body (F5), real sockets ----

    [TestMethod]
    public async Task SubmitToCallbackAsync_TricklingBody_TimesOutWithTheClientTimeout()
    {
        using var server = new LoopbackHttpServer(TrickleBody);
        using var client = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false })
        {
            Timeout = TimeSpan.FromSeconds(1),
        };
        using var uriScheme = new UriScheme(client);
        var stopwatch = Stopwatch.StartNew();

        var ex = await Assert.ThrowsExceptionAsync<TaskCanceledException>(() =>
            uriScheme.SubmitToCallbackAsync($"http://127.0.0.1:{server.Port}/cb", "AAAA"));

        Assert.IsInstanceOfType(ex.InnerException, typeof(TimeoutException));
        Assert.IsTrue(stopwatch.Elapsed < TimeSpan.FromSeconds(10), $"took {stopwatch.Elapsed}");
        StringAssert.Contains(ex.Message, "it had answered HTTP 200");
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_TricklingBody_TimesOutWithTheClientTimeout()
    {
        using var server = new LoopbackHttpServer(TrickleBody);
        using var client = new HttpClient(new RedirectToLoopbackHandler(server.Port))
        {
            Timeout = TimeSpan.FromSeconds(1),
        };
        using var uriScheme = new UriScheme(client);
        var stopwatch = Stopwatch.StartNew();

        var ex = await Assert.ThrowsExceptionAsync<TaskCanceledException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));

        Assert.IsInstanceOfType(ex.InnerException, typeof(TimeoutException));
        Assert.IsTrue(stopwatch.Elapsed < TimeSpan.FromSeconds(10), $"took {stopwatch.Elapsed}");
    }

    [TestMethod]
    public async Task SubmitToCallbackAsync_CallerCancellation_IsNotReportedAsTimeout()
    {
        using var server = new LoopbackHttpServer(TrickleBody);
        using var uriScheme = new UriScheme(new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false }));
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

        var ex = await Assert.ThrowsExceptionAsync<TaskCanceledException>(() =>
            uriScheme.SubmitToCallbackAsync($"http://127.0.0.1:{server.Port}/cb", "AAAA", cancellation.Token));

        Assert.IsNotInstanceOfType(ex.InnerException, typeof(TimeoutException));
    }

    // ---- transport failures stay inside the documented exception contract (F6), real sockets ----

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_ConnectionDroppedMidBody_IsATomlException()
    {
        using var server = new LoopbackHttpServer(TruncatedBody);
        using var uriScheme = new UriScheme(new HttpClient(new RedirectToLoopbackHandler(server.Port)));

        var ex = await Assert.ThrowsExceptionAsync<OriginDomainStellarTomlException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));
        Assert.IsInstanceOfType(ex.InnerException, typeof(IOException));

        var result = await uriScheme.IsValidSignedUriAsync(Sep7TestVectors.SpecSignedPayUri);
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task SubmitToCallbackAsync_ConnectionDroppedMidBody_IsAnHttpRequestException()
    {
        using var server = new LoopbackHttpServer(TruncatedBody);
        using var uriScheme = new UriScheme();

        var ex = await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            uriScheme.SubmitToCallbackAsync($"http://127.0.0.1:{server.Port}/cb", "AAAA"));
        Assert.IsInstanceOfType(ex.InnerException, typeof(IOException));
        // The callback had answered, so it has the transaction: the caller must be able to tell.
        StringAssert.Contains(ex.Message, "after the callback answered HTTP 200");
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_ResilienceTimeout_IsATomlException()
    {
        using var server = new LoopbackHttpServer(StallBeforeHeaders);
        using var client = new DefaultStellarSdkHttpClient(
            resilienceOptions: new HttpResilienceOptions { RequestTimeout = TimeSpan.FromMilliseconds(300) },
            innerHandler: new RedirectToLoopbackHandler(server.Port));
        using var uriScheme = new UriScheme(client);

        var ex = await Assert.ThrowsExceptionAsync<OriginDomainStellarTomlException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));
        Assert.IsInstanceOfType(ex.InnerException, typeof(TimeoutRejectedException));

        var result = await uriScheme.IsValidSignedUriAsync(Sep7TestVectors.SpecSignedPayUri);
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task SubmitToCallbackAsync_ResilienceTimeout_IsAnHttpRequestException()
    {
        using var server = new LoopbackHttpServer(StallBeforeHeaders);
        using var client = new DefaultStellarSdkHttpClient(
            resilienceOptions: new HttpResilienceOptions { RequestTimeout = TimeSpan.FromMilliseconds(300) },
            innerHandler: new SocketsHttpHandler { AllowAutoRedirect = false });
        using var uriScheme = new UriScheme(client);

        var ex = await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            uriScheme.SubmitToCallbackAsync($"http://127.0.0.1:{server.Port}/cb", "AAAA"));
        Assert.IsInstanceOfType(ex.InnerException, typeof(TimeoutRejectedException));
    }

    [TestMethod]
    public async Task IsValidSignedUriAsync_NullUri_IsInvalid()
    {
        using var uriScheme = new UriScheme(new HttpClient(TomlHandler("")));

        var result = await uriScheme.IsValidSignedUriAsync(null);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("The URI is null.", result.Reason);
    }

    // ---- minor-finding fixes ----

    [TestMethod]
    public async Task Submit_IsSuccess_ReflectsTheReceiversAnswer()
    {
        using var ok = new UriScheme(new HttpClient(new RecordingHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.Accepted))));
        Assert.IsTrue((await ok.SubmitToCallbackAsync("https://cb.example.com", "AAAA")).IsSuccess);

        using var signer = KeyPair.Random();
        var uri = UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr,
            networkPassphrase: Network.TestnetPassphrase);
        using var server = await Utils.CreateTestServerWithJson("Responses/serverSuccess.json");
        using var horizon = new UriScheme(new HttpClient(new RecordingHandler(_ => new HttpResponseMessage())));
        Assert.IsTrue((await horizon.SignAndSubmitTransactionAsync(uri, signer, server)).IsSuccess);
    }

    [TestMethod]
    public async Task SignAndSubmitTransactionAsync_SignerWithoutPrivateKey_SendsNothing()
    {
        var uri = UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr, callback: "https://cb.example.com");
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using var uriScheme = new UriScheme(new HttpClient(handler));

        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            uriScheme.SignAndSubmitTransactionAsync(uri, KeyPair.FromAccountId(Sep7TestVectors.SonesoAccountId)));
        Assert.AreEqual(0, handler.Requests.Count);
    }

    [TestMethod]
    public async Task SignAndSubmitTransactionAsync_NoNetworkPassphrase_SignsForThePublicNetwork()
    {
        using var signer = KeyPair.Random();
        var uri = UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr, callback: "https://cb.example.com");
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using var uriScheme = new UriScheme(new HttpClient(handler));

        await uriScheme.SignAndSubmitTransactionAsync(uri, signer);

        var posted = TransactionBuilder.FromEnvelopeXdr(WebUtility.UrlDecode(handler.Bodies.Single()!.Substring(4)));
        var signature = posted.Signatures.Single().Signature.InnerValue;
        Assert.IsTrue(signer.Verify(posted.Hash(Network.Public()), signature));
        Assert.IsFalse(signer.Verify(posted.Hash(Network.Test()), signature));
    }

    [TestMethod]
    public async Task SignAndSubmitTransactionAsync_CallbackTakesPrecedenceOverTheServer()
    {
        using var signer = KeyPair.Random();
        var uri = UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr, callback: "https://cb.example.com");
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using var server = await Utils.CreateTestServerWithJson("Responses/serverSuccess.json");
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var result = await uriScheme.SignAndSubmitTransactionAsync(uri, signer, server);

        Assert.IsTrue(result.SubmittedToCallback);
        Assert.IsNull(result.HorizonResponse);
        Assert.AreEqual(1, handler.Requests.Count);
    }

    [TestMethod]
    public async Task SubmitTransactionAsync_PayRequestWithCallback_PostsTheWalletsTransaction()
    {
        // SEP-7 defines callback for pay requests too: the wallet builds the payment and posts it.
        var request = UriScheme.ParseUri(Sep7TestVectors.SpecPayExample1 + "&callback=url%3Ahttps%3A%2F%2Fcb.example.com");
        using var signer = KeyPair.Random();
        var transaction = TransactionBuilder.FromEnvelopeXdr(Sep7TestVectors.SpecTxXdr);
        transaction.Sign(signer, Network.Public());
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var result = await uriScheme.SubmitTransactionAsync(request, transaction);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(HttpMethod.Post, handler.Requests.Single().Method);
    }

    [TestMethod]
    [DataRow("http://localhost:8000/cb")]
    [DataRow("http://[::1]:8000/cb")]
    public async Task SubmitToCallbackAsync_LoopbackHttpVariants_AreAllowed(string callback)
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using var uriScheme = new UriScheme(new HttpClient(handler));

        Assert.IsTrue((await uriScheme.SubmitToCallbackAsync(callback, "AAAA")).IsSuccess);
    }

    [TestMethod]
    public async Task SubmitToCallbackAsync_HttpToALoopbackLookingHost_IsRefused()
    {
        using var uriScheme = new UriScheme(new HttpClient(new RecordingHandler(_ => new HttpResponseMessage())));

        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            uriScheme.SubmitToCallbackAsync("http://localhost.evil.example/cb", "AAAA"));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task VerifyOriginDomainSignatureAsync_BodyOfExactlyTheCap_IsAccepted(bool declaredLength)
    {
        var key = $"URI_REQUEST_SIGNING_KEY=\"{Sep7TestVectors.SpecSigningAccountId}\"\n";
        var body = Encoding.UTF8.GetBytes(key + "#" + new string('x', MaxResponseBodyBytes - key.Length - 1));
        Assert.AreEqual(MaxResponseBodyBytes, body.Length);
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = declaredLength
                ? new ByteArrayContent(body)
                : new StreamContent(new NonSeekableStream(new MemoryStream(body))),
        });
        using var uriScheme = new UriScheme(new HttpClient(handler));

        Assert.AreEqual(Sep7TestVectors.SpecSigningAccountId,
            await uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri));
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_KeyChanged_IsReportedBeforeTheSignatureIsChecked()
    {
        // Signed by the spec key; the stellar.toml now publishes another key; the wallet pinned a third. The
        // key change is what the user must be alerted to, whatever the signature does.
        var handler = TomlHandler($"URI_REQUEST_SIGNING_KEY=\"{Sep7TestVectors.SonesoMismatchAccountId}\"");
        using var uriScheme = new UriScheme(new HttpClient(handler));

        await Assert.ThrowsExceptionAsync<UriRequestSigningKeyChangedException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri,
                Sep7TestVectors.SonesoAccountId));
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_PinnedKeyInAnotherCase_IsTheSameKey()
    {
        var handler = TomlHandler($"URI_REQUEST_SIGNING_KEY=\"{Sep7TestVectors.SpecSigningAccountId}\"");
        using var uriScheme = new UriScheme(new HttpClient(handler));

        Assert.AreEqual(Sep7TestVectors.SpecSigningAccountId,
            await uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri,
                Sep7TestVectors.SpecSigningAccountId.ToLowerInvariant()));
    }

    [TestMethod]
    public async Task IsValidSignedUriAsync_ExposesTheFailureType()
    {
        var handler = TomlHandler($"URI_REQUEST_SIGNING_KEY=\"{Sep7TestVectors.SpecSigningAccountId}\"");
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var changed = await uriScheme.IsValidSignedUriAsync(Sep7TestVectors.SpecSignedPayUri,
            Sep7TestVectors.SonesoAccountId);
        var valid = await uriScheme.IsValidSignedUriAsync(Sep7TestVectors.SpecSignedPayUri);

        Assert.IsInstanceOfType(changed.Error, typeof(UriRequestSigningKeyChangedException));
        Assert.AreEqual(changed.Error!.Message, changed.Reason);
        Assert.IsNull(valid.Error);
    }

    [TestMethod]
    public async Task CancelledToken_PropagatesUnwrapped()
    {
        using var uriScheme = new UriScheme(new HttpClient(new CancellationObservingHandler()));
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();

        await Assert.ThrowsExceptionAsync<TaskCanceledException>(() =>
            uriScheme.VerifyOriginDomainSignatureAsync(Sep7TestVectors.SpecSignedPayUri, null, cancelled.Token));
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(() =>
            uriScheme.IsValidSignedUriAsync(Sep7TestVectors.SpecSignedPayUri, null, cancelled.Token));
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(() =>
            uriScheme.SubmitToCallbackAsync("https://cb.example.com", "AAAA", cancelled.Token));
    }

    [TestMethod]
    public async Task Dispose_DisposesTheClientTheInstanceCreated()
    {
        var uriScheme = new UriScheme();
        uriScheme.Dispose();

        await Assert.ThrowsExceptionAsync<ObjectDisposedException>(() =>
            uriScheme.SubmitToCallbackAsync("http://127.0.0.1:1/cb", "AAAA"));
    }

    // ---- batch-4 ----

    [TestMethod]
    [DataRow(HttpStatusCode.OK, true)]
    [DataRow(HttpStatusCode.NoContent, true)]
    [DataRow(HttpStatusCode.Found, false)]
    [DataRow(HttpStatusCode.NotFound, false)]
    public async Task SubmitToCallbackAsync_IsSuccess_MeansA2xxStatus(HttpStatusCode status, bool success)
    {
        using var uriScheme = new UriScheme(new HttpClient(new RecordingHandler(_ => new HttpResponseMessage(status))));

        Assert.AreEqual(success, (await uriScheme.SubmitToCallbackAsync("https://cb.example.com", "AAAA")).IsSuccess);
    }

    [TestMethod]
    public async Task SignAndSubmitTransactionAsync_TransactionHorizonRejects_IsReturnedWithIsSuccessFalse()
    {
        using var signer = KeyPair.Random();
        var uri = UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr,
            networkPassphrase: Network.TestnetPassphrase);
        using var server = await Utils.CreateTestServerWithJson("Responses/serverFailure.json",
            HttpStatusCode.BadRequest);
        using var uriScheme = new UriScheme(new HttpClient(new RecordingHandler(_ => new HttpResponseMessage())));

        var result = await uriScheme.SignAndSubmitTransactionAsync(uri, signer, server);

        Assert.IsNotNull(result.HorizonResponse);
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task SignAndSubmitTransactionAsync_NoWayToDeliver_FailsBeforeFetchingTheStellarToml()
    {
        var (_, wallet) = SignedTxRequestWithCallback();
        var withoutCallback = UriScheme.SignUri(UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr,
            originDomain: "someDomain.com"), KeyPair.FromSecretSeed(Sep7TestVectors.SpecSigningSeed));
        var handler = TomlAndCallbackHandler(Sep7TestVectors.SpecSigningAccountId);
        using var uriScheme = new UriScheme(new HttpClient(handler));

        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            uriScheme.SignAndSubmitTransactionAsync(withoutCallback, wallet));
        var plainHttp = UriScheme.SignUri(UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr,
                callback: "http://cb.example.com/submit", originDomain: "someDomain.com"),
            KeyPair.FromSecretSeed(Sep7TestVectors.SpecSigningSeed));
        await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
            uriScheme.SignAndSubmitTransactionAsync(plainHttp, wallet));

        Assert.AreEqual(0, handler.Requests.Count, "local argument errors must not cost a stellar.toml fetch");
    }

    [TestMethod]
    public async Task VerifyOriginDomainSignatureAsync_HostileRedirectLocation_IsEscapedInTheMessage()
    {
        var handler = new RecordingHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.Found);
            response.Headers.TryAddWithoutValidation("Location",
                "http://evil.example/" + new string('a', 20_000) + "%E2%80%AEspoof");
            return response;
        });
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var result = await uriScheme.IsValidSignedUriAsync(Sep7TestVectors.SpecSignedPayUri);

        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(result.Reason, "not https");
        Assert.IsTrue(result.Reason!.Length < 1_000, $"reason length {result.Reason.Length}");
        Assert.IsFalse(result.Reason.Contains('‮'));
    }

    // ---- batch-5: every message that names a remote URL is escaped ----

    [TestMethod]
    public async Task TomlRedirectToAHostileUrl_ThenNoUsableLocation_IsEscapedInTheMessage()
    {
        var hostile = "https://evil.example/" + new string('a', 20_000) + "%E2%80%AE";
        var handler = new RecordingHandler(request => request.RequestUri!.Host == "evil.example"
            ? new HttpResponseMessage(HttpStatusCode.Found)
            : Redirect(HttpStatusCode.Found, new Uri(hostile)));
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var result = await uriScheme.IsValidSignedUriAsync(Sep7TestVectors.SpecSignedPayUri);

        StringAssert.Contains(result.Reason, "no usable Location");
        AssertEscaped(result.Reason!);
    }

    [TestMethod]
    public async Task TomlRedirectToAHostileUrl_ThenAnError_IsEscapedInTheMessage()
    {
        var hostile = "https://evil.example/" + new string('a', 20_000) + "%E2%80%AE";
        var handler = new RecordingHandler(request => request.RequestUri!.Host == "evil.example"
            ? new HttpResponseMessage(HttpStatusCode.NotFound)
            : Redirect(HttpStatusCode.Found, new Uri(hostile)));
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var result = await uriScheme.IsValidSignedUriAsync(Sep7TestVectors.SpecSignedPayUri);

        StringAssert.Contains(result.Reason, "HTTP status 404");
        AssertEscaped(result.Reason!);
    }

    [TestMethod]
    public async Task CallbackRedirectFollowedByTheClient_IsEscapedInTheMessage()
    {
        using var server = new LoopbackHttpServer((requestLine, stream, cancellationToken) =>
            LoopbackHttpServer.WriteAsync(stream, requestLine.Contains(" /cb ")
                    ? "HTTP/1.1 307 Temporary Redirect\r\nLocation: /" + new string('a', 20_000) +
                      "%E2%80%AE\r\nContent-Length: 0\r\nConnection: close\r\n\r\n"
                    : "HTTP/1.1 200 OK\r\nContent-Length: 2\r\nConnection: close\r\n\r\nok",
                cancellationToken));
        using var client = new HttpClient();
        using var uriScheme = new UriScheme(client);

        var ex = await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            uriScheme.SubmitToCallbackAsync($"http://127.0.0.1:{server.Port}/cb", "AAAA"));

        StringAssert.Contains(ex.Message, "followed a redirect");
        AssertEscaped(ex.Message);
    }

    [TestMethod]
    public async Task CallbackTimeout_IsEscapedInTheMessage()
    {
        using var server = new LoopbackHttpServer(TrickleBody);
        using var client = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false })
        {
            Timeout = TimeSpan.FromSeconds(1),
        };
        using var uriScheme = new UriScheme(client);

        var ex = await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => uriScheme.SubmitToCallbackAsync(
            $"http://127.0.0.1:{server.Port}/cb/" + new string('a', 20_000) + "%E2%80%AE", "AAAA"));

        StringAssert.Contains(ex.Message, "did not complete");
        AssertEscaped(ex.Message);
    }

    [TestMethod]
    public async Task CallbackFailureMessages_AreEscaped()
    {
        var hostilePath = "/cb/" + new string('a', 20_000) + "%E2%80%AE";

        // Reply over the size cap.
        using var overCap = new UriScheme(new HttpClient(new RecordingHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new EndlessContent() })));
        var ex = await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            overCap.SubmitToCallbackAsync("https://cb.example.com" + hostilePath, "AAAA"));
        AssertEscaped(ex.Message);

        // Connection dropped mid-body.
        using var server = new LoopbackHttpServer(TruncatedBody);
        using var truncated = new UriScheme();
        ex = await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            truncated.SubmitToCallbackAsync($"http://127.0.0.1:{server.Port}" + hostilePath, "AAAA"));
        AssertEscaped(ex.Message);
    }

    [TestMethod]
    public async Task TomlSecondRedirectHop_IsEscapedInTheMessage()
    {
        // somedomain.com → evil (hostile URL) → http: the message names the hostile hop as its source.
        var hostile = "https://evil.example/" + new string('a', 20_000) + "%E2%80%AE";
        var handler = new RecordingHandler(request => request.RequestUri!.Host == "evil.example"
            ? Redirect(HttpStatusCode.Found, new Uri("http://evil.example/plain"))
            : Redirect(HttpStatusCode.Found, new Uri(hostile)));
        using var uriScheme = new UriScheme(new HttpClient(handler));

        var result = await uriScheme.IsValidSignedUriAsync(Sep7TestVectors.SpecSignedPayUri);

        StringAssert.Contains(result.Reason, "not https");
        AssertEscaped(result.Reason!);
    }

    private static void AssertEscaped(string message)
    {
        Assert.IsTrue(message.Length < 1_000, $"message length {message.Length}");
        Assert.IsFalse(message.Contains('‮'));
    }

    private static HttpResponseMessage Redirect(HttpStatusCode status, Uri location)
    {
        var response = new HttpResponseMessage(status);
        response.Headers.Location = location;
        return response;
    }

    private static LoopbackHttpServer RedirectingCallbackServer(int status)
    {
        return new LoopbackHttpServer((requestLine, stream, cancellationToken) => LoopbackHttpServer.WriteAsync(stream,
            requestLine.Contains(" /cb ")
                ? $"HTTP/1.1 {status} Redirect\r\nLocation: /elsewhere\r\nContent-Length: 0\r\nConnection: close\r\n\r\n"
                : "HTTP/1.1 200 OK\r\nContent-Length: 6\r\nConnection: close\r\n\r\nlanded",
            cancellationToken));
    }

    private static async Task TrickleBody(string requestLine, NetworkStream stream,
        CancellationToken cancellationToken)
    {
        await LoopbackHttpServer.WriteAsync(stream,
            "HTTP/1.1 200 OK\r\nContent-Length: 1000\r\nConnection: close\r\n\r\n", cancellationToken);
        for (var i = 0; i < 1000; i++)
        {
            await Task.Delay(100, cancellationToken);
            await LoopbackHttpServer.WriteAsync(stream, "#", cancellationToken);
        }
    }

    private static Task TruncatedBody(string requestLine, NetworkStream stream, CancellationToken cancellationToken)
    {
        return LoopbackHttpServer.WriteAsync(stream,
            "HTTP/1.1 200 OK\r\nContent-Length: 1000\r\nConnection: close\r\n\r\n# only 20 bytes of 1000", cancellationToken);
    }

    private static Task StallBeforeHeaders(string requestLine, NetworkStream stream,
        CancellationToken cancellationToken)
    {
        return Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
    }

    /// <summary>A tx request with a callback, signed by the spec's origin-domain key; and a wallet key to sign it.</summary>
    private static (string Uri, KeyPair Wallet) SignedTxRequestWithCallback()
    {
        var unsigned = UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr,
            callback: "https://cb.example.com/submit", networkPassphrase: Network.TestnetPassphrase,
            originDomain: "someDomain.com");
        using var requester = KeyPair.FromSecretSeed(Sep7TestVectors.SpecSigningSeed);
        return (UriScheme.SignUri(unsigned, requester), KeyPair.Random());
    }

    private static RecordingHandler TomlAndCallbackHandler(string publishedSigningKey)
    {
        return new RecordingHandler(request => request.RequestUri!.AbsolutePath == "/.well-known/stellar.toml"
            ? new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"URI_REQUEST_SIGNING_KEY=\"{publishedSigningKey}\""),
            }
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("accepted") });
    }

    private static RecordingHandler TomlHandler(string toml)
    {
        return new RecordingHandler(request => request.RequestUri!.AbsolutePath == "/.well-known/stellar.toml"
            ? new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(toml, Encoding.UTF8, "text/plain"),
            }
            : new HttpResponseMessage(HttpStatusCode.NotFound));
    }

    /// <summary>Answers requests from a delegate and records each request with its body and content type.</summary>
    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

        public RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
        {
            _respond = respond;
        }

        public List<HttpRequestMessage> Requests { get; } = new();
        public List<string?> Bodies { get; } = new();
        public List<string?> ContentTypes { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            Bodies.Add(request.Content == null ? null : await request.Content.ReadAsStringAsync());
            ContentTypes.Add(request.Content?.Headers.ContentType?.MediaType);
            return _respond(request);
        }
    }

    /// <summary>A body with no Content-Length that never ends; counts how much of it was read.</summary>
    private sealed class EndlessContent : HttpContent
    {
        public long BytesServed => _stream.BytesServed;
        private readonly EndlessStream _stream = new();

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            throw new NotSupportedException();
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            return Task.FromResult<Stream>(_stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }

    private sealed class EndlessStream : Stream
    {
        public long BytesServed { get; private set; }
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
            Array.Fill(buffer, (byte)'#', offset, count);
            BytesServed += count;
            return count;
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    /// <summary>Fails the request as soon as it is sent if its token is cancelled, like a real handler would.</summary>
    private sealed class CancellationObservingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    /// <summary>A read-only stream that hides its length, so the response has no Content-Length.</summary>
    private sealed class NonSeekableStream : Stream
    {
        private readonly Stream _inner;

        public NonSeekableStream(Stream inner)
        {
            _inner = inner;
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

        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
