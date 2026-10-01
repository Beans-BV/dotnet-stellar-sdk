using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Assets;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Sep.Sep0007;
using StellarDotnetSdk.Sep.Sep0007.Exceptions;
using StellarDotnetSdk.Tests.Sep.Sep0007.Fixtures;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Tests.Sep.Sep0007;

[TestClass]
public class UriSchemeParseTest
{
    private const string Destination = "GCALNQQBXAPZ2WIRSDDBMSTAKCUH5SG6U76YBFLQLIXJTF7FE5AX7AOO";
    private const string PayPrefix = "web+stellar:pay?destination=" + Destination;
    private static readonly string TxPrefix = "web+stellar:tx?xdr=" + Uri.EscapeDataString(Sep7TestVectors.SpecTxXdr);

    // Structurally valid (64 base64 bytes); an origin_domain without any signature makes the request invalid.
    private static readonly string WellFormedSignature =
        "&signature=" + Uri.EscapeDataString(Convert.ToBase64String(new byte[64]));

    [TestMethod]
    [DataRow(Sep7TestVectors.SpecTxExample1)]
    [DataRow(Sep7TestVectors.SpecTxExample2)]
    [DataRow(Sep7TestVectors.SpecPayExample1)]
    [DataRow(Sep7TestVectors.SpecPayExample2)]
    [DataRow(Sep7TestVectors.SpecSignedPayUri)]
    [DataRow(Sep7TestVectors.TsSignedPayUri)]
    [DataRow(Sep7TestVectors.SonesoExpectedPayUri)]
    public void ValidateUri_SpecAndPeerExamples_AreValid(string uri)
    {
        var result = UriScheme.ValidateUri(uri);

        Assert.IsTrue(result.IsValid, result.Reason);
        Assert.IsNull(result.Reason);
    }

    [TestMethod]
    public void ParseUri_SpecTxExample1_ExposesEveryParameterDecoded()
    {
        var request = UriScheme.ParseUri(Sep7TestVectors.SpecTxExample1);

        Assert.AreEqual(Sep7OperationType.Tx, request.OperationType);
        Assert.AreEqual(Sep7TestVectors.SpecTxExample1, request.Uri);
        Assert.AreEqual(Sep7TestVectors.SpecTxXdr, request.Xdr);
        Assert.AreEqual("url:https://someSigningService.com/a8f7asdfkjha", request.Callback);
        Assert.AreEqual("https://someSigningService.com/a8f7asdfkjha", request.CallbackUrl);
        Assert.AreEqual("GAU2ZSYYEYO5S5ZQSMMUENJ2TANY4FPXYGGIMU6GMGKTNVDG5QYFW6JS", request.PublicKey);
        Assert.AreEqual("order number 24", request.Message);
        Assert.IsNull(request.Replace);
        Assert.AreEqual(0, request.Replacements.Count);
        Assert.IsNull(request.Chain);
        Assert.IsNull(request.ChainedUri);
        Assert.IsNull(request.NetworkPassphrase);
        Assert.IsNull(request.OriginDomain);
        Assert.IsNull(request.Signature);
        Assert.IsNull(request.Destination);
        Assert.AreEqual(4, request.Parameters.Count);
    }

    [TestMethod]
    public void ParseUri_SpecTxExample1_GetTransactionDecodesEnvelope()
    {
        var request = UriScheme.ParseUri(Sep7TestVectors.SpecTxExample1);

        var transaction = request.GetTransaction();

        Assert.IsInstanceOfType(transaction, typeof(Transaction));
        Assert.AreEqual(Sep7TestVectors.SpecTxXdr,
            transaction.ToUnsignedEnvelopeXdrBase64(TransactionBase.TransactionXdrVersion.V0));
        Assert.AreNotSame(transaction, request.GetTransaction(), "each call returns a fresh instance");
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(4)]
    [DataRow(8)]
    public void ParseUri_RejectsTrailingBytesAfterTheEnvelope(int trailingBytes)
    {
        // A decoder that stops after one envelope would accept these and silently drop the tail.
        var envelope = Convert.FromBase64String(Sep7TestVectors.SpecTxXdr);
        var padded = new byte[envelope.Length + trailingBytes];
        Buffer.BlockCopy(envelope, 0, padded, 0, envelope.Length);
        var uri = "web+stellar:tx?xdr=" + Uri.EscapeDataString(Convert.ToBase64String(padded));

        var ex = Assert.ThrowsException<InvalidSep7UriException>(() => UriScheme.ParseUri(uri));
        StringAssert.Contains(ex.Message, "not a valid base64 XDR");
        StringAssert.Contains(ex.InnerException!.Message, "trailing byte");
        Assert.IsFalse(UriScheme.ValidateUri(uri).IsValid);
    }

    [TestMethod]
    [DataRow(8, " ")]
    [DataRow(8, "\n")]
    [DataRow(8, "\r\n")]
    [DataRow(0, " ")]
    [DataRow(-1, "\t")]
    // Four characters keep the length a multiple of 4, so only the alphabet check can reject these.
    [DataRow(8, "    ")]
    [DataRow(8, "\r\n\r\n")]
    public void ParseUri_RejectsWhitespaceInTheEnvelope(int position, string whitespace)
    {
        var xdr = position < 0
            ? Sep7TestVectors.SpecTxXdr + whitespace
            : Sep7TestVectors.SpecTxXdr.Insert(position, whitespace);
        // Convert.FromBase64String skips whitespace, so only a strict check tells this apart from the vector.
        CollectionAssert.AreEqual(Convert.FromBase64String(Sep7TestVectors.SpecTxXdr), Convert.FromBase64String(xdr));
        var uri = "web+stellar:tx?xdr=" + Uri.EscapeDataString(xdr);

        var ex = Assert.ThrowsException<InvalidSep7UriException>(() => UriScheme.ParseUri(uri));
        StringAssert.Contains(ex.Message, "not a valid base64 XDR");
        StringAssert.Contains(ex.InnerException!.Message, "strict base64");
        Assert.IsFalse(UriScheme.ValidateUri(uri).IsValid);
    }

