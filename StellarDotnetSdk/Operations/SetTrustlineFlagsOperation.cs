using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Xdr;
using Asset = StellarDotnetSdk.Assets.Asset;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Allows issuing account to configure authorization and trustline flags to an asset.
///     See:
///     <a
///         href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#set-trustline-flags">
///         Set trustline flags
///     </a>
/// </summary>
public class SetTrustlineFlagsOperation : Operation
{
    /// <summary>
    ///     Constructs a <see cref="SetTrustlineFlagsOperation" />.
    /// </summary>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public SetTrustlineFlagsOperation(
        Asset asset,
        KeyPair trustor,
        uint setFlags,
        uint clearFlags,
        IAccountId? sourceAccount = null) : base(sourceAccount)
    {
        Asset = asset;
        Trustor = trustor;
        SetFlags = setFlags;
        ClearFlags = clearFlags;
    }

    /// <summary>
    ///     The asset trustline whose flags are being modified.
    /// </summary>
    public Asset Asset { get; }

    /// <summary>
    ///     The account that established this trustline.
    /// </summary>
    public KeyPair Trustor { get; }


    /// <summary>
    ///     One or more flags (combined via bitwise-OR) indicating which flags to set.
    ///     Possible flags are: 1 if the trustor is authorized to transact with the asset or 2 if the trustor is authorized
    ///     to maintain offers but not to perform other transactions.
    /// </summary>
    public uint SetFlags { get; }

    /// <summary>
    ///     One or more flags (combined via bitwise OR) indicating which flags to clear. Possibilities include those for
    ///     SetFlags as well as 4, which prevents the issuer from clawing back its asset (both from accounts and claimable
    ///     balances).
    /// </summary>
    public uint ClearFlags { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        var body = new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.SET_TRUST_LINE_FLAGS),
            SetTrustLineFlagsOp = new SetTrustLineFlagsOp
            {
                Asset = Asset.ToXdr(),
                Trustor = new AccountID(Trustor.XdrPublicKey),
                SetFlags = new Uint32 { InnerValue = SetFlags },
                ClearFlags = new Uint32 { InnerValue = ClearFlags }
            }
        };
        return body;
    }

    public static SetTrustlineFlagsOperation FromXdr(SetTrustLineFlagsOp setTrustLineFlagsOp)
    {
        return new SetTrustlineFlagsOperation(
            Asset.FromXdr(setTrustLineFlagsOp.Asset),
            KeyPair.FromXdrPublicKey(setTrustLineFlagsOp.Trustor.InnerValue),
            setTrustLineFlagsOp.SetFlags.InnerValue,
            setTrustLineFlagsOp.ClearFlags.InnerValue
        );
    }
}