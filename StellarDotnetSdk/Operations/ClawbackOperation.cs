using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using MuxedAccount = StellarDotnetSdk.Accounts.MuxedAccount;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Burns an amount in a specific asset from a receiving account.
///     <p>Use <see cref="Builder" /> to create a new <c>ClawbackOperation</c>.</p>
///     See: <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#clawback">Clawback</a>
/// </summary>
public class ClawbackOperation : Operation
{
    private ClawbackOperation(Asset asset, string amount, IAccountId from)
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
                From = From.MuxedAccount
            }
        };
    }

    /// <summary>
    ///     Builder for <c>ClawbackOperation</c>.
    /// </summary>
    public class Builder
    {
        private readonly string _amount;
        private readonly Asset _asset;
        private readonly IAccountId _from;
        private KeyPair? _sourceAccount;

        /// <summary>
        ///     Constructs a new <c>ClawbackClaimableBalanceOperation</c> builder.
        /// </summary>
        /// <param name="clawbackOp">A <c>ClawbackOp</c> XDR object.</param>
        public Builder(ClawbackOp clawbackOp)
        {
            _asset = Asset.FromXdr(clawbackOp.Asset);
            _amount = FromXdrAmount(clawbackOp.Amount.InnerValue);
            _from = MuxedAccount.FromXdrMuxedAccount(clawbackOp.From);
        }

        /// <summary>
        ///     Constructs a new <c>ClawbackClaimableBalanceOperation</c> builder.
        /// </summary>
        /// <param name="asset">The asset to claw back.</param>
        /// <param name="amount">The amount to claw back.</param>
        /// <param name="from">The account to claw back the amount from.</param>
        public Builder(Asset asset, string amount, IAccountId from)
        {
            _asset = asset;
            _amount = amount;
            _from = from;
        }

        /// <summary>
        ///     Sets the source account for this operation.
        /// </summary>
        /// <param name="account">The operation's source account.</param>
        /// <returns>Builder object so you can chain methods.</returns>
        public Builder SetSourceAccount(KeyPair account)
        {
            _sourceAccount = account;
            return this;
        }

        /// <summary>
        ///     Builds an operation.
        /// </summary>
        public ClawbackOperation Build()
        {
            var operation = new ClawbackOperation(_asset, _amount, _from);
            if (_sourceAccount != null)
                operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}