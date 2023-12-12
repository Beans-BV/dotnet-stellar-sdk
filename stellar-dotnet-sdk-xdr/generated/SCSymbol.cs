// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace stellar_dotnet_sdk.xdr;

// === xdr source ============================================================

//  typedef string SCSymbol<SCSYMBOL_LIMIT>;

//  ===========================================================================
public class SCSymbol
{
    public SCSymbol()
    {
    }

    public SCSymbol(string value)
    {
        InnerValue = value;
    }

    public string InnerValue { get; set; } = default;

    public static void Encode(XdrDataOutputStream stream, SCSymbol encodedSCSymbol)
    {
        stream.WriteString(encodedSCSymbol.InnerValue);
    }

    public static SCSymbol Decode(XdrDataInputStream stream)
    {
        var decodedSCSymbol = new SCSymbol();
        decodedSCSymbol.InnerValue = stream.ReadString();
        return decodedSCSymbol;
    }
}