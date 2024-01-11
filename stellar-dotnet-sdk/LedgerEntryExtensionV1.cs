using stellar_dotnet_sdk.xdr;

namespace stellar_dotnet_sdk;

public class LedgerEntryExtensionV1
{
    public KeyPair SponsoringID { get; set; }

    public static LedgerEntryExtensionV1 FromXdr(xdr.LedgerEntryExtensionV1 xdrEntryExtensionV1)
    {
        return new LedgerEntryExtensionV1
        {
            SponsoringID = KeyPair.FromXdrPublicKey(xdrEntryExtensionV1.SponsoringID.InnerValue.InnerValue)
        };
    }

    public xdr.LedgerEntryExtensionV1 ToXdr()
    {
        return new xdr.LedgerEntryExtensionV1
        {
            SponsoringID = new SponsorshipDescriptor(new AccountID { InnerValue = SponsoringID.XdrPublicKey }),
            Ext = new xdr.LedgerEntryExtensionV1.LedgerEntryExtensionV1Ext
            {
                Discriminant = 0
            }
        };
    }
}