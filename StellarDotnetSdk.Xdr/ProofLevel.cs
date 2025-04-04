// Automatically generated by xdrgen
// DO NOT EDIT or your changes may be overwritten

namespace StellarDotnetSdk.Xdr;

// === xdr source ============================================================

//  typedef ArchivalProofNode ProofLevel<>;

//  ===========================================================================
public class ProofLevel
{
    public ProofLevel()
    {
    }

    public ProofLevel(ArchivalProofNode[] value)
    {
        InnerValue = value;
    }

    public ArchivalProofNode[] InnerValue { get; set; }

    public static void Encode(XdrDataOutputStream stream, ProofLevel encodedProofLevel)
    {
        var ProofLevelsize = encodedProofLevel.InnerValue.Length;
        stream.WriteInt(ProofLevelsize);
        for (var i = 0; i < ProofLevelsize; i++)
        {
            ArchivalProofNode.Encode(stream, encodedProofLevel.InnerValue[i]);
        }
    }

    public static ProofLevel Decode(XdrDataInputStream stream)
    {
        var decodedProofLevel = new ProofLevel();
        var ProofLevelsize = stream.ReadInt();
        decodedProofLevel.InnerValue = new ArchivalProofNode[ProofLevelsize];
        for (var i = 0; i < ProofLevelsize; i++)
        {
            decodedProofLevel.InnerValue[i] = ArchivalProofNode.Decode(stream);
        }
        return decodedProofLevel;
    }
}