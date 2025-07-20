using System.Linq;
using StellarDotnetSdk.LedgerEntries;

namespace StellarDotnetSdk.Soroban;

public class OperationMetaV2
{
    public ExtensionPoint ExtensionPoint { get; private set; } = new ExtensionPointZero();
    public LedgerEntryChange[] Changes { get; private set; }
    public ContractEvent[] Events { get; private set; }

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