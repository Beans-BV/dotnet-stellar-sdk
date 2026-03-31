namespace StellarDotnetSdk.LedgerEntries;

/// <summary>
///     Represents version 2 extensions to a trustline entry, including the liquidity pool use count.
/// </summary>
public class TrustLineEntryExtensionV2
{
    private TrustLineEntryExtensionV2(int liquidityPoolUseCount)
    {
        LiquidityPoolUseCount = liquidityPoolUseCount;
    }

    /// <summary>
    ///     The number of liquidity pools that use this trustline's asset.
    /// </summary>
    public int LiquidityPoolUseCount { get; }

    /// <summary>
    ///     Creates a <see cref="TrustLineEntryExtensionV2" /> from an XDR
    ///     <see cref="Xdr.TrustLineEntryExtensionV2" /> object.
    /// </summary>
    /// <param name="xdrExtensionV2">The XDR extension object.</param>
    /// <returns>A <see cref="TrustLineEntryExtensionV2" /> instance.</returns>
    public static TrustLineEntryExtensionV2 FromXdr(Xdr.TrustLineEntryExtensionV2 xdrExtensionV2)
    {
        return new TrustLineEntryExtensionV2(xdrExtensionV2.LiquidityPoolUseCount.InnerValue);
    }
}