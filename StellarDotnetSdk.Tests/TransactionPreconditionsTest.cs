using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StellarDotnetSdk.Transactions;
using xdrSDK = StellarDotnetSdk.Xdr;
using FormatException = StellarDotnetSdk.Exceptions.FormatException;

namespace StellarDotnetSdk.Tests;

[TestClass]
public class TransactionPreconditionsTest
{
    [TestMethod]
    public void ItConvertsFromXdr()
    {
        var preconditions = new xdrSDK.Preconditions
        {
            Discriminant =
            {
                InnerValue = xdrSDK.PreconditionType.PreconditionTypeEnum.PRECOND_V2
            }
        };

        var preconditionsV2 = new xdrSDK.PreconditionsV2
        {
            ExtraSigners = Array.Empty<xdrSDK.SignerKey>(),
            MinSeqAge = new xdrSDK.Duration(new xdrSDK.Uint64(2L)),
            LedgerBounds = new xdrSDK.LedgerBounds
            {
                MinLedger = new xdrSDK.Uint32(1),
                MaxLedger = new xdrSDK.Uint32(2)
            },
            MinSeqNum = new xdrSDK.SequenceNumber(new xdrSDK.Int64(4L)),
            MinSeqLedgerGap = new xdrSDK.Uint32(0)
        };
        preconditions.V2 = preconditionsV2;

        var stream = new xdrSDK.XdrDataOutputStream();
        xdrSDK.Preconditions.Encode(stream, preconditions);
        preconditions = xdrSDK.Preconditions.Decode(new xdrSDK.XdrDataInputStream(stream.ToArray()));

        var transactionPreconditions = TransactionPreconditions.FromXDR(preconditions);

        Assert.AreEqual(transactionPreconditions.MinSeqAge, 2UL);
        Assert.AreEqual(transactionPreconditions.LedgerBounds.MinLedger, 1U);
        Assert.AreEqual(transactionPreconditions.LedgerBounds.MaxLedger, 2U);
        Assert.AreEqual(transactionPreconditions.MinSeqNumber, 4L);
        Assert.AreEqual(transactionPreconditions.MinSeqLedgerGap, 0U);
    }

    [TestMethod]
    public void ItRoundTripsFromV2ToV1IfOnlyTimeBoundsPresent()
    {
        var preconditions = new xdrSDK.Preconditions
        {
            Discriminant =
            {
                InnerValue = xdrSDK.PreconditionType.PreconditionTypeEnum.PRECOND_V2
            }
        };

        var preconditionsV2 = new xdrSDK.PreconditionsV2();

        var xdrTimeBounds = new xdrSDK.TimeBounds
        {
            MinTime = new xdrSDK.TimePoint(new xdrSDK.Uint64(1L)),
            MaxTime = new xdrSDK.TimePoint(new xdrSDK.Uint64(2L))
        };
        preconditionsV2.TimeBounds = xdrTimeBounds;
        preconditionsV2.MinSeqLedgerGap = new xdrSDK.Uint32(0);
        preconditionsV2.MinSeqAge = new xdrSDK.Duration(new xdrSDK.Uint64(0L));
        preconditionsV2.ExtraSigners = Array.Empty<xdrSDK.SignerKey>();
        preconditions.V2 = preconditionsV2;

        var stream = new xdrSDK.XdrDataOutputStream();
        xdrSDK.Preconditions.Encode(stream, preconditions);
        preconditions = xdrSDK.Preconditions.Decode(new xdrSDK.XdrDataInputStream(stream.ToArray()));

        var transactionPreconditions = TransactionPreconditions.FromXDR(preconditions);
        Assert.AreEqual(transactionPreconditions.TimeBounds, new TimeBounds(1L, 2L));

        preconditions = transactionPreconditions.ToXDR();

        Assert.AreEqual(preconditions.Discriminant.InnerValue,
            xdrSDK.PreconditionType.PreconditionTypeEnum.PRECOND_TIME);
        Assert.AreEqual(preconditions.TimeBounds.MinTime.InnerValue.InnerValue,
            xdrTimeBounds.MinTime.InnerValue.InnerValue);
        Assert.AreEqual(preconditions.TimeBounds.MaxTime.InnerValue.InnerValue,
            xdrTimeBounds.MaxTime.InnerValue.InnerValue);
        Assert.IsNull(preconditions.V2);
    }

