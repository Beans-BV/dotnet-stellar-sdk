using System.Linq;
using StellarDotnetSdk.LedgerEntries;

namespace StellarDotnetSdk.Soroban;

/// <summary>
///     Represents version 2 operation metadata containing ledger entry changes and contract events
///     produced by a Soroban operation.
/// </summary>
public class OperationMetaV2
{
    /// <summary>
    ///     Reserved for future use.
    /// </summary>
    public ExtensionPoint ExtensionPoint { get; private set; } = new ExtensionPointZero();

    /// <summary>
    ///     The ledger entry changes produced by this operation.
    /// </summary>
    public LedgerEntryChange[] Changes { get; private set; }

    /// <summary>
    ///     The contract events emitted during execution of this operation.
    /// </summary>
    public ContractEvent[] Events { get; private set; }

    /// <summary>
    ///     Creates a new <see cref="OperationMetaV2" /> from an XDR <see cref="Xdr.OperationMetaV2" /> object.
    /// </summary>
    /// <param name="xdrMeta">The XDR operation metadata to convert.</param>
    /// <returns>An <see cref="OperationMetaV2" /> instance.</returns>
    public static OperationMetaV2 FromXdr(Xdr.OperationMetaV2 xdrMeta)
    {
        var meta = new OperationMetaV2
        {
            ExtensionPoint = ExtensionPoint.FromXdr(xdrMeta.Ext),
            Changes = xdrMeta.Changes.InnerValue.Select(LedgerEntryChange.FromXdr).ToArray(),
            Events = xdrMeta.Events.Select(ContractEvent.FromXdr).ToArray(),
        };
        return meta;
    }
}