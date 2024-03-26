using StellarDotnetSdk.Accounts;

namespace StellarDotnetSdk.LedgerEntries;

public class LedgerEntryExtensionV1
{
    public LedgerEntryExtensionV1(KeyPair sponsoringID)
    {
        SponsoringID = sponsoringID;
    }

    public KeyPair SponsoringID { get; }

    public static LedgerEntryExtensionV1 FromXdr(Xdr.LedgerEntryExtensionV1 xdrEntryExtensionV1)
    {
        return new LedgerEntryExtensionV1(
            KeyPair.FromXdrPublicKey(xdrEntryExtensionV1.SponsoringID.InnerValue.InnerValue));
    }
}