    [TestMethod]
    public void ParseUri_SpecPayExample2_ExposesPayParameters()
    {
        var request = UriScheme.ParseUri(Sep7TestVectors.SpecPayExample2);

        Assert.AreEqual(Sep7OperationType.Pay, request.OperationType);
        Assert.AreEqual(Destination, request.Destination);
        Assert.AreEqual("120.123", request.Amount);
        Assert.AreEqual("USD", request.AssetCode);
        Assert.AreEqual("GCRCUE2C5TBNIPYHMEP7NK5RWTT2WBSZ75CMARH7GDOHDDCQH3XANFOB", request.AssetIssuer);
        Assert.AreEqual("hasysda987fs", request.Memo);
        Assert.AreEqual(Sep7MemoType.MemoText, request.MemoType);
        Assert.AreEqual("https://someSigningService.com/hasysda987fs?asset=USD", request.CallbackUrl);
        Assert.IsNull(request.Xdr);

        var asset = request.GetAsset();
        Assert.AreEqual("USD", ((AssetTypeCreditAlphaNum)asset).Code);
        Assert.AreEqual(new MemoText("hasysda987fs"), request.GetMemo());
    }

    [TestMethod]
    public void ParseUri_PayWithoutAsset_GetAssetIsNative()
    {
        var request = UriScheme.ParseUri(PayPrefix);

        Assert.IsInstanceOfType(request.GetAsset(), typeof(AssetTypeNative));
        Assert.IsNull(request.GetMemo());
    }

    [TestMethod]
    public void GetNetwork_DefaultsToPublic_AndHonoursNetworkPassphrase()
    {
        var publicRequest = UriScheme.ParseUri(PayPrefix);
        var testRequest = UriScheme.ParseUri(
            PayPrefix + "&network_passphrase=" + Uri.EscapeDataString(Network.TestnetPassphrase));

        Assert.AreEqual(Network.PublicPassphrase, publicRequest.GetNetwork().NetworkPassphrase);
        Assert.AreEqual(Network.TestnetPassphrase, testRequest.NetworkPassphrase);
        Assert.AreEqual(Network.TestnetPassphrase, testRequest.GetNetwork().NetworkPassphrase);
    }

    [TestMethod]
    public void GetTransaction_OnPayRequest_Throws()
    {
        var request = UriScheme.ParseUri(PayPrefix);

        Assert.ThrowsException<InvalidOperationException>(() => request.GetTransaction());
    }

    [TestMethod]
    public void GetAsset_OnTxRequest_Throws()
    {
        var request = UriScheme.ParseUri(Sep7TestVectors.SpecTxExample1);

        Assert.ThrowsException<InvalidOperationException>(() => request.GetAsset());
    }

    [TestMethod]
    public void ParseUri_FormEncodedSpaces_DecodeAsSpaces()
    {
        // What the typescript-wallet-sdk emits: URLSearchParams writes a space as '+'.
        var uri = PayPrefix + "&memo=order+42&msg=pay+me+now" +
                  "&network_passphrase=Test+SDF+Network+%3B+September+2015";

        var request = UriScheme.ParseUri(uri);

        Assert.AreEqual("order 42", request.Memo);
        Assert.AreEqual(new MemoText("order 42"), request.GetMemo());
        Assert.AreEqual("pay me now", request.Message);
        Assert.AreEqual(Network.TestnetPassphrase, request.NetworkPassphrase);
        Assert.AreEqual(Network.TestnetPassphrase, request.GetNetwork().NetworkPassphrase);
    }

    [TestMethod]
    public void ParseUri_PercentEncodedPlus_IsAPlus()
    {
        var request = UriScheme.ParseUri(PayPrefix + "&memo=a%2Bb&msg=1%2B1");

        Assert.AreEqual("a+b", request.Memo);
        Assert.AreEqual("1+1", request.Message);
    }

    [TestMethod]
    public void ParseUri_RawPlusInXdr_IsKept()
    {
        // A space is never valid base64, so an unencoded '+' in a base64 value is kept as '+'.
        var uri = "web+stellar:tx?xdr=" + Sep7TestVectors.SpecTxXdr.Replace("/", "%2F").Replace("=", "%3D");
        StringAssert.Contains(uri, "+", "the vector must contain a raw '+' to test anything");

        var request = UriScheme.ParseUri(uri);

        Assert.AreEqual(Sep7TestVectors.SpecTxXdr, request.Xdr);
    }

    [TestMethod]
    [DataRow(Sep7MemoType.MemoHash)]
    [DataRow(Sep7MemoType.MemoReturn)]
    public void ParseUri_RawPlusInHashMemo_IsKept(string memoType)
    {
        var hash = new byte[32];
        hash[0] = 0xFB; // base64 "+..."
        var base64 = Convert.ToBase64String(hash);
        StringAssert.StartsWith(base64, "+");

        var request = UriScheme.ParseUri(PayPrefix + "&memo=" + base64.Replace("=", "%3D") + "&memo_type=" +
                                         memoType);

        Assert.AreEqual(base64, request.Memo);
        CollectionAssert.AreEqual(hash, ((MemoHashAbstract)request.GetMemo()!).MemoBytes);
    }

