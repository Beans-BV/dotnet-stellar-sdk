using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Sep.Sep0012;

namespace StellarDotnetSdk.Tests.Sep.Sep0012;

/// <summary>
///     Unit tests for <see cref="KycCallbackSignature" />, the SEP-12 callback <c>Signature</c> header check.
/// </summary>
[TestClass]
public class KycCallbackSignatureTest
{
    private const string Host = "wallet.example.com";
    private const string Body = "{\"id\":\"d1ce2f48-3ff1-495d-9240-7a50d806cfed\",\"status\":\"ACCEPTED\"}";

    private static readonly DateTimeOffset SignedAt = DateTimeOffset.FromUnixTimeSeconds(1_700_000_000);

    private static readonly KeyPair AnchorKey =
        KeyPair.FromSecretSeed("SBGWSG6BTNCKCOB3DIFBGCVMUPQFYPA2G4O34RMTB343OYPXU5DJDVMN");

    private static KeyPair AnchorPublicKey => KeyPair.FromAccountId(AnchorKey.AccountId);

    private static string SignedHeader(string body = Body, string host = Host)
    {
        return KycCallbackSignature.CreateSignatureHeader(AnchorKey, host, body, SignedAt);
    }

    /// <summary>
    ///     A header whose signature is valid over <paramref name="timestampText" /> exactly as written, so only the
    ///     header parser — never the signature check — can reject it.
    /// </summary>
    private static string HeaderSignedOver(string timestampText)
    {
        var signature = AnchorKey.Sign(Encoding.UTF8.GetBytes($"{timestampText}.{Host}.{Body}"));
        return $"t={timestampText}, s={Convert.ToBase64String(signature)}";
    }

    [TestMethod]
    public void CreateSignatureHeader_FollowsSep12Format()
    {
        var header = SignedHeader();

        StringAssert.StartsWith(header, "t=1700000000, s=");
        var signature = Convert.FromBase64String(header.Substring("t=1700000000, s=".Length));
        Assert.AreEqual(64, signature.Length);
        // The signature covers "<timestamp>.<host>.<body>" exactly.
        Assert.IsTrue(AnchorPublicKey.Verify(Encoding.UTF8.GetBytes($"1700000000.{Host}.{Body}"), signature));
    }

    [TestMethod]
    public void Verify_WithValidFreshSignature_ReturnsTrue()
    {
        Assert.IsTrue(KycCallbackSignature.Verify(SignedHeader(), Body, Host, AnchorPublicKey,
            SignedAt.AddSeconds(30)));
    }

    [TestMethod]
    public void Verify_WithinWindowInEitherDirection_ReturnsTrue()
    {
        var header = SignedHeader();

        Assert.IsTrue(KycCallbackSignature.Verify(header, Body, Host, AnchorPublicKey, SignedAt.AddMinutes(2)));
        Assert.IsTrue(KycCallbackSignature.Verify(header, Body, Host, AnchorPublicKey, SignedAt.AddMinutes(-2)));
    }

    [TestMethod]
    public void Verify_WithStaleTimestamp_ReturnsFalse()
    {
        Assert.IsFalse(KycCallbackSignature.Verify(SignedHeader(), Body, Host, AnchorPublicKey,
            SignedAt.AddMinutes(2).AddSeconds(1)));
    }

    [TestMethod]
    public void Verify_WithFutureTimestampBeyondWindow_ReturnsFalse()
    {
        Assert.IsFalse(KycCallbackSignature.Verify(SignedHeader(), Body, Host, AnchorPublicKey,
            SignedAt.AddMinutes(-3)));
    }

    [TestMethod]
    public void Verify_WithCustomMaxAge_HonoursIt()
    {
        var header = SignedHeader();
        var now = SignedAt.AddMinutes(10);

        Assert.IsFalse(KycCallbackSignature.Verify(header, Body, Host, AnchorPublicKey, now));
        Assert.IsTrue(KycCallbackSignature.Verify(header, Body, Host, AnchorPublicKey, now, TimeSpan.FromMinutes(10)));
    }

    [TestMethod]
    public void Verify_WithTamperedBody_ReturnsFalse()
    {
        Assert.IsFalse(KycCallbackSignature.Verify(SignedHeader(), Body.Replace("ACCEPTED", "REJECTED"), Host,
            AnchorPublicKey, SignedAt));
    }

    [TestMethod]
    public void Verify_ForAnotherHost_ReturnsFalse()
    {
        // A callback relayed to a different wallet host must not verify.
        Assert.IsFalse(KycCallbackSignature.Verify(SignedHeader(), Body, "evil.example.com", AnchorPublicKey,
            SignedAt));
    }

    [TestMethod]
    public void Verify_WithAnotherSigningKey_ReturnsFalse()
    {
        Assert.IsFalse(KycCallbackSignature.Verify(SignedHeader(), Body, Host, KeyPair.Random(), SignedAt));
    }

