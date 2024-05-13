﻿using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Xdr;
using MuxedAccount = StellarDotnetSdk.Accounts.MuxedAccount;
using xdr_Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Transactions;

public class FeeBumpTransaction : TransactionBase
{
    internal FeeBumpTransaction(IAccountId feeSource, Transaction innerTx, long fee)
    {
        FeeSource = feeSource;
        InnerTransaction = innerTx;
        Fee = fee;
    }

    public long Fee { get; }

    public IAccountId FeeSource { get; }

    public Transaction InnerTransaction { get; }

    public override byte[] SignatureBase(Network network)
    {
        if (network == null)
            throw new NoNetworkSelectedException();

        // Hashed NetworkID
        var networkHash = new Hash(network.NetworkId);
        var taggedTransaction = new TransactionSignaturePayload.TransactionSignaturePayloadTaggedTransaction
        {
            Discriminant = EnvelopeType.Create(EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX_FEE_BUMP),
            FeeBump = ToXdr()
        };

        var txSignature = new TransactionSignaturePayload
        {
            NetworkId = networkHash,
            TaggedTransaction = taggedTransaction
        };

        var writer = new XdrDataOutputStream();
        TransactionSignaturePayload.Encode(writer, txSignature);
        return writer.ToArray();
    }

    public Xdr.FeeBumpTransaction ToXdr()
    {
        var fee = new xdr_Int64(Fee);
        var feeSource = FeeSource.MuxedAccount;

        var inner = new Xdr.FeeBumpTransaction.FeeBumpTransactionInnerTx
        {
            Discriminant = EnvelopeType.Create(EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX),
            V1 = new TransactionV1Envelope
            {
                Tx = InnerTransaction.ToXdrV1(),
                Signatures = InnerTransaction.Signatures.ToArray()
            }
        };

        var ext = new Xdr.FeeBumpTransaction.FeeBumpTransactionExt { Discriminant = 0 };

        return new Xdr.FeeBumpTransaction
        {
            Fee = fee,
            FeeSource = feeSource,
            InnerTx = inner,
            Ext = ext
        };
    }

    public override TransactionEnvelope ToEnvelopeXdr(TransactionXdrVersion version = TransactionXdrVersion.V1)
    {
        if (Signatures.Count == 0)
            throw new NotEnoughSignaturesException(
                "FeeBumpTransaction must be signed by at least one signer. Use transaction.sign().");

        return ToEnvelopeXdr(Signatures.ToArray());
    }

    public override TransactionEnvelope ToUnsignedEnvelopeXdr(TransactionXdrVersion version = TransactionXdrVersion.V1)
    {
        if (Signatures.Count > 0)
            throw new TooManySignaturesException("FeeBumpTransaction must not be signed. Use ToEnvelopeXdr().");
        return ToEnvelopeXdr(Array.Empty<DecoratedSignature>());
    }

    private TransactionEnvelope ToEnvelopeXdr(DecoratedSignature[] signatures)
    {
        return new TransactionEnvelope
        {
            Discriminant = EnvelopeType.Create(EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX_FEE_BUMP),
            FeeBump = new FeeBumpTransactionEnvelope { Tx = ToXdr(), Signatures = signatures }
        };
    }

    public static FeeBumpTransaction FromEnvelopeXdr(string envelope)
    {
        var bytes = Convert.FromBase64String(envelope);

        var transactionEnvelope = TransactionEnvelope.Decode(new XdrDataInputStream(bytes));
        return FromEnvelopeXdr(transactionEnvelope);
    }

    public static FeeBumpTransaction FromEnvelopeXdr(TransactionEnvelope envelope)
    {
        {
            switch (envelope.Discriminant.InnerValue)
            {
                case EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX_FEE_BUMP:
                    var tx = envelope.FeeBump.Tx;
                    var fee = tx.Fee.InnerValue;
                    var feeSource = MuxedAccount.FromXdrMuxedAccount(tx.FeeSource);
                    var innerTx = Transaction.FromEnvelopeXdrV1(tx.InnerTx.V1);

                    var transaction = new FeeBumpTransaction(feeSource, innerTx, fee);
                    foreach (var signature in envelope.FeeBump.Signatures) transaction.Signatures.Add(signature);

                    return transaction;
                default:
                    throw new ArgumentException(
                        $"Invalid TransactionEnvelope: expected an ENVELOPE_TYPE_TX_FEE_BUMP but received {envelope.Discriminant.InnerValue}");
            }
        }
    }
}