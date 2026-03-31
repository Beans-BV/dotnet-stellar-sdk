using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Xdr;
using MuxedAccount = StellarDotnetSdk.Accounts.MuxedAccount;
using xdr_Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Transactions;

/// <summary>
///     Represents a fee bump transaction that wraps an inner transaction with a higher fee to prioritize inclusion in a
///     ledger.
/// </summary>
public class FeeBumpTransaction : TransactionBase
{
    internal FeeBumpTransaction(IAccountId feeSource, Transaction innerTx, long fee)
    {
        FeeSource = feeSource;
        InnerTransaction = innerTx;
        Fee = fee;
    }

    /// <summary>
    ///     Gets the total fee (in stroops) for the fee bump transaction, covering all operations in the inner transaction.
    /// </summary>
    public long Fee { get; }

    /// <summary>
    ///     Gets the account that pays the fee for this fee bump transaction.
    /// </summary>
    public IAccountId FeeSource { get; }

    /// <summary>
    ///     Gets the original inner transaction being wrapped by this fee bump.
    /// </summary>
    public Transaction InnerTransaction { get; }

    /// <inheritdoc />
    public override byte[] SignatureBase(Network network)
    {
        if (network == null)
        {
            throw new NoNetworkSelectedException();
        }

        // Hashed NetworkID
        var networkHash = new Hash(network.NetworkId);
        var taggedTransaction = new TransactionSignaturePayload.TransactionSignaturePayloadTaggedTransaction
        {
            Discriminant = EnvelopeType.Create(EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX_FEE_BUMP),
            FeeBump = ToXdr(),
        };

        var txSignature = new TransactionSignaturePayload
        {
            NetworkId = networkHash,
            TaggedTransaction = taggedTransaction,
        };

        var writer = new XdrDataOutputStream();
        TransactionSignaturePayload.Encode(writer, txSignature);
        return writer.ToArray();
    }

    /// <summary>
    ///     Serializes this fee bump transaction to its XDR representation.
    /// </summary>
    /// <returns>An XDR <see cref="Xdr.FeeBumpTransaction" /> object.</returns>
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
                Signatures = InnerTransaction.Signatures.ToArray(),
            },
        };

        var ext = new Xdr.FeeBumpTransaction.FeeBumpTransactionExt { Discriminant = 0 };

        return new Xdr.FeeBumpTransaction
        {
            Fee = fee,
            FeeSource = feeSource,
            InnerTx = inner,
            Ext = ext,
        };
    }

    /// <inheritdoc />
    public override TransactionEnvelope ToEnvelopeXdr(TransactionXdrVersion version = TransactionXdrVersion.V1)
    {
        if (Signatures.Count == 0)
        {
            throw new NotEnoughSignaturesException(
                "FeeBumpTransaction must be signed by at least one signer. Use transaction.sign().");
        }

        return ToEnvelopeXdr(Signatures.ToArray());
    }

    /// <inheritdoc />
    public override TransactionEnvelope ToUnsignedEnvelopeXdr(TransactionXdrVersion version = TransactionXdrVersion.V1)
    {
        if (Signatures.Count > 0)
        {
            throw new TooManySignaturesException("FeeBumpTransaction must not be signed. Use ToEnvelopeXdr().");
        }
        return ToEnvelopeXdr(Array.Empty<DecoratedSignature>());
    }

    private TransactionEnvelope ToEnvelopeXdr(DecoratedSignature[] signatures)
    {
        return new TransactionEnvelope
        {
            Discriminant = EnvelopeType.Create(EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX_FEE_BUMP),
            FeeBump = new FeeBumpTransactionEnvelope { Tx = ToXdr(), Signatures = signatures },
        };
    }

    /// <summary>
    ///     Decodes a <see cref="FeeBumpTransaction" /> from a base64-encoded transaction envelope XDR string.
    /// </summary>
    /// <param name="envelope">The base64-encoded XDR transaction envelope.</param>
    /// <returns>The decoded <see cref="FeeBumpTransaction" />.</returns>
    public static FeeBumpTransaction FromEnvelopeXdr(string envelope)
    {
        var bytes = Convert.FromBase64String(envelope);

        var transactionEnvelope = TransactionEnvelope.Decode(new XdrDataInputStream(bytes));
        return FromEnvelopeXdr(transactionEnvelope);
    }

    /// <summary>
    ///     Decodes a <see cref="FeeBumpTransaction" /> from a <see cref="TransactionEnvelope" /> XDR object.
    /// </summary>
    /// <param name="envelope">The XDR transaction envelope (must be <c>ENVELOPE_TYPE_TX_FEE_BUMP</c>).</param>
    /// <returns>The decoded <see cref="FeeBumpTransaction" /> with its signatures.</returns>
    /// <exception cref="ArgumentException">Thrown when the envelope type is not a fee bump.</exception>
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
                    foreach (var signature in envelope.FeeBump.Signatures)
                    {
                        transaction.Signatures.Add(signature);
                    }

                    return transaction;
                default:
                    throw new ArgumentException(
                        $"Invalid TransactionEnvelope: expected an ENVELOPE_TYPE_TX_FEE_BUMP but received {envelope.Discriminant.InnerValue}");
            }
        }
    }
}