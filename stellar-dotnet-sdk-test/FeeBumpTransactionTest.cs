using System;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using stellar_dotnet_sdk;

namespace stellar_dotnet_sdk_test;

[TestClass]
public class FeeBumpTransactionTest
{
    [TestInitialize]
    public void Initialize()
    {
        BaseFee = 200;
        NetworkPassphrase = "Standalone Network ; February 2017";
        Network = new Network(NetworkPassphrase);
        InnerSource = KeyPair.FromSecretSeed(Network.NetworkId);
        InnerAccount = new Account(InnerSource.AccountId, 7);
        Destination = KeyPair.FromAccountId("GDQERENWDDSQZS7R7WKHZI3BSOYMV3FSWR7TFUYFTKQ447PIX6NREOJM");
        Amount = "2000.0000000";
        Asset = new AssetTypeNative();
        InnerTransaction = new TransactionBuilder(InnerAccount)
            .SetFee(100)
            .AddTimeBounds(new TimeBounds(0, 0))
            .AddOperation(
                new PaymentOperation.Builder(Destination, Asset, Amount)
                    .Build())
            .AddMemo(new MemoText("Happy birthday!"))
            .Build();
        InnerTransaction.Sign(InnerSource, Network);
        FeeSource = KeyPair.FromSecretSeed("SB7ZMPZB3YMMK5CUWENXVLZWBK4KYX4YU5JBXQNZSK2DP2Q7V3LVTO5V");
        Transaction = TransactionBuilder.BuildFeeBumpTransaction(FeeSource, InnerTransaction, 100);
    }

    [TestMethod]
    public void TestLessThanInnerBaseFeeRate()
    {
        var ex = Assert.ThrowsException<Exception>(() =>
            TransactionBuilder.BuildFeeBumpTransaction(FeeSource, InnerTransaction, 50));
        var innerOps = InnerTransaction.Operations.Length;
        var innerBaseFeeRate = InnerTransaction.Fee / innerOps;
        Assert.AreEqual(ex.Message, $"Invalid fee, it should be at least {innerBaseFeeRate} stroops");
    }

    [TestMethod]
    public void TestBuildFromTransactionEnvelope()
    {
        Transaction.Sign(FeeSource, Network);
        Assert.AreEqual(FeeSource, Transaction.FeeSource);
        Assert.AreEqual(200, Transaction.Fee);

        var expectedXdr =
            "AAAABQAAAADgSJG2GOUMy/H9lHyjYZOwyuyytH8y0wWaoc596L+bEgAAAAAAAADIAAAAAgAAAABzdv3ojkzWHMD7KUoXhrPx0GH18vHKV0ZfqpMiEblG1gAAAGQAAAAAAAAACAAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAQAAAA9IYXBweSBiaXJ0aGRheSEAAAAAAQAAAAAAAAABAAAAAOBIkbYY5QzL8f2UfKNhk7DK7LK0fzLTBZqhzn3ov5sSAAAAAAAAAASoF8gAAAAAAAAAAAERuUbWAAAAQK933Dnt1pxXlsf1B5CYn81PLxeYsx+MiV9EGbMdUfEcdDWUySyIkdzJefjpR5ejdXVp/KXosGmNUQ+DrIBlzg0AAAAAAAAAAei/mxIAAABAijIIQpL6KlFefiL4FP8UWQktWEz4wFgGNSaXe7mZdVMuiREntehi1b7MRqZ1h+W+Y0y+Z2HtMunsilT2yS5mAA==";
        Assert.AreEqual(expectedXdr, Transaction.ToEnvelopeXdrBase64());
    }

    [TestMethod]
    public void TestSign()
    {
        Transaction.Sign(FeeSource, Network);
        var xdr = Transaction.ToEnvelopeXdr();
        Assert.AreEqual(1, xdr.FeeBump.Signatures.Length);
        var rawSig = xdr.FeeBump.Signatures[0];
        Assert.IsTrue(FeeSource.Verify(Transaction.Hash(Network), rawSig.Signature));
    }

    [TestMethod]
    public void TestSignUsingPreImage()
    {
        var rng = RandomNumberGenerator.Create();
        var preImage = new byte[64];
        rng.GetBytes(preImage);
        Transaction.Sign(preImage);
        var xdr = Transaction.ToEnvelopeXdr();
        var rawSig = xdr.FeeBump.Signatures[0];
        CollectionAssert.AreEqual(preImage, rawSig.Signature.InnerValue);
    }

