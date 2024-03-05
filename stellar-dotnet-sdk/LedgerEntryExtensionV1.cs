namespace stellar_dotnet_sdk;

public class LedgerEntryExtensionV1
{
    public LedgerEntryExtensionV1(KeyPair sponsoringID)
    {
        SponsoringID = sponsoringID;
    }

    public KeyPair SponsoringID { get; }

    public static LedgerEntryExtensionV1 FromXdr(xdr.LedgerEntryExtensionV1 xdrEntryExtensionV1)
    {
        return new LedgerEntryExtensionV1(
            KeyPair.FromXdrPublicKey(xdrEntryExtensionV1.SponsoringID.InnerValue.InnerValue));
    }
}