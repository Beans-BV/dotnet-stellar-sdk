namespace StellarDotnetSdk.LedgerEntries;

public class TrustLineEntryExtensionV2
{
    private TrustLineEntryExtensionV2(int liquidityPoolUseCount)
    {
        LiquidityPoolUseCount = liquidityPoolUseCount;
    }

    public int LiquidityPoolUseCount { get; }

    public static TrustLineEntryExtensionV2 FromXdr(Xdr.TrustLineEntryExtensionV2 xdrExtensionV2)
    {
        return new TrustLineEntryExtensionV2(xdrExtensionV2.LiquidityPoolUseCount.InnerValue);
    }
}