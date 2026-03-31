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

    /// <summary>
    ///     Claimable balance flags (e.g. CLAIMABLE_BALANCE_CLAWBACK_ENABLED).
    /// </summary>
    public uint Flags { get; }

    /// <summary>
    ///     Creates a <see cref="ClaimableBalanceEntryExtensionV1" /> from an XDR
    ///     <see cref="Xdr.ClaimableBalanceEntryExtensionV1" /> object.
    /// </summary>
    /// <param name="xdrExtensionV1">The XDR extension object.</param>
    /// <returns>A <see cref="ClaimableBalanceEntryExtensionV1" /> instance.</returns>
    public static ClaimableBalanceEntryExtensionV1 FromXdr(Xdr.ClaimableBalanceEntryExtensionV1 xdrExtensionV1)
    {
        return new ClaimableBalanceEntryExtensionV1(xdrExtensionV1.Flags.InnerValue);
    }
}