    [TestMethod]
    public void TestFromEnvelopeXdr()
    {
        var xdr =
            "AAAABQAAAADgSJG2GOUMy/H9lHyjYZOwyuyytH8y0wWaoc596L+bEgAAAAAAAADIAAAAAgAAAABzdv3ojkzWHMD7KUoXhrPx0GH18vHKV0ZfqpMiEblG1gAAAGQAAAAAAAAACAAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAQAAAA9IYXBweSBiaXJ0aGRheSEAAAAAAQAAAAAAAAABAAAAAOBIkbYY5QzL8f2UfKNhk7DK7LK0fzLTBZqhzn3ov5sSAAAAAAAAAASoF8gAAAAAAAAAAAERuUbWAAAAQK933Dnt1pxXlsf1B5CYn81PLxeYsx+MiV9EGbMdUfEcdDWUySyIkdzJefjpR5ejdXVp/KXosGmNUQ+DrIBlzg0AAAAAAAAAAei/mxIAAABAijIIQpL6KlFefiL4FP8UWQktWEz4wFgGNSaXe7mZdVMuiREntehi1b7MRqZ1h+W+Y0y+Z2HtMunsilT2yS5mAA==";
        var tx = TransactionBuilder.FromEnvelopeXdr(xdr);
        Assert.AreEqual(xdr, tx.ToEnvelopeXdrBase64());
    }

    [TestMethod]
    public void TestMuxedAccounts()
    {
        var muxed = new MuxedAccountMed25519(FeeSource, 0);
        var tx = TransactionBuilder.BuildFeeBumpTransaction(muxed, InnerTransaction, 100);
        var xdr = tx.ToUnsignedEnvelopeXdr();
        var txMuxed = MuxedAccount.FromXdrMuxedAccount(xdr.FeeBump.Tx.FeeSource);
        Assert.AreEqual(muxed.Address, txMuxed.Address);
    }

    [TestMethod]
    public void TestBaseFeeOverflowsLong()
    {
        var network = Network.Test();
        var innerTx = CreateInnerTransaction(100 + 1, network);
        var feeSource = KeyPair.FromAccountId("GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3");
        Assert.ThrowsException<OverflowException>(() =>
        {
            TransactionBuilder.BuildFeeBumpTransaction(feeSource, innerTx, long.MaxValue);
        });
    }

    [TestMethod]
    public void TestTransactionHash()
    {
        var network = Network.Test();
        var innerTx = CreateInnerTransaction(100, network);

        Assert.AreEqual("2a8ead3351faa7797b284f59027355ddd69c21adb8e4da0b9bb95531f7f32681",
            Util.BytesToHex(innerTx.Hash(network)).ToLowerInvariant());

        var feeSource = KeyPair.FromAccountId("GDQNY3PBOJOKYZSRMK2S7LHHGWZIUISD4QORETLMXEWXBI7KFZZMKTL3");
        var feeBumpTx = TransactionBuilder.BuildFeeBumpTransaction(feeSource, innerTx, 200);

        Assert.AreEqual("58266712c0c1d1cd98faa0e0159605a361cf2a5ca44ad69650eeb1d27ee62334",
            Util.BytesToHex(feeBumpTx.Hash(network)).ToLowerInvariant());
    }

    private Transaction CreateInnerTransaction(uint fee, Network network)
    {
        var source = KeyPair.FromSecretSeed("SCH27VUZZ6UAKB67BDNF6FA42YMBMQCBKXWGMFD5TZ6S5ZZCZFLRXKHS");
        var destination = KeyPair.FromAccountId("GA7QYNF7SOWQ3GLR2BGMZEHXAVIRZA4KVWLTJJFC7MGXUA74P7UJVSGZ");
        var account = new Account(source, 2908908335136768L);
        var innerTx = new TransactionBuilder(account)
            .AddOperation(new PaymentOperation.Builder(destination, new AssetTypeNative(), "200.0").Build())
            .SetFee(fee)
            .AddTimeBounds(new TimeBounds(10, 11))
            .Build();
        innerTx.Sign(source, network);
        return innerTx;
    }
#nullable disable
    private long BaseFee { get; set; }
    private string NetworkPassphrase { get; set; }
    private Network Network { get; set; }
    private KeyPair InnerSource { get; set; }
    private Account InnerAccount { get; set; }
    private KeyPair Destination { get; set; }
    private string Amount { get; set; }
    private Asset Asset { get; set; }
    private Transaction InnerTransaction { get; set; }
    private KeyPair FeeSource { get; set; }
    private FeeBumpTransaction Transaction { get; set; }
}