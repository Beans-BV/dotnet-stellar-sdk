using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class SorobanTransactionData
{
    public ExtensionPoint ExtensionPoint { get; set; }
    public SorobanResources Resources { get; set; }
    public long ResourceFee { get; set; }

    public xdr.SorobanTransactionData ToXdr()
    {
        return new xdr.SorobanTransactionData
        {
            Ext = ExtensionPoint.ToXdr(),
            Resources = Resources.ToXdr(),
            ResourceFee = new Int64(ResourceFee)
        };
    }

    public static SorobanTransactionData? FromXdr(xdr.SorobanTransactionData xdr)
    {
        if (xdr == null) return null;
        return new SorobanTransactionData
        {
            ExtensionPoint = ExtensionPoint.FromXdr(xdr.Ext),
            Resources = SorobanResources.FromXdr(xdr.Resources),
            ResourceFee = xdr.ResourceFee.InnerValue
        };
    }
}