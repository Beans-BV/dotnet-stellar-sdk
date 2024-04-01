using StellarDotnetSdk.Soroban;

namespace StellarDotnetSdk.LedgerEntries;

public class AccountEntryExtensionV3
{
    private AccountEntryExtensionV3(ExtensionPoint extensionPoint, uint sequenceLedger, ulong sequenceTime)
    {
        ExtensionPoint = extensionPoint;
        SequenceLedger = sequenceLedger;
        SequenceTime = sequenceTime;
    }

    public ExtensionPoint ExtensionPoint { get; }

    public uint SequenceLedger { get; }

    public ulong SequenceTime { get; }

    public static AccountEntryExtensionV3 FromXdr(Xdr.AccountEntryExtensionV3 xdr)
    {
        return new AccountEntryExtensionV3(ExtensionPoint.FromXdr(xdr.Ext),
            sequenceTime: xdr.SeqTime.InnerValue.InnerValue, sequenceLedger: xdr.SeqLedger.InnerValue);
    }
}