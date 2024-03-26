using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class Signer
{
    public Signer(SignerKey key, uint weight)
    {
        Key = key;
        Weight = weight;
    }

    public SignerKey Key { get; }

    public uint Weight { get; }

    public xdr.Signer ToXdr()
    {
        return new xdr.Signer
        {
            Key = Key,
            Weight = new Uint32(Weight)
        };
    }

    public static Signer FromXdr(xdr.Signer xdrSigner)
    {
        return new Signer(xdrSigner.Key, xdrSigner.Weight.InnerValue);
    }
}