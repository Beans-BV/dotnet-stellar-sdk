using System;
using stellar_dotnet_sdk.xdr;
using Int64 = stellar_dotnet_sdk.xdr.Int64;

namespace stellar_dotnet_sdk;

public class SorobanTransactionData
{
    public ExtensionPoint ExtensionPoint { get; set; } = new ExtensionPointZero();
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

    /// <summary>
    ///     Creates a new SorobanTransactionData object from the given SorobanTransactionData XDR base64 string.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>SorobanTransactionData object</returns>
    public static SorobanTransactionData FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = xdr.SorobanTransactionData.Decode(reader);
        return FromXdr(thisXdr);
    }
}