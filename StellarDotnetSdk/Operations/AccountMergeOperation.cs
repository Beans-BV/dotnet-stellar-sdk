using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Xdr;
using MuxedAccount = StellarDotnetSdk.Accounts.MuxedAccount;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Transfers the XLM balance of an account to another account and removes the source account from the ledger.
///     <p>Use <see cref="Builder" /> to create a new <c>AccountMergeOperation</c>.</p>
///     <p>
///         See:
///         <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#account-merge">Account merge</a>
///     </p>
/// </summary>
public class AccountMergeOperation : Operation
{
    private AccountMergeOperation(IAccountId destination)
    {
        Destination = destination ?? throw new ArgumentNullException(nameof(destination), "destination cannot be null");
    }

    /// <summary>
    ///     The account that receives the remaining XLM balance of the source account.
    /// </summary>
    public IAccountId Destination { get; }

    public override OperationThreshold Threshold => OperationThreshold.HIGH;

    /// <summary>
    ///     Returns the Account Merge XDR Operation Body
    /// </summary>
    /// <returns></returns>
    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.ACCOUNT_MERGE),
            Destination = Destination.MuxedAccount
        };
    }

    /// <summary>
    ///     Builder for <c>AccountMergeOperation</c>.
    /// </summary>
    public class Builder
    {
        private readonly IAccountId _destination;
        private KeyPair? _mSourceAccount;

        /// <summary>
        ///     Constructs a new <c>AccountMergeOperation</c> builder.
        /// </summary>
        /// <param name="muxedAccount">An <c>Xdr.MuxedAccount</c> object.</param>
        public Builder(Xdr.MuxedAccount muxedAccount)
        {
            _destination = MuxedAccount.FromXdrMuxedAccount(muxedAccount);
        }

        /// <summary>
        ///     Constructs a new <c>AccountMergeOperation</c> builder.
        /// </summary>
        /// <param name="destination">The account that receives the remaining XLM balance of the source account.</param>
        public Builder(IAccountId destination)
        {
            _destination = destination;
        }

        /// <summary>
        ///     Sets the source account for this operation.
        /// </summary>
        /// <param name="sourceAccount">The operation's source account.</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetSourceAccount(KeyPair sourceAccount)
        {
            _mSourceAccount = sourceAccount ??
                              throw new ArgumentNullException(nameof(sourceAccount), "sourceAccount cannot be null");
            return this;
        }

        /// <summary>
        ///     Builds an operation.
        /// </summary>
        public AccountMergeOperation Build()
        {
            var operation = new AccountMergeOperation(_destination);
            if (_mSourceAccount != null)
                operation.SourceAccount = _mSourceAccount;
            return operation;
        }
    }
}