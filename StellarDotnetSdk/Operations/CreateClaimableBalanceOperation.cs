using System.Linq;
using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;
using SdkClaimant = StellarDotnetSdk.Claimants.Claimant;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Moves an amount of asset from the operation source account into a new ClaimableBalanceEntry.
///     See:
///     <a
///         href="https://developers.stellar.org/docs/learn/fundamentals/transactions/list-of-operations#create-claimable-balance">
///         Create claimable balance
///     </a>
/// </summary>
public class CreateClaimableBalanceOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>CreateClaimableBalanceOperation</c>.
    /// </summary>
    /// <param name="asset">Asset that will be held in the ClaimableBalanceEntry.</param>
    /// <param name="amount">Amount of <c>Asset</c> stored in the ClaimableBalanceEntry.</param>
    /// <param name="claimants">The claimants that can claim this ClaimableBalanceEntry.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public CreateClaimableBalanceOperation(Asset asset, string amount, SdkClaimant[] claimants,
        IAccountId? sourceAccount = null) : base(sourceAccount)
    {
        Asset = asset;
        Amount = amount;
        Claimants = claimants;
    }

    /// <summary>
    ///     Asset that will be held in the ClaimableBalanceEntry.
    /// </summary>
    public Asset Asset { get; }

    /// <summary>
    ///     Amount of <c>Asset</c> stored in the ClaimableBalanceEntry.
    /// </summary>
    public string Amount { get; }

    /// <summary>
    ///     List of <c>Claimant</c>s (account address and ClaimPredicate pair) that can claim this ClaimableBalanceEntry.
    /// </summary>
    public SdkClaimant[] Claimants { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.CREATE_CLAIMABLE_BALANCE),
            CreateClaimableBalanceOp = new CreateClaimableBalanceOp
            {
                Amount = new Int64(ToXdrAmount(Amount)),
                Asset = Asset.ToXdr(),
                Claimants = Claimants.Select(claimant => claimant.ToXdr()).ToArray(),
            },
        };
    }

    public static CreateClaimableBalanceOperation FromXdr(CreateClaimableBalanceOp createClaimableBalanceOp)
    {
        return new CreateClaimableBalanceOperation(
            Asset.FromXdr(createClaimableBalanceOp.Asset),
            StellarDotnetSdk.Amount.FromXdr(createClaimableBalanceOp.Amount.InnerValue),
            createClaimableBalanceOp.Claimants.Select(SdkClaimant.FromXdr).ToArray()
        );
    }
}