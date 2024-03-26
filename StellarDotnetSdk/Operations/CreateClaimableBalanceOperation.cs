using System.Linq;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using claimant_Claimant = StellarDotnetSdk.Claimant.Claimant;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Moves an amount of asset from the operation source account into a new ClaimableBalanceEntry.
///     <p>Use <see cref="Builder" /> to create a new <c>CreateClaimableBalanceOperation</c>.</p>
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#create-claimable-balance">
///         Create claimable balance
///     </a>
/// </summary>
public class CreateClaimableBalanceOperation : Operation
{
    private CreateClaimableBalanceOperation(Asset asset, string amount, claimant_Claimant[] claimants)
    {
        Asset = asset;
        Amount = amount;
        Claimants = claimants;
    }

    /// <summary>
    /// Asset that will be held in the ClaimableBalanceEntry.
    /// </summary>
    public Asset Asset { get; }
    
    /// <summary>
    /// Amount of <c>Asset</c> stored in the ClaimableBalanceEntry.
    /// </summary>
    public string Amount { get; }
    
    /// <summary>
    /// List of <c>Claimant</c>s (account address and ClaimPredicate pair) that can claim this ClaimableBalanceEntry.
    /// </summary>
    public claimant_Claimant[] Claimants { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CREATE_CLAIMABLE_BALANCE),
            CreateClaimableBalanceOp = new CreateClaimableBalanceOp
            {
                Amount = new Int64(ToXdrAmount(Amount)),
                Asset = Asset.ToXdr(),
                Claimants = Claimants.Select(claimant => claimant.ToXdr()).ToArray()
            }
        };
    }

    /// <summary>
    ///     Builder for <c>CreateClaimableBalanceOperation</c> operation.
    /// </summary>
    public class Builder
    {
        private readonly string _amount;
        private readonly Asset _asset;
        private readonly claimant_Claimant[] _claimants;
        private KeyPair? _sourceAccount;

        /// <summary>
        ///     Constructs a new <c>CreateClaimableBalanceOperation</c> builder.
        /// </summary>
        /// <param name="createClaimableBalanceOp">A <c>CreateClaimableBalanceOp</c> XDR object.</param>
        public Builder(CreateClaimableBalanceOp createClaimableBalanceOp)
        {
            _asset = Asset.FromXdr(createClaimableBalanceOp.Asset);
            _amount = FromXdrAmount(createClaimableBalanceOp.Amount.InnerValue);
            _claimants = createClaimableBalanceOp.Claimants.Select(claimant_Claimant.FromXdr).ToArray();
        }

        /// <summary>
        ///     Constructs a new <c>ClawbackClaimableBalanceOperation</c> builder.
        /// </summary>
        /// <param name="asset">Asset that will be held in the ClaimableBalanceEntry.</param>
        /// <param name="amount">The amount of <c>asset</c> stored in the ClaimableBalanceEntry.</param>
        /// <param name="claimants">The claimants that can claim this ClaimableBalanceEntry.</param>
        public Builder(Asset asset, string amount, claimant_Claimant[] claimants)
        {
            _asset = asset;
            _amount = amount;
            _claimants = claimants;
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
        public CreateClaimableBalanceOperation Build()
        {
            var operation = new CreateClaimableBalanceOperation(_asset, _amount, _claimants);
            if (_sourceAccount != null)
                operation.SourceAccount = _sourceAccount;
            return operation;
        }
    }
}