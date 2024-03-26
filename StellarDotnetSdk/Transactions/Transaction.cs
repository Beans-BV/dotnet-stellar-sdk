using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Exceptions;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Xdr;
using memos_Memo = StellarDotnetSdk.Memos.Memo;
using MuxedAccount = StellarDotnetSdk.Accounts.MuxedAccount;
using Operation = StellarDotnetSdk.Xdr.Operation;
using Operations_Operation = StellarDotnetSdk.Operations.Operation;
using Operations_SorobanAuthorizationEntry = StellarDotnetSdk.Operations.SorobanAuthorizationEntry;
using Soroban_SorobanTransactionData = StellarDotnetSdk.Soroban.SorobanTransactionData;
using xdr_Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Transactions;

public class Transaction : TransactionBase
{
    internal Transaction(IAccountId sourceAccount, uint fee, long sequenceNumber, Operations_Operation[] operations,
        memos_Memo? memo,
        TransactionPreconditions preconditions)
    {
        SourceAccount = sourceAccount ??
                        throw new ArgumentNullException(nameof(sourceAccount), "sourceAccount cannot be null");
        Fee = fee;
        SequenceNumber = sequenceNumber;
        Operations = operations ?? throw new ArgumentNullException(nameof(operations), "operations cannot be null");

        if (operations.Length == 0)
            throw new ArgumentNullException(nameof(operations), "At least one operation required");

        Memo = memo ?? memos_Memo.None();
        Preconditions = preconditions;
    }

    internal Transaction(IAccountId sourceAccount, uint fee, long sequenceNumber, Operations_Operation[] operations,
        memos_Memo? memo,
        TransactionPreconditions preconditions, Soroban_SorobanTransactionData? sorobanData) : this(sourceAccount, fee,
        sequenceNumber, operations, memo, preconditions)
    {
        SorobanTransactionData = sorobanData;
    }

    public uint Fee { get; private set; }

    public IAccountId SourceAccount { get; }

    public long SequenceNumber { get; }

    public Operations_Operation[] Operations { get; }

    public memos_Memo? Memo { get; }

    public TransactionPreconditions Preconditions { get; }

    public TimeBounds? TimeBounds => Preconditions.TimeBounds;

    public Soroban_SorobanTransactionData? SorobanTransactionData { get; set; }

    public void AddResourceFee(uint resourceFee)
    {
        Fee += resourceFee;
    }

    public void SetSorobanAuthorization(Operations_SorobanAuthorizationEntry[] auth)
    {
        foreach (var operation in Operations)
            if (operation is InvokeHostFunctionOperation invokeHostFunctionOperation)
                invokeHostFunctionOperation.Auth = auth;
    }

