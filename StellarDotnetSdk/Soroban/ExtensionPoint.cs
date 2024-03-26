using System;
using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk.Soroban;

/// <summary>
///     Base class for extension points.
/// </summary>
public abstract class ExtensionPoint
{
    public Xdr.ExtensionPoint ToXdr()
    {
        return this switch
        {
            ExtensionPointZero extensionPointZero => extensionPointZero.ToExtensionPointXdr(),
            _ => throw new InvalidOperationException("Unknown ExtensionPoint type.")
        };
    }

    /// <summary>
    ///     Creates the corresponding <c>ExtensionPoint</c> object from an <c>xdr.ExtensionPoint</c> object.
    /// </summary>
    /// <param name="xdrExtensionPoint">An <c>xdr.ExtensionPoint</c> object to be converted.</param>
    /// <returns>A <c>ExtensionPoint</c> object. Returns null if the provided object is null.</returns>
    public static ExtensionPoint FromXdr(Xdr.ExtensionPoint xdrExtensionPoint)
    {
        return xdrExtensionPoint.Discriminant switch
        {
            0 => ExtensionPointZero.FromExtensionPointXdr(xdrExtensionPoint),
            _ => throw new InvalidOperationException("Unknown ExtensionPoint type.")
        };
    }

    /// <summary>
    ///     Creates a new <c>ExtensionPoint</c> object from the given <see cref="Xdr.ExtensionPoint" /> base-64 encoded XDR
    ///     string.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>An <c>ExtensionPoint</c> object decoded and deserialized from the provided string.</returns>
    public static ExtensionPoint FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = Xdr.ExtensionPoint.Decode(reader);
        return FromXdr(thisXdr);
    }

    /// <summary>
    ///     Returns base64-encoded ExtensionPoint XDR object.
    /// </summary>
    public string ToXdrBase64()
    {
        var xdrValue = ToXdr();
        var writer = new XdrDataOutputStream();
        Xdr.ExtensionPoint.Encode(writer, xdrValue);
        return Convert.ToBase64String(writer.ToArray());
    }
}

public class ExtensionPointZero : ExtensionPoint
{
    public Xdr.ExtensionPoint ToExtensionPointXdr()
    {
        return new Xdr.ExtensionPoint
        {
            Discriminant = 0
        };
    }

    public static ExtensionPointZero FromExtensionPointXdr(Xdr.ExtensionPoint xdrExtensionPoint)
    {
        if (xdrExtensionPoint.Discriminant != 0)
            throw new ArgumentException("Not an ExtensionPointZero", nameof(xdrExtensionPoint));

        return FromXdr();
    }

    public static ExtensionPointZero FromXdr()
    {
        return new ExtensionPointZero();
    }
}