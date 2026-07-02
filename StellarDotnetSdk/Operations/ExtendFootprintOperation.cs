using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using ExtensionPoint = StellarDotnetSdk.Soroban.ExtensionPoint;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Extend the time to live (TTL) of entries for Soroban smart contracts.
///     This operation extends the TTL of the entries specified in the readOnly footprint of
///     the transaction so that they will live at least extendTo ledgers past the last-closed ledger.
///     See:
///     <a
///         href="https://developers.stellar.org/docs/learn/fundamentals/transactions/list-of-operations#extend-footprint-ttl">
///         Extend footprint TTL
///     </a>
/// </summary>
/// <remarks>Note that Soroban transactions can only contain one operation per transaction.</remarks>
public class ExtendFootprintOperation : Operation
{
    /// <summary>
    ///     Constructs a new <c>ExtendFootprintOperation</c>.
    /// </summary>
    /// <param name="extendTo">
    ///     The number of ledgers past the last-closed ledger the entries should live for (relative, not an absolute
    ///     ledger sequence number). The new live-until ledger becomes approximately the current ledger plus
    ///     <c>extendTo</c> when that exceeds the entries' current TTL.
    /// </param>
    /// <param name="extensionPoint">(Optional) Reserved for later use.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public ExtendFootprintOperation(uint extendTo, ExtensionPoint? extensionPoint = null,
        IAccountId? sourceAccount = null) : base(sourceAccount)
    {
        ExtendTo = extendTo;
        ExtensionPoint = extensionPoint ?? new ExtensionPointZero();
    }

    /// <summary>
    ///     The number of ledgers past the last-closed ledger the entries should live for (relative, not an absolute
    ///     ledger sequence number). The new live-until ledger becomes approximately the current ledger plus
    ///     <c>ExtendTo</c> when that exceeds the entries' current TTL.
    /// </summary>
    public uint ExtendTo { get; }

    /// <summary>
    ///     Reserved for later use.
    /// </summary>
    public ExtensionPoint ExtensionPoint { get; }

    /// <summary>
    ///     Generates the XDR operation body for this operation.
    /// </summary>
    /// <returns>The XDR operation body.</returns>
    public override Xdr.Operation.OperationBody ToOperationBody()
    {
        return new Xdr.Operation.OperationBody
        {
            Discriminant = OperationType.Create(OperationType.OperationTypeEnum.EXTEND_FOOTPRINT_TTL),
            ExtendFootprintTTLOp = new ExtendFootprintTTLOp
            {
                Ext = ExtensionPoint.ToXdr(),
                ExtendTo = new Uint32(ExtendTo),
            },
        };
    }

    /// <summary>
    ///     Creates an <see cref="ExtendFootprintOperation" /> from its XDR representation.
    /// </summary>
    /// <param name="extendFootprintTtlOp">The XDR ExtendFootprintTTLOp object.</param>
    /// <returns>A new <see cref="ExtendFootprintOperation" /> instance.</returns>
    public static ExtendFootprintOperation FromXdr(ExtendFootprintTTLOp extendFootprintTtlOp)
    {
        return new ExtendFootprintOperation(
            extendFootprintTtlOp.ExtendTo.InnerValue,
            ExtensionPoint.FromXdr(extendFootprintTtlOp.Ext)
        );
    }
}