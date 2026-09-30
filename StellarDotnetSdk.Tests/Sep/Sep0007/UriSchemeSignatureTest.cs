using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Sep.Sep0007;
using StellarDotnetSdk.Sep.Sep0007.Exceptions;
using StellarDotnetSdk.Tests.Sep.Sep0007.Fixtures;

namespace StellarDotnetSdk.Tests.Sep.Sep0007;

[TestClass]
public class UriSchemeSignatureTest
{
    [TestMethod]
    public void SignUri_SpecVector_ProducesSpecSignedUri()
    {
        using var signer = KeyPair.FromSecretSeed(Sep7TestVectors.SpecSigningSeed);
        Assert.AreEqual(Sep7TestVectors.SpecSigningAccountId, signer.AccountId);

        var signed = UriScheme.SignUri(Sep7TestVectors.SpecUnsignedPayUri, signer);

        Assert.AreEqual(Sep7TestVectors.SpecSignedPayUri, signed);
        Assert.AreEqual(Sep7TestVectors.SpecSignature, UriScheme.ParseUri(signed).Signature);
    }

    [TestMethod]
    public void SignUri_TypescriptWalletSdkVector_ProducesSameSignedUri()
    {
        using var signer = KeyPair.FromSecretSeed(Sep7TestVectors.TsSigningSeed);
        Assert.AreEqual(Sep7TestVectors.TsSigningAccountId, signer.AccountId);

        Assert.AreEqual(Sep7TestVectors.TsSignedPayUri, UriScheme.SignUri(Sep7TestVectors.TsUnsignedPayUri, signer));
    }

    [TestMethod]
    public void VerifySignature_SpecAndTypescriptVectors_Verify()
    {
        Assert.IsTrue(UriScheme.VerifySignature(Sep7TestVectors.SpecSignedPayUri,
            Sep7TestVectors.SpecSigningAccountId));
        Assert.IsTrue(UriScheme.VerifySignature(Sep7TestVectors.TsSignedPayUri, Sep7TestVectors.TsSigningAccountId));
    }

    [TestMethod]
    public void VerifySignature_WrongKey_ReturnsFalse()
    {
        Assert.IsFalse(UriScheme.VerifySignature(Sep7TestVectors.SpecSignedPayUri,
            Sep7TestVectors.TsSigningAccountId));
    }

    [TestMethod]
    public void VerifySignature_TamperedUri_ReturnsFalse()
    {
        var tampered = Sep7TestVectors.SpecSignedPayUri.Replace("amount=120.1234567", "amount=920.1234567");

        Assert.IsFalse(UriScheme.VerifySignature(tampered, Sep7TestVectors.SpecSigningAccountId));
    }

    [TestMethod]
    public void VerifySignature_ReEncodedUri_ReturnsFalse()
    {
        // The signature covers the literal encoded string: '+' for a space is a different URI than '%20'.
        var reEncoded = Sep7TestVectors.SpecSignedPayUri.Replace("pay%20me%20with%20lumens", "pay+me+with+lumens");

        Assert.IsTrue(UriScheme.TryParseUri(reEncoded, out _));
        Assert.IsFalse(UriScheme.VerifySignature(reEncoded, Sep7TestVectors.SpecSigningAccountId));
    }

    [TestMethod]
    public void VerifySignature_SignatureNotLast_StillVerifies()
    {
        // SEP-7 asks for the signature to be last, but it is stripped wherever it appears.
        var uri = Sep7TestVectors.SpecUnsignedPayUri;
        var signature = Sep7TestVectors.SpecSignedPayUri.Substring(uri.Length); // "&signature=..."
        var insertAt = uri.IndexOf("&amount", StringComparison.Ordinal);
        var moved = uri.Substring(0, insertAt) + signature + uri.Substring(insertAt);

        Assert.IsTrue(UriScheme.VerifySignature(moved, Sep7TestVectors.SpecSigningAccountId));
    }

    [TestMethod]
    public void VerifySignature_UnsignedOrInvalidInput_ReturnsFalse()
    {
        Assert.IsFalse(UriScheme.VerifySignature(Sep7TestVectors.SpecUnsignedPayUri,
            Sep7TestVectors.SpecSigningAccountId));
        Assert.IsFalse(UriScheme.VerifySignature("not a uri", Sep7TestVectors.SpecSigningAccountId));
        Assert.IsFalse(UriScheme.VerifySignature(Sep7TestVectors.SpecSignedPayUri, "GABC"));
        // typescript-wallet-sdk "invalid signature" vector: '?signature=' is not a parameter at all.
        Assert.IsFalse(UriScheme.VerifySignature(
            Sep7TestVectors.SpecUnsignedPayUri.Replace("someDomain.com", "someDomain.com?signature=invalid"),
            Sep7TestVectors.SpecSigningAccountId));
    }