    /// <summary>
    ///     Returns signature base for the given network.
    /// </summary>
    /// <param name="network">The network <see cref="Network" /> the transaction will be sent to.</param>
    /// <returns></returns>
    public override byte[] SignatureBase(Network? network)
    {
        if (network == null)
            throw new NoNetworkSelectedException();

        // Hashed NetworkID
        var networkHash = new Hash { InnerValue = network.NetworkId };
        var taggedTransaction = new TransactionSignaturePayload.TransactionSignaturePayloadTaggedTransaction
        {
            Discriminant = EnvelopeType.Create(EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX),
            Tx = ToXdrV1()
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


    /// <summary>
    ///     Generates Transaction XDR object.
    /// </summary>
    /// <returns></returns>
    public TransactionV0 ToXdr()
    {
        return ToXdrV0();
    }

    /// <summary>
    ///     Generates Transaction XDR object.
    /// </summary>
    /// <returns></returns>
    public TransactionV0 ToXdrV0()
    {
        if (!(SourceAccount is KeyPair))
            throw new Exception("TransactionEnvelope V0 expects a KeyPair source account");

        // fee
        var fee = new Uint32 { InnerValue = Fee };

        // sequenceNumber
        var sequenceNumberUint = new xdr_Int64(SequenceNumber);
        var sequenceNumber = new SequenceNumber { InnerValue = sequenceNumberUint };

        // sourceAccount
        var sourceAccount = new Uint256(SourceAccount.PublicKey);

        // operations
        var operations = new Operation[Operations.Length];

        for (var i = 0; i < Operations.Length; i++)
            operations[i] = Operations[i].ToXdr();

        // ext
        var ext = new TransactionV0.TransactionV0Ext { Discriminant = 0 };

        var transaction = new TransactionV0
        {
            Fee = fee,
            SeqNum = sequenceNumber,
            SourceAccountEd25519 = sourceAccount,
            Operations = operations,
            Memo = Memo?.ToXdr(),
            TimeBounds = TimeBounds?.ToXdr(),
            Ext = ext
        };
        return transaction;
    }

    /// <summary>
    ///     Generates Transaction XDR object.
    /// </summary>
    /// <returns></returns>
    public Xdr.Transaction ToXdrV1()
    {
        // fee
        var fee = new Uint32 { InnerValue = Fee };

        // sequenceNumber
        var sequenceNumberUint = new xdr_Int64(SequenceNumber);
        var sequenceNumber = new SequenceNumber { InnerValue = sequenceNumberUint };

        // sourceAccount
        var sourceAccount = SourceAccount.MuxedAccount;

        // operations
        var operations = new Operation[Operations.Length];

        for (var i = 0; i < Operations.Length; i++)
            operations[i] = Operations[i].ToXdr();

        // ext
        var ext = new Xdr.Transaction.TransactionExt { Discriminant = 0 };
        if (SorobanTransactionData != null)
        {
            ext.Discriminant = 1;
            ext.SorobanData = SorobanTransactionData.ToXdr();
        }

        var transaction = new Xdr.Transaction
        {
            Fee = fee,
            SeqNum = sequenceNumber,
            SourceAccount = sourceAccount,
            Operations = operations,
            Memo = Memo?.ToXdr(),
            Cond = Preconditions.ToXDR(),
            Ext = ext
        };
        return transaction;
    }

    /// <summary>
    ///     Generates TransactionEnvelope XDR object. Transaction need to have at least one signature.
    /// </summary>
    /// <returns></returns>
    public override TransactionEnvelope ToEnvelopeXdr(TransactionXdrVersion version = TransactionXdrVersion.V1)
    {
        if (Signatures.Count == 0)
            throw new NotEnoughSignaturesException(
                "Transaction must be signed by at least one signer. Use transaction.sign().");

        return ToEnvelopeXdr(version, Signatures.ToArray());
    }

    /// <summary>
    ///     Generates TransactionEnvelope XDR object. This transaction MUST be signed before being useful
    /// </summary>
    /// <returns></returns>
    public override TransactionEnvelope ToUnsignedEnvelopeXdr(
        TransactionXdrVersion version = TransactionXdrVersion.V1)
    {
        if (Signatures.Count > 0)
            throw new TooManySignaturesException("Transaction must not be signed. Use ToEnvelopeXDR.");

        return ToEnvelopeXdr(version, new DecoratedSignature[0]);
    }

    private TransactionEnvelope ToEnvelopeXdr(TransactionXdrVersion version, DecoratedSignature[] signatures)
    {
        var thisXdr = new TransactionEnvelope();
        if (version == TransactionXdrVersion.V0)
        {
            thisXdr.Discriminant = new EnvelopeType
                { InnerValue = EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX_V0 };
            thisXdr.V0 = new TransactionV0Envelope();

            var transaction = ToXdrV0();
            thisXdr.V0.Tx = transaction;
            thisXdr.V0.Signatures = signatures;
        }
        else if (version == TransactionXdrVersion.V1)
        {
            thisXdr.Discriminant = new EnvelopeType { InnerValue = EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX };
            thisXdr.V1 = new TransactionV1Envelope();
            var transaction = ToXdrV1();
            thisXdr.V1.Tx = transaction;
            thisXdr.V1.Signatures = signatures;
        }
        else
        {
            throw new Exception($"Invalid TransactionXdrVersion {version}");
        }

        return thisXdr;
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
            switch (envelope.Discriminant.InnerValue)
            {
                case EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX_V0:
                    return FromEnvelopeXdrV0(envelope.V0);
                case EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX:
                    return FromEnvelopeXdrV1(envelope.V1);
                default:
                    throw new ArgumentException(
                        $"Invalid TransactionEnvelope: expected an ENVELOPE_TYPE_TX or ENVELOPE_TYPE_TX_V0 but received {envelope.Discriminant.InnerValue}");
            }
        }
    }

    public static Transaction FromEnvelopeXdrV0(TransactionV0Envelope envelope)
    {
        var transactionXdr = envelope.Tx;
        var fee = transactionXdr.Fee.InnerValue;
        var sourceAccount = KeyPair.FromPublicKey(transactionXdr.SourceAccountEd25519.InnerValue);
        var sequenceNumber = transactionXdr.SeqNum.InnerValue.InnerValue;
        var memo = transactionXdr.Memo != null ? memos_Memo.FromXdr(transactionXdr.Memo) : null;
        var preconditions = new TransactionPreconditions
        {
            TimeBounds = TimeBounds.FromXdr(transactionXdr.TimeBounds)
        };

        var operations = new Operations_Operation[transactionXdr.Operations.Length];
        for (var i = 0; i < transactionXdr.Operations.Length; i++)
            operations[i] = Operations_Operation.FromXdr(transactionXdr.Operations[i]);

        var transaction =
            new Transaction(sourceAccount, fee, sequenceNumber, operations, memo, preconditions);

        foreach (var signature in envelope.Signatures) transaction.Signatures.Add(signature);

        return transaction;
    }

    public static Transaction FromEnvelopeXdrV1(TransactionV1Envelope envelope)
    {
        var transactionXdr = envelope.Tx;
        var fee = transactionXdr.Fee.InnerValue;
        var sourceAccount = MuxedAccount.FromXdrMuxedAccount(transactionXdr.SourceAccount);
        var sequenceNumber = transactionXdr.SeqNum.InnerValue.InnerValue;
        var memo = memos_Memo.FromXdr(transactionXdr.Memo);
        var preconditions = TransactionPreconditions.FromXDR(transactionXdr.Cond);

        var operations = new Operations_Operation[transactionXdr.Operations.Length];
        for (var i = 0; i < transactionXdr.Operations.Length; i++)
            operations[i] = Operations_Operation.FromXdr(transactionXdr.Operations[i]);

        var sorobanData = transactionXdr.Ext.SorobanData != null
            ? Soroban_SorobanTransactionData.FromXdr(transactionXdr.Ext.SorobanData)
            : null;

        var transaction =
            new Transaction(sourceAccount, fee, sequenceNumber, operations, memo, preconditions, sorobanData);

        foreach (var signature in envelope.Signatures) transaction.Signatures.Add(signature);

        return transaction;
    }
}