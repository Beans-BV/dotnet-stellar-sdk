using System;
using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public abstract class ExtensionPoint
{
    public xdr.ExtensionPoint ToXdr()
    {
        return this switch
        {
            ExtensionPointZero extensionPointZero => extensionPointZero.ToExtensionPointXdr(),
            _ => throw new InvalidOperationException("Unknown ExtensionPoint type")
        };
    }

    public static ExtensionPoint FromXdr(xdr.ExtensionPoint xdr)
    {
        return xdr.Discriminant switch
        {
            0 => ExtensionPointZero.FromExtensionPointXdr(xdr),
            _ => throw new InvalidOperationException("Unknown ExtensionPoint type")
        };
    }
    
    /// <summary>
    /// Creates a new ExtensionPoint object from the given ExtensionPoint XDR base64 string.
    /// </summary>
    /// <param name="xdrBase64"></param>
    /// <returns>ExtensionPoint object</returns>
    public static ExtensionPoint FromXdrBase64(string xdrBase64)
    {
        var bytes = Convert.FromBase64String(xdrBase64);
        var reader = new XdrDataInputStream(bytes);
        var thisXdr = xdr.ExtensionPoint.Decode(reader);
        return FromXdr(thisXdr);
    }
    
    ///<summary>
    /// Returns base64-encoded ExtensionPoint XDR object.
    ///</summary>
    public string ToXdrBase64()
    {
        var xdrValue = ToXdr();
        var writer = new XdrDataOutputStream();
        xdr.ExtensionPoint.Encode(writer, xdrValue);
        return Convert.ToBase64String(writer.ToArray());
    }
}

public class ExtensionPointZero : ExtensionPoint
{
    public new void ToXdr() {}
    
    public xdr.ExtensionPoint ToExtensionPointXdr()
    {
        return new xdr.ExtensionPoint
        {
            Discriminant = 0
        };
    }

    public static ExtensionPointZero FromExtensionPointXdr(xdr.ExtensionPoint xdrExtensionPoint)
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