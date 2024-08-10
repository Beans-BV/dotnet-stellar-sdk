using System;
using System.Linq;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Xdr;
using Memo = StellarDotnetSdk.Memos.Memo;
using MuxedAccount = StellarDotnetSdk.Accounts.MuxedAccount;
using Operation = StellarDotnetSdk.Operations.Operation;
using SorobanAuthorizationEntry = StellarDotnetSdk.Operations.SorobanAuthorizationEntry;
using SorobanTransactionData = StellarDotnetSdk.Soroban.SorobanTransactionData;
using Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Transactions;

public class Transaction : TransactionBase
{
    internal Transaction(IAccountId sourceAccount, uint fee, long sequenceNumber,
        Operation[] operations, Memo? memo, TransactionPreconditions? preconditions)
    {
        SourceAccount = sourceAccount ??
                        throw new ArgumentNullException(nameof(sourceAccount), "sourceAccount cannot be null");
        Fee = fee;
        SequenceNumber = sequenceNumber;
        Operations = operations ?? throw new ArgumentNullException(nameof(operations), "operations cannot be null");

        if (operations.Length == 0)
        {
            throw new ArgumentNullException(nameof(operations), "At least one operation required");
        }

        Memo = memo ?? Memo.None();
        Preconditions = preconditions;
    }

    internal Transaction(IAccountId sourceAccount, uint fee, long sequenceNumber, Operation[] operations,
        Memo? memo, TransactionPreconditions? preconditions, SorobanTransactionData? sorobanData)
        : this(sourceAccount, fee, sequenceNumber, operations, memo, preconditions)
    {
        SorobanTransactionData = sorobanData;
    }

    public uint Fee { get; private set; }

    public IAccountId SourceAccount { get; }

    public long SequenceNumber { get; }

    public Operation[] Operations { get; }

    public Memo Memo { get; }

    public TransactionPreconditions? Preconditions { get; }

    public TimeBounds? TimeBounds => Preconditions?.TimeBounds;

    public SorobanTransactionData? SorobanTransactionData { get; private set; }

    public void AddResourceFee(uint resourceFee)
    {
        Fee += resourceFee;
    }

    public void SetSorobanAuthorization(SorobanAuthorizationEntry[] auth)
    {
        foreach (var operation in Operations)
        {
            if (operation is InvokeHostFunctionOperation invokeHostFunctionOperation)
            {
                invokeHostFunctionOperation.Auth = auth;
            }
        }
    }

    public void SetSorobanTransactionData(SorobanTransactionData sorobanTransactionData)
    {
        SorobanTransactionData = sorobanTransactionData;
    }

