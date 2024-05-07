using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using ExtensionPoint = StellarDotnetSdk.Soroban.ExtensionPoint;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Make archived Soroban smart contract entries accessible again by restoring them with this restore footprint
///     operation.
///     This operation restores the archived entries specified in the readWrite footprint.
///     See:
///     <a href="https://developers.stellar.org/docs/learn/fundamentals/list-of-operations#restore-footprint">
///         Restore
///         footprint
///     </a>
/// </summary>
/// <remarks>Note that Soroban transactions can only contain one operation per transaction.</remarks>
public class RestoreFootprintOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>RestoreFootprintOperation</c>.
    /// </summary>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public RestoreFootprintOperation(ExtensionPoint? extensionPoint = null, IAccountId? sourceAccount = null) :
        base(sourceAccount)
    {
        ExtensionPoint = extensionPoint ?? new ExtensionPointZero();
    }

    /// <summary>
    ///     Reserved for later use.
    /// </summary>
    public ExtensionPoint ExtensionPoint { get; }

    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.RESTORE_FOOTPRINT),
            RestoreFootprintOp = new RestoreFootprintOp
            {
                Ext = ExtensionPoint.ToXdr()
            }
        };
    }

    public static RestoreFootprintOperation FromXdr(RestoreFootprintOp restoreFootprintOp)
    {
        return new RestoreFootprintOperation(
            ExtensionPoint.FromXdr(restoreFootprintOp.Ext)
        );
    }
}