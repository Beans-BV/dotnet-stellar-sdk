using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using MuxedAccount = StellarDotnetSdk.Accounts.MuxedAccount;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Burns an amount in a specific asset from a receiving account.
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/transactions/list-of-operations#clawback">Clawback</a>
/// </summary>
public class ClawbackOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>ClawbackOperation</c>.
    /// </summary>
    /// <param name="asset">The asset to claw back.</param>
    /// <param name="amount">The amount to claw back.</param>
    /// <param name="from">The account to claw back the amount from.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public ClawbackOperation(Asset asset, string amount, IAccountId from, IAccountId? sourceAccount = null) :
        base(sourceAccount)
    {
        Asset = asset;
        Amount = amount;
        From = from;
    }

    /// <summary>
    ///     The asset to claw.
    /// </summary>
    public Asset Asset { get; }

    /// <summary>
    ///     Amount to claw back.
    /// </summary>
    public string Amount { get; }

    /// <summary>
    ///     The account to claw back the amount from.
    /// </summary>
    public IAccountId From { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CLAWBACK),
            ClawbackOp = new ClawbackOp
            {
                Amount = new Int64(ToXdrAmount(Amount)),
                Asset = Asset.ToXdr(),
                From = From.MuxedAccount,
            },
        };
    }

    public static ClawbackOperation FromXdr(ClawbackOp clawbackOp)
    {
        return new ClawbackOperation(
            Asset.FromXdr(clawbackOp.Asset),
            StellarDotnetSdk.Amount.FromXdr(clawbackOp.Amount.InnerValue),
            MuxedAccount.FromXdrMuxedAccount(clawbackOp.From)
        );
    }
}