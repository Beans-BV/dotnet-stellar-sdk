// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  typedef opaque AssetCode12[12];

//  ===========================================================================
public class AssetCode12
{
    public AssetCode12()
    {
    }

    public AssetCode12(byte[] value)
    {
        InnerValue = value;
    }

    public byte[] InnerValue { get; set; }

    public static void Encode(XdrDataOutputStream stream, AssetCode12 encodedAssetCode12)
    {
        var AssetCode12size = encodedAssetCode12.InnerValue.Length;
        stream.Write(encodedAssetCode12.InnerValue, 0, AssetCode12size);
    }

    public static AssetCode12 Decode(XdrDataInputStream stream)
    {
        var decodedAssetCode12 = new AssetCode12();
        var AssetCode12size = 12;
        decodedAssetCode12.InnerValue = new byte[AssetCode12size];
        stream.Read(decodedAssetCode12.InnerValue, 0, AssetCode12size);
        return decodedAssetCode12;
    }
}