    /// <summary>
    ///     Returns signature base for the given network.
    /// </summary>
    /// <param name="network">The network <see cref="Network" /> the transaction will be sent to.</param>
    public override byte[] SignatureBase(Network? network)
    {
        if (network == null)
        {
            throw new NoNetworkSelectedException();
        }

        // Hashed NetworkID
        var networkHash = new Hash { InnerValue = network.NetworkId };
        var taggedTransaction = new TransactionSignaturePayload.TransactionSignaturePayloadTaggedTransaction
        {
            Discriminant = EnvelopeType.Create(EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX),
            Tx = ToXdrV1(),
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
    ///     Generates Transaction XDR object.
    /// </summary>
    public TransactionV0 ToXdr()
    {
        return ToXdrV0();
    }

    /// <summary>
    ///     Generates Transaction XDR object.
    /// </summary>
    public TransactionV0 ToXdrV0()
    {
        if (SourceAccount is not KeyPair)
        {
            throw new Exception("TransactionEnvelope V0 expects a KeyPair source account");
        }

        return new TransactionV0
        {
            Fee = new Uint32(Fee),
            SeqNum = new SequenceNumber(new Int64(SequenceNumber)),
            SourceAccountEd25519 = new Uint256(SourceAccount.PublicKey),
            Operations = Operations.Select(x => x.ToXdr()).ToArray(),
            Memo = Memo.ToXdr(),
            TimeBounds = TimeBounds?.ToXdr(),
            Ext = new TransactionV0.TransactionV0Ext { Discriminant = 0 },
        };
    }

    /// <summary>
    ///     Generates Transaction XDR object.
    /// </summary>
    public Xdr.Transaction ToXdrV1()
    {
        // ext
        var ext = new Xdr.Transaction.TransactionExt { Discriminant = 0 };
        if (SorobanTransactionData != null)
        {
            ext.Discriminant = 1;
            ext.SorobanData = SorobanTransactionData.ToXdr();
        }

        return new Xdr.Transaction
        {
            Fee = new Uint32(Fee),
            SeqNum = new SequenceNumber(new Int64(SequenceNumber)),
            SourceAccount = SourceAccount.MuxedAccount,
            Operations = Operations.Select(x => x.ToXdr()).ToArray(),
            Memo = Memo.ToXdr(),
            Cond = Preconditions?.ToXdr() ?? new Preconditions(),
            Ext = ext,
        };
    }

    /// <summary>
    ///     Generates TransactionEnvelope XDR object. Transaction need to have at least one signature.
    /// </summary>
    public override TransactionEnvelope ToEnvelopeXdr(TransactionXdrVersion version = TransactionXdrVersion.V1)
    {
        if (Signatures.Count == 0)
        {
            throw new NotEnoughSignaturesException(
                "Transaction must be signed by at least one signer. Use transaction.Sign().");
        }

        return ToEnvelopeXdr(version, Signatures.ToArray());
    }

    /// <summary>
    ///     Generates TransactionEnvelope XDR object. This transaction MUST be signed before being useful
    /// </summary>
    public override TransactionEnvelope ToUnsignedEnvelopeXdr(
        TransactionXdrVersion version = TransactionXdrVersion.V1)
    {
        if (Signatures.Count > 0)
        {
            throw new TooManySignaturesException("Transaction must not be signed. Use ToEnvelopeXDR.");
        }

        return ToEnvelopeXdr(version, Array.Empty<DecoratedSignature>());
    }

    private TransactionEnvelope ToEnvelopeXdr(TransactionXdrVersion version, DecoratedSignature[] signatures)
    {
        return version switch
        {
            TransactionXdrVersion.V0 => new TransactionEnvelope
            {
                Discriminant = EnvelopeType.Create(EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX_V0),
                V0 = new TransactionV0Envelope { Tx = ToXdrV0(), Signatures = signatures },
            },
            TransactionXdrVersion.V1 => new TransactionEnvelope
            {
                Discriminant = EnvelopeType.Create(EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX),
                V1 = new TransactionV1Envelope { Tx = ToXdrV1(), Signatures = signatures },
            },
            _ => throw new ArgumentException($"Invalid TransactionXdrVersion {version}", nameof(version)),
        };
    }

    public static Transaction FromEnvelopeXdr(string envelope)
    {
        var bytes = Convert.FromBase64String(envelope);

        var transactionEnvelope = TransactionEnvelope.Decode(new XdrDataInputStream(bytes));
        return FromEnvelopeXdr(transactionEnvelope);
    }

    public static Transaction FromEnvelopeXdr(TransactionEnvelope envelope)
    {
        {
            return envelope.Discriminant.InnerValue switch
            {
                EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX_V0 => FromEnvelopeXdrV0(envelope.V0),
                EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX => FromEnvelopeXdrV1(envelope.V1),
                _ => throw new ArgumentException(
                    $"Invalid TransactionEnvelope: expected an ENVELOPE_TYPE_TX or ENVELOPE_TYPE_TX_V0 but received {envelope.Discriminant.InnerValue}"),
            };
        }
    }

    public static Transaction FromEnvelopeXdrV0(TransactionV0Envelope envelope)
    {
        var transactionXdr = envelope.Tx;
        var fee = transactionXdr.Fee.InnerValue;
        var sourceAccount = KeyPair.FromPublicKey(transactionXdr.SourceAccountEd25519.InnerValue);
        var sequenceNumber = transactionXdr.SeqNum.InnerValue.InnerValue;
        var memo = Memo.FromXdr(transactionXdr.Memo);
        var preconditions = transactionXdr.TimeBounds != null
            ? new TransactionPreconditions { TimeBounds = TimeBounds.FromXdr(transactionXdr.TimeBounds) }
            : null;

        var operations = new Operation[transactionXdr.Operations.Length];
        for (var i = 0; i < transactionXdr.Operations.Length; i++)
        {
            operations[i] = Operation.FromXdr(transactionXdr.Operations[i]);
        }

        var transaction = new Transaction(sourceAccount, fee, sequenceNumber, operations, memo, preconditions);

        foreach (var signature in envelope.Signatures)
        {
            transaction.Signatures.Add(signature);
        }

        return transaction;
    }

    public static Transaction FromEnvelopeXdrV1(TransactionV1Envelope envelope)
    {
        var transactionXdr = envelope.Tx;
        var fee = transactionXdr.Fee.InnerValue;
        var sourceAccount = MuxedAccount.FromXdrMuxedAccount(transactionXdr.SourceAccount);
        var sequenceNumber = transactionXdr.SeqNum.InnerValue.InnerValue;
        var memo = Memo.FromXdr(transactionXdr.Memo);
        var preconditions = TransactionPreconditions.FromXdr(transactionXdr.Cond);

        var operations = new Operation[transactionXdr.Operations.Length];
        for (var i = 0; i < transactionXdr.Operations.Length; i++)
        {
            operations[i] = Operation.FromXdr(transactionXdr.Operations[i]);
        }

        var sorobanData = transactionXdr.Ext.SorobanData != null
            ? SorobanTransactionData.FromXdr(transactionXdr.Ext.SorobanData)
            : null;

        var transaction =
            new Transaction(sourceAccount, fee, sequenceNumber, operations, memo, preconditions, sorobanData);

        foreach (var signature in envelope.Signatures)
        {
            transaction.Signatures.Add(signature);
        }

        return transaction;
    }
}