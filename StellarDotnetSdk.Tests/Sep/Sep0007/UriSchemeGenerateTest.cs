using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Memos;
using StellarDotnetSdk.Sep.Sep0007;
using StellarDotnetSdk.Tests.Sep.Sep0007.Fixtures;
using StellarDotnetSdk.Transactions;

namespace StellarDotnetSdk.Tests.Sep.Sep0007;

[TestClass]
public class UriSchemeGenerateTest
{
    private const string Destination = "GCALNQQBXAPZ2WIRSDDBMSTAKCUH5SG6U76YBFLQLIXJTF7FE5AX7AOO";

    [TestMethod]
    public void GeneratePayOperationUri_SonesoVector_MatchesExactly()
    {
        var uri = UriScheme.GeneratePayOperationUri(Sep7TestVectors.SonesoAccountId, "123.21", "ANA",
            Sep7TestVectors.SonesoAssetIssuer);

        Assert.AreEqual(Sep7TestVectors.SonesoExpectedPayUri, uri);
    }

    [TestMethod]
    public void GeneratePayOperationUri_SpecPayExample1_MatchesExactly()
    {
        var uri = UriScheme.GeneratePayOperationUri(Destination, "120.1234567", memo: Memo.Text("skdjfasf"),
            message: "pay me with lumens");

        Assert.AreEqual(Sep7TestVectors.SpecPayExample1, uri);
    }

    [TestMethod]
    public void GeneratePayOperationUri_SpecPayExample2_MatchesExactly()
    {
        var uri = UriScheme.GeneratePayOperationUri(Destination, "120.123", "USD",
            "GCRCUE2C5TBNIPYHMEP7NK5RWTT2WBSZ75CMARH7GDOHDDCQH3XANFOB", Memo.Text("hasysda987fs"),
            "url:https://someSigningService.com/hasysda987fs?asset=USD");

        Assert.AreEqual(Sep7TestVectors.SpecPayExample2, uri);
    }

    [TestMethod]
    public void GeneratePayOperationUri_EmitsParametersInSpecOrder()
    {
        var uri = UriScheme.GeneratePayOperationUri(Destination, "1", "USD",
            "GCRCUE2C5TBNIPYHMEP7NK5RWTT2WBSZ75CMARH7GDOHDDCQH3XANFOB", Memo.Id(7), "https://cb.example.com",
            "hi", Network.TestnetPassphrase, "example.com");

        var names = uri.Substring(uri.IndexOf('?') + 1).Split('&').Select(p => p.Split('=')[0]).ToArray();
        CollectionAssert.AreEqual(new[]
        {
            "destination", "amount", "asset_code", "asset_issuer", "memo", "memo_type", "callback", "msg",
            "network_passphrase", "origin_domain",
        }, names);
        StringAssert.Contains(uri, "&callback=url%3Ahttps%3A%2F%2Fcb.example.com&");
        StringAssert.Contains(uri, "&network_passphrase=Test%20SDF%20Network%20%3B%20September%202015&");
    }

    [TestMethod]
    public void GeneratePayOperationUri_EncodesEachMemoTypeAsSpecified()
    {
        var hash = Enumerable.Range(0, 32).Select(i => (byte)(i * 7)).ToArray();

        var text = UriScheme.ParseUri(UriScheme.GeneratePayOperationUri(Destination, memo: Memo.Text("a b&c")));
        var id = UriScheme.ParseUri(UriScheme.GeneratePayOperationUri(Destination, memo: Memo.Id(ulong.MaxValue)));
        var hashMemo = UriScheme.ParseUri(UriScheme.GeneratePayOperationUri(Destination, memo: Memo.Hash(hash)));
        var returnMemo = UriScheme.ParseUri(UriScheme.GeneratePayOperationUri(Destination,
            memo: Memo.ReturnHash(hash)));
        var none = UriScheme.ParseUri(UriScheme.GeneratePayOperationUri(Destination, memo: Memo.None()));

        Assert.AreEqual(("a b&c", Sep7MemoType.MemoText), (text.Memo, text.MemoType));
        Assert.AreEqual(("18446744073709551615", Sep7MemoType.MemoId), (id.Memo, id.MemoType));
        // Hash memos carry base64 of the raw 32 bytes (not of their hex text).
        Assert.AreEqual((Convert.ToBase64String(hash), Sep7MemoType.MemoHash), (hashMemo.Memo, hashMemo.MemoType));
        Assert.AreEqual((Convert.ToBase64String(hash), Sep7MemoType.MemoReturn),
            (returnMemo.Memo, returnMemo.MemoType));
        Assert.IsNull(none.Memo);
        Assert.IsNull(none.MemoType);
        CollectionAssert.AreEqual(hash, ((MemoHash)hashMemo.GetMemo()!).MemoBytes);
        CollectionAssert.AreEqual(hash, ((MemoReturnHash)returnMemo.GetMemo()!).MemoBytes);
    }

