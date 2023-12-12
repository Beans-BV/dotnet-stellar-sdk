// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace stellar_dotnet_sdk.xdr;

// === xdr source ============================================================

//  union SCMetaEntry switch (SCMetaKind kind)
//  {
//  case SC_META_V0:
//      SCMetaV0 v0;
//  };

//  ===========================================================================
public class SCMetaEntry
{
    public SCMetaKind Discriminant { get; set; } = new();

    public SCMetaV0 V0 { get; set; }

    public static void Encode(XdrDataOutputStream stream, SCMetaEntry encodedSCMetaEntry)
    {
        stream.WriteInt((int)encodedSCMetaEntry.Discriminant.InnerValue);
        switch (encodedSCMetaEntry.Discriminant.InnerValue)
        {
            case SCMetaKind.SCMetaKindEnum.SC_META_V0:
                SCMetaV0.Encode(stream, encodedSCMetaEntry.V0);
                break;
        }
    }

    public static SCMetaEntry Decode(XdrDataInputStream stream)
    {
        var decodedSCMetaEntry = new SCMetaEntry();
        var discriminant = SCMetaKind.Decode(stream);
        decodedSCMetaEntry.Discriminant = discriminant;
        switch (decodedSCMetaEntry.Discriminant.InnerValue)
        {
            case SCMetaKind.SCMetaKindEnum.SC_META_V0:
                decodedSCMetaEntry.V0 = SCMetaV0.Decode(stream);
                break;
        }

        return decodedSCMetaEntry;
    }
}