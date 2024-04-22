using System.Linq;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

/// <summary>
///     Represents a <see cref="CreateClaimableBalanceOperation" />.
///     Use <see cref="Builder" /> to create a new CreateClaimableBalanceOperation.
///     See also:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#create-claimable-balance">
///         Create
///         Claimable Balance
///     </a>
/// </summary>
public class CreateClaimableBalanceOperation : Operation
{
    private CreateClaimableBalanceOperation(Asset asset, string amount, Claimant[] claimants)
    {
        Asset = asset;
        Amount = amount;
        Claimants = claimants;
    }

    public Asset Asset { get; }
    public string Amount { get; }
    public Claimant[] Claimants { get; }

    public override xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CREATE_CLAIMABLE_BALANCE),
            CreateClaimableBalanceOp = new CreateClaimableBalanceOp
            {
                Amount = new Int64
                {
                    InnerValue = ToXdrAmount(Amount)
                },
                Asset = Asset.ToXdr(),
                Claimants = Claimants.Select(claimant => claimant.ToXdr()).ToArray()
            }
        };
        return body;
    }

    /// <summary>
    ///     Builder for <c>CreateClaimableBalanceOperation</c> operation.
    /// </summary>
    /// <see cref="CreateClaimableBalanceOperation" />
    public class Builder
    {
        private readonly string _amount;
        private readonly Asset _asset;
        private readonly Claimant[] _claimants;
        private KeyPair? _sourceAccount;

        public Builder(CreateClaimableBalanceOp op)
        {
            _asset = Asset.FromXdr(op.Asset);
            _amount = FromXdrAmount(op.Amount.InnerValue);
            _claimants = op.Claimants.Select(Claimant.FromXdr).ToArray();
        }

        public Builder(Asset asset, string amount, Claimant[] claimants)
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
        ///     Builds an operation
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