    [TestMethod]
    public void GeneratePayOperationUri_EmptyOptionalValues_AreOmitted()
    {
        // Only values whose absence means nothing else; see Generate_EmptyValueThatChangesTheRequest_Throws.
        var uri = UriScheme.GeneratePayOperationUri(Destination, "", message: "");

        Assert.AreEqual("web+stellar:pay?destination=" + Destination, uri);
    }

    [TestMethod]
    [DataRow("not-an-account", null, null, null, "'destination'")]
    [DataRow(Destination, "0", null, null, "'amount'")]
    [DataRow(Destination, null, "USD", null, "must be given together")]
    [DataRow(Destination, null, null, "not.a.domain.123", "origin_domain")]
    public void GeneratePayOperationUri_InvalidInput_ThrowsArgumentException(string destination, string? amount,
        string? assetCode, string? originDomain, string reasonFragment)
    {
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            UriScheme.GeneratePayOperationUri(destination, amount, assetCode, originDomain: originDomain));

        StringAssert.Contains(ex.Message, reasonFragment);
        Assert.IsNotNull(ex.InnerException);
    }

    [TestMethod]
    public void GeneratePayOperationUri_MessageTooLong_Throws()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            UriScheme.GeneratePayOperationUri(Destination, message: new string('m', 301)));
    }

    [TestMethod]
    public void GenerateSignTransactionUri_SpecTxExample1_MatchesExactly()
    {
        var uri = UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr,
            callback: "url:https://someSigningService.com/a8f7asdfkjha",
            publicKey: "GAU2ZSYYEYO5S5ZQSMMUENJ2TANY4FPXYGGIMU6GMGKTNVDG5QYFW6JS", message: "order number 24");

        Assert.AreEqual(Sep7TestVectors.SpecTxExample1, uri);
    }

    [TestMethod]
    public void GenerateSignTransactionUri_SpecTxExample2_MatchesExactly()
    {
        var uri = UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr,
            new[] { new Sep7Replacement("X", "sourceAccount", "account on which to create the trustline") });

        Assert.AreEqual(Sep7TestVectors.SpecTxExample2, uri);
    }

    [TestMethod]
    public void GenerateSignTransactionUri_SpecReplace_EncodesAsSpecPrints()
    {
        var replacements = UriScheme.ParseReplacements(Sep7TestVectors.SpecReplace);

        var uri = UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr, replacements);

        StringAssert.EndsWith(uri, "&replace=" + Sep7TestVectors.SpecReplaceEncoded);
    }

    [TestMethod]
    public void GenerateSignTransactionUri_ChainWithUnsignedOriginDomain_Throws()
    {
        // The requester's own origin_domain is signed afterwards, but a chained request is someone else's and must
        // already be valid; otherwise no wallet could parse the result.
        var ex = Assert.ThrowsException<ArgumentException>(() => UriScheme.GenerateSignTransactionUri(
            Sep7TestVectors.SpecTxXdr, chain: Sep7TestVectors.SpecUnsignedPayUri, originDomain: "example.com"));

        StringAssert.Contains(ex.Message, "'signature'");
        Assert.IsTrue(UriScheme.ValidateUri(UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr,
            chain: Sep7TestVectors.SpecSignedPayUri)).IsValid);
    }

    [TestMethod]
    public void GenerateSignTransactionUri_AllParameters_RoundTrip()
    {
        var chain = Sep7TestVectors.SpecPayExample1;
        var uri = UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr,
            new[] { new Sep7Replacement("SRC", "sourceAccount", "source account") },
            "https://example.com/cb?id=1&x=y", Destination, chain, "sign please", Network.TestnetPassphrase,
            "example.com");
        using var signer = KeyPair.Random();

        // A request with an origin_domain is only valid once signed.
        var request = UriScheme.ParseUri(UriScheme.SignUri(uri, signer));

        Assert.AreEqual(Sep7OperationType.Tx, request.OperationType);
        Assert.AreEqual(Sep7TestVectors.SpecTxXdr, request.Xdr);
        Assert.AreEqual("sourceAccount:SRC;SRC:source account", request.Replace);
        Assert.AreEqual("https://example.com/cb?id=1&x=y", request.CallbackUrl);
        Assert.AreEqual(Destination, request.PublicKey);
        Assert.AreEqual(chain, request.Chain);
        Assert.AreEqual(Sep7OperationType.Pay, request.ChainedUri!.OperationType);
        Assert.AreEqual("sign please", request.Message);
        Assert.AreEqual(Network.TestnetPassphrase, request.NetworkPassphrase);
        Assert.AreEqual("example.com", request.OriginDomain);
    }

    [TestMethod]
    public void GenerateSignTransactionUri_FromUnsignedTransaction_EncodesUnsignedEnvelope()
    {
        var transaction = TransactionBuilder.FromEnvelopeXdr(Sep7TestVectors.SpecTxXdr);

        var uri = UriScheme.GenerateSignTransactionUri(transaction);

        Assert.AreEqual(transaction.ToUnsignedEnvelopeXdrBase64(), UriScheme.ParseUri(uri).Xdr);
    }

    [TestMethod]
    public void GenerateSignTransactionUri_FromSignedTransaction_KeepsSignatures()
    {
        using var signer = KeyPair.Random();
        var transaction = TransactionBuilder.FromEnvelopeXdr(Sep7TestVectors.SpecTxXdr);
        transaction.Sign(signer, Network.Test());

        var uri = UriScheme.GenerateSignTransactionUri(transaction);

        Assert.AreEqual(1, UriScheme.ParseUri(uri).GetTransaction().Signatures.Count);
    }

    [TestMethod]
    public void GenerateSignTransactionUri_InvalidInput_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(() => UriScheme.GenerateSignTransactionUri("not-xdr"));
        Assert.ThrowsException<ArgumentException>(() =>
            UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr, publicKey: "GABC"));
        Assert.ThrowsException<ArgumentException>(() =>
            UriScheme.GenerateSignTransactionUri(Sep7TestVectors.SpecTxXdr, chain: "https://example.com"));
        Assert.ThrowsException<ArgumentException>(() => UriScheme.GenerateSignTransactionUri(""));
        Assert.ThrowsException<ArgumentNullException>(() =>
            UriScheme.GenerateSignTransactionUri((TransactionBase)null!));
    }

    // ---- minor-finding fixes ----

    [TestMethod]
    [DataRow("callback")]
    [DataRow("publicKey")]
    [DataRow("networkPassphrase")]
    [DataRow("originDomain")]
    public void Generate_EmptyValueThatChangesTheRequest_Throws(string argument)
    {
        // Omitting these would silently make a different request: no callback = submit to Horizon, no pubkey =
        // any signer, no network_passphrase = the public network, no origin_domain = unattributed.
        var ex = Assert.ThrowsException<ArgumentException>(() => UriScheme.GenerateSignTransactionUri(
            Sep7TestVectors.SpecTxXdr,
            callback: argument == "callback" ? "" : null,
            publicKey: argument == "publicKey" ? "" : null,
            networkPassphrase: argument == "networkPassphrase" ? "" : null,
            originDomain: argument == "originDomain" ? "" : null));

        Assert.AreEqual(argument, ex.ParamName);
        if (argument != "publicKey")
        {
            var payEx = Assert.ThrowsException<ArgumentException>(() => UriScheme.GeneratePayOperationUri(
                Destination,
                callback: argument == "callback" ? "" : null,
                networkPassphrase: argument == "networkPassphrase" ? "" : null,
                originDomain: argument == "originDomain" ? "" : null));
            Assert.AreEqual(argument, payEx.ParamName);
        }
    }

    [TestMethod]
    public void GeneratePayOperationUri_EmptyTextMemo_Throws()
    {
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            UriScheme.GeneratePayOperationUri(Destination, memo: Memo.Text("")));

        StringAssert.Contains(ex.Message, "empty text memo");
    }

    [TestMethod]
    [DataRow(new byte[] { 0x61, 0xFF, 0x62 })]
    [DataRow(new byte[] { 0xC3 })] // truncated two-byte sequence
    [DataRow(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF })]
    [DataRow(new byte[] { 0xED, 0xA0, 0x80 })] // encoded surrogate
    public void GeneratePayOperationUri_TextMemoThatIsNotValidUtf8_Throws(byte[] bytes)
    {
        // Decoding would replace the bad bytes with U+FFFD, so the URI would carry a different memo.
        var ex = Assert.ThrowsException<ArgumentException>(() =>
            UriScheme.GeneratePayOperationUri(Destination, memo: Memo.Text(bytes)));

        StringAssert.Contains(ex.Message, "valid UTF-8");
        Assert.AreEqual("memo", ex.ParamName);
    }

    [TestMethod]
    public void GeneratePayOperationUri_MultiByteUtf8TextMemo_RoundTripsByteForByte()
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes("é€😀");

        var request = UriScheme.ParseUri(UriScheme.GeneratePayOperationUri(Destination, memo: Memo.Text(bytes)));

        CollectionAssert.AreEqual(bytes, ((MemoText)request.GetMemo()!).MemoBytesValue);
    }

    [TestMethod]
    public void GeneratePayOperationUri_FederationDestination_RoundTrips()
    {
        var uri = UriScheme.GeneratePayOperationUri("bob*example.com", "10");

        Assert.AreEqual("web+stellar:pay?destination=bob%2Aexample.com&amount=10", uri);
        Assert.AreEqual("bob*example.com", UriScheme.ParseUri(uri).Destination);
    }
}
