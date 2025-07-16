using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Soroban;

/// <summary>
/// Abstract class for transaction meta.
/// </summary>
public abstract class TransactionMeta
{
    private static TransactionMeta FromXdr(Xdr.TransactionMeta xdrMeta)
    {
        return xdrMeta.Discriminant switch
        {
            3 => TransactionMetaV3.FromXdr(xdrMeta.V3),
            4 => TransactionMetaV4.FromXdr(xdrMeta.V4),
            _ => throw new ArgumentOutOfRangeException(nameof(xdrMeta.Discriminant), "Unknown TransactionMeta type"),
        };
    }

    /// <summary>
    ///     Creates a new <c>TransactionMeta</c> object from the given
    ///     <see cref="Xdr.TransactionMeta">xdr.TransactionMeta</see> base-64 encoded XDR string.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>A <c>TransactionMeta</c> object decoded and deserialized from the provided string.</returns>
    public static TransactionMeta FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = Xdr.TransactionMeta.Decode(reader);
        return FromXdr(thisXdr);
    }
}