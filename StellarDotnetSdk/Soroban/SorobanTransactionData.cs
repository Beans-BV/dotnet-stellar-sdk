using System;
using StellarDotnetSdk.Operations;
using StellarDotnetSdk.Xdr;
using LedgerKey = StellarDotnetSdk.LedgerKeys.LedgerKey;
using Int64 = StellarDotnetSdk.Xdr.Int64;

namespace StellarDotnetSdk.Soroban;

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
            ? new LedgerFootprint { ReadOnly = [key] }
            : new LedgerFootprint { ReadWrite = [key] };
        Resources = new SorobanResources(footprint, 0, 0, 0);
    }

    public SorobanTransactionData(
        SorobanResources resources,
        long resourceFee,
        SorobanResourceExtensionV0? extension = null
    )
    {
        Resources = resources;
        ResourceFee = resourceFee;
        Extension = extension;
    }

    public SorobanResourceExtensionV0? Extension { get; private set; }
    public SorobanResources Resources { get; }
    public long ResourceFee { get; }

    public Xdr.SorobanTransactionData ToXdr()
    {
        var data = new Xdr.SorobanTransactionData
        {
            Ext = new Xdr.SorobanTransactionData.SorobanTransactionDataExt
            {
                Discriminant = 0,
            },
            Resources = Resources.ToXdr(),
            ResourceFee = new Int64(ResourceFee),
        };
        if (Extension != null)
        {
            data.Ext = new Xdr.SorobanTransactionData.SorobanTransactionDataExt
            {
                Discriminant = 1,
                ResourceExt = Extension.ToXdr(),
            };
        }
        return data;
    }

    /// <summary>
    ///     Converts an <c>xdr.SorobanTransactionData</c> object to a <c>SorobanTransactionData</c> object.
    /// </summary>
    /// <param name="xdrData">The <c>xdr.SorobanTransactionData</c> to be converted.</param>
    /// <returns>A <c>SorobanTransactionData</c> object equivalent to the specified <c>xdr.SorobanTransactionData</c> object.</returns>
    public static SorobanTransactionData FromXdr(Xdr.SorobanTransactionData xdrData)
    {
        var data = new SorobanTransactionData(
            SorobanResources.FromXdr(xdrData.Resources),
            xdrData.ResourceFee.InnerValue
        );
        if (xdrData.Ext.Discriminant == 1)
        {
            data.Extension = SorobanResourceExtensionV0.FromXdr(xdrData.Ext.ResourceExt);
        }
        return data;
    }

    /// <summary>
    ///     Creates a new SorobanTransactionData object from the given SorobanTransactionData XDR base64 string.
    /// </summary>
    /// <param name="xdrBase64">
    ///     A base-64 encoded XDR string of an
    ///     <see cref="Xdr.SorobanTransactionData">xdr.SorobanTransactionData</see> object.
    /// </param>
    /// <returns>A <see cref="SorobanTransactionData" /> that decoded and deserialized from the specified string.</returns>
    public static SorobanTransactionData FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = Xdr.SorobanTransactionData.Decode(reader);
        return FromXdr(thisXdr);
    }
}