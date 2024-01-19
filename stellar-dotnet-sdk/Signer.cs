using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class Signer
{
    public SignerKey Key { get; set; }
    
    public uint Weight { get; set; }
    
    public xdr.Signer ToXdr()
    {
        return new xdr.Signer()
        {
            Key = Key,
            Weight = new Uint32(Weight)
        };
    }
    
    public static Signer FromXdr(xdr.Signer xdrSigner)
    {
        return new Signer
        {
            Key = xdrSigner.Key,
            Weight = xdrSigner.Weight.InnerValue
        };
    }
}