    [TestMethod]
    public void VerifySignature_NullArguments_Throw()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            UriScheme.VerifySignature(null!, Sep7TestVectors.SpecSigningAccountId));
        Assert.ThrowsException<ArgumentNullException>(() =>
            UriScheme.VerifySignature(Sep7TestVectors.SpecSignedPayUri, null!));
    }

    [TestMethod]
    public void SignUri_TxRequest_RoundTripsWithVerify()
    {
        using var signer = KeyPair.Random();
        var uri = UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr,
            UriScheme.ParseReplacements(Sep7TestVectors.SpecReplace), "https://example.com/cb",
            message: "héllo wörld ✓", originDomain: "example.com");

        var signed = UriScheme.SignUri(uri, signer);

        StringAssert.StartsWith(signed, uri + "&signature=");
        Assert.IsTrue(UriScheme.VerifySignature(signed, signer.AccountId));
        Assert.IsFalse(UriScheme.VerifySignature(signed, KeyPair.Random().AccountId));
    }

    [TestMethod]
    public void SignUri_AlreadySigned_Throws()
    {
        using var signer = KeyPair.FromSecretSeed(Sep7TestVectors.SpecSigningSeed);

        var ex = Assert.ThrowsException<InvalidSep7UriException>(() =>
            UriScheme.SignUri(Sep7TestVectors.SpecSignedPayUri, signer));
        StringAssert.Contains(ex.Message, "already");
    }

    [TestMethod]
    public void SignUri_WithoutOriginDomain_Throws()
    {
        using var signer = KeyPair.Random();

        Assert.ThrowsException<MissingOriginDomainException>(() =>
            UriScheme.SignUri(Sep7TestVectors.SpecPayExample1, signer));
    }

    [TestMethod]
    public void SignUri_PublicKeyOnly_Throws()
    {
        var signer = KeyPair.FromAccountId(Sep7TestVectors.SpecSigningAccountId);

        Assert.ThrowsException<ArgumentException>(() => UriScheme.SignUri(Sep7TestVectors.SpecUnsignedPayUri, signer));
    }

    [TestMethod]
    public void SignUri_InvalidUri_Throws()
    {
        using var signer = KeyPair.Random();

        Assert.ThrowsException<InvalidSep7UriException>(() => UriScheme.SignUri("web+stellar:pay", signer));
        Assert.ThrowsException<ArgumentNullException>(() => UriScheme.SignUri(Sep7TestVectors.SpecUnsignedPayUri, null!));
        Assert.ThrowsException<ArgumentNullException>(() => UriScheme.SignUri(null!, signer));
    }

    [TestMethod]
    public void BuildSignaturePayload_Has35ZeroBytesThen4ThenPrefixedUri()
    {
        var payload = UriScheme.BuildSignaturePayload("web+stellar:pay?destination=G");

        Assert.IsTrue(payload.Take(35).All(b => b == 0));
        Assert.AreEqual(4, payload[35]);
        Assert.AreEqual("stellar.sep.7 - URI Schemeweb+stellar:pay?destination=G",
            Encoding.UTF8.GetString(payload, 36, payload.Length - 36));
    }

    [TestMethod]
    public void RemoveSignatureParameter_LeavesEverythingElseUntouched()
    {
        Assert.AreEqual(Sep7TestVectors.SpecUnsignedPayUri,
            UriScheme.RemoveSignatureParameter(Sep7TestVectors.SpecSignedPayUri));
        Assert.AreEqual("web+stellar:pay?a=1&b=2", UriScheme.RemoveSignatureParameter("web+stellar:pay?a=1&signature=x&b=2"));
        Assert.AreEqual("web+stellar:pay?a=1", UriScheme.RemoveSignatureParameter("web+stellar:pay?a=1&%73ignature=x"));
        Assert.AreEqual("web+stellar:pay", UriScheme.RemoveSignatureParameter("web+stellar:pay"));
    }

    // ---- minor-finding fixes (coverage) ----

    [TestMethod]
    public void VerifySignature_NonAsciiUri_UsesTheUtf8BytesOfTheUriAsReceived()
    {
        // A URI whose msg is sent unencoded; the signature is computed here independently of the SDK's payload
        // builder, over "stellar.sep.7 - URI Scheme" + the URI's UTF-8 bytes.
        using var signer = KeyPair.Random();
        var uri = "web+stellar:pay?destination=" + Sep7TestVectors.SonesoAccountId +
                  "&msg=héllo wörld ✓&origin_domain=example.com";
        var data = Encoding.UTF8.GetBytes("stellar.sep.7 - URI Scheme" + uri);
        var payload = new byte[36 + data.Length];
        payload[35] = 4;
        Array.Copy(data, 0, payload, 36, data.Length);
        var signed = uri + "&signature=" + Uri.EscapeDataString(Convert.ToBase64String(signer.Sign(payload)));

        Assert.IsTrue(UriScheme.VerifySignature(signed, signer.AccountId));
    }
}
