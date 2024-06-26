// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  struct SCSpecUDTStructFieldV0
//  {
//      string doc<SC_SPEC_DOC_LIMIT>;
//      string name<30>;
//      SCSpecTypeDef type;
//  };

//  ===========================================================================
public class SCSpecUDTStructFieldV0
{
    public string Doc { get; set; }
    public string Name { get; set; }
    public SCSpecTypeDef Type { get; set; }

    public static void Encode(XdrDataOutputStream stream, SCSpecUDTStructFieldV0 encodedSCSpecUDTStructFieldV0)
    {
        stream.WriteString(encodedSCSpecUDTStructFieldV0.Doc);
        stream.WriteString(encodedSCSpecUDTStructFieldV0.Name);
        SCSpecTypeDef.Encode(stream, encodedSCSpecUDTStructFieldV0.Type);
    }

    public static SCSpecUDTStructFieldV0 Decode(XdrDataInputStream stream)
    {
        var decodedSCSpecUDTStructFieldV0 = new SCSpecUDTStructFieldV0();
        decodedSCSpecUDTStructFieldV0.Doc = stream.ReadString();
        decodedSCSpecUDTStructFieldV0.Name = stream.ReadString();
        decodedSCSpecUDTStructFieldV0.Type = SCSpecTypeDef.Decode(stream);
        return decodedSCSpecUDTStructFieldV0;
    }
}