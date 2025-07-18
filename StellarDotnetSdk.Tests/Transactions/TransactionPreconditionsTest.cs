using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Transactions;
using xdrSDK = StellarDotnetSdk.Xdr;
using FormatException = StellarDotnetSdk.Exceptions.FormatException;

namespace StellarDotnetSdk.Tests.Transactions;

[TestClass]
public class TransactionPreconditionsTest
{
    [TestMethod]
    public void ItConvertsFromXdr()
    {
        var preconditions = new xdrSDK.Preconditions
        {
            Discriminant = xdrSDK.PreconditionType.Create(xdrSDK.PreconditionType.PreconditionTypeEnum.PRECOND_V2),
            V2 = new xdrSDK.PreconditionsV2
            {
                ExtraSigners = [],
                MinSeqAge = new xdrSDK.Duration(new xdrSDK.Uint64(2L)),
                LedgerBounds = new xdrSDK.LedgerBounds
                {
                    MinLedger = new xdrSDK.Uint32(1),
                    MaxLedger = new xdrSDK.Uint32(2),
                },
                MinSeqNum = new xdrSDK.SequenceNumber(new xdrSDK.Int64(4L)),
                MinSeqLedgerGap = new xdrSDK.Uint32(0),
            },
        };

        var stream = new xdrSDK.XdrDataOutputStream();
        xdrSDK.Preconditions.Encode(stream, preconditions);
        preconditions = xdrSDK.Preconditions.Decode(new xdrSDK.XdrDataInputStream(stream.ToArray()));

        var transactionPreconditions = TransactionPreconditions.FromXdr(preconditions);
        Assert.IsNotNull(transactionPreconditions);
        Assert.AreEqual(transactionPreconditions.MinSequenceAge, 2UL);
        Assert.IsNotNull(transactionPreconditions.LedgerBounds);
        Assert.AreEqual(transactionPreconditions.LedgerBounds.MinLedger, 1U);
        Assert.AreEqual(transactionPreconditions.LedgerBounds.MaxLedger, 2U);
        Assert.AreEqual(transactionPreconditions.MinSequenceNumber, 4L);
        Assert.AreEqual(transactionPreconditions.MinSequenceLedgerGap, 0U);
    }

    [TestMethod]
    public void ItRoundTripsFromV2ToV1IfOnlyTimeBoundsPresent()
    {
        const ulong minTime = 1UL;
        const ulong maxTime = 2UL;
        var preconditions = new xdrSDK.Preconditions
        {
            Discriminant = xdrSDK.PreconditionType.Create(xdrSDK.PreconditionType.PreconditionTypeEnum.PRECOND_V2),
            V2 = new xdrSDK.PreconditionsV2
            {
                TimeBounds = new xdrSDK.TimeBounds
                {
                    MinTime = new xdrSDK.TimePoint(new xdrSDK.Uint64(minTime)),
                    MaxTime = new xdrSDK.TimePoint(new xdrSDK.Uint64(maxTime)),
                },
                MinSeqLedgerGap = new xdrSDK.Uint32(0),
                MinSeqAge = new xdrSDK.Duration(new xdrSDK.Uint64(0L)),
                ExtraSigners = [],
            },
        };

        var stream = new xdrSDK.XdrDataOutputStream();
        xdrSDK.Preconditions.Encode(stream, preconditions);
        preconditions = xdrSDK.Preconditions.Decode(new xdrSDK.XdrDataInputStream(stream.ToArray()));

        var transactionPreconditions = TransactionPreconditions.FromXdr(preconditions);
        Assert.IsNotNull(transactionPreconditions);
        Assert.AreEqual(transactionPreconditions.TimeBounds, new TimeBounds(1L, 2L));

        preconditions = transactionPreconditions.ToXdr();

        Assert.AreEqual(preconditions.Discriminant.InnerValue,
            xdrSDK.PreconditionType.PreconditionTypeEnum.PRECOND_TIME);
        Assert.AreEqual(minTime, preconditions.TimeBounds.MinTime.InnerValue.InnerValue);
        Assert.AreEqual(maxTime, preconditions.TimeBounds.MaxTime.InnerValue.InnerValue);
        Assert.IsNull(preconditions.V2);
    }