    [TestMethod]
    public void ParseUri_RawPlusInHashMemo_IsKept_WhenMemoTypeIsPercentEncoded()
    {
        // memo_type is decoded before it decides how memo is decoded.
        var hash = new byte[32];
        hash[0] = 0xFB;
        var base64 = Convert.ToBase64String(hash);

        var request = UriScheme.ParseUri(PayPrefix + "&memo=" + base64.Replace("=", "%3D") +
                                         "&memo_type=MEMO%5FHASH");

        Assert.AreEqual(base64, request.Memo);
    }

    [TestMethod]
    public void ParseUri_PlusInParameterName_IsASpace()
    {
        var request = UriScheme.ParseUri(PayPrefix + "&x+y=1");

        Assert.AreEqual("1", request.Parameters["x y"]);
    }

    [TestMethod]
    public void ParseUri_RawPlusInSignature_IsKept()
    {
        var signature = Sep7TestVectors.SpecSignedPayUri.Substring(
            Sep7TestVectors.SpecSignedPayUri.IndexOf("&signature=", StringComparison.Ordinal) + "&signature=".Length);
        var rawSignature = signature.Replace("%2B", "+");
        StringAssert.Contains(rawSignature, "+", "the vector must contain a '+' to test anything");

        var request = UriScheme.ParseUri(Sep7TestVectors.SpecUnsignedPayUri + "&signature=" + rawSignature);

        Assert.AreEqual(Uri.UnescapeDataString(signature), request.Signature);
    }

    [TestMethod]
    public void OriginDomainWithoutSignature_IsInvalid()
    {
        var uri = Sep7TestVectors.SpecUnsignedPayUri;

        var ex = Assert.ThrowsException<InvalidSep7UriException>(() => UriScheme.ParseUri(uri));
        StringAssert.Contains(ex.Message, "'signature'");
        Assert.IsFalse(UriScheme.TryParseUri(uri, out var parsed));
        Assert.IsNull(parsed);
        var validation = UriScheme.ValidateUri(uri);
        Assert.IsFalse(validation.IsValid);
        Assert.AreEqual(ex.Message, validation.Reason);
    }

    [TestMethod]
    public void OriginDomainWithoutSignature_InChain_IsInvalid()
    {
        var uri = TxPrefix + "&chain=" + Uri.EscapeDataString(Sep7TestVectors.SpecUnsignedPayUri);

        var validation = UriScheme.ValidateUri(uri);

        Assert.IsFalse(validation.IsValid);
        StringAssert.Contains(validation.Reason, "'signature'");
        Assert.IsTrue(UriScheme.ValidateUri(
            TxPrefix + "&chain=" + Uri.EscapeDataString(Sep7TestVectors.SpecSignedPayUri)).IsValid);
    }

    [TestMethod]
    public void OriginDomain_WithSignatureNotLast_IsValid()
    {
        // The signature parameter is found wherever it sits in the query.
        var uri = Sep7TestVectors.SpecUnsignedPayUri.Replace("&amount=",
            Sep7TestVectors.SpecSignedPayUri.Substring(Sep7TestVectors.SpecUnsignedPayUri.Length) + "&amount=");

        Assert.IsTrue(UriScheme.ValidateUri(uri).IsValid);
    }

    [TestMethod]
    public void ParseUri_SchemeIsCaseInsensitive()
    {
        Assert.IsTrue(UriScheme.ValidateUri("WEB+STELLAR:pay?destination=" + Destination).IsValid);
    }

    [TestMethod]
    public void ParseUri_KeepsUnknownParameters()
    {
        var request = UriScheme.ParseUri(PayPrefix + "&x_future=some%20value");

        Assert.AreEqual("some value", request.Parameters["x_future"]);
    }

    [TestMethod]
    public void ParseUri_Invalid_ThrowsInvalidSep7UriException()
    {
        var ex = Assert.ThrowsException<InvalidSep7UriException>(() => UriScheme.ParseUri("https://example.com"));

        StringAssert.Contains(ex.Message, "web+stellar:");
    }

