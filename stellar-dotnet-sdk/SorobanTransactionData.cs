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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="xdr">The <see cref="xdr.SorobanTransactionData"/> can still be null when its containing <see cref="xdr.Transaction.TransactionExt"/>'s Discriminant is 0.</param>
    /// <returns></returns>
    public static SorobanTransactionData? FromXdr(xdr.SorobanTransactionData? xdr)
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