    [TestMethod]
    public void Verify_WithRelabelledTimestamp_ReturnsFalse()
    {
        // Re-labelling an old signature with a fresh timestamp breaks the signature.
        var header = SignedHeader().Replace("t=1700000000", "t=1700000100");

        Assert.IsFalse(KycCallbackSignature.Verify(header, Body, Host, AnchorPublicKey,
            DateTimeOffset.FromUnixTimeSeconds(1_700_000_100)));
    }

    [TestMethod]
    public void Verify_ToleratesWhitespaceKeyOrderAndUnknownKeys()
    {
        var header = SignedHeader();
        var signature = header.Substring(header.IndexOf("s=", StringComparison.Ordinal) + 2);

        Assert.IsTrue(KycCallbackSignature.Verify($"s={signature},t=1700000000", Body, Host, AnchorPublicKey,
            SignedAt));
        Assert.IsTrue(KycCallbackSignature.Verify($"  t = 1700000000 ,  s = {signature} , v=1", Body, Host,
            AnchorPublicKey, SignedAt));
    }

    [TestMethod]
    [DataRow(null, DisplayName = "missing header")]
    [DataRow("", DisplayName = "empty header")]
    [DataRow("t=1700000000", DisplayName = "no signature")]
    [DataRow("s=AAAA", DisplayName = "no timestamp")]
    [DataRow("t=, s=AAAA", DisplayName = "empty timestamp")]
    [DataRow("t=1700000000, s=not*base64", DisplayName = "invalid base64")]
    [DataRow("t=1700000000; s=AAAA", DisplayName = "wrong separator")]
    [DataRow("garbage", DisplayName = "garbage")]
    public void Verify_WithMalformedHeader_ReturnsFalse(string? header)
    {
        Assert.IsFalse(KycCallbackSignature.Verify(header, Body, Host, AnchorPublicKey, SignedAt));
    }

    [TestMethod]
    [DataRow("abc", DisplayName = "non-numeric")]
    [DataRow("-1700000000", DisplayName = "negative")]
    [DataRow("+1700000000", DisplayName = "explicit sign")]
    [DataRow("99999999999999999999999", DisplayName = "overflowing")]
    [DataRow("1.7e9", DisplayName = "exponent")]
    public void Verify_WithMalformedTimestamp_ReturnsFalseEvenWhenSigned(string timestampText)
    {
        var header = HeaderSignedOver(timestampText);

        Assert.IsFalse(KycCallbackSignature.Verify(header, Body, Host, AnchorPublicKey, SignedAt));
    }

    [TestMethod]
    public void Verify_SignsTheTimestampExactlyAsSent()
    {
        // A leading zero parses to the same instant, but the anchor signed the text as written.
        Assert.IsTrue(KycCallbackSignature.Verify(HeaderSignedOver("01700000000"), Body, Host, AnchorPublicKey,
            SignedAt));
    }

    [TestMethod]
    public void Verify_AcceptsTheSameCallbackAgainWithinTheWindow()
    {
        // Documented limitation: the freshness window bounds replay, it does not prevent it.
        var header = SignedHeader();

        Assert.IsTrue(KycCallbackSignature.Verify(header, Body, Host, AnchorPublicKey, SignedAt));
        Assert.IsTrue(KycCallbackSignature.Verify(header, Body, Host, AnchorPublicKey, SignedAt.AddSeconds(60)));
    }

    [TestMethod]
    public void Verify_WithRawBodyBytes_VerifiesWhatTheAnchorSigned()
    {
        // An anchor that signs a body starting with a UTF-8 byte order mark: decoding to a string drops the BOM.
        var bodyBytes = new byte[] { 0xEF, 0xBB, 0xBF }.Concat(Encoding.UTF8.GetBytes(Body)).ToArray();
        var prefix = Encoding.UTF8.GetBytes($"1700000000.{Host}.");
        var signature = AnchorKey.Sign(prefix.Concat(bodyBytes).ToArray());
        var header = $"t=1700000000, s={Convert.ToBase64String(signature)}";

        Assert.IsTrue(KycCallbackSignature.Verify(header, bodyBytes, Host, AnchorPublicKey, SignedAt));
        Assert.IsFalse(KycCallbackSignature.Verify(header, Encoding.UTF8.GetString(bodyBytes.Skip(3).ToArray()), Host,
            AnchorPublicKey, SignedAt));
    }

    [TestMethod]
    public void Verify_StringAndByteOverloadsAgree()
    {
        var header = SignedHeader();

        Assert.IsTrue(KycCallbackSignature.Verify(header, Encoding.UTF8.GetBytes(Body), Host, AnchorPublicKey,
            SignedAt));
        Assert.IsFalse(KycCallbackSignature.Verify(header, Encoding.UTF8.GetBytes(Body + " "), Host,
            AnchorPublicKey, SignedAt));
    }

