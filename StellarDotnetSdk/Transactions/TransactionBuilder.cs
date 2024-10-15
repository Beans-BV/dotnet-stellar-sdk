using System;
using System.Collections.Concurrent;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using memos_Memo = StellarDotnetSdk.Memos.Memo;
using Operations_Operation = StellarDotnetSdk.Operations.Operation;
using Soroban_SorobanTransactionData = StellarDotnetSdk.Soroban.SorobanTransactionData;

namespace StellarDotnetSdk.Transactions;

public class TransactionBuilder
{
    private const uint BaseFee = 100;

    private readonly BlockingCollection<Operations_Operation> _operations;
    private readonly ITransactionBuilderAccount _sourceAccount;
    private uint _fee;
    private memos_Memo? _memo;
    private TransactionPreconditions _preconditions;
    private Soroban_SorobanTransactionData? _sorobanData;

    /// <summary>
    ///     Construct a new transaction builder.
    /// </summary>
    /// <param name="sourceAccount">
    ///     The source account for this transaction. This account is the account
    ///     who will use a sequence number. When build() is called, the account object's sequence number will be incremented.
    /// </param>
    public TransactionBuilder(ITransactionBuilderAccount sourceAccount)
    {
        _sourceAccount = sourceAccount ??
                         throw new ArgumentNullException(nameof(sourceAccount), "sourceAccount cannot be null");
        _operations = new BlockingCollection<Operations_Operation>();
        _fee = BaseFee;
        _preconditions = new TransactionPreconditions();
    }

    public int OperationsCount => _operations.Count;

    public static FeeBumpTransaction BuildFeeBumpTransaction(IAccountId feeSource, Transaction inner)
    {
        if (inner.Operations.Length == 0)
        {
            throw new Exception("Invalid fee bump transaction, it should contain at least one operation");
        }

        var innerFee = inner.Fee / inner.Operations.Length;
        var feeBumpFee = checked(innerFee * (inner.Operations.Length + 1));

        return new FeeBumpTransaction(feeSource, inner, feeBumpFee);
    }

    public static FeeBumpTransaction BuildFeeBumpTransaction(IAccountId feeSource, Transaction inner, long fee)
    {
        if (inner.Operations.Length == 0)
        {
            throw new Exception("Invalid fee bump transaction, it should contain at least one operation");
        }

        var innerFee = inner.Fee / inner.Operations.Length;
        if (fee < innerFee)
        {
            throw new Exception($"Invalid fee, it should be at least {innerFee} stroops");
        }

        var feeBumpFee = checked(fee * (inner.Operations.Length + 1));

        return new FeeBumpTransaction(feeSource, inner, feeBumpFee);
    }

    /// <summary>
    ///     Adds a new operation to this transaction.
    ///     See:
    ///     <a href="https://developers.stellar.org/docs/learn/fundamentals/transactions/list-of-operations">List of Operations</a>
    /// </summary>
    /// <param name="operation">The operation to be added.</param>
    /// <returns>Builder object so you can chain methods.</returns>
    public TransactionBuilder AddOperation(Operations_Operation operation)
    {
        if (operation == null)
        {
            throw new ArgumentNullException(nameof(operation), "operation cannot be null");
        }

        _operations.Add(operation);
        return this;
    }

    public TransactionBuilder AddPreconditions(TransactionPreconditions preconditions)
    {
        _preconditions = preconditions ??
                         throw new ArgumentNullException(nameof(preconditions), "preconditions cannot be null");
        return this;
    }

    /// <summary>
    ///     Adds a memo to this transaction.
    ///     See: https://www.stellar.org/developers/learn/concepts/transactions.html
    /// </summary>
    /// <param name="memo">Memo</param>
    /// <returns>Builder object so you can chain methods.</returns>
    public TransactionBuilder AddMemo(memos_Memo memo)
    {
        if (_memo != null)
        {
            throw new ArgumentException("Memo has been already added.", nameof(memo));
        }

        _memo = memo ?? throw new ArgumentNullException(nameof(memo), "memo cannot be null");

        return this;
    }

    /// <summary>
    ///     Adds a time-bounds to this transaction.
    ///     See: https://www.stellar.org/developers/learn/concepts/transactions.html
    /// </summary>
    /// <param name="timeBounds">timeBounds</param>
    /// <returns>Builder object so you can chain methods.</returns>
    public TransactionBuilder AddTimeBounds(TimeBounds timeBounds)
    {
        if (timeBounds == null)
        {
            throw new ArgumentNullException(nameof(timeBounds), "timeBounds cannot be null");
        }

        if (_preconditions.TimeBounds != null)
        {
            throw new Exception("TimeBounds already set.");
        }

        _preconditions.TimeBounds = timeBounds;

        return this;
    }

    /// <summary>
    ///     Set the transaction fee (in Stroops) per operation.
    ///     See: https://www.stellar.org/developers/learn/concepts/transactions.html
    /// </summary>
    /// <param name="fee">fee (in Stroops) for each operation in the transaction</param>
    /// <returns>Builder object so you can chain methods.</returns>
    public TransactionBuilder SetFee(uint fee)
    {
        if (_fee <= 0)
        {
            throw new ArgumentException("Fee must be a positive amount", nameof(fee));
        }

        _fee = fee;

        return this;
    }

    /// <summary>
    ///     Sets the transaction's internal <see cref="Soroban.SorobanTransactionData" /> (resources, footprint, etc.).
    /// </summary>
    /// <param name="sorobanData">an <see cref="Soroban.SorobanTransactionData" /> object containing the data.</param>
    /// <returns>Builder object so you can chain methods.</returns>
    /// <seealso
    ///     href="https://soroban.stellar.org/docs/fundamentals-and-concepts/invoking-contracts-with-transactions#transaction-resources" />
    public TransactionBuilder SetSorobanTransactionData(Soroban_SorobanTransactionData sorobanData)
    {
        _sorobanData = sorobanData;
        return this;
    }

    /// <summary>
    ///     Builds a transaction. It will increment sequence number of the source account.
    /// </summary>
    /// <returns></returns>
    public Transaction Build()
    {
        var operations = _operations.ToArray();

        //var totalFee = operations.Length * _fee;
        var opsCount = Convert.ToUInt32(operations.Length);
        var totalFee = checked(opsCount * _fee);
        var transaction = new Transaction(_sourceAccount.MuxedAccount, totalFee,
            _sourceAccount.IncrementedSequenceNumber, operations, _memo, _preconditions, _sorobanData);
        // Increment sequence number when there were no exceptions when creating a transaction
        _sourceAccount.IncrementSequenceNumber();
        return transaction;
    }

    public static TransactionBase FromEnvelopeXdr(string envelope)
    {
        var bytes = Convert.FromBase64String(envelope);

        var transactionEnvelope = TransactionEnvelope.Decode(new XdrDataInputStream(bytes));
        return FromEnvelopeXdr(transactionEnvelope);
    }

    public static TransactionBase FromEnvelopeXdr(TransactionEnvelope envelope)
    {
        return envelope.Discriminant.InnerValue switch
        {
            EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX_V0 => Transaction.FromEnvelopeXdr(envelope),
            EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX => Transaction.FromEnvelopeXdr(envelope),
            EnvelopeType.EnvelopeTypeEnum.ENVELOPE_TYPE_TX_FEE_BUMP => FeeBumpTransaction.FromEnvelopeXdr(envelope),
            _ => throw new ArgumentException($"Unknown envelope type {envelope.Discriminant.InnerValue}"),
        };
    }
}