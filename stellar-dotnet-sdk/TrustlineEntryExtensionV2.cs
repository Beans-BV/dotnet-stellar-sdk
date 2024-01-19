using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class TrustLineEntryExtensionV2
{
    public int LiquidityPoolUseCount { get; set; }

    public static TrustLineEntryExtensionV2 FromXdr(xdr.TrustLineEntryExtensionV2 xdrExtensionV2)
    {
        return new TrustLineEntryExtensionV2
        {
            LiquidityPoolUseCount = xdrExtensionV2.LiquidityPoolUseCount.InnerValue
        };
    }

    public xdr.TrustLineEntryExtensionV2 ToXdr()
    {
        return new xdr.TrustLineEntryExtensionV2
        {
            LiquidityPoolUseCount = new Int32(LiquidityPoolUseCount),
            Ext = new xdr.TrustLineEntryExtensionV2.TrustLineEntryExtensionV2Ext
            {
                Discriminant = 0
            }
        };
    }
}