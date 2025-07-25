// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  union LedgerCloseMeta switch (int v)
//  {
//  case 0:
//      LedgerCloseMetaV0 v0;
//  case 1:
//      LedgerCloseMetaV1 v1;
//  case 2:
//      LedgerCloseMetaV2 v2;
//  };

//  ===========================================================================
public class LedgerCloseMeta
{
    public int Discriminant { get; set; }

    public LedgerCloseMetaV0 V0 { get; set; }
    public LedgerCloseMetaV1 V1 { get; set; }
    public LedgerCloseMetaV2 V2 { get; set; }

    public static void Encode(XdrDataOutputStream stream, LedgerCloseMeta encodedLedgerCloseMeta)
    {
        stream.WriteInt(encodedLedgerCloseMeta.Discriminant);
        switch (encodedLedgerCloseMeta.Discriminant)
        {
            case 0:
                LedgerCloseMetaV0.Encode(stream, encodedLedgerCloseMeta.V0);
                break;
            case 1:
                LedgerCloseMetaV1.Encode(stream, encodedLedgerCloseMeta.V1);
                break;
            case 2:
                LedgerCloseMetaV2.Encode(stream, encodedLedgerCloseMeta.V2);
                break;
        }
    }

    public static LedgerCloseMeta Decode(XdrDataInputStream stream)
    {
        var decodedLedgerCloseMeta = new LedgerCloseMeta();
        var discriminant = stream.ReadInt();
        decodedLedgerCloseMeta.Discriminant = discriminant;
        switch (decodedLedgerCloseMeta.Discriminant)
        {
            case 0:
                decodedLedgerCloseMeta.V0 = LedgerCloseMetaV0.Decode(stream);
                break;
            case 1:
                decodedLedgerCloseMeta.V1 = LedgerCloseMetaV1.Decode(stream);
                break;
            case 2:
                decodedLedgerCloseMeta.V2 = LedgerCloseMetaV2.Decode(stream);
                break;
        }
        return decodedLedgerCloseMeta;
    }
}