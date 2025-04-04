﻿using StellarDotnetSdk.Accounts;
using StellarDotnetSdk.Soroban;
using StellarDotnetSdk.Xdr;
using ExtensionPoint = StellarDotnetSdk.Soroban.ExtensionPoint;

namespace StellarDotnetSdk.Operations;

/// <summary>
///     Extend the time to live (TTL) of entries for Soroban smart contracts.
///     This operation extends the TTL of the entries specified in the readOnly footprint of
///     the transaction so that they will live at least until the extendTo ledger sequence number is reached.
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
    /// <param name="extendTo">The ledger sequence number the entries will live until.</param>
    /// <param name="sourceAccount">(Optional) Source account of the operation.</param>
    public ExtendFootprintOperation(uint extendTo, ExtensionPoint? extensionPoint = null,
        IAccountId? sourceAccount = null) : base(sourceAccount)
    {
        ExtendTo = extendTo;
        ExtensionPoint = extensionPoint ?? new ExtensionPointZero();
    }

    /// <summary>
    ///     The ledger sequence number the entries will live until.
    /// </summary>
    public uint ExtendTo { get; }

    /// <summary>
    ///     Reserved for later use.
    /// </summary>
    public ExtensionPoint ExtensionPoint { get; }

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

    public static ExtendFootprintOperation FromXdr(ExtendFootprintTTLOp extendFootprintTtlOp)
    {
        return new ExtendFootprintOperation(
            extendFootprintTtlOp.ExtendTo.InnerValue,
            ExtensionPoint.FromXdr(extendFootprintTtlOp.Ext)
        );
    }
}