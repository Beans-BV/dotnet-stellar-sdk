using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class AccountEntryExtensionV3
{
    public ExtensionPoint ExtensionPoint { get; set; }

    public uint SequenceLedger { get; set; }

    public ulong SequenceTime { get; set; }

    public xdr.AccountEntryExtensionV3 ToXdr()
    {
        return new xdr.AccountEntryExtensionV3
        {
            Ext = ExtensionPoint.ToXdr(),
            SeqLedger = new Uint32(SequenceLedger),
            SeqTime = new TimePoint(new Uint64(SequenceTime))
        };
    }

    public static AccountEntryExtensionV3 FromXdr(xdr.AccountEntryExtensionV3 xdr)
    {
        return new AccountEntryExtensionV3
        {
            ExtensionPoint = ExtensionPoint.FromXdr(xdr.Ext),
            SequenceTime = xdr.SeqTime.InnerValue.InnerValue,
            SequenceLedger = xdr.SeqLedger.InnerValue
        };
    }
}