    [TestMethod]
    public void ItConvertsToV2Xdr()
    {
        var payload = Util.HexToBytes("0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20");
        var signerKey = new xdrSDK.SignerKey
        {
            Discriminant =
                xdrSDK.SignerKeyType.Create(xdrSDK.SignerKeyType.SignerKeyTypeEnum
                    .SIGNER_KEY_TYPE_ED25519_SIGNED_PAYLOAD),
            Ed25519SignedPayload = new xdrSDK.SignerKey.SignerKeyEd25519SignedPayload
            {
                Ed25519 = new xdrSDK.Uint256(
                    StrKey.DecodeEd25519PublicKey("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR")),
                Payload = payload,
            },
        };

        var preconditions = new TransactionPreconditions
        {
            TimeBounds = new TimeBounds(1, 2),
            MinSequenceNumber = 3,
            ExtraSigners = new List<xdrSDK.SignerKey> { signerKey, signerKey, signerKey },
        };

        var xdr = preconditions.ToXdr();

        var stream = new xdrSDK.XdrDataOutputStream();
        xdrSDK.Preconditions.Encode(stream, xdr);
        xdr = xdrSDK.Preconditions.Decode(new xdrSDK.XdrDataInputStream(stream.ToArray()));

        Assert.AreEqual(xdr.Discriminant.InnerValue, xdrSDK.PreconditionType.PreconditionTypeEnum.PRECOND_V2);
        Assert.AreEqual(xdr.V2.TimeBounds.MinTime.InnerValue.InnerValue, 1UL);
        Assert.AreEqual(xdr.V2.TimeBounds.MaxTime.InnerValue.InnerValue, 2UL);
        Assert.AreEqual(xdr.V2.MinSeqNum.InnerValue.InnerValue, 3L);
        Assert.AreEqual(xdr.V2.MinSeqLedgerGap.InnerValue, 0U);
        Assert.AreEqual(xdr.V2.MinSeqAge.InnerValue.InnerValue, 0UL);
        Assert.AreEqual(xdr.V2.ExtraSigners.Length, 3);
    }

    [TestMethod]
    public void ItConvertsOnlyTimeBoundsXdr()
    {
        var preconditions = new TransactionPreconditions
        {
            TimeBounds = new TimeBounds(1, 2),
        };

        var xdr = preconditions.ToXdr();

        var stream = new xdrSDK.XdrDataOutputStream();
        xdrSDK.Preconditions.Encode(stream, xdr);
        xdr = xdrSDK.Preconditions.Decode(new xdrSDK.XdrDataInputStream(stream.ToArray()));

        Assert.AreEqual(xdr.Discriminant.InnerValue, xdrSDK.PreconditionType.PreconditionTypeEnum.PRECOND_TIME);
        Assert.AreEqual(xdr.TimeBounds.MinTime.InnerValue.InnerValue, 1UL);
        Assert.AreEqual(xdr.TimeBounds.MaxTime.InnerValue.InnerValue, 2UL);
        Assert.IsNull(xdr.V2);
    }

    [TestMethod]
    public void ItConvertsNullTimeBoundsXdr()
    {
        var preconditions = new TransactionPreconditions();
        var xdr = preconditions.ToXdr();

        var stream = new xdrSDK.XdrDataOutputStream();
        xdrSDK.Preconditions.Encode(stream, xdr);
        xdr = xdrSDK.Preconditions.Decode(new xdrSDK.XdrDataInputStream(stream.ToArray()));

        Assert.AreEqual(xdr.Discriminant.InnerValue, xdrSDK.PreconditionType.PreconditionTypeEnum.PRECOND_NONE);
        Assert.IsNull(xdr.TimeBounds);
    }

    [TestMethod]
    public void ItChecksNonValidityOfExtraSignersSize()
    {
        var preconditions = new TransactionPreconditions
        {
            TimeBounds = new TimeBounds(1, 2),
            ExtraSigners = new List<xdrSDK.SignerKey> { new(), new(), new() },
        };
        Assert.ThrowsException<FormatException>(() => preconditions.IsValid());
    }

    [TestMethod]
    public void ItChecksV2Status()
    {
        var preconditions = new xdrSDK.Preconditions
        {
            Discriminant = xdrSDK.PreconditionType.Create(xdrSDK.PreconditionType.PreconditionTypeEnum.PRECOND_V2),
            V2 = new xdrSDK.PreconditionsV2
            {
                ExtraSigners = [],
                MinSeqAge = new xdrSDK.Duration(new xdrSDK.Uint64(2L)),
                LedgerBounds = new xdrSDK.LedgerBounds
                {
                    MinLedger = new xdrSDK.Uint32(1),
                    MaxLedger = new xdrSDK.Uint32(2),
                },
                MinSeqNum = new xdrSDK.SequenceNumber(new xdrSDK.Int64(4L)),
            },
        };

        var transactionPreconditions = TransactionPreconditions.FromXdr(preconditions);
        Assert.IsNotNull(transactionPreconditions);
        Assert.IsTrue(transactionPreconditions.HasV2());
    }
}