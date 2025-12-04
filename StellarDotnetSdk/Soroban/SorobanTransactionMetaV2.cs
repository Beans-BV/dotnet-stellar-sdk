namespace StellarDotnetSdk.Soroban;

/// <summary>
///     Holds the Soroban transaction metadata.
/// </summary>
public class SorobanTransactionMetaV2
{
    public SorobanTransactionMetaExtensionV1? SorobanTransactionMetaExtensionV1 { get; private set; }

    public SCVal? ReturnValue { get; private set; }

    /// <summary>
    ///     Creates the corresponding <c>SorobanTransactionMetaV2</c> object from an <c>xdr.SorobanTransactionMetaV2</c>
    ///     object.
    /// </summary>
    /// <param name="xdrMeta">An <c>xdr.SorobanTransactionMetaV2</c> object to be converted.</param>
    /// <returns>A <c>SorobanTransactionMetaV2</c> object. Returns null if the provided object is null.</returns>
    public static SorobanTransactionMetaV2? FromXdr(Xdr.SorobanTransactionMetaV2? xdrMeta)
    {
        if (xdrMeta == null)
        {
            return null;
        }
        var meta = new SorobanTransactionMetaV2();
        if (xdrMeta.ReturnValue != null)
        {
            meta.ReturnValue = SCVal.FromXdr(xdrMeta.ReturnValue);
        }

        if (xdrMeta.Ext.Discriminant == 1)
        {
            meta.SorobanTransactionMetaExtensionV1 =
                SorobanTransactionMetaExtensionV1.FromXdr(xdrMeta.Ext.V1);
        }

        return meta;
    }
}