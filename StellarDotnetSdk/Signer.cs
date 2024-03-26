using StellarDotnetSdk.Xdr;

namespace StellarDotnetSdk;

public class Signer
{
    public Signer(SignerKey key, uint weight)
    {
        Key = key;
        Weight = weight;
    }

    public SignerKey Key { get; }

    public uint Weight { get; }

    public Xdr.Signer ToXdr()
    {
        return new Xdr.Signer
        {
            Key = Key,
            Weight = new Uint32(Weight)
        };
    }

    public static Signer FromXdr(Xdr.Signer xdrSigner)
    {
        return new Signer(xdrSigner.Key, xdrSigner.Weight.InnerValue);
    }
}