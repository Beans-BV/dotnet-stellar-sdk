// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace stellar_dotnet_sdk.xdr;

// === xdr source ============================================================

//  struct SCSpecTypeUDT
//  {
//      string name<60>;
//  };

//  ===========================================================================
public class SCSpecTypeUDT
{
    public string Name { get; set; }

    public static void Encode(XdrDataOutputStream stream, SCSpecTypeUDT encodedSCSpecTypeUDT)
    {
        stream.WriteString(encodedSCSpecTypeUDT.Name);
    }

    public static SCSpecTypeUDT Decode(XdrDataInputStream stream)
    {
        var decodedSCSpecTypeUDT = new SCSpecTypeUDT();
        decodedSCSpecTypeUDT.Name = stream.ReadString();
        return decodedSCSpecTypeUDT;
    }
}