    [TestMethod]
    [DataRow("https://wallet.example.com/kyc/callback", "wallet.example.com")]
    [DataRow("https://wallet.example.com:8443/kyc/callback", "wallet.example.com:8443")]
    [DataRow("https://wallet.example.com:443/callback", "wallet.example.com:443")]
    [DataRow("http://localhost:8080", "localhost:8080")]
    [DataRow("https://Wallet.Example.com/callback?x=1", "Wallet.Example.com")]
    [DataRow("https://user:pw@wallet.example.com:9000#frag", "wallet.example.com:9000")]
    [DataRow("  https://wallet.example.com/cb  ", "wallet.example.com")]
    [DataRow("https://wallet.example.com:/cb", "wallet.example.com", DisplayName = "empty port")]
    [DataRow("https://wallet.example.com:08443/cb", "wallet.example.com:8443", DisplayName = "leading zero")]
    [DataRow("https://[::1]:8443/cb", "[::1]:8443")]
    [DataRow("https://[::1]/cb", "[::1]")]
    [DataRow("http://loopback:8080/cb", "loopback:8080", DisplayName = "not rewritten to localhost")]
    public void GetSignedHost_MatchesTheAnchorPlatformHostString(string url, string expected)
    {
        Assert.AreEqual(expected, KycCallbackSignature.GetSignedHost(url));
    }

    [TestMethod]
    public void GetSignedHost_RoundTripsWithAPortSignedHeader()
    {
        const string url = "https://wallet.example.com:8443/callback";
        var header = KycCallbackSignature.CreateSignatureHeader(AnchorKey, "wallet.example.com:8443", Body, SignedAt);

        Assert.IsTrue(KycCallbackSignature.Verify(header, Body, KycCallbackSignature.GetSignedHost(url),
            AnchorPublicKey, SignedAt));
        Assert.IsFalse(KycCallbackSignature.Verify(header, Body, new Uri(url).Host, AnchorPublicKey, SignedAt));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("wallet.example.com")]
    [DataRow("ftp://wallet.example.com")]
    [DataRow("https:\\\\wallet.example.com/cb?r=https://evil.example.com/", DisplayName = "backslash authority")]
    [DataRow("https:/\\wallet.example.com/cb?r=https://evil.example.com/", DisplayName = "mixed slashes")]
    public void GetSignedHost_WithInvalidUrl_ThrowsArgumentException(string url)
    {
        Assert.ThrowsException<ArgumentException>(() => KycCallbackSignature.GetSignedHost(url));
    }

    [TestMethod]
    public void CreateSignatureHeader_BeforeUnixEpoch_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            KycCallbackSignature.CreateSignatureHeader(AnchorKey, Host, Body, DateTimeOffset.FromUnixTimeSeconds(-5)));
    }

    [TestMethod]
    public void CreateSignatureHeader_WithPublicKeyOnly_ThrowsInvalidOperationException()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
            KycCallbackSignature.CreateSignatureHeader(AnchorPublicKey, Host, Body, SignedAt));
    }

    [TestMethod]
    public void Verify_WithDuplicatedKey_ReturnsFalse()
    {
        var valid = SignedHeader();
        var signature = valid.Substring(valid.IndexOf("s=", StringComparison.Ordinal) + 2);

        // Repeating a key is rejected even when every copy carries the valid value, so neither first-wins nor
        // last-wins resolution can ever be relied on.
        Assert.IsFalse(KycCallbackSignature.Verify(valid + ", t=1700000000", Body, Host, AnchorPublicKey,
            SignedAt));
        Assert.IsFalse(KycCallbackSignature.Verify(valid + $", s={signature}", Body, Host, AnchorPublicKey,
            SignedAt));
    }

    [TestMethod]
    public void Verify_WithNegativeMaxAge_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            KycCallbackSignature.Verify(SignedHeader(), Body, Host, AnchorPublicKey, SignedAt,
                TimeSpan.FromSeconds(-1)));
    }

    [TestMethod]
    public void Verify_WithNullArguments_Throws()
    {
        var header = SignedHeader();
        Assert.ThrowsException<ArgumentNullException>(() =>
            KycCallbackSignature.Verify(header, (string)null!, Host, AnchorPublicKey));
        Assert.ThrowsException<ArgumentNullException>(() =>
            KycCallbackSignature.Verify(header, (byte[])null!, Host, AnchorPublicKey));
        Assert.ThrowsException<ArgumentNullException>(() =>
            KycCallbackSignature.Verify(header, Body, null!, AnchorPublicKey));
        Assert.ThrowsException<ArgumentNullException>(() =>
            KycCallbackSignature.Verify(header, Body, Host, null!));
    }

    [TestMethod]
    public void HeaderNames_MatchSep12()
    {
        Assert.AreEqual("Signature", KycCallbackSignature.SignatureHeaderName);
        Assert.AreEqual("X-Stellar-Signature", KycCallbackSignature.DeprecatedSignatureHeaderName);
        Assert.AreEqual(TimeSpan.FromMinutes(2), KycCallbackSignature.DefaultMaxAge);
    }
}
