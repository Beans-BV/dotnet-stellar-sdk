using System;
using stellar_dotnet_sdk.xdr;
using Int64 = stellar_dotnet_sdk.xdr.Int64;

namespace stellar_dotnet_sdk;

public class SorobanTransactionData
{
    /// <summary>
    ///     A shortcut to create a <c>SorobanTransactionData</c> object containing a <c>LedgerKey</c> of an entry to be
    ///     restored.
    ///     <seealso cref="RestoreFootprintOperation" />
    ///     <param name="key"></param>
    ///     <param name="toExtend">True: To be used in an RestoreTtl operation. False: To be used in a ExtendTtl operation.</param>
    /// </summary>
    public SorobanTransactionData(LedgerKey key, bool toExtend)
    {
        var footprint = toExtend
            ? new LedgerFootprint { ReadOnly = new[] { key } }
            : new LedgerFootprint { ReadWrite = new[] { key } };
        Resources = new SorobanResources(footprint, 0, 0, 0);
    }

    public SorobanTransactionData(SorobanResources resources, long resourceFee, ExtensionPoint? extensionPoint = null)
    {
        Resources = resources;
        ResourceFee = resourceFee;
        if (extensionPoint != null) ExtensionPoint = extensionPoint;
    }

    public ExtensionPoint ExtensionPoint { get; } = new ExtensionPointZero();
    public SorobanResources Resources { get; }
    public long ResourceFee { get; }

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
    ///     Converts an <c>xdr.SorobanTransactionData</c> object to a <c>SorobanTransactionData</c> object.
    /// </summary>
    /// <param name="xdrSorobanTransactionData">The <c>xdr.SorobanTransactionData</c> to be converted.</param>
    /// <returns>A <c>SorobanTransactionData</c> object equivalent to the specified <c>xdr.SorobanTransactionData</c> object.</returns>
    public static SorobanTransactionData FromXdr(xdr.SorobanTransactionData xdrSorobanTransactionData)
    {
        return new SorobanTransactionData(SorobanResources.FromXdr(xdrSorobanTransactionData.Resources),
            xdrSorobanTransactionData.ResourceFee.InnerValue, ExtensionPoint.FromXdr(xdrSorobanTransactionData.Ext));
    }

    /// <summary>
    ///     Creates a new SorobanTransactionData object from the given SorobanTransactionData XDR base64 string.
    /// </summary>
    /// <param name="xdrBase64">
    ///     A base-64 encoded XDR string of an
    ///     <see cref="xdr.SorobanTransactionData">xdr.SorobanTransactionData</see> object.
    /// </param>
    /// <returns>A <see cref="SorobanTransactionData" /> that decoded and deserialized from the specified string.</returns>
    public static SorobanTransactionData FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = xdr.SorobanTransactionData.Decode(reader);
        return FromXdr(thisXdr);
    }
}