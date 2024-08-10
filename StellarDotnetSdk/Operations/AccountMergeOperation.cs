using System;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Transactions;
using StellarDotnetSdk.Xdr;
using MuxedAccount = StellarDotnetSdk.Accounts.MuxedAccount;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Transfers the XLM balance of an account to another account and removes the source account from the ledger.
///     <p>
///         See:
///         <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#account-merge">Account merge</a>
///     </p>
/// </summary>
public class AccountMergeOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>AccountMergeOperation</c>.
    /// </summary>
    /// <param name="destination">The account that receives the remaining XLM balance of the source account.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public AccountMergeOperation(IAccountId destination, IAccountId? sourceAccount = null) : base(sourceAccount)
    {
        Destination = destination ?? throw new ArgumentNullException(nameof(destination), "destination cannot be null");
    }

    /// <summary>
    ///     Constructs a new <c>AccountMergeOperation</c>.
    /// </summary>
    /// <param name="muxedAccount">An <c>Xdr.MuxedAccount</c> object representing the destination.</param>
    public AccountMergeOperation(Xdr.MuxedAccount muxedAccount) : base(null)
    {
        Destination = MuxedAccount.FromXdrMuxedAccount(muxedAccount);
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
            Destination = Destination.MuxedAccount,
        };
    }
}