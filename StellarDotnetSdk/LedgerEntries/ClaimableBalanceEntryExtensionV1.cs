namespace StellarDotnetSdk.LedgerEntries;

public class ClaimableBalanceEntryExtensionV1
{
    private ClaimableBalanceEntryExtensionV1(uint flags)
    {
        Flags = flags;
    }

    public uint Flags { get; }

    public static ClaimableBalanceEntryExtensionV1 FromXdr(Xdr.ClaimableBalanceEntryExtensionV1 xdrExtensionV1)
    {
        return new ClaimableBalanceEntryExtensionV1(xdrExtensionV1.Flags.InnerValue);
    }
}