    [TestMethod]
    public void ItConvertsToV2Xdr()
    {
        var payload = Util.HexToBytes("0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20");
        var signerKey = new xdrSDK.SignerKey
        {
            Discriminant =
            {
                InnerValue = xdrSDK.SignerKeyType.SignerKeyTypeEnum.SIGNER_KEY_TYPE_ED25519_SIGNED_PAYLOAD
            },
            Ed25519SignedPayload = new xdrSDK.SignerKey.SignerKeyEd25519SignedPayload
            {
                Ed25519 = new xdrSDK.Uint256(
                    StrKey.DecodeStellarAccountId("GDW6AUTBXTOC7FIKUO5BOO3OGLK4SF7ZPOBLMQHMZDI45J2Z6VXRB5NR")),
                Payload = payload
            }
        };

        var preconditions = new TransactionPreconditions
        {
            TimeBounds = new TimeBounds(1, 2),
            MinSeqNumber = 3,
            ExtraSigners = new List<xdrSDK.SignerKey> { signerKey, signerKey, signerKey }
        };

        var xdr = preconditions.ToXDR();

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
            TimeBounds = new TimeBounds(1, 2)
        };

        var xdr = preconditions.ToXDR();

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
        var xdr = preconditions.ToXDR();

        var stream = new xdrSDK.XdrDataOutputStream();
        xdrSDK.Preconditions.Encode(stream, xdr);
        xdr = xdrSDK.Preconditions.Decode(new xdrSDK.XdrDataInputStream(stream.ToArray()));

        Assert.AreEqual(xdr.Discriminant.InnerValue, xdrSDK.PreconditionType.PreconditionTypeEnum.PRECOND_NONE);
        Assert.IsNull(xdr.TimeBounds);
    }

    [TestMethod]
    public void ItChecksValidityWhenTimeBounds()
    {
        var preconditions = new TransactionPreconditions
        {
            TimeBounds = new TimeBounds(1, 2)
        };
        preconditions.IsValid();
    }

    [TestMethod]
    public void ItChecksNonValidityOfTimeBounds()
    {
        var preconditions = new TransactionPreconditions();
        Assert.ThrowsException<FormatException>(() => preconditions.IsValid());
    }

    [TestMethod]
    public void ItChecksNonValidityOfExtraSignersSize()
    {
        var preconditions = new TransactionPreconditions
        {
            TimeBounds = new TimeBounds(1, 2),
            ExtraSigners = new List<xdrSDK.SignerKey> { new(), new(), new() }
        };
        Assert.ThrowsException<FormatException>(() => preconditions.IsValid());
    }

    [TestMethod]
    public void ItChecksValidityWhenNoTimeBoundsSet()
    {
        var preconditions = new TransactionPreconditions();
        var ex = Assert.ThrowsException<FormatException>(() => preconditions.IsValid());
        Assert.AreEqual("Invalid preconditions, must define time bounds.", ex.Message);
    }

    [TestMethod]
    public void ItChecksV2Status()
    {
        var preconditions = new xdrSDK.Preconditions
        {
            Discriminant =
            {
                InnerValue = xdrSDK.PreconditionType.PreconditionTypeEnum.PRECOND_V2
            }
        };
        var preconditionsV2 = new xdrSDK.PreconditionsV2
        {
            ExtraSigners = Array.Empty<xdrSDK.SignerKey>(),
            MinSeqAge = new xdrSDK.Duration(new xdrSDK.Uint64(2L)),
            LedgerBounds = new xdrSDK.LedgerBounds
            {
                MinLedger = new xdrSDK.Uint32(1),
                MaxLedger = new xdrSDK.Uint32(2)
            },
            MinSeqNum = new xdrSDK.SequenceNumber(new xdrSDK.Int64(4L))
        };

        preconditions.V2 = preconditionsV2;

        var transactionPreconditions = TransactionPreconditions.FromXDR(preconditions);
        Assert.IsTrue(transactionPreconditions.HasV2());
    }
}