    [TestMethod]
    public void ParseUri_Null_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() => UriScheme.ParseUri(null!));
    }

    [TestMethod]
    public void TryParseUri_ReportsSuccessAndFailure()
    {
        Assert.IsTrue(UriScheme.TryParseUri(PayPrefix, out var parsed));
        Assert.IsNotNull(parsed);
        Assert.IsFalse(UriScheme.TryParseUri("web+stellar:pay", out var failed));
        Assert.IsNull(failed);
        Assert.IsFalse(UriScheme.TryParseUri(null, out _));
    }

    [TestMethod]
    public void ValidateUri_Null_IsInvalid()
    {
        Assert.IsFalse(UriScheme.ValidateUri(null).IsValid);
    }

    // Rejections — each row is a URI and a fragment of the expected reason.
    [TestMethod]
    [DataRow("stellar:pay?destination=" + Destination, "must start with")]
    [DataRow("web+stellar:tx/pay?destination=" + Destination, "not supported")] // Soneso vector
    [DataRow("web+stellar:203842", "not supported")] // Soneso vector
    [DataRow("web+stellar:swap?destination=" + Destination, "not supported")]
    [DataRow("web+stellar:pay", "Missing required parameter 'destination'")]
    [DataRow("web+stellar:tx", "Missing required parameter 'xdr'")]
    [DataRow("web+stellar:tx?xdr=12345673773", "not a valid base64 XDR")] // Soneso vector
    [DataRow("web+stellar:pay?destination=12345673773", "'destination'")] // Soneso vector
    [DataRow(PayPrefix + "#frag", "fragment")]
    [DataRow(PayPrefix + "&", "not a 'name=value' pair")]
    [DataRow(PayPrefix + "&amount", "not a 'name=value' pair")]
    [DataRow(PayPrefix + "&=5", "not a 'name=value' pair")]
    [DataRow(PayPrefix + "&amount=1&amount=2", "more than once")]
    [DataRow(PayPrefix + "&msg=", "must not be empty")]
    [DataRow(PayPrefix + "&xdr=AAAA", "not allowed for the 'pay' operation")]
    [DataRow(PayPrefix + "&pubkey=" + Destination, "not allowed for the 'pay' operation")]
    [DataRow(PayPrefix + "&replace=a%3AX%3BX%3Ah", "not allowed for the 'pay' operation")]
    [DataRow(PayPrefix + "&chain=x", "not allowed for the 'pay' operation")]
    [DataRow(PayPrefix + "&amount=0", "'amount'")]
    [DataRow(PayPrefix + "&amount=-1", "'amount'")]
    [DataRow(PayPrefix + "&amount=1.12345678", "'amount'")]
    [DataRow(PayPrefix + "&amount=1e5", "'amount'")]
    [DataRow(PayPrefix + "&amount=922337203685.4775808", "'amount'")]
    [DataRow(PayPrefix + "&asset_code=USD", "must be given together")]
    [DataRow(PayPrefix + "&asset_issuer=" + Destination, "must be given together")]
    [DataRow(PayPrefix + "&asset_code=ABCDEFGHIJKLM&asset_issuer=" + Destination, "'asset_code'")]
    [DataRow(PayPrefix + "&asset_code=US%24&asset_issuer=" + Destination, "'asset_code'")]
    [DataRow(PayPrefix + "&asset_code=USD&asset_issuer=GABC", "'asset_issuer'")]
    [DataRow(PayPrefix + "&memo=x&memo_type=zulu", "'memo_type'")] // Soneso vector
    [DataRow(PayPrefix + "&memo_type=MEMO_TEXT", "requires a 'memo'")]
    [DataRow(PayPrefix + "&memo=abracadabra&memo_type=MEMO_ID", "MEMO_ID")] // Soneso vector
    [DataRow(PayPrefix + "&memo=18446744073709551616&memo_type=MEMO_ID", "MEMO_ID")]
    [DataRow(PayPrefix + "&memo=-1&memo_type=MEMO_ID", "MEMO_ID")]
    [DataRow(PayPrefix + "&memo=abracadabra&memo_type=MEMO_HASH", "base64")] // Soneso vector
    [DataRow(PayPrefix + "&memo=abracadabra&memo_type=MEMO_RETURN", "base64")]
    [DataRow(PayPrefix + "&memo=12345678901234567890123456789&memo_type=MEMO_TEXT", "28 bytes")]
    [DataRow(PayPrefix + "&memo=12345678901234567890123456789", "28 bytes")]
    [DataRow(PayPrefix + "&callback=https%3A%2F%2Fexample.com", "must start with 'url:'")]
    [DataRow(PayPrefix + "&callback=url%3Aftp%3A%2F%2Fexample.com", "absolute http(s) URL")]
    [DataRow(PayPrefix + "&callback=url%3A%2Frelative", "absolute http(s) URL")]
    [DataRow(PayPrefix + "&signature=abc", "'signature'")]
    [DataRow(PayPrefix + "&signature=AAAA", "'signature'")]
    public void ValidateUri_InvalidUris_AreRejectedWithReason(string uri, string reasonFragment)
    {
        var result = UriScheme.ValidateUri(uri);

        Assert.IsFalse(result.IsValid, $"expected '{uri}' to be rejected");
        StringAssert.Contains(result.Reason, reasonFragment);
    }

    [TestMethod]
    public void ValidateUri_TxWithPayOnlyParameter_IsRejected()
    {
        foreach (var parameter in new[] { "destination", "amount", "asset_code", "asset_issuer", "memo", "memo_type" })
        {
            var result = UriScheme.ValidateUri(TxPrefix + "&" + parameter + "=x");
            Assert.IsFalse(result.IsValid, parameter);
            StringAssert.Contains(result.Reason, "not allowed for the 'tx' operation");
        }
    }

    [TestMethod]
    public void ValidateUri_TxPubkey_MustBeAccountId()
    {
        Assert.IsFalse(UriScheme.ValidateUri(TxPrefix + "&pubkey=" + Sep7TestVectors.TsMuxedDestination).IsValid);
        Assert.IsTrue(UriScheme.ValidateUri(TxPrefix + "&pubkey=" + Destination).IsValid);
    }

    [TestMethod]
    public void ValidateUri_Destination_AcceptsAccountMuxedAndContract()
    {
        var contractId = StrKey.EncodeContractId(new byte[32]);

        Assert.IsTrue(UriScheme.ValidateUri(PayPrefix).IsValid);
        Assert.IsTrue(UriScheme.ValidateUri(
            "web+stellar:pay?destination=" + Sep7TestVectors.TsMuxedDestination).IsValid);
        Assert.IsTrue(UriScheme.ValidateUri("web+stellar:pay?destination=" + contractId).IsValid);
    }

    [TestMethod]
    [DataRow("1")]
    [DataRow("0.0000001")]
    [DataRow("922337203685.4775807")]
    [DataRow("120.1234567")]
    public void ValidateUri_Amount_AcceptsValidAmounts(string amount)
    {
        Assert.IsTrue(UriScheme.ValidateUri(PayPrefix + "&amount=" + amount).IsValid);
    }

    [TestMethod]
    public void ValidateUri_Message_AllowsExactly300Characters()
    {
        var ok = new string('a', UriScheme.MaxMessageLength);
        var tooLong = new string('a', UriScheme.MaxMessageLength + 1);

        Assert.IsTrue(UriScheme.ValidateUri(PayPrefix + "&msg=" + ok).IsValid);
        var result = UriScheme.ValidateUri(PayPrefix + "&msg=" + tooLong);
        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(result.Reason, "300");
    }

    [TestMethod]
    public void ValidateUri_Message_CountsCharactersBeforeEncoding()
    {
        // 300 emoji: 300 characters (600 UTF-16 units, 3600 bytes once percent-encoded) — still valid.
        var emoji = string.Concat(Enumerable.Repeat("\U0001F600", UriScheme.MaxMessageLength));

        Assert.IsTrue(UriScheme.ValidateUri(PayPrefix + "&msg=" + Uri.EscapeDataString(emoji)).IsValid);
    }

    [TestMethod]
    public void ValidateUri_MemoHash_AcceptsBase64Of32Bytes_AndDecodesTyped()
    {
        var hash = Enumerable.Range(1, 32).Select(i => (byte)i).ToArray();
        var uri = PayPrefix + "&memo=" + Uri.EscapeDataString(Convert.ToBase64String(hash)) + "&memo_type=MEMO_HASH";

        var memo = UriScheme.ParseUri(uri).GetMemo();

        Assert.IsInstanceOfType(memo, typeof(MemoHash));
        CollectionAssert.AreEqual(hash, ((MemoHash)memo!).MemoBytes);
    }

    [TestMethod]
    public void ValidateUri_MemoHash_RejectsMoreThan32Bytes()
    {
        // Soneso's "valid-looking" hash memo: well-formed base64, but it decodes to more than 32 bytes.
        const string memo =
            "YWxrc2RmajA5MzIxOTA0dWtkbm1sc2EgeDJlb2pmZGxzd2tkajg5YXMgd3PDtmRhc0pEQVNVOVVESiBBU0Rhc0RBc2R3cWVxdw%3D%3D";

        var result = UriScheme.ValidateUri(PayPrefix + "&memo=" + memo + "&memo_type=MEMO_RETURN");

        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(result.Reason, "32 bytes");
    }

    [TestMethod]
    public void GetMemo_DecodesIdAndReturn()
    {
        var idRequest = UriScheme.ParseUri(PayPrefix + "&memo=18446744073709551615&memo_type=MEMO_ID");
        var returnRequest = UriScheme.ParseUri(
            PayPrefix + "&memo=" + Uri.EscapeDataString(Convert.ToBase64String(new byte[32])) +
            "&memo_type=MEMO_RETURN");

        Assert.AreEqual(ulong.MaxValue, ((MemoId)idRequest.GetMemo()!).IdValue);
        Assert.IsInstanceOfType(returnRequest.GetMemo(), typeof(MemoReturnHash));
    }

    [TestMethod]
    public void GetMemo_WithoutMemoType_IsText()
    {
        var request = UriScheme.ParseUri(PayPrefix + "&memo=hello%20world");

        Assert.AreEqual(new MemoText("hello world"), request.GetMemo());
    }

    [TestMethod]
    [DataRow("example.com")]
    [DataRow("subdomain.example.com")]
    [DataRow("test-domain.example.co.uk")]
    [DataRow("api.v2.stellar.example.com")]
    [DataRow("someDomain.com")]
    public void ValidateUri_OriginDomain_AcceptsFullyQualifiedDomainNames(string domain)
    {
        Assert.IsTrue(UriScheme.ValidateUri(PayPrefix + "&origin_domain=" + domain + WellFormedSignature).IsValid,
            domain);
    }

    // iOS SDK and typescript-wallet-sdk rejection vectors; the last six would otherwise redirect the stellar.toml
    // fetch (userinfo, port, path, IP, CRLF).
    [TestMethod]
    [DataRow("localhost")]
    [DataRow("foo.localhost")]
    [DataRow("foo.LocalHost")]
    [DataRow("test")]
    [DataRow("-invalid.com")]
    [DataRow("invalid-.com")]
    [DataRow("example.123")]
    [DataRow("a.b")]
    [DataRow("exam_ple.com")]
    [DataRow("exam%21ple.com")]
    [DataRow("exam%40ple.com")]
    [DataRow("192.168.1.1")]
    [DataRow("911")] // Soneso vector
    [DataRow("example.com.")]
    [DataRow("stellar.org%40evil.com")]
    [DataRow("example.com%3A8443")]
    [DataRow("example.com%2Fpath")]
    [DataRow("127.0.0.1")]
    [DataRow("stellar.org%0d%0aevil.com")]
    [DataRow("example.com%0a")]
    public void ParseUri_OriginDomain_RejectsNonFqdn(string domain)
    {
        var ex = Assert.ThrowsException<InvalidOriginDomainException>(() =>
            UriScheme.ParseUri(PayPrefix + "&origin_domain=" + domain));

        Assert.AreEqual(Uri.UnescapeDataString(domain), ex.OriginDomain);
    }

    [TestMethod]
    public void ParseUri_OriginDomain_RejectsLabelLongerThan63()
    {
        var domain = new string('a', 64) + ".com";

        Assert.ThrowsException<InvalidOriginDomainException>(() =>
            UriScheme.ParseUri(PayPrefix + "&origin_domain=" + domain));
    }

    [TestMethod]
    public void ParseUri_Chain_IsParsedRecursively()
    {
        var uri = TxPrefix + "&chain=" + Uri.EscapeDataString(Sep7TestVectors.SpecPayExample1);

        var request = UriScheme.ParseUri(uri);

        Assert.AreEqual(Sep7TestVectors.SpecPayExample1, request.Chain);
        Assert.IsNotNull(request.ChainedUri);
        Assert.AreEqual(Sep7OperationType.Pay, request.ChainedUri!.OperationType);
        Assert.AreEqual("pay me with lumens", request.ChainedUri.Message);
    }

    [TestMethod]
    public void ParseUri_Chain_MustBeValidSep7Uri()
    {
        var result = UriScheme.ValidateUri(TxPrefix + "&chain=" + Uri.EscapeDataString("https://example.com"));

        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(result.Reason, "'chain' parameter is not a valid SEP-7 URI");
    }

    [TestMethod]
    public void ParseUri_Chain_AllowsSevenNestedLevels()
    {
        var uri = NestChains(UriScheme.MaxChainNestingLevels);

        var request = UriScheme.ParseUri(uri);

        var depth = 0;
        for (var current = request.ChainedUri; current != null; current = current.ChainedUri)
        {
            depth++;
        }
        Assert.AreEqual(7, depth);
    }

    [TestMethod]
    public void ParseUri_Chain_RejectsEightNestedLevels()
    {
        var uri = NestChains(UriScheme.MaxChainNestingLevels + 1);

        var result = UriScheme.ValidateUri(uri);

        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(result.Reason, "nests more than 7 levels");
    }

    /// <summary>Wraps a pay request in <paramref name="levels" /> tx requests, each chaining the previous one.</summary>
    private static string NestChains(int levels)
    {
        var uri = PayPrefix;
        for (var i = 0; i < levels; i++)
        {
            uri = TxPrefix + "&chain=" + Uri.EscapeDataString(uri);
        }
        return uri;
    }

    // ---- minor-finding fixes ----

    [TestMethod]
    [DataRow(Sep7MemoType.MemoHash, 31)]
    [DataRow(Sep7MemoType.MemoHash, 33)]
    [DataRow(Sep7MemoType.MemoHash, 1)]
    [DataRow(Sep7MemoType.MemoReturn, 16)]
    public void ParseUri_HashMemoNot32Bytes_IsInvalid(string memoType, int length)
    {
        // A shorter hash would be zero-padded into a different 32-byte hash than the one sent.
        var memo = Uri.EscapeDataString(Convert.ToBase64String(new byte[length]));

        var result = UriScheme.ValidateUri(PayPrefix + "&memo=" + memo + "&memo_type=" + memoType);

        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(result.Reason, "exactly 32 bytes");
        Assert.IsTrue(UriScheme.ValidateUri(PayPrefix + "&memo=" +
                                            Uri.EscapeDataString(Convert.ToBase64String(new byte[32])) +
                                            "&memo_type=" + memoType).IsValid);
    }

    [TestMethod]
    [DataRow("bob*example.com")]
    [DataRow("bob@mail.example.com*stellar.example.org")]
    public void ParseUri_FederationAddressDestination_IsValid(string address)
    {
        var request = UriScheme.ParseUri("web+stellar:pay?destination=" + Uri.EscapeDataString(address));

        Assert.AreEqual(address, request.Destination);
    }

    [TestMethod]
    [DataRow("*example.com")]
    [DataRow("bob*")]
    [DataRow("bob*localhost")]
    [DataRow("bob*foo.localhost")]
    [DataRow("bo b*example.com")]
    [DataRow("bob")]
    public void ParseUri_MalformedFederationAddressDestination_IsInvalid(string address)
    {
        Assert.IsFalse(UriScheme.ValidateUri("web+stellar:pay?destination=" + Uri.EscapeDataString(address)).IsValid);
    }

    [TestMethod]
    public void ParseUri_LowerCaseStrkeys_AreInvalid()
    {
        // The decoder accepts lower case, but a lower-case key compares unequal to the same key elsewhere.
        var lower = Destination.ToLowerInvariant();

        Assert.IsFalse(UriScheme.ValidateUri("web+stellar:pay?destination=" + lower).IsValid);
        Assert.IsFalse(UriScheme.ValidateUri(PayPrefix + "&asset_code=USD&asset_issuer=" + lower).IsValid);
        Assert.IsFalse(UriScheme.ValidateUri(TxPrefix + "&pubkey=" + lower).IsValid);
        Assert.IsTrue(UriScheme.ValidateUri(TxPrefix + "&pubkey=" + Destination).IsValid);
    }

    [TestMethod]
    [DataRow("example.xn--p1ai")]
    [DataRow("xn--e1afmkfd.xn--p1ai")]
    public void ValidateUri_OriginDomain_AcceptsPunycodeTlds(string domain)
    {
        Assert.IsTrue(UriScheme.ValidateUri(PayPrefix + "&origin_domain=" + domain + WellFormedSignature).IsValid);
    }

    [TestMethod]
    public void ParseUri_OriginDomain_RejectsNamesLongerThan253()
    {
        var domain = string.Join(".", Enumerable.Repeat(new string('a', 63), 4)) + ".com"; // 259 characters

        Assert.ThrowsException<InvalidOriginDomainException>(() =>
            UriScheme.ParseUri(PayPrefix + "&origin_domain=" + domain + WellFormedSignature));
    }

    [TestMethod]
    [DataRow("url%3Ahttps%3A%2F%2Fcb.example.com%0A")]
    [DataRow("url%3A%20https%3A%2F%2Fcb.example.com")]
    [DataRow("url%3Ahttps%3A%2F%2Fuser%3Apass%40cb.example.com%2F")]
    public void ValidateUri_CallbackWithWhitespaceOrCredentials_IsInvalid(string callback)
    {
        Assert.IsFalse(UriScheme.ValidateUri(PayPrefix + "&callback=" + callback).IsValid);
    }

    [TestMethod]
    public void ValidateUri_HttpCallback_IsValidSep7()
    {
        // SEP-7 allows http; SubmitToCallbackAsync is where plain http is refused (except loopback).
        Assert.IsTrue(UriScheme.ValidateUri(PayPrefix + "&callback=url%3Ahttp%3A%2F%2Fcb.example.com").IsValid);
    }

    [TestMethod]
    public void ParseUri_OriginDomainInChain_KeepsItsExceptionType()
    {
        var inner = PayPrefix + "&origin_domain=localhost" + WellFormedSignature;

        var ex = Assert.ThrowsException<InvalidOriginDomainException>(() =>
            UriScheme.ParseUri(TxPrefix + "&chain=" + Uri.EscapeDataString(inner)));

        Assert.AreEqual("localhost", ex.OriginDomain);
    }

    [TestMethod]
    public void InvalidUri_ReasonIsBoundedAndEscaped()
    {
        var hostile = "web+stellar:" + new string('x', 200_000) + "\n\u001b[31m";

        var result = UriScheme.ValidateUri(hostile);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Reason!.Length < 1_000, $"reason length {result.Reason.Length}");
        Assert.IsFalse(result.Reason.Contains('\n') || result.Reason.Contains('\u001b'));
        Assert.IsInstanceOfType(result.Error, typeof(InvalidSep7UriException));
    }

    [TestMethod]
    public void ValidateUri_Valid_HasNoError()
    {
        var result = UriScheme.ValidateUri(PayPrefix);

        Assert.IsNull(result.Error);
        Assert.IsNull(result.Reason);
    }

    [TestMethod]
    public void ParseUri_TextMemo_LimitIsInUtf8Bytes()
    {
        var fourteenTwoByteChars = string.Concat(Enumerable.Repeat("%C3%A9", 14)); // 28 bytes
        var fifteenTwoByteChars = string.Concat(Enumerable.Repeat("%C3%A9", 15)); // 30 bytes, 15 characters

        Assert.IsTrue(UriScheme.ValidateUri(PayPrefix + "&memo=" + fourteenTwoByteChars).IsValid);
        Assert.IsFalse(UriScheme.ValidateUri(PayPrefix + "&memo=" + fifteenTwoByteChars).IsValid);
        Assert.IsTrue(UriScheme.ValidateUri(PayPrefix + "&memo=" + new string('a', 28)).IsValid);
        Assert.IsFalse(UriScheme.ValidateUri(PayPrefix + "&memo=" + new string('a', 29)).IsValid);
    }

    [TestMethod]
    [DataRow("%2B5")]
    [DataRow("%205")]
    [DataRow("5%20")]
    [DataRow("-1")]
    public void ParseUri_MemoIdMustBePlainDigits(string memo)
    {
        Assert.IsFalse(UriScheme.ValidateUri(PayPrefix + "&memo=" + memo + "&memo_type=MEMO_ID").IsValid);
    }

    // ---- batch-4 coverage ----

    [TestMethod]
    [DataRow("url%3Ahttps%3A%2F%2Fcb.example.com%2Fcb%7F")] // DEL: a control character that is not whitespace
    [DataRow("url%3Ahttps%3A%2F%2Fcb.example.com%2Fc%E2%80%8Bb")] // zero-width space (format)
    [DataRow("url%3Ahttps%3A%2F%2Fcb.example.com%2F%E2%80%AEbc")] // right-to-left override (format)
    [DataRow("url%3Ahttps%3A%2F%2F%40cb.example.com%2Fcb")] // empty user info
    public void ValidateUri_CallbackWithInvisibleCharactersOrEmptyUserInfo_IsInvalid(string callback)
    {
        Assert.IsFalse(UriScheme.ValidateUri(PayPrefix + "&callback=" + callback).IsValid);
    }

    // The URL a wallet shows must be the one POSTed to: HTTP never sends a fragment, and Uri re-escapes a stray '%'.
    [TestMethod]
    [DataRow("url%3Ahttps%3A%2F%2Fcb.example.com%2Fcb%23frag", "fragment")]
    [DataRow("url%3Ahttps%3A%2F%2Fevil.example%23.good.example", "fragment")] // reads as good.example, POSTs to evil
    [DataRow("url%3Ahttps%3A%2F%2Fcb.example.com%2Fcb%23", "fragment")] // empty trailing '#'
    [DataRow("url%3Ahttps%3A%2F%2Fcb.example.com%2F%ZZ", "'%'")] // requested as /%25ZZ
    [DataRow("url%3Ahttps%3A%2F%2Fcb.example.com%2Fa%2", "'%'")] // truncated escape
    [DataRow("url%3Ahttps%3A%2F%2Fcb.example.com%2F%2G", "'%'")] // second digit not hex
    [DataRow("url%3Ahttps%3A%2F%2Fcb.example.com%2F%G2", "'%'")] // first digit not hex
    [DataRow("url%3Ahttps%3A%2F%2Fcb.example.com%2F%2541%25ZZ", "'%'")] // a good escape before the bad one
    [DataRow("url%3Ahttps%3A%2F%2Fcb.example.com%2Fa%25", "'%'")] // decodes to a trailing bare '%'
    public void ValidateUri_CallbackWithFragmentOrMalformedEscape_IsInvalid(string callback, string reason)
    {
        var result = UriScheme.ValidateUri(PayPrefix + "&callback=" + callback);

        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(result.Reason, reason);
    }

    [TestMethod]
    [DataRow("url%3Ahttps%3A%2F%2Fcb.example.com%2Fa%252Fb")] // decodes to a well-formed escape, /a%2Fb
    [DataRow("url%3Ahttps%3A%2F%2Fcb.example.com%2F%25e0%25A4")] // well-formed escapes of any case
    [DataRow("url%3Ahttps%3A%2F%2Fcb.example.com%2F%25af%25AF%2509")] // hex range edges: a f A F 0 9
    public void ValidateUri_CallbackWithWellFormedEscapes_IsValid(string callback)
    {
        Assert.IsTrue(UriScheme.ValidateUri(PayPrefix + "&callback=" + callback).IsValid);
    }

    [TestMethod]
    public void ParseUri_LowerCaseMuxedAndContractDestinations_AreInvalid()
    {
        Assert.IsFalse(UriScheme.ValidateUri("web+stellar:pay?destination=" +
                                             Sep7TestVectors.TsMuxedDestination.ToLowerInvariant()).IsValid);
        Assert.IsTrue(UriScheme.ValidateUri("web+stellar:pay?destination=" + Sep7TestVectors.TsMuxedDestination)
            .IsValid);
        var contract = StrKey.EncodeContractId(new byte[32]);
        Assert.IsFalse(UriScheme.ValidateUri("web+stellar:pay?destination=" + contract.ToLowerInvariant()).IsValid);
        Assert.IsTrue(UriScheme.ValidateUri("web+stellar:pay?destination=" + contract).IsValid);
    }

    [TestMethod]
    [DataRow("a*b*example.com")] // SEP-2: the name cannot contain '*'
    [DataRow("a>b*example.com")] // nor '>'
    [DataRow("bo\u0007b*example.com")] // nor control characters
    [DataRow("bo​b*example.com")] // nor invisible formatting characters
    public void ParseUri_FederationNamesSep2Excludes_AreInvalid(string address)
    {
        Assert.IsFalse(UriScheme.ValidateUri("web+stellar:pay?destination=" + Uri.EscapeDataString(address)).IsValid);
    }

    [TestMethod]
    public void ParseUri_FederationNameWithNonAsciiCharacters_IsValid()
    {
        // SEP-2 names are printable UTF-8.
        var address = "jürgen*example.com";

        Assert.AreEqual(address,
            UriScheme.ParseUri("web+stellar:pay?destination=" + Uri.EscapeDataString(address)).Destination);
    }

    [TestMethod]
    [DataRow("example.xn--")]
    [DataRow("example.xn--p1ai-")]
    [DataRow("example.xn---")]
    public void ParseUri_OriginDomain_RejectsMalformedPunycodeTlds(string domain)
    {
        Assert.ThrowsException<InvalidOriginDomainException>(() =>
            UriScheme.ParseUri(PayPrefix + "&origin_domain=" + domain + WellFormedSignature));
    }

    [TestMethod]
    public void UntrustedValuesInMessages_AreBoundedAndEscaped_AtEverySite()
    {
        var huge = new string('9', 100_000) + "%0A";
        AssertBounded(UriScheme.ValidateUri(PayPrefix + "&amount=" + huge));
        AssertBounded(UriScheme.ValidateUri(PayPrefix + "&memo=x&memo_type=" + new string('M', 100_000) + "%0A"));
        AssertBounded(UriScheme.ValidateUri(TxPrefix + "&replace=" + new string('p', 100_000) + "%0A:X;X:h"));
        AssertBounded(UriScheme.ValidateUri(PayPrefix + "&origin_domain=" + new string('d', 100_000) + "%0A"));

        static void AssertBounded(Sep7ValidationResult result)
        {
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Reason!.Length < 1_000, $"reason length {result.Reason.Length}");
            Assert.IsFalse(result.Reason.Contains('\n'));
        }
    }

    // ---- batch-5 ----

    [TestMethod]
    [DataRow("\U000E0041")] // tag character, the invisible-text range
    [DataRow("\U0001D173")] // musical symbol begin beam (format)
    public void SupplementaryPlaneInvisibleCharacters_AreRejected(string hidden)
    {
        Assert.IsFalse(UriScheme.ValidateUri("web+stellar:pay?destination=" +
                                             Uri.EscapeDataString("bo" + hidden + "b*example.com")).IsValid);
        Assert.IsFalse(UriScheme.ValidateUri(PayPrefix + "&callback=" +
                                             Uri.EscapeDataString("url:https://cb.example.com/" + hidden)).IsValid);
    }

    [TestMethod]
    public void SupplementaryPlanePrintableCharacters_AreAcceptedInFederationNames()
    {
        Assert.IsTrue(UriScheme.ValidateUri("web+stellar:pay?destination=" +
                                            Uri.EscapeDataString("bob\U0001F680*example.com")).IsValid);
    }

    [TestMethod]
    public void ParseUri_OriginDomain_AcceptsA63CharacterPunycodeTld()
    {
        var tld = "xn--" + new string('a', 59); // 63 characters, the DNS maximum for a label

        Assert.IsTrue(UriScheme.ValidateUri(PayPrefix + "&origin_domain=example." + tld + WellFormedSignature).IsValid);
        Assert.IsFalse(UriScheme.ValidateUri(PayPrefix + "&origin_domain=example." + tld + "a" + WellFormedSignature)
            .IsValid);
    }

    [TestMethod]
    public void LoneSurrogates_AreRejected()
    {
        // Only a raw string can carry one: percent-decoding never produces a lone surrogate.
        Assert.IsFalse(UriScheme.ValidateUri("web+stellar:pay?destination=bo\uD800b*example.com").IsValid);
        Assert.IsFalse(UriScheme.ValidateUri(PayPrefix + "&callback=url:https://cb.example.com/\uDC00").IsValid);
    }
}
