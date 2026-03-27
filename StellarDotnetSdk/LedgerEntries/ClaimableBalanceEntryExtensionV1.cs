namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents version 1 extensions to a claimable balance entry, including flags such as clawback enabled